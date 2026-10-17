using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.MarioKart8.BinData.Parts;

namespace PartsEditor
{
    public class TireShaftTransform : ITireShaftTransform
    {
        [PartParam(SectionIdentifier.Body)]
        public float FrontScaleX { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public float FrontScaleYZ { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public float BackScaleX { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public float BackScaleYZ { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F FrontOffset { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F FrontAntiGravityOffset { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F Unknown1 { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F BackOffset { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F BackAntiGravityOffset { get; set; }

        [PartParam(SectionIdentifier.Body)]
        public Vector3F Unknown2 { get; set; }

    }
}
