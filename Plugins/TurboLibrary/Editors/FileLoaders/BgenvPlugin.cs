using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapStudio.UI;
using Toolbox.Core;
using System.IO;
using AGraphicsLibrary;
using GLFrameworkEngine;
using Toolbox.Core.ViewModels;
using CafeLibrary;

namespace TurboLibrary
{
    public class BgenvPlugin : FileEditor, IFileFormat
    {
        public string[] Description { get; }
        public string[] Extension { get; }

        /// <summary>
        /// Wether or not the file can be saved.
        /// </summary>
        public bool CanSave { get; set; }

        /// <summary>
        /// Information of the loaded file.
        /// </summary>
        public File_Info FileInfo { get; set; }

        //Lighting
        private SARC BgenvArchive;

        //Course model file. Need this to obtain bones.
        //Bones are used to place splot/point lighting
        public BFRES Bfres;

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            return false;
        }

        public override bool CreateNew()
        {
            FileInfo = new File_Info();
            FileInfo.FileName = "course_model.bgenv";
            Root.Header = "course.bgenv";

            return true;
        }

        public void Load(Stream stream)
        {
            BgenvArchive = new SARC();
            BgenvArchive.Load(stream);
        }

        public void SetupLighting(BFRES bfres, GLScene scene)
        {
            Root.Tag = this;
            Bfres = bfres;
            //Display bfres in the editor when switching to seperate light editor
            AddRender(bfres.Renderer);

            LightingEngine lightingEngine = new LightingEngine();
            lightingEngine.LoadArchive(BgenvArchive.Files.ToList(), BgenvArchive.BigEndian);
            LightingEngine.LightSettings = lightingEngine;
            LightingEngine.LightSettings.UpdateColorCorrectionTable();

            var context = GLContext.ActiveContext;

            //Generate light maps (area based lighting from directional and hemi lighting)
            foreach (var lmap in lightingEngine.Resources.LightMapFiles.Values)
            {
                foreach (var lightMapArea in lmap.LightAreas)
                    LightingEngine.LightSettings.UpdateLightmap(context, lightMapArea.Settings.Name);
            }

            lightingEngine.CubeMapsUpdate += delegate
            {
                //Load in map objects into map scene
                var renders = scene.Objects.Where(x => x is GenericRenderer).Cast<GenericRenderer>().ToList();

                LightingEngine.LightSettings.UpdateCubemap(renders, false);
            };
            lightingEngine.CubeMapsUpdate.Invoke(this, EventArgs.Empty);

            ReloadOutliner();
        }

        private void ReloadOutliner()
        {
            if (Bfres == null)
                return;

            return;

            var lightingEngine = LightingEngine.LightSettings;
            var course_lights = lightingEngine.Resources.EnvFiles["pointlight_course.baglenv"];

            NodeBase spotLightFolder = new NodeBase("SPOT_LIGHTS");
            NodeBase pointLightFolder = new NodeBase("POINT_LIGHTS");

            Root.AddChild(spotLightFolder);
            Root.AddChild(pointLightFolder);

            var bones = Bfres.ResFile.Models[0].Skeleton.Bones;
            foreach (var bone in bones.Values)
            {
                foreach (var light in course_lights.PointLights) {
                    if (string.IsNullOrEmpty(light.BonePrefix))
                        continue;

                    if (bone.Name.StartsWith(light.BonePrefix)) {
                        AddLightSource(light, bone, pointLightFolder);
                    }
                }
                foreach (var light in course_lights.SpotLights) {
                    if (string.IsNullOrEmpty(light.BonePrefix))
                        continue;

                    if (bone.Name.StartsWith(light.BonePrefix)) {
                        AddLightSource(light, bone, spotLightFolder);
                    }
                }
            }
        }

        private void AddLightSource(LightObject light, BfresLibrary.Bone bone, NodeBase parentFolder)
        {
            TransformableObject obj = new TransformableObject(parentFolder);
            obj.Transform.Position = new OpenTK.Vector3(
                bone.Position.X, bone.Position.Y, bone.Position.Z);
            obj.Transform.RotationEuler = new OpenTK.Vector3(
                bone.Rotation.X, bone.Rotation.Y, bone.Rotation.Z);
            obj.Transform.Scale = new OpenTK.Vector3(
                bone.Scale.X, bone.Scale.Y, bone.Scale.Z);
            obj.Transform.UpdateMatrix(true);

            obj.Transform.TransformUpdated += delegate
            {
                bone.Position = new Syroot.Maths.Vector3F(
                    obj.Transform.Position.X,
                    obj.Transform.Position.Y,
                    obj.Transform.Position.Z);
            };

            obj.UINode.Tag = light;
            obj.UINode.Header = bone.Name;
            AddRender(obj);
        }

        public void Save(Stream stream) {
            if (BgenvArchive == null)
                return;

            LightingEngine.LightSettings.SaveArchive(BgenvArchive.SarcData.Files);

            BgenvArchive.Save(stream);
        }
    }
}
