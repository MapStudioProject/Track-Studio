using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.MarioKart8.BinData;
using Syroot.NintenTools.MarioKart8.BinData.Parts;

namespace PartsEditor.TireBody
{
    public class TireShaftPointScale
    {
        [PartParam(SectionIdentifier.TireBody)]
        public Vector3F ArmPointPos { get; set; }

        [PartParam(SectionIdentifier.TireBody)]
        public Vector3F ArmPointAntiGPos { get; set; }

        [PartParam(SectionIdentifier.TireBody)]
        public Vector3F ArmPointUnused { get; set; } //unused? Always 0
    }
}
