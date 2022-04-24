using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class AmbientLight : EnvObject
    {
        internal override ParamObject Parent { get; set; }

        [BindGUI("Color", Category = "Properties")]
        public STColor Color
        {
            get { return Parent.GetEntryValue<Vector4F>("Color").ToSTColor(); }
            set { Parent.SetEntryValue("Color", value.ToColorF()); }
        }

        [BindGUI("Intensity", Category = "Properties")]
        public float Intensity
        {
            get { return Parent.GetEntryValue<float>("Intensity"); }
            set { Parent.SetEntryValue("Intensity", value); }
        }

        public AmbientLight(string name = "AmbientLight")
        {
            Parent = new ParamObject();
            Enable = true;
            Name = name;
            Group = "default";
            Intensity = 1.0f;
        }

        public AmbientLight(ParamObject paramObject) {
            Parent = paramObject;
        }
    }
}
