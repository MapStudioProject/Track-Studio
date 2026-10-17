using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.MarioKart8.BinData;
using Syroot.NintenTools.MarioKart8.BinData.Parts;

namespace PartsEditor
{
     public class PartParam : Attribute
    {
        public SectionIdentifier Section { get; set; }

        public int StartIndex { get; set; } 

        public PartParam(SectionIdentifier sectionIdentifier, int startIndex = -1)
        {
            Section = sectionIdentifier;
            StartIndex = startIndex;
        }
    }
}
