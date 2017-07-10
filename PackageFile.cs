using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LZ4;


namespace UrhoPackageExtract
{
    public class PackageFile : IDisposable
    {
        private FileStream _file = null;
        private BinaryReader _reader = null;
        private UInt32 _numberOfFiles = 0;
        private Dictionary<string, PackageEntry> _files = new Dictionary<string, PackageEntry>();

		public bool Compressed = false;
        public Dictionary<string, PackageEntry>.ValueCollection Entries
        {
            get
            {
                return _files.Values;    
            }
        }

		public PackageFile()
        {
        }

        public PackageFile(string filename)
        {
            Open(filename);
        }

        public void Dispose()
        {
            if (_file != null)
            {
                _file.Close();
                _file = null;
            }
        }

        public void Open(string filename)
        {
            _file = new FileStream(filename, FileMode.Open, FileAccess.Read);
            _reader = new BinaryReader(_file);

            CheckFileId();

            _numberOfFiles = _reader.ReadUInt32();

            // Skip checksum
            _reader.ReadUInt32();

            ReadFileEntries();
        }

        public byte[] GetFile(string name)
        {
            if (!_files.ContainsKey(name)) throw new Exception("File not in package");
            return GetFile(_files[name]);
        }

        public byte[] GetFile(PackageEntry entry)
        {
            AssertFile();

            byte[] data = new byte[entry.Length];
			_file.Seek(entry.Offset, SeekOrigin.Begin);

			if (Compressed)
            {
                UInt32 sizeLeft = entry.Length;
                UInt32 bufferOffset = 0;


                while (sizeLeft > 0)
                {
                    UInt16 unpackedSize = _reader.ReadUInt16();
                    UInt16 packedSize = _reader.ReadUInt16();

                    byte[] packedData = new byte[packedSize];
                    byte[] unpackedData = new byte[unpackedSize];

                    _file.Read(packedData, 0, packedData.Length);

                    LZ4Codec.Decode(packedData, 0, packedSize, unpackedData, 0, unpackedSize, true);

                    Array.Copy(unpackedData, 0, data, bufferOffset, unpackedData.Length);
                    bufferOffset += (UInt32) unpackedData.Length;
                    sizeLeft -= (UInt32) unpackedData.Length;
                }
			}
            else
            {
                _file.Read(data, 0, (int)entry.Length);
            }

            return data;
        }

        private void ReadFileEntries()
        {
			AssertFile();

            for (UInt32 i = 0; i < _numberOfFiles; i++)
            {
                string name = ReadFileName();
				UInt32 offset = _reader.ReadUInt32();
				UInt32 size = _reader.ReadUInt32();

				// Skip checksum
				_reader.ReadUInt32();

                _files[name] = new PackageEntry
                {
                    Name = name,
                    Offset = offset,
                    Length = size
                }; 
			}
		}

        private string ReadFileName()
        {
			AssertFile();
			
            MemoryStream mstr = new MemoryStream();

            byte b = _reader.ReadByte();
            while (b != 0)
            {
                mstr.WriteByte(b);
                b = _reader.ReadByte();
            }

            if (mstr.Length == 0) throw new Exception("Empty file name in archive");

            return Encoding.ASCII.GetString(mstr.ToArray());
        }

        private void CheckFileId()
        {
            AssertFile();

            byte[] id = _reader.ReadBytes(4);
            string idstr = Encoding.ASCII.GetString(id);

            if (idstr == "UPAK") return;
            if (idstr == "ULZ4")
            {
                Compressed = true;
                return;
            }

            throw new Exception("Not a valid package file");
        }

        private void AssertFile()
        {
            if (_file == null || _reader == null) throw new Exception("File not open");
        }
    }
}
