using System;
using System.Collections.Generic;

namespace TurboLibrary
{
    /// <summary>
    /// Represents an element of the Clip array, which consists of 4 short values.
    /// Each bit represents what area gets used of the 64 total bits.
    /// When a clip is set from a crossed lap point, any map object/bfres sub mesh will be hidden if inside any areas of the clip.
    /// </summary>
    [ByamlObject]
    public class Clip : List<int>
    {
        public List<ClipArea> Areas = new List<ClipArea>();

        public bool IsEdited = false;

        public Clip()
        {
      
        }

       public long Bitflag
        {
            get { return GetFlags(); }
        }

        public void DeserializeReferences(CourseDefinition courseDefinition)
        {
            foreach (var index in GetAreaIndices())
            {
                if (index < courseDefinition.ClipAreas.Count)
                    Areas.Add(courseDefinition.ClipAreas[index]);
            }
        }

        public void SerializeReferences(CourseDefinition courseDefinition)
        {
            List<int> indices = new List<int>();
            //Check for valid areas and add them to an index list to turn into flags
            foreach (var area in this.Areas)
            {
                int index = courseDefinition.ClipAreas.IndexOf(area);
                if (index != -1)
                    indices.Add(index);
            }
            //Turn indices into flags
            SetAreaIndices(indices);
        }

        public List<int> GetAreaIndices()
        {
            var bitFlag = GetFlags();
            List<int> areaIndices = new List<int>();

            //Search for all 64 bits to find areas (a max of 64 areas per clip)
            for (short i = 0; i < 64; i++)
            {
                //Check if the bit is set to 1 or not for an area present
                if ((bitFlag >> i & 1) != 0)
                    areaIndices.Add(i);
            }
            return areaIndices;
        }

        public void SetAreaIndices(List<int> indices)
        {
            //Create 2 seperate flags for bit calculating then combine them later
            uint flag1 = 0;
            uint flag2 = 0;

            for (int j = 0; j < 32; j++) {
                if (indices.Contains(j)) flag1 |= (1u << j);
            }
            for (int j = 32; j < 64; j++) {
                if (indices.Contains(j)) flag2 |= (1u << j);
            }

            var bytes1 = BitConverter.GetBytes(flag1);
            var bytes2 = BitConverter.GetBytes(flag2);

            //turn into 4 shorts
            this.Clear();
            this.Add(BitConverter.ToUInt16(new byte[2] { bytes1[0], bytes1[1] }));
            this.Add(BitConverter.ToUInt16(new byte[2] { bytes1[2], bytes1[3] }));
            this.Add(BitConverter.ToUInt16(new byte[2] { bytes2[0], bytes2[1] }));
            this.Add(BitConverter.ToUInt16(new byte[2] { bytes2[2], bytes2[3] }));
        }

        private long GetFlags()
        {
            //Store 4 ushorts into a single 8 bit flag.
            //Each bit representing an area in that clip.
            List<byte> flags = new List<byte>();
            for (int j = 0; j < 4; j++)
                flags.AddRange(BitConverter.GetBytes((ushort)this[j]));

            return BitConverter.ToInt64(flags.ToArray(), 0);
        }
    }
}
