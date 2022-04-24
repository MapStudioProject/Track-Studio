using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AampLibraryCSharp;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public class EnvironmentGraphics
    {
        private AampFile AampFile { get; set; }
        private ParamList Root { get; set; }

        public List<PointLightRig> PointLights = new List<PointLightRig>();
        public List<SpotLightRig> SpotLights = new List<SpotLightRig>();

        public List<DirectionalLight> DirectionalLights = new List<DirectionalLight>();
        public List<HemisphereLight> HemisphereLights = new List<HemisphereLight>();
        public List<AmbientLight> AmbientLights = new List<AmbientLight>();

        public List<Fog> FogObjects = new List<Fog>();
        public List<BloomObj> BloomObjects = new List<BloomObj>();
        public List<OfxLargeLensFlareRig> LensFlareRigs = new List<OfxLargeLensFlareRig>();

        public EnvironmentGraphics() {
            Root = new ParamList();
            InitiallizeArea();
        }

        public HemisphereLight GetAreaHemisphereLight(string type, int index)
        {
            for (int i = 0; i < HemisphereLights.Count; i++)
            {
                if (HemisphereLights[i].Name == $"HemiLight_{type}{index}")
                    return HemisphereLights[i];
            }
            HemisphereLights.Add(new HemisphereLight($"HemiLight_{type}{index}"));
            return HemisphereLights.Last();
        }


        public Fog GetAreaFog(bool isY, int index)
        {
            string name = isY ? $"Fog_Y{index}" : $"Fog_Main{index}";
            for (int i = 0; i < FogObjects.Count; i++)
            {
                if (FogObjects[i].Name == name)
                    return FogObjects[i];
            }
            FogObjects.Add(new Fog(name));
            return FogObjects.Last();
        }

        public BloomObj GetAreaBloom(int index)
        {
            for (int i = 0; i < BloomObjects.Count; i++)
            {
                if (BloomObjects[i].Name == $"Bloom{index}")
                    return BloomObjects[i];
            }
            BloomObjects.Add(new BloomObj($"Bloom{index}"));
            return BloomObjects.Last();
        }

        private void InitiallizeArea()
        {
            CreateHemi();
            CreateDirLights();
            CreateFog();
            CreateBloom();
        }

        private void InitiallizeLights()
        {
            CreatePointLights();
            CreateSpotLights();
        }

        private void CreatePointLights()
        {

        }

        private void CreateSpotLights()
        {

        }

        private void CreateDirLights()
        {
            DirectionalLights.Add(new DirectionalLight("MainLight0"));
        }

        private void CreateHemi()
        {
            //Create 10 hemisphere lights. 5 course, 5 character

            //The first 2 lights are the main focus so create specific settings for these
            HemisphereLights.Add(new HemisphereLight("HemiLight_chara0")
            {
                SkyColor = new STColor(0.15f, 1f, 1.85f, 1),
                GroundColor = new STColor(1.1f, 1.1f, 0.8f, 1),
                Intensity = 0.12f,
            });
            HemisphereLights.Add(new HemisphereLight("HemiLight_course0")
            {
                SkyColor = new STColor(1.1f, 1.1f, 0.8f, 1),
                GroundColor = new STColor(0.927106f, 0.950518f, 1.194f, 1),
                Intensity = 0.35f,
           });

            HemisphereLights.Add(new HemisphereLight("HemiLight_course1"));
            HemisphereLights.Add(new HemisphereLight("HemiLight_course2"));
            HemisphereLights.Add(new HemisphereLight("HemiLight_chara1"));
            HemisphereLights.Add(new HemisphereLight("HemiLight_chara2"));
            HemisphereLights.Add(new HemisphereLight("HemiLight_chara3"));
            HemisphereLights.Add(new HemisphereLight("HemiLight_chara4"));
            HemisphereLights.Add(new HemisphereLight("HemiLight_course3"));
            HemisphereLights.Add(new HemisphereLight("HemiLight_course4")); 
        }

        private void CreateFog()
        {
            FogObjects.Add(new Fog("Fog_Main0")
            {
                Start = -100.0f,
                End = 1000,
                Color = new STColor(0.05f,0.01f,0.1f, 0.1f),
            });
            FogObjects.Add(new Fog("Fog_Y0"));
            FogObjects.Add(new Fog("Fog_Main1"));
            FogObjects.Add(new Fog("Fog_Main2"));
            FogObjects.Add(new Fog("Fog_Main3"));
            FogObjects.Add(new Fog("Fog_Y1"));
            FogObjects.Add(new Fog("Fog_Y3"));
            FogObjects.Add(new Fog("Fog_Main4"));
            FogObjects.Add(new Fog("Fog_Y4"));
            FogObjects.Add(new Fog("Fog_Y2"));
        }

        private void CreateBloom()
        {

        }

        public EnvironmentGraphics(AampFile aamp) {
            LoadFile(aamp);
        }

        public void SaveFile(string fileName) {
            AampFile.Save(fileName);
        }

        public void LoadFile(string fileName) {
            LoadFile(AampFile.LoadFile(fileName));
        }

        public void LoadFile(AampFile aamp)
        {
            AampFile = aamp;
            Root = aamp.RootNode;

            HemisphereLights.Clear();
            FogObjects.Clear();
            DirectionalLights.Clear();
            BloomObjects.Clear();
            SpotLights.Clear();
            PointLights.Clear();

            foreach (var obj in aamp.RootNode.childParams)
            { 
                switch (obj.HashString)
                {
                    case "AmbientLight": break;
                    case "DirectionalLight":
                        foreach (var child in obj.paramObjects)
                            DirectionalLights.Add(new DirectionalLight(child));
                        break;
                    case "HemisphereLight":
                        foreach (var child in obj.paramObjects)
                            HemisphereLights.Add(new HemisphereLight(child));
                        break;
                    case "Fog":
                        foreach (var child in obj.paramObjects)
                            FogObjects.Add(new Fog(child));
                        break;
                    case "BloomObj":
                        foreach (var child in obj.paramObjects)
                            BloomObjects.Add(new BloomObj(child));
                        break;
                    case "PointLightRig":
                        foreach (var child in obj.paramObjects)
                            PointLights.Add(new PointLightRig(child));
                        Console.WriteLine($"PointLights {PointLights.Count}");
                        break;
                    case "SpotLightRig":
                        foreach (var child in obj.paramObjects)
                            SpotLights.Add(new SpotLightRig(child));
                        Console.WriteLine($"SpotLights {SpotLights.Count}");
                        break;
                    case "OfxLargeLensFlareRig":
                        foreach (var child in obj.paramObjects)
                            LensFlareRigs.Add(new OfxLargeLensFlareRig(child));
                        break;
                }
            }
        }

        public byte[] Save(bool isVersion2)
        {
            AampFile aamp = new AampFile();
            aamp.RootNode = this.Root;
            aamp.ParameterIOType = "aglenv";

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
