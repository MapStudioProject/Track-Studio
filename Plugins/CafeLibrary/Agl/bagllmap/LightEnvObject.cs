using System;
using System.Collections.Generic;
using System.Text;
using AampLibraryCSharp;

namespace AGraphicsLibrary
{
    public class LightEnvObject
    {
        private ParamObject Parent;

        public string Type
        {
            get { return Parent.GetEntryValue<StringEntry>("type").ToString(); }
            set { Parent.SetEntryValue("type", new StringEntry(value, 32)); }
        }

        public string Name
        {
            get { return Parent.GetEntryValue<StringEntry>("name").ToString(); }
            set { Parent.SetEntryValue("name", new StringEntry(value, 32)); }
        }

        public int calc_type
        {
            get { return Parent.GetEntryValue<int>("calc_type"); }
            set { Parent.SetEntryValue("calc_type", value); }
        }

        public string LutName
        {
            get { return Parent.GetEntryValue<StringEntry>("lut_name").ToString(); }
            set { Parent.SetEntryValue("lut_name", new StringEntry(value, 32)); }
        }

        public float effect
        {
            get { return Parent.GetEntryValue<float>("effect"); }
            set { Parent.SetEntryValue("effect", value); }
        }

        public float pow
        {
            get { return Parent.GetEntryValue<float>("pow"); }
            set { Parent.SetEntryValue("pow", value); }
        }

        public float pow_mip_max
        {
            get { return Parent.GetEntryValue<float>("pow_mip_max"); }
            set { Parent.SetEntryValue("pow_mip_max", value); }
        }

        public bool enable_mip0
        {
            get { return Parent.GetEntryValue<bool>("enable_mip0"); }
            set { Parent.SetEntryValue("enable_mip0", value); }
        }

        public bool enable_mip1
        {
            get { return Parent.GetEntryValue<bool>("enable_mip1"); }
            set { Parent.SetEntryValue("enable_mip1", value); }
        }

        public LightEnvObject(string type, string name, bool enable_mip0, bool enable_mip1)
        {
            Parent = new ParamObject();
            Name = name;
            LutName = "Lambert";
            pow = 1.0f;
            pow_mip_max = 1.0f;
            effect = 1.0f;
            calc_type = 0;
            this.Type = type;
            this.enable_mip0 = enable_mip0;
            this.enable_mip1 = enable_mip1;
        }

        public LightEnvObject(ParamObject obj)
        {
            Parent = obj;
        }
    }

}
