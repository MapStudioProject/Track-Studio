using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Toolbox.Core.IO;

namespace TurboLibrary
{
    public class SLink
    {
        public static SLink SoundLink = new SLink();

        public List<SoundTable> Tables = new List<SoundTable>();

        static uint stringTblOffset = 0;

        public SLink()
        {
        }

        public void Init()
        {
            if (Tables.Count > 0)
                return;

            string filePath = GlobalSettings.GetContentPath(System.IO.Path.Combine("audio","bin","slink.bin"));
            if (!File.Exists(filePath))
                return;

            Read(new FileReader(filePath));
        }

        private void Read(FileReader reader)
        {
            reader.SetByteOrder(true);
            reader.ReadSignature(4, "SLNK");
            reader.ReadUInt32(); //size
            reader.ReadUInt32(); //unk
            uint numEntries = reader.ReadUInt32();
            stringTblOffset = 16 + numEntries * 4;

            for (int i = 0; i < numEntries; i++)
            {
                uint offset = reader.ReadUInt32();
                using (reader.TemporarySeek(offset, SeekOrigin.Begin)) {
                    Tables.Add(new SoundTable(reader));
                }
            }
        }

        public class SoundTable
        {
            public string Name { get; set; }
            public SoundParameter[] Sounds { get; set; }
            public SoundSlot[] Slots { get; set; }

            public SoundTable(FileReader reader)
            {
                long pos = reader.Position;

                uint sectionCount = reader.ReadUInt32(); //guess. Always 1
                uint sectionSize = reader.ReadUInt32();
                Name = ReadString(reader);
                Console.WriteLine(Name);

                uint numSlots = reader.ReadUInt32();
                uint numActionSlots = reader.ReadUInt32();
                uint numSounds = reader.ReadUInt32();
                uint numTriggers = reader.ReadUInt32();
                uint unkValueOffset = reader.ReadUInt32(); //points to a uint with unk purpose
                float[] values = reader.ReadSingles(3);
                uint[] unks = reader.ReadUInt32s(2);
                Slots = new SoundSlot[numSlots];
                for (int i = 0; i < numSlots; i++)
                    Slots[i] = new SoundSlot(reader);
                reader.ReadBytes(20 * (int)numActionSlots);

                Sounds = new SoundParameter[numSounds];
                for (int i = 0; i < numSounds; i++)
                    Sounds[i] = new SoundParameter(reader);
            }
        }

        public class SoundSlot
        {
            public ushort[] Indices;
            public int[] values;

            public SoundSlot(FileReader reader)
            {
                Indices = reader.ReadUInt16s(2);
                values = reader.ReadInt32s(4);
            }
        }

        public class SoundParameter
        {
            public string Name { get; set; }

            public SoundParameter(FileReader reader)
            {
                reader.ReadUInt32s(5);

                Name = ReadString(reader);
                reader.ReadSingles(15);
                reader.ReadUInt32s(2);
            }
        }

        static string ReadString(FileReader reader)
        {
            uint offset = reader.ReadUInt32();
            using (reader.TemporarySeek(stringTblOffset + offset, SeekOrigin.Begin)) {
                return reader.ReadZeroTerminatedString();
            }
        }
    }
}
