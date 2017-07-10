using System;
namespace UrhoPackageExtract
{
    public class PackageEntry
    {
        public string Name { get; set; }
        public UInt32 Offset { get; set; }
        public UInt32 Length { get; set; }
    }
}
