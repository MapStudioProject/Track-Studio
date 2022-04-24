using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class CubeMapUint
    {
        private ParamObject Parent;

        [BindGUI("Name", Category = "Cube Map Uint")]
        public string Name
        {
            get { return Parent.GetEntryValue<StringEntry>("name").ToString(); }
            set { Parent.SetEntryValue("name", new StringEntry(value, 64)); }
        }

        [BindGUI("Position", Category = "Cube Map Uint")]
        public Vector3F Position
        {
            get { return Parent.GetEntryValue<Vector3F>("position"); }
            set { Parent.SetEntryValue("position", value); }
        }

        [BindGUI("Near", Category = "Cube Map Uint")]
        public float Near
        {
            get { return Parent.GetEntryValue<float>("near"); }
            set { Parent.SetEntryValue("near", value); }
        }

        [BindGUI("Far", Category = "Cube Map Uint")]
        public float Far
        {
            get { return Parent.GetEntryValue<float>("far"); }
            set { Parent.SetEntryValue("far", value); }
        }

        [BindGUI("IlluminantDistance", Category = "Cube Map Uint")]
        public float IlluminantDistance
        {
            get { return Parent.GetEntryValue<float>("illuminant_dist"); }
            set { Parent.SetEntryValue("illuminant_dist", value); }
        }

        [BindGUI("Gaussian_Repetition_Num", Category = "Cube Map Uint")]
        public int Gaussian_Repetition_Num
        {
            get { return Parent.GetEntryValue<int>("gaussian_repetition_num"); }
            set { Parent.SetEntryValue("gaussian_repetition_num", value); }
        }

        [BindGUI("Rendering_Repetition_Num", Category = "Cube Map Uint")]
        public int Rendering_Repetition_Num
        {
            get { return Parent.GetEntryValue<int>("rendering_repetition_num"); }
            set { Parent.SetEntryValue("rendering_repetition_num", value); }
        }

        public bool Enable
        {
            get { return Parent.GetEntryValue<bool>("enable"); }
            set { Parent.SetEntryValue("enable", value); }
        }

        public CubeMapUint(string name)
        {
            Parent = new ParamObject();
            Name = name;
            Position = new Vector3F(0, 1000, 0);
            Near = 1.0f;
            Far = 500000.0f;
            IlluminantDistance = -1;
            Gaussian_Repetition_Num = 1;
            Rendering_Repetition_Num = 1;
            Enable = true;
        }

        public CubeMapUint(ParamObject paramObject) {
            Parent = paramObject;
        }
    }
}
