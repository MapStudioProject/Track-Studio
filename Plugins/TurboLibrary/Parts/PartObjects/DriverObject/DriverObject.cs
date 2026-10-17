using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.MarioKart8.BinData;
using Syroot.NintenTools.MarioKart8.BinData.Parts;

namespace PartsEditor
{
    public  class DriverObject : PartObject
    {
        [PartParam(SectionIdentifier.DriverTOM)]
        internal Vector4B ModelFlags { get; set; }

        public byte DriverModel
        {
            get { return ModelFlags.X; }
            set { ModelFlags = new Vector4B(value, ModelFlags.Y); }
        }

        public byte BikerModel
        {
            get { return ModelFlags.Y; }
            set { ModelFlags = new Vector4B(ModelFlags.X, value); }
        }

        [PartParam(SectionIdentifier.DriverEffect, 0)]
        public Vector2S EffectFlags { get; set; }

        public DriverObject(string filePath, string partName)
            : base(filePath, partName)
        {

        }
    }
}
