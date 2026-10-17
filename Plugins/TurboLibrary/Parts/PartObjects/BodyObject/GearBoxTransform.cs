using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.MarioKart8.BinData;
using Syroot.NintenTools.MarioKart8.BinData.Parts;

namespace PartsEditor
{
    public class GearBoxTransform
    {
        [PartParam(SectionIdentifier.Body)]
        public Vector3F Scale { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F FrontOffset { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F BackOffset { get; set; }
    }
}
