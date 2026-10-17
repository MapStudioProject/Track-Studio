using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.MarioKart8.BinData.Parts;

namespace PartsEditor
{
    public class TireKartTransform : ITireTransform
    {
        [PartParam(SectionIdentifier.Body)]
        public Vector3F FrontOffset { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F BackOffset { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F FrontAntiGravityOffset { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F BackAntiGravityOffset { get; set; }
    }
}
