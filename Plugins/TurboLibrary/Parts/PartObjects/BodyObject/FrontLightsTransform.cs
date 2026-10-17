using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.MarioKart8.BinData;
using Syroot.NintenTools.MarioKart8.BinData.Parts;

namespace PartsEditor
{
    public class FrontLightsTransform
    {
        [PartParam(SectionIdentifier.Body)]
        public float SusLengthFront { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public float SusLengthBack { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F FrontLightOffset { get; set; }
    }
}
