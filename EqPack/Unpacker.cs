using EqPack;
using System.IO.Compression;

public class Unpacker
{
    public Unpacker()
    {
    }

    public void Unpack(string romPath, string outputPath)
    {
        var romData = File.ReadAllBytes(romPath);
        Unpack(romData, out var outputData);
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



    static byte[] Decompress(byte[] gzip)
    {
        var x = lzw.LzwDecompress(gzip);

        // Create a GZIP stream with decompression mode.
        // ... Then create a buffer and write into while reading from the GZIP stream.
        using (ZLibStream stream = new ZLibStream(new MemoryStream(gzip), CompressionMode.Decompress))
        {
            const int size = 8000;
            byte[] buffer = new byte[size];
            using (MemoryStream memory = new MemoryStream())
            {
                int count = 0;
                do
                {
                    count = stream.Read(buffer, 0, size);
                    if (count > 0)
                    {
                        memory.Write(buffer, 0, count);
                    }
                }
                while (count > 0);
                return memory.ToArray();
            }
        }
    }

    public bool Unpack(byte[] romData, out byte[] outputData)
    {
        const string evoxData = "$EvoxRom$";
        const string sharedRom = "SharedRom";

        outputData = new byte[0];

        Console.WriteLine("Eq Unpacker V1.0 for CerBios.");

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

        //if (!memcmp((char*)&unk_140BAA0 + i, aSharedrom_0, 9u))
        //    *(_DWORD*)(v11 + 14) -= &unk_800000;



        var data2blSize = ReadUint32(ref romData, romNameOffset + 10);
        Console.WriteLine($"2bl Size                : {data2blSize:x8}");
        var packedKernelOffset = ReadUint32(ref romData, romNameOffset + 14);
        Console.WriteLine($"PackedKernelOffset      : {packedKernelOffset:x8}");
        var unpackedKernelLength = ReadUint32(ref romData, romNameOffset + 22);
        Console.WriteLine($"UnpackedKernelLength    : {unpackedKernelLength:x8}");
        var packedKernelLength = ReadUint32(ref romData, romNameOffset + 18);
        Console.WriteLine($"PackedKernelLength      : {packedKernelLength:x8}");
        var packedOffset = 0xFFFFFF & packedKernelOffset + romSize - 0x800000;
        Console.WriteLine($"PackedOffset            : {packedOffset:x8}");

        var dest = new byte[packedKernelLength];
        Array.Copy(romData, packedOffset, dest, 0, packedKernelLength);



        //var disctionary = new byte[0x100];
        //var dest = new byte[packedKernelLength - disctionary.Length];
        //Array.Copy(romData, packedOffset, disctionary, 0, disctionary.Length);
        //Array.Copy(romData, packedOffset + disctionary.Length, dest, 0, packedKernelLength - disctionary.Length);

        ////File.WriteAllBytes(@"d:\kern.bin", dest);

        //var output = new byte[1 * 1024 * 1024];
        //K4os.Compression.LZ4.LZ4Codec.Decode(dest, output);

        //packedKernelLength += 100;

        //var kernelKey = new byte[] { 0xcb, 0xb8, 0xdb, 0x0f, 0x90, 0xb9, 0x60, 0x49, 0x0f, 0x7b, 0xac, 0xb9, 0x56, 0x20, 0xeb, 0xf2, 0xa2, 0xa5, 0x74, 0x3b };

        //var rc4 = new Rc4();
        //rc4.SetKey(kernelKey, kernelKey.Length);
        //rc4.Crypt(dest, 0, dest.Length);

        //File.WriteAllBytes(@"d:\kern.bin", dest);



        //var qq = File.ReadAllBytes(@"D:\ExtractedBios2");




        //for (int i = 0; i < 200000; i++)
        //{
        //    try
        //    {
        //        Array.Copy(romData, newofs+(i* 2), dest, 0, packedKernelLength);
        //        var r = h.UnpackKernelCabinet(qq);
        //        if (r.Length > 0)
        //        {
        //            var me = 1;
        //        }
        //        //File.WriteAllBytes(@"D:\ExtractedBios2", dest);
        //        //var x = lzw.LzwDecompress(dest);
        //        var q = 1;
        //    }
        //    catch
        //    {

        //    }
        //}

        var result = Decompress(dest);
        var q1 = 0;

        //sub_401040((int)&unk_140BAA0 + v5, (int)&unk_40BA60, *(_DWORD*)(v11 + 22));

        //v5 = (unsigned int)&unk_FFFFFF & (*(_DWORD *)(v11 + 14) + v7 - 0x800000);
        //printf(aPackOffset08x, (unsigned int) & unk_FFFFFF & (*(_DWORD*)(v11 + 14) + v7 - 0x800000));
        //sub_401040((int)&unk_140BAA0 + v5, (int)&unk_40BA60, *(_DWORD*)(v11 + 22));
        //printf(aData_raw08x, dword_40BA90);
        //printf(aData_vir08x, dword_40BA94);

        //    Evox_Pack_Kernel: ff7c4dda
        //Evox_Unpack_Len         : 000b1520 - (726304)
        //Evox_Pack_Len: 00036dfe - (224766)

        //*(_DWORD*)(v20 + 14) = 0xFF800000 - (DONORsIZE - 3C900);
        //*(_DWORD*)(v20 + 22) = UNPACKED KERNEL SIZE;
        //*(_DWORD*)(v20 + 18) = PACKED KERNEL SIZE;

        //var headedr1Size = ReadUint32(ref kernelData, 60);
        //var headedr2Size = ReadUint16(ref kernelData, headedr1Size + 20);
        //var sectionOffset = headedr1Size + headedr2Size + 24;
        //var sectionCount = ReadUint16(ref kernelData, headedr1Size + 6);
        //var dataSectionOffset = FindDataSection(ref kernelData, sectionOffset, sectionCount);
        //if (dataSectionOffset == 0)
        //{
        //    return false;
        //}

        //Console.WriteLine($"Name                    : .data");
        //var virtualAddress = ReadUint32(ref kernelData, dataSectionOffset + 12);
        //Console.WriteLine($"VirtualAddress          : {virtualAddress:x8}");
        //var virtualSize = ReadUint32(ref kernelData, dataSectionOffset + 8);
        //Console.WriteLine($"VirtualSize             : {virtualSize:x8}");
        //var pointerToRawData = ReadUint32(ref kernelData, dataSectionOffset + 20);
        //Console.WriteLine($"PointerToRawData        : {pointerToRawData:x8}");
        //var sizeOfRawData = ReadUint32(ref kernelData, dataSectionOffset + 16);
        //Console.WriteLine($"SizeOfRawData           : {sizeOfRawData:x8}");

        //Console.WriteLine("----------------------------------");

        //uint dataZeroFillSize = 0;
        //Console.WriteLine($"Data_ZeroFillSize       : {dataZeroFillSize:x8}");
        //uint dataSize = 0;
        //Console.WriteLine($"Data_Size               : {dataSize:x8}");
        //uint dataRaw = 0;
        //Console.WriteLine($"Data_Raw                : {dataRaw:x8}");
        //uint dataVir = 0;
        //Console.WriteLine($"Data_Vir                : {dataVir:x8}");

        //Console.WriteLine("----------------------------------");

        //for (uint i = 0; i < virtualSize; i += 4)
        //{
        //    var value = ReadUint32(ref kernelData, pointerToRawData + i);
        //    if (value != 0)
        //    {
        //        dataSize = i + 4;
        //    }
        //}

        //var data2blSize = ReadUint32(ref donorData, donorNameOffset + 10);

        //dataZeroFillSize = virtualSize - dataSize;
        //dataRaw = (uint)-(donorSize - (donorSize - data2blSize - dataSize));
        //dataVir = virtualAddress - 2147418112;

        //Console.WriteLine($"Data_ZeroFillSize       : {dataZeroFillSize:x8}");
        //Console.WriteLine($"Data_Size               : {dataSize:x8}");
        //Console.WriteLine($"Data_Raw                : {dataRaw:x8}");
        //Console.WriteLine($"Data_Vir                : {dataVir:x8}");

        //var tempOutputData = new byte[donorSize];
        //Array.Copy(kernelData, pointerToRawData, tempOutputData, 0, dataSize);
        //Array.Fill<byte>(kernelData, 0, (int)pointerToRawData, (int)dataSize);

        //Console.WriteLine($"Last NonZero            : {dataSize:x8}");
        //Console.WriteLine($"2bl Size                : {data2blSize:x8}");

        //Console.WriteLine("----------------------------------");

        //Console.WriteLine("Packing Kernel...");

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
        //var k = ReadUint32(ref kernelData, 20) + 24;

        //var c = ReadUint16(ref kernelData, k);


        var a = 1;
        // so far i know evox looks for $EvoxRom$ x3 for SharedRom - 0x1000 from end of rom and grabs the 32 bit offset from there which is typically 0x3700 the rest i dont know yet

        outputData = new byte[0];
        return true;
    }


}
