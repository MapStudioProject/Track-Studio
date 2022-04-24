using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;

namespace AGraphicsLibrary
{
    public class OfxLargeLensFlareRig : EnvObject
    {
        internal override ParamObject Parent { get; set; }

        [Category("Lens Flare")]
        public string ModelName
        {
            get { return Parent.GetEntryValue<StringEntry>("ModelName").ToString(); }
            set { Parent.SetEntryValue("ModelName", new StringEntry(value, 64)); }
        }

        [Category("Lens Flare")]
        public string BonePrefix
        {
            get { return Parent.GetEntryValue<StringEntry>("BonePrefix").ToString(); }
            set { Parent.SetEntryValue("BonePrefix", new StringEntry(value, 64)); }
        }

        [Category("Lens Flare")]
        public string MaterialPrefix
        {
            get { return Parent.GetEntryValue<StringEntry>("MaterialPrefix").ToString(); }
            set { Parent.SetEntryValue("MaterialPrefix", new StringEntry(value, 64)); }
        }

        [Category("Lens Flare")]
        public int PresetIdx
        {
            get { return Parent.GetEntryValue<int>("PresetIdx"); }
            set { Parent.SetEntryValue("PresetIdx", value); }
        }

        [Category("Lens Flare")]
        public Vector3F Offset
        {
            get { return Parent.GetEntryValue<Vector3F>("Offset"); }
            set { Parent.SetEntryValue("Offset", value); }
        }

        [Category("Lens Flare")]
        public Vector3F Direction
        {
            get { return Parent.GetEntryValue<Vector3F>("Direction"); }
            set { Parent.SetEntryValue("Direction", value); }
        }

        public OfxLargeLensFlareRig(ParamObject paramObject) {
            Parent = paramObject;
        }
    }
}
