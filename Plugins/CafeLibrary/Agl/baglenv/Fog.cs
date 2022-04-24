using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class Fog : EnvObject
    {
        internal override ParamObject Parent { get; set; }

        [BindGUI("Start", Category = "Properties")]
        public float Start
        {
            get { return Parent.GetEntryValue<float>("Start"); }
            set { Parent.SetEntryValue("Start", value); }
        }

        [BindGUI("End", Category = "Properties")]
        public float End
        {
            get { return Parent.GetEntryValue<float>("End"); }
            set { Parent.SetEntryValue("End", value); }
        }

        [BindGUI("Color", Category = "Properties")]
        public STColor Color
        {
            get { return Parent.GetEntryValue<Vector4F>("Color").ToSTColor(); }
            set { Parent.SetEntryValue("Color", value.ToColorF()); }
        }

        [BindGUI("Direction", Category = "Properties")]
        public Vector3F Direction
        {
            get { return Parent.GetEntryValue<Vector3F>("Direction"); }
            set { Parent.SetEntryValue("Direction", value); }
        }

        public Fog(string name)
        {
            Parent = new ParamObject();
            Enable = true;
            Name = name;
            Group = "Course_area";
            Start = 1000.0f;
            End = 10000.0f;
            Color = new STColor(1, 1, 1, 1);
            Direction = new Vector3F(0,0,-1);
        }

        public Fog(ParamObject paramObject)
        {
            Parent = paramObject;
        }
    }
}
