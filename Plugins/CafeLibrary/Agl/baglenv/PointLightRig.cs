using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class PointLightRig : LightObject
    {
        internal override ParamObject Parent { get; set; }

        [BindGUI("Specular Size", Category = "Properties")]
        public float SpecularSize
        {
            get { return Parent.GetEntryValue<float>("SpecularSize"); }
            set { Parent.SetEntryValue("SpecularSize", value); }
        }

        public PointLightRig(ParamObject paramObject) {
            Parent = paramObject;
        }
    }
}
