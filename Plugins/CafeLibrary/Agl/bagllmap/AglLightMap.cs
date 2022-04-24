using System;
using System.Collections.Generic;
using System.IO;
using AampLibraryCSharp;
using GLFrameworkEngine;
using OpenTK.Graphics.OpenGL;

namespace AGraphicsLibrary
{
    public class AglLightMap
    {
        public List<LightArea> LightAreas = new List<LightArea>();

        //A lookup of lightmaps per area. Used to get the static base light map generated on level load
        public Dictionary<string, GLTexture> Lightmaps = new Dictionary<string, GLTexture>();

        //A lookup of lut data configurable per light source.
        public GLTexture TextureLUT;

        public LUTParameter[] LutTable = new LUTParameter[32];

        internal ParamObject Parent { get; set; }

        public class LUTParameter
        {
            public string Name { get; set; }

            public Curve Intensity { get; set; }

            public static LUTParameter Create(string name, float[] curves) {
                return Create(name, 9, CurveType.Hermit2D, curves);
            }

            public static LUTParameter Create(string name, uint numUses, float[] curves) {
                return Create(name, numUses, CurveType.Hermit2D, curves);
            }

            public static LUTParameter Create(string name, uint numUses, CurveType type, float[] curves)
            {
                return new LUTParameter()
                {
                    Name = name,
                    Intensity = new Curve()
                    {
                        CurveType = type,
                        NumUses = numUses,
                        valueFloats = curves
                    },
                };
            }
        }

        public class LightArea
        {
            public LightSettings Settings { get; set; }
            public List<LightEnvObject> Lights = new List<LightEnvObject>();

            public bool Initialized = false;

            public LightArea()
            {

            }
        }

        class LightObject
        {
            public StringEntry Name { get; set; }
            public Curve Intensity { get; set; }
        }

        public AglLightMap()
        {
            LutTable[0] = LUTParameter.Create($"Lambert", new float[] { 0, 0, 0.5f, 0.5f, 0.5f, 0.5f, 1, 1, 0.5f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            LutTable[1] = LUTParameter.Create($"Half-Lambert", new float[] { 0, 0, 0.5f, 0.5f, 0.5f, 0.5f, 1, 1, 0.5f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            LutTable[2] = LUTParameter.Create($"Hemisphere", new float[] { 0, 0, 0.5f, 0.5f, 0.5f, 0.5f, 1, 1, 0.5f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            LutTable[3] = LUTParameter.Create($"Toon1", 18, new float[] { 0, 0, 0.042381f, 0.5f, 0, 0.090593f, 0.567106f, 0.345158f, 0.626382f, 0.582622f, 0.439986f, 0.791862f, 0.593483f, 0.560675f, 1.115393f, 1, 1, 0.042553f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });

            for (int i = 4; i < 32; i++)
                LutTable[i] = LUTParameter.Create($"UserData{i+1}", new float[] { 0, 0, 0.5f, 0.5f, 0.5f, 0.5f, 1, 1, 0.5f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });

            //Order matters.
            //Games have a default lmap name list and materials pull from these.
            string[] lmapNames = new string[] {
                "diffuse", "diffuse1", "diffuse2", "diffuse_course4", "diffuse_course3",
                "diffuse_course0", "diffuse3", "diffuse_course2", "diffuse_course1", "diffuse4",
            };
            string[] hemiNames = new string[] {
                "HemiLight_chara0", "HemiLight_chara1", "HemiLight_chara2", "HemiLight_course4", "HemiLight_course3",
                "HemiLight_course0", "HemiLight_chara3", "HemiLight_course2", "HemiLight_course1", "HemiLight_chara4",
            };

            for (int i = 0; i < 8; i++)
            {
                var area = new LightArea();
                area.Settings = new LightSettings(lmapNames[i]);
                area.Lights.Add(new LightEnvObject("AmbientLight", "", true, true));
                area.Lights.Add(new LightEnvObject("DirectionalLight", "MainLight0", true, false));
                area.Lights.Add(new LightEnvObject("DirectionalLight", "", true, false));
                area.Lights.Add(new LightEnvObject("DirectionalLight", "", true, false));
                area.Lights.Add(new LightEnvObject("DirectionalLight", "", true, false));
                area.Lights.Add(new LightEnvObject("HemisphereLight", hemiNames[i], true, true));
                LightAreas.Add(area);
            }
        }

        public AglLightMap(Stream stream) {
            var aamp = AampFile.LoadFile(stream);
            LightAreas.Clear();

            foreach (var ob in aamp.RootNode.paramObjects)
            {
                if (ob.HashString == "lut_param") {
                    //32 curves.
                    for (int i = 0; i < 32; i++)
                    {
                        LUTParameter param = new LUTParameter();
                        param.Name = ob.GetEntryValue<StringEntry>($"name{i}").ToString();
                        param.Intensity = GetCurve(ob, $"intensity{i}")[0];
                           LutTable[i] = param;
                    }
                }
            }
            foreach (var lightAreaParam in aamp.RootNode.childParams)
            {
                var lightArea = new LightArea();
                LightAreas.Add(lightArea);

                foreach (var ob in lightAreaParam.paramObjects)
                {
                    if (ob.HashString == "setting")
                        lightArea.Settings = new LightSettings(ob);
                }
                foreach (var c in lightAreaParam.childParams) {
                    if (c.HashString == "env_obj_ref_array")
                    {
                        foreach (var childObj in c.paramObjects)
                            lightArea.Lights.Add(new LightEnvObject(childObj));
                    }
                }
            }
            Console.WriteLine();
        }

        public int GetLUTIndex(string name)
        {
            for (int i = 0; i < LutTable.Length; i++)
            {
                if (LutTable[i].Name == name)
                    return i;
            }
            return -1;
        }

        public LightArea GetLightMapArea(int index)
        {
            return LightAreas[index];
        }

        public void GenerateLightmap(GLContext control, EnvironmentGraphics env, string name)
        {
            if (name == null)
                return;

            bool isTexture2DArray = LightingEngine.LightSettings.Resources.IsWiiU;

            GLTexture output = null;
            if (Lightmaps.ContainsKey(name)) {
                output = Lightmaps[name];
            }
            else
            {
                output = GLTextureCube.CreateEmptyCubemap(
                    32, PixelInternalFormat.Rgb32f, PixelFormat.Rgb, PixelType.Float, 2);
                if (isTexture2DArray)
                    output = GLTexture2DArray.CreateUncompressedTexture(
                32, 32, 6, 2, PixelInternalFormat.Rgb32f, PixelFormat.Rgb, PixelType.Float);

                Lightmaps.Add(name, output);

                //Allocate mip data. Need 2 seperate mip levels
                output.Bind();
                output.MinFilter = TextureMinFilter.LinearMipmapLinear;
                output.MagFilter = TextureMagFilter.Linear;
                output.UpdateParameters();
                output.GenerateMipmaps();
                output.Unbind();
            }

            LightmapManager.CreateLightmapTexture(control, this, env, name, output);

            Lightmaps[name] = output;
        }

        public void Setup()
        {
            TextureLUT = GenerateLutTexture();
        }

        public GLTexture GenerateLutTexture()
        {
            uint height = (uint)LutTable.Length;
            uint width = height * 4;

            float[] buffer = GenerateLUTImage(width, height);
            //var lut = GLTexture2D.CreateFloat32Texture((int)width, (int)height, buffer);
            var lut =  GLTexture2D.FromBitmap(CafeLibrary.Properties.Resources.gradient);

            lut.Bind();
            lut.WrapR = OpenTK.Graphics.OpenGL.TextureWrapMode.ClampToEdge;
            lut.WrapT = OpenTK.Graphics.OpenGL.TextureWrapMode.ClampToEdge;
            lut.WrapS = OpenTK.Graphics.OpenGL.TextureWrapMode.ClampToEdge;
            lut.MinFilter = OpenTK.Graphics.OpenGL.TextureMinFilter.Linear;
            lut.MagFilter = OpenTK.Graphics.OpenGL.TextureMagFilter.Linear;
            lut.UpdateParameters();
            lut.Unbind();

            return lut;
        }

        public float[] GenerateLUTImage(uint width, uint height)
        {
            float ratio = 1.0f / (width - 1);
            int index = 0;

            float[] data = new float[width * height];

            for (int h = 0; h < height; h++) {
                for (int w = 0; w < width; w++) {
                    data[index++] = CurveHelper.Interpolate(LutTable[h].Intensity, w * ratio);
                }
            }
            return data;
        }

        static Curve[] GetCurve(ParamObject ob, string hashName)
        {
            foreach (var entry in ob.paramEntries) {
                if (entry.HashString == hashName) {
                    return (Curve[])entry.Value;
                }
            }
            return new Curve[1];
        }

    }
}
