using System;
using System.IO;


namespace UrhoPackageExtract
{
    class MainClass
    {
        private static PackageFile _package;
        private static string _outputDir = ".";

        public static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("Usage: UrhoPackageExtract <file.pak> [output_directory]");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Package File {0} not found", args[0]);
                return;
            }

            if (args.Length == 2) _outputDir = args[1];

            _package = new PackageFile(args[0]);
            foreach (var entry in _package.Entries)
            {
                WriteFile(entry);
            }

        }

        private static void WriteFile(PackageEntry entry)
        {
            byte[] fileData = _package.GetFile(entry);
            string outFile = Path.Combine(_outputDir, entry.Name);

            EnsureDirectory(outFile);

            WriteFileData(outFile, fileData);
        }

        private static void WriteFileData(string outFile, byte[] fileData)
        {
            using (FileStream fs = new FileStream(outFile, FileMode.Create, FileAccess.Write))
            {
                fs.Write(fileData, 0, fileData.Length);
            }
        }

		private static void EnsureDirectory(string name)
        {
            string dir = Path.GetDirectoryName(name);
            if (String.IsNullOrEmpty(dir)) return;      // current dir

            if (! Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
