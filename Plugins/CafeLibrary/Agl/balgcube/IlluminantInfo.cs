using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class IlluminantInfo
    {
        private ParamObject Parent;

        [BindGUI("Enable", Category = "Illuminantion")]
        public bool Enable
        {
            get { return Parent.GetEntryValue<bool>("enable"); }
            set { Parent.SetEntryValue("enable", value); }
        }

        [BindGUI("Refer Entity", Category = "Illuminantion")]
        public string ReferEntity
        {
            get { return Parent.GetEntryValue<StringEntry>("refer_entity").ToString(); }
            set { Parent.SetEntryValue("refer_entity", new StringEntry(value, 32)); }
        }

        [BindGUI("Refer Texture", Category = "Illuminantion")]
        public string ReferTexture
        {
            get { return Parent.GetEntryValue<StringEntry>("refer_tex").ToString(); }
            set { Parent.SetEntryValue("refer_tex", new StringEntry(value, 32)); }
        }

        [BindGUI("Intensity", Category = "Illuminantion")]
        public float Intensity
        {
            get { return Parent.GetEntryValue<float>("intensity"); }
            set { Parent.SetEntryValue("intensity", value); }
        }

        [BindGUI("Scale", Category = "Illuminantion")]
        public float Scale
        {
            get { return Parent.GetEntryValue<float>("scale"); }
            set { Parent.SetEntryValue("scale", value); }
        }

        [BindGUI("Color", Category = "Illuminantion")]
        public STColor Color
        {
            get { return Parent.GetEntryValue<Vector4F>("color").ToSTColor(); }
            set { Parent.SetEntryValue("color", value.ToColorF()); }
        }

        [BindGUI("Distance", Category = "Illuminantion")]
        public float Distance
        {
            get { return Parent.GetEntryValue<float>("distance"); }
            set { Parent.SetEntryValue("distance", value); }
        }

        [BindGUI("Blend Type", Category = "Illuminantion")]
        public uint BlendType
        {
            get { return Parent.GetEntryValue<uint>("blend_type"); }
            set { Parent.SetEntryValue("blend_type", value); }
        }

        public IlluminantInfo(ParamObject paramObject) {
            Parent = paramObject;
        }
    }
}
