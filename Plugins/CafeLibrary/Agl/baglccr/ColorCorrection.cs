using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;
using Toolbox.Core.ViewModels;

namespace AGraphicsLibrary
{
    public class ColorCorrection
    {
        private ParamList Root;

        private ParamObject Parent;

        public ColorCorrection()
        {
            Root = new ParamList() { HashString = "param_root" };
            Root.childParams = new ParamList[1] { new ParamList() { HashString = "color_correction" } };
            Parent = new ParamObject();
            Root.childParams[0].paramObjects = new ParamObject[1] { Parent };
            Enable = true;
            Hue = 0;
            Saturation = 1;
            Brightness = 1;
            Gamma = 1;
            ToyCamEnable = true;
            ToyCamOffset = new STColor(0,0,0,1);
            ToyCamOffset2 = new STColor(0,0,0,1);
            ToyCamLevel1 = new STColor(1, 1, 1, 1);
            ToyCamLevel2 = new STColor(1, 1, 1, 1);
            ToyCamSaturation1 = 0.8f;
            ToyCamSaturation2 = 1.2f;
            ToyCamMulColor = new STColor(1, 1, 1, 1);
            ToyCamContrast = 1.0f;
            ToyCamBrightness = 1.0f;

            var curve = new Curve() {
                NumUses = 9,
                CurveType = CurveType.Hermit2D,
                valueFloats = new float[30] { 0,0,0.5f,0.5f,0.5f,0.5f,1,1,0.5f,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 },
            };

            Level = new Curve[4]
            {
                curve,curve,curve,curve
            };
        }

        [BindGUI("Enable", Category = "Color Correction")]
        public bool Enable
        {
            get { return Parent.GetEntryValue<bool>("enable"); }
            set { Parent.SetEntryValue("enable", value); }
        }

        [BindGUI("Hue", Category = "Color Correction")]
        [BindSlider(Min = 0, Max = 1, Increment = 0.0027f)]
        public float Hue
        {
            get { return Parent.GetEntryValue<float>("hue"); }
            set { Parent.SetEntryValue("hue", value); }
        }

        [BindGUI("Saturation", Category = "Color Correction")]
        [BindSlider(Min = 0, Max = 2, Increment = 0.1f)]
        public float Saturation
        {
            get { return Parent.GetEntryValue<float>("saturation"); }
            set { Parent.SetEntryValue("saturation", value); }
        }

        [BindGUI("Brightness", Category = "Color Correction")]
        [BindSlider(Min = 0, Max = 2, Increment = 0.1f)]
        public float Brightness
        {
            get { return Parent.GetEntryValue<float>("brightness"); }
            set { Parent.SetEntryValue("brightness", value); }
        }

        [BindGUI("Gamma", Category = "Color Correction")]
        [BindSlider(Min = 0, Max = 2.2f, Increment = 0.1f)]
        public float Gamma
        {
            get { return Parent.GetEntryValue<float>("gamma"); }
            set { Parent.SetEntryValue("gamma", value); }
        }

        [BindGUI("Enable", Category = "Toy Cam")]
        public bool ToyCamEnable
        {
            get { return Parent.GetEntryValue<bool>("toycam_enable"); }
            set { Parent.SetEntryValue("toycam_enable", value); }
        }

        [BindGUI("Offset", Category = "Toy Cam")]
        public STColor ToyCamOffset
        {
            get { return Parent.GetEntryValue<Vector4F>("toycam_offset1").ToSTColor(); }
            set { Parent.SetEntryValue("toycam_offset1", value.ToColorF()); }
        }

        [BindGUI("Offset 2", Category = "Toy Cam")]
        public STColor ToyCamOffset2
        {
            get { return Parent.GetEntryValue<Vector4F>("toycam_offset2").ToSTColor(); }
            set { Parent.SetEntryValue("toycam_offset2", value.ToColorF()); }
        }

        [BindGUI("Level 1", Category = "Toy Cam")]
        public STColor ToyCamLevel1
        {
            get { return Parent.GetEntryValue<Vector4F>("toycam_level1").ToSTColor(); }
            set { Parent.SetEntryValue("toycam_level1", value.ToColorF()); }
        }

        [BindGUI("Level 2", Category = "Toy Cam")]
        public STColor ToyCamLevel2
        {
            get { return Parent.GetEntryValue<Vector4F>("toycam_level2").ToSTColor(); }
            set { Parent.SetEntryValue("toycam_level2", value.ToColorF()); }
        }

        [BindGUI("Saturation 1", Category = "Toy Cam")]
        public float ToyCamSaturation1
        {
            get { return Parent.GetEntryValue<float>("toycam_saturation1"); }
            set { Parent.SetEntryValue("toycam_saturation1", value); }
        }

        [BindGUI("Saturation 2", Category = "Toy Cam")]
        public float ToyCamSaturation2
        {
            get { return Parent.GetEntryValue<float>("toycam_saturation2"); }
            set { Parent.SetEntryValue("toycam_saturation2", value); }
        }

        [BindGUI("Brightness", Category = "Toy Cam")]
        public float ToyCamBrightness
        {
            get { return Parent.GetEntryValue<float>("toycam_brightness"); }
            set { Parent.SetEntryValue("toycam_brightness", value); }
        }

        [BindGUI("Contrast", Category = "Toy Cam")]
        public float ToyCamContrast
        {
            get { return Parent.GetEntryValue<float>("toycam_contrast"); }
            set { Parent.SetEntryValue("toycam_contrast", value); }
        }

        [BindGUI("MulColor", Category = "Toy Cam")]
        public STColor ToyCamMulColor
        {
            get { return Parent.GetEntryValue<Vector4F>("toycam_mul_color").ToSTColor(); }
            set { Parent.SetEntryValue("toycam_mul_color", value.ToColorF()); }
        }

        public Curve[] Level
        {
            get {
                var param = Parent.paramEntries.FirstOrDefault(x => x.HashString == "level");
                return (Curve[])param.Value; }
            set { Parent.SetEntryValue("level", value); }
        }

        public ColorCorrection(AampFile aamp)
        {
            //Note this file always has only one object
            this.Root = aamp.RootNode;
            Parent = aamp.RootNode.paramObjects.FirstOrDefault();
        }

        public byte[] Save(bool isVersion2)
        {
            AampFile aamp = new AampFile();
            aamp.RootNode = this.Root;
            aamp.ParameterIOType = "aglccr";

            if (isVersion2)
                aamp = aamp.ConvertToVersion2();
            else
                aamp = aamp.ConvertToVersion1();

            var mem = new System.IO.MemoryStream();
            aamp.unknownValue = 1;
            aamp.Save(mem);
            return mem.ToArray();
        }
    }
}
