using System;
using System.Collections.Generic;
using System.Text;
using AampLibraryCSharp;

namespace AGraphicsLibrary
{
    public class LightSettings
    {
        private ParamObject Parent;

        public string Name
        {
            get { return Parent.GetEntryValue<StringEntry>("name").ToString(); }
            set { Parent.SetEntryValue("name", new StringEntry(value, 32)); }
        }

        public int lighting_hint
        {
            get { return Parent.GetEntryValue<int>("lighting_hint"); }
            set { Parent.SetEntryValue("lighting_hint", value); }
        }

        public bool hdr_enable
        {
            get { return Parent.GetEntryValue<bool>("hdr_enable"); }
            set { Parent.SetEntryValue("hdr_enable", value); }
        }

        public string rim_light_ref
        {
            get { return Parent.GetEntryValue<StringEntry>("rim_light_ref").ToString(); }
            set { Parent.SetEntryValue("rim_light_ref", new StringEntry(value, 32)); }
        }

        public bool rim_enable
        {
            get { return Parent.GetEntryValue<bool>("rim_enable"); }
            set { Parent.SetEntryValue("rim_enable", value); }
        }

        public float rim_effect
        {
            get { return Parent.GetEntryValue<float>("rim_effect"); }
            set { Parent.SetEntryValue("rim_effect", value); }
        }

        public float rim_width
        {
            get { return Parent.GetEntryValue<float>("rim_width"); }
            set { Parent.SetEntryValue("rim_width", value); }
        }
            
        public float rim_angle
        {
            get { return Parent.GetEntryValue<float>("rim_angle"); }
            set { Parent.SetEntryValue("rim_angle", value); }
        }

        public float rim_pow
        {
            get { return Parent.GetEntryValue<float>("rim_pow"); }
            set { Parent.SetEntryValue("rim_pow", value); }
        }

        public string parent_map
        {
            get { return Parent.GetEntryValue<StringEntry>("parent_map").ToString(); }
            set { Parent.SetEntryValue("parent_map", new StringEntry(value, 32)); }
        }

        public string copy_map
        {
            get { return Parent.GetEntryValue<StringEntry>("copy_map").ToString(); }
            set { Parent.SetEntryValue("copy_map", new StringEntry(value, 32)); }
        }

        public int mapping_type
        {
            get { return Parent.GetEntryValue<int>("mapping_type"); }
            set { Parent.SetEntryValue("mapping_type", value); }
        }

        public int priority
        {
            get { return Parent.GetEntryValue<int>("priority"); }
            set { Parent.SetEntryValue("priority", value); }
        }

        public LightSettings(string name) {
            Parent = new ParamObject();
            Name = name;
            lighting_hint = 0;
            hdr_enable = false;
            rim_light_ref = "";
            rim_effect = 1.0f;
            rim_angle = 1.0f;
            rim_pow = 2.0f;
            parent_map = "";
            copy_map = "";
            mapping_type = 0; 
        }

        public LightSettings(ParamObject obj) {
            Parent = obj;
        }
    }

}
