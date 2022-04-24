using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class HemisphereLight : EnvObject
    {
        internal override ParamObject Parent { get; set; }

        [BindGUI("SkyColor", Category = "Properties")]
        public STColor SkyColor
        {
            get { return Parent.GetEntryValue<Vector4F>("SkyColor").ToSTColor(); }
            set { Parent.SetEntryValue("SkyColor", value.ToColorF()); }
        }

        [BindGUI("GroundColor", Category = "Properties")]
        public STColor GroundColor
        {
            get { return Parent.GetEntryValue<Vector4F>("GroundColor").ToSTColor(); }
            set { Parent.SetEntryValue("GroundColor", value.ToColorF()); }
        }

        [BindGUI("Intensity", Category = "Properties")]
        [BindNumberBox(Min = 0, Max = 30f, Increment = 0.1f)]
        public float Intensity
        {
            get { return Parent.GetEntryValue<float>("Intensity"); }
            set { Parent.SetEntryValue("Intensity", value); }
        }

        [BindGUI("Direction", Category = "Properties")]
        public Vector3F Direction
        {
            get { return Parent.GetEntryValue<Vector3F>("Direction"); }
            set { Parent.SetEntryValue("Direction", value); }
        }

        public HemisphereLight(string name)
        {
            Parent = new ParamObject();
            Enable = true;
            Name = name;
            Group = "Course_area";
            SkyColor = new STColor(1.1f, 1.1f, 0.8f, 1);
            GroundColor = new STColor(0.927106f, 0.950518f, 1.194f, 1);
            Intensity = 0.35f;
            Direction = new Vector3F(0, 1, 0);
        }

        public HemisphereLight(ParamObject paramObject) {
            Parent = paramObject;
        }
    }
}
