using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AampLibraryCSharp;
using Syroot.Maths;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class LightObject : EnvObject
    {
        /// <summary>
        /// Gets or sets the name of the model that is affected by this object.
        /// </summary>
        [BindGUI("Model Name", Category = "Light")]
        public string ModelName
        {
            get { return Parent.GetEntryValue<StringEntry>("ModelName").ToString(); }
            set { Parent.SetEntryValue("ModelName", new StringEntry(value, 64)); }
        }

        /// <summary>
        /// Gets or sets the bone name prefix which determines which bones place this Light.
        /// </summary>
        [BindGUI("Bone Prefix", Category = "Light")]
        public string BonePrefix
        {
            get { return Parent.GetEntryValue<StringEntry>("BonePrefix").ToString(); }
            set { Parent.SetEntryValue("BonePrefix", new StringEntry(value, 64)); }
        }

        /// <summary>
        /// Gets or sets the material name prefix which determines which materials place this Light.
        /// </summary>
        [BindGUI("Material Prefix", Category = "Light")]
        public string MaterialPrefix
        {
            get { return Parent.GetEntryValue<StringEntry>("MaterialPrefix").ToString(); }
            set { Parent.SetEntryValue("MaterialPrefix", new StringEntry(value, 64)); }
        }

        /// <summary>
        /// Gets or sets a value that determines to use specular lighting.
        /// </summary>
        [BindGUI("Specular", Category = "Light")]
        public bool Specular
        {
            get { return Parent.GetEntryValue<bool>("Specular"); }
            set { Parent.SetEntryValue("Specular", value); }
        }

        /// <summary>
        /// Gets or sets the inverted specular color.
        /// </summary>
        [BindGUI("IndvSpecCol", Category = "Light")]
        public bool IndvSpecCol
        {
            get { return Parent.GetEntryValue<bool>("IndvSpecCol"); }
            set { Parent.SetEntryValue("IndvSpecCol", value); }
        }

        [BindGUI("Offset World", Category = "Light")]
        public bool IsOffsetWorld
        {
            get { return Parent.GetEntryValue<bool>("IsOffsetWorld"); }
            set { Parent.SetEntryValue("IsOffsetWorld", value); }
        }

        /// <summary>
        /// Gets or sets the lighting color.
        /// </summary>
        [BindGUI("Color", Category = "Light")]
        public STColor Color
        {
            get { return Parent.GetEntryValue<Vector4F>("Color").ToSTColor(); }
            set { Parent.SetEntryValue("Color", value.ToColorF()); }
        }

        /// <summary>
        /// Gets or sets the lighting color offset.
        /// </summary>
        [BindGUI("Color Offset", Category = "Light")]
        public STColor ColorOffset
        {
            get { return Parent.GetEntryValue<Vector4F>("ColorOffset").ToSTColor(); }
            set { Parent.SetEntryValue("ColorOffset", value.ToColorF()); }
        }

        /// <summary>
        /// Gets or sets the specular lighting color.
        /// </summary>
        [BindGUI("Specular Color", Category = "Light")]
        public STColor SpecularColor
        {
            get { return Parent.GetEntryValue<Vector4F>("SpecularColor").ToSTColor(); }
            set { Parent.SetEntryValue("SpecularColor", value.ToColorF()); }
        }

        /// <summary>
        /// Gets or sets the animation type.
        /// </summary>
        [BindGUI("AnmType", Category = "Light")]
        public int AnmType
        {
            get { return Parent.GetEntryValue<int>("AnmType"); }
            set { Parent.SetEntryValue("AnmType", value); }
        }

        /// <summary>
        /// Gets or sets the light intensity.
        /// </summary>
        [BindGUI("Intensity", Category = "Light")]
        public float Intensity
        {
            get { return Parent.GetEntryValue<float>("Intensity"); }
            set { Parent.SetEntryValue("Intensity", value); }
        }

        /// <summary>
        /// Gets or sets the lighting radius.
        /// </summary>
        [BindGUI("Radius", Category = "Light")]
        public float Radius
        {
            get { return Parent.GetEntryValue<float>("Radius"); }
            set { Parent.SetEntryValue("Radius", value); }
        }

        /// <summary>
        /// Gets or sets the lighting radius offset.
        /// </summary>
        [BindGUI("Radius Offset", Category = "Light")]
        public float RadiusOffset
        {
            get { return Parent.GetEntryValue<float>("RadiusOffset"); }
            set { Parent.SetEntryValue("RadiusOffset", value); }
        }

        [BindGUI("Cycle A", Category = "Light")]
        public float CycleA
        {
            get { return Parent.GetEntryValue<float>("CycleA"); }
            set { Parent.SetEntryValue("CycleA", value); }
        }

        [BindGUI("Cycle B", Category = "Light")]
        public float CycleB
        {
            get { return Parent.GetEntryValue<float>("CycleB"); }
            set { Parent.SetEntryValue("CycleB", value); }
        }

        [BindGUI("Cycle AR", Category = "Light")]
        public float CycleAR
        {
            get { return Parent.GetEntryValue<float>("CycleA"); }
            set { Parent.SetEntryValue("CycleA", value); }
        }

        [BindGUI("Cycle BR", Category = "Light")]
        public float CycleBR
        {
            get { return Parent.GetEntryValue<float>("CycleB"); }
            set { Parent.SetEntryValue("CycleB", value); }
        }

        [BindGUI("Damp Param", Category = "Light")]
        public float DampParam
        {
            get { return Parent.GetEntryValue<float>("DampParam"); }
            set { Parent.SetEntryValue("DampParam", value); }
        }

        [BindGUI("Offset", Category = "Light")]
        public Vector3F Offset
        {
            get { return Parent.GetEntryValue<Vector3F>("Offset"); }
            set { Parent.SetEntryValue("Offset", value); }
        }


        [BindGUI("Is Glare", Category = "Glare")]
        public bool IsGlare
        {
            get { return Parent.GetEntryValue<bool>("IsGlare"); }
            set { Parent.SetEntryValue("IsGlare", value); }
        }

        [BindGUI("Shape", Category = "Glare")]
        public int GlareShape
        {
            get { return Parent.GetEntryValue<int>("GlareShape"); }
            set { Parent.SetEntryValue("GlareShape", value); }
        }

        [BindGUI("Size", Category = "Glare")]
        public float GlareSize
        {
            get { return Parent.GetEntryValue<float>("GlareSize"); }
            set { Parent.SetEntryValue("GlareSize", value); }
        }

        [BindGUI("Direction Power", Category = "Glare")]
        public float GlareDirPower
        {
            get { return Parent.GetEntryValue<float>("GlareDirPower"); }
            set { Parent.SetEntryValue("GlareDirPower", value); }
        }

        [BindGUI("Intensity", Category = "Glare")]
        public float GlareIntensity
        {
            get { return Parent.GetEntryValue<float>("GlareIntensity"); }
            set { Parent.SetEntryValue("GlareIntensity", value); }
        }

        [BindGUI("Depth Offset", Category = "Glare")]
        public float GlareDepthOfs
        {
            get { return Parent.GetEntryValue<float>("GlareDepthOfs"); }
            set { Parent.SetEntryValue("GlareDepthOfs", value); }
        }

        [BindGUI("Direction", Category = "Glare")]
        public Vector3F GlareDir
        {
            get { return Parent.GetEntryValue<Vector3F>("GlareDir"); }
            set { Parent.SetEntryValue("GlareDir", value); }
        }
    }
}
