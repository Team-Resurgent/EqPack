var unpacker = new Unpacker();
//unpacker.Unpack(@"D:\Git\ResurgentBios\yoshi_tools\Yoshi-final-256k-2005.bin", @"");

unpacker.Unpack(@"d:\xbox\xboxbuilds\fre\Cerbios16.bin", @"");

var packer = new Packer();
packer.Pack(@"D:\Git\ResurgentBios\xboxbuilds\fre\xboxkrnl.exe", @"D:\Git\ResurgentBios\yoshi_tools\Yoshi-final-256k-2005.bin", @"");