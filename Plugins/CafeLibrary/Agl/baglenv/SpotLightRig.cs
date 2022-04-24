using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    /// <summary>
    /// Represents an object that casts lighting from a bone to a spot.
    /// </summary>
    public class SpotLightRig : LightObject
    {
        internal override ParamObject Parent { get; set; }

        [BindGUI("Follow Direction", Category = "Properties")]
        public bool IsFollowDir
        {
            get { return Parent.GetEntryValue<bool>("IsFollowDir"); }
            set { Parent.SetEntryValue("IsFollowDir", value); }
        }

        [BindGUI("Angle", Category = "Properties")]
        public float Angle
        {
            get { return Parent.GetEntryValue<float>("Angle"); }
            set { Parent.SetEntryValue("Angle", value); }
        }

        [BindGUI("Angle Damp", Category = "Properties")]
        public float AngleDamp
        {
            get { return Parent.GetEntryValue<float>("AngleDamp"); }
            set { Parent.SetEntryValue("AngleDamp", value); }
        }

        [BindGUI("Direction", Category = "Properties")]
        public Vector3F Direction
        {
            get { return Parent.GetEntryValue<Vector3F>("Direction"); }
            set { Parent.SetEntryValue("Direction", value); }
        }

        public SpotLightRig(ParamObject paramObject) {
            Parent = paramObject;
        }
    }
}
