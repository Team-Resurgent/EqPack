public class Packer
{
	public Packer()
    {
    }

    public void Pack(string kernelPath, string romPath, string outputPath)
    {
        var kernelData = File.ReadAllBytes(kernelPath);
        var romData = File.ReadAllBytes(romPath);
        Pack(kernelData, romData, out var outputData);
    }

    private bool StringInBuffer(ref byte[] fileData, uint offset, string search)
    {
        for (int i = 0; i < search.Length; i++)
        {
            if (fileData[offset + i] != search[i])
            {
                return false;
            }

        }
        return true;
    }

    private ushort ReadUint16(ref byte[] fileData, uint offset)
    {
        ushort result = 0;
        for (int i = 0; i < 2; i++)
        {
            result |= (ushort)(fileData[i + offset] << (8 * i));
        }
        return result;
    }

    private uint ReadUint32(ref byte[] fileData, uint offset)
    {
        uint result = 0;
        for (int i = 0; i < 4; i++)
        {
            result |= (uint)(fileData[i + offset] << (8 * i));
        }
        return result;
    }

    private uint FindDataSection(ref byte[] kernelData, uint sectionOffset, uint sectionCount)
    {
        const string dataSection = ".data";

        for (int i = 0; i < sectionCount; i++)
        {
            if (StringInBuffer(ref kernelData, sectionOffset, dataSection))
            {
                return sectionOffset;
            }
            sectionOffset += 40;
        }
        return 0;
    }

    public bool Pack(byte[] kernelData, byte[] romData, out byte[] outputData)
    {
        const string evoxData = "$EvoxRom$";
        const string sharedRom = "SharedRom";

        outputData = new byte[0];

        Console.WriteLine("Eq Packer V1.0 for CerBios.");
        Console.WriteLine($"Kernel Size {kernelData.Length}");

        uint romNameOffset = 0;
        uint romSize = (uint)romData.Length;
        for (uint i = (romSize - 1) & 0xfffffff0; i > (romSize - 0x1000); i -= 2)
        {
            if (StringInBuffer(ref romData, i, evoxData))
            {
                romNameOffset = i;
                Console.WriteLine($"Found Evox bios @ {i:x8}");
                break;
            }
            if (StringInBuffer(ref romData, i, sharedRom))
            {
                romNameOffset = i;
                Console.WriteLine($"Found Yoshi bios @ {i:x8}");
                break;
            }
        }

        if (romNameOffset == 0)
        {
            return false;
        }

        var headedr1Size = ReadUint32(ref kernelData, 60); 
        var headedr2Size = ReadUint16(ref kernelData, headedr1Size + 20);
        var sectionOffset = headedr1Size + headedr2Size + 24;
        var sectionCount = ReadUint16(ref kernelData, headedr1Size + 6);
        var dataSectionOffset = FindDataSection(ref kernelData, sectionOffset, sectionCount);
        if (dataSectionOffset == 0)
        {
            return false; 
        }

        Console.WriteLine($"Name                    : .data");
        var virtualAddress = ReadUint32(ref kernelData, dataSectionOffset + 12);
        Console.WriteLine($"VirtualAddress          : {virtualAddress:x8}");
        var virtualSize = ReadUint32(ref kernelData, dataSectionOffset + 8);
        Console.WriteLine($"VirtualSize             : {virtualSize:x8}");
        var pointerToRawData = ReadUint32(ref kernelData, dataSectionOffset + 20);
        Console.WriteLine($"PointerToRawData        : {pointerToRawData:x8}");
        var sizeOfRawData = ReadUint32(ref kernelData, dataSectionOffset + 16);
        Console.WriteLine($"SizeOfRawData           : {sizeOfRawData:x8}");

        Console.WriteLine("----------------------------------");

        uint dataZeroFillSize = 0;
        Console.WriteLine($"Data_ZeroFillSize       : {dataZeroFillSize:x8}");
        uint dataSize = 0;
        Console.WriteLine($"Data_Size               : {dataSize:x8}");
        uint dataRaw = 0;
        Console.WriteLine($"Data_Raw                : {dataRaw:x8}");
        uint dataVir = 0;
        Console.WriteLine($"Data_Vir                : {dataVir:x8}");

        Console.WriteLine("----------------------------------");

        for (uint i = 0; i < virtualSize; i += 4)
        {
            var value = ReadUint32(ref kernelData, pointerToRawData + i);
            if (value != 0) {
                dataSize = i + 4;
            }
        }

        var data2blSize = ReadUint32(ref romData, romNameOffset + 10);

        dataZeroFillSize = virtualSize - dataSize;
        dataRaw = (uint)-(romSize - (romSize - data2blSize - dataSize));
        dataVir = virtualAddress - 2147418112;

        Console.WriteLine($"Data_ZeroFillSize       : {dataZeroFillSize:x8}");
        Console.WriteLine($"Data_Size               : {dataSize:x8}");
        Console.WriteLine($"Data_Raw                : {dataRaw:x8}");
        Console.WriteLine($"Data_Vir                : {dataVir:x8}");

        var tempOutputData = new byte[romSize];
        Array.Copy(kernelData, pointerToRawData, tempOutputData, 0, dataSize);
        Array.Fill<byte>(kernelData, 0, (int)pointerToRawData, (int)dataSize);

        Console.WriteLine($"Last NonZero            : {dataSize:x8}");
        Console.WriteLine($"2bl Size                : {data2blSize:x8}");

        Console.WriteLine("----------------------------------");

        Console.WriteLine("Packing Kernel...");

        //*(_DWORD*)(v20 + 14) = 0xFF800000 - (DONORsIZE - 3C900);
        //*(_DWORD*)(v20 + 22) = UNPACKED KERNEL SIZE;
        //*(_DWORD*)(v20 + 18) = PACKED KERNEL SIZE;
        //kernel is stored at 3C900

        // memcpy(&v14[dataSize], pointerToRawData, dataSize);

        //        ----------------------------------
        //Data_ZeroFillSize       : 00005674
        //Data_Size: 00000e98
        //Data_Raw: ffffba68
        //Data_Vir                : 8003e1a0


        //3732
        //var aa = donorSize - ReadUint32(ref donorData, donorNameOffset + 10);

        //Kernel check
        var k = ReadUint32(ref kernelData, 20) + 24;

        var c = ReadUint16(ref kernelData, k);


        var a = 1;
        // so far i know evox looks for $EvoxRom$ x3 for SharedRom - 0x1000 from end of rom and grabs the 32 bit offset from there which is typically 0x3700 the rest i dont know yet

        outputData = new byte[0];
        return true;
    }


}
