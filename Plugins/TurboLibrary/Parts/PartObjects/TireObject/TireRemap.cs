using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.MarioKart8.BinData;
using Syroot.NintenTools.MarioKart8.BinData.Parts;

namespace PartsEditor
{
    public class TireRemap : PartObject
    {
        /// <summary>
        /// Gets or sets the spin speed of the wheel.
        /// </summary>
        [PartParam(SectionIdentifier.TireRP)]
        public int BodyTireIndex { get; set; }

        private int TireIndex;

        public TireRemap(int tireIndex) 
        {
            TireIndex = tireIndex;
            UpdateGetters();
        }

        internal override Dword GetValue(SectionIdentifier type, int index)
        {
            Section section = MushResParts.BinFile.GetSectionByID((uint)type);
            var data = ((DwordSectionData)section.Data).Data;
            //Get a list of all active indices for the current combination of parts
            return data[0][TireIndex][index];
        }

        internal override void SetValue(SectionIdentifier type, int index, Dword value)
        {
            Section section = MushResParts.BinFile.GetSectionByID((uint)type);
            var data = ((DwordSectionData)section.Data).Data;
            //Get a list of all active indices for the current combination of parts
            data[0][TireIndex][index] = value;
        }
    }
}
