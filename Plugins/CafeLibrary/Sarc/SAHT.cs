using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using Toolbox.Core.IO;

namespace CafeLibrary
{
    public class SAHT
    {
        public SAHT() { }

        public SAHT(string filePath) {
            Read(new FileReader(filePath));
        }

        public SAHT(byte[] data) {
            Read(new FileReader(data));
        }

        public Dictionary<uint, string> HashEntries = new Dictionary<uint, string>();

        private void Read(FileReader reader)
        {
            if (reader.ReadString(4) != "SAHT")
                throw new Exception("Wrong magic");
            uint FileSize = reader.ReadUInt32();
            uint Offset = reader.ReadUInt32();
            if (Offset !=  0x10) //EFE uses big endian. WT uses little.
            {
                Offset = 0x10;
                reader.SetByteOrder(true);
            }


            uint EntryCount = reader.ReadUInt32();

            Console.WriteLine($"FileSize {FileSize} Offset {Offset} EntryCount {EntryCount}");

            reader.SeekBegin(Offset);
            for (int i = 0; i < EntryCount; i++)
            {
                HashEntry entry = new HashEntry();
                entry.Read(reader);
                reader.Align(16);
                if (EntryCount == 4)
                Console.WriteLine($"{entry.Name} {entry.Hash} {EntryCount}");

                HashEntries.Add(entry.Hash, entry.Name);
            }
        }

        public class HashEntry
        {
            public uint Hash { get; set; }
            public string Name { get; set; }

            public void Read(BinaryDataReader reader)
            {
                Hash = reader.ReadUInt32();
                Name = reader.ReadString(BinaryStringFormat.ZeroTerminated);
            }
        }
    }
}
