using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class DirectionalLight : EnvObject
    {
        internal override ParamObject Parent { get; set; }

        [BindGUI("Diffuse Color", Category = "Properties")]
        public STColor DiffuseColor
        {
            get { return Parent.GetEntryValue<Vector4F>("DiffuseColor").ToSTColor(); }
            set { Parent.SetEntryValue("DiffuseColor", value.ToColorF()); }
        }

        [BindGUI("Specular Color", Category = "Properties")]
        public STColor SpecularColor
        {
            get { return Parent.GetEntryValue<Vector4F>("SpecularColor").ToSTColor(); }
            set { Parent.SetEntryValue("SpecularColor", value.ToColorF()); }
        }

        [BindGUI("Backside Color", Category = "Properties")]
        public STColor BacksideColor
        {
            get { return Parent.GetEntryValue<Vector4F>("BacksideColor").ToSTColor(); }
            set { Parent.SetEntryValue("BacksideColor", value.ToColorF()); }
        }

        [BindGUI("Intensity", Category = "Properties")]
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

        [BindGUI("View Coordinate", Category = "Properties")]
        public bool ViewCoordinate
        {
            get { return Parent.GetEntryValue<bool>("ViewCoordinate"); }
            set { Parent.SetEntryValue("ViewCoordinate", value); }
        }

        public DirectionalLight(string name)
        {
            Parent = new ParamObject();
            Enable = true;
            Name = name;
            Group = "Course_area";
            DiffuseColor = new STColor(1.05f, 1.05f, 0.9f, 1);
            SpecularColor = new STColor(0, 0, 0, 1);
            BacksideColor = new STColor(0, 0, 0, 1);
            Intensity = 1.8f;
            Direction = new Vector3F(-0.17101f, -0.866025f, 0.469846f);
            ViewCoordinate = false;
        }

        public DirectionalLight(ParamObject paramObject) {
            Parent = paramObject;
        }
    }
}
