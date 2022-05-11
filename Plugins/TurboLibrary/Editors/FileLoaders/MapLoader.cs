using System;
using System.Collections.Generic;
using System.IO;
using CafeLibrary;
using KclLibrary;
using MapStudio.UI;
using Toolbox.Core.IO;
using Toolbox.Core.Hashes;
using CafeLibrary.Rendering;
using GLFrameworkEngine;

namespace TurboLibrary.MuuntEditor
{
    public class MapLoader
    {
        //Debugging
        private bool DEBUG_PROBES = false;

        public CourseMuuntPlugin Plugin;

        /// <summary>
        /// The course information such as object, area, lap path and ai path placement.
        /// </summary>
        public CourseDefinition CourseDefinition = new CourseDefinition();

        /// <summary>
        /// The collision file.
        /// </summary>
        public KclPlugin CollisionFile = new KclPlugin();

        //Lighting
        public BgenvPlugin BgenvFile = null;

        public MapCamera MapCamera = new MapCamera();

        public Toolbox.Core.STGenericTexture MapTexture;

        //Models

        /// <summary>
        /// The main course model.
        /// </summary>
        public BfresRender CourseModel = null;

        public BFRES BfresEditor;

        string ModelHash;

        //Game handling
        private MapFieldAccessor MapFieldAccessor = new MapFieldAccessor();

        //Effects

        public static MapLoader Instance = null;

        public MapLoader(CourseMuuntPlugin plugin)
        {
            Instance = this;
            Plugin = plugin;
        }

        /// <summary>
        /// Initiates an empty scene with blank files.
        /// </summary>
        public void Init(CourseMuuntPlugin plugin, bool isSwitch, string model_path = "")
        {
            BfresEditor = new BFRES();
            BfresEditor.Root.IsExpanded = true;
            BfresEditor.CreateNew();
            BfresEditor.FileInfo.FileName = "course_model.szs";
            BfresEditor.Root.Header = "course_model.szs";
            BfresEditor.ResFile.Name = "course_model.szs";

            if (isSwitch)
            {
                BfresEditor.ResFile.ChangePlatform(true, 4096, 0, 5, 0, 3,
                  new BfresLibrary.PlatformConverters.ConverterHandle());
                BfresEditor.ResFile.Alignment = 0x0C;
            }
            if (!string.IsNullOrEmpty(model_path))
                BfresEditor.ModelFolder.ImportNewModel(model_path);

            CollisionFile = new KclPlugin();
            CollisionFile.CreateNew();
            CollisionFile.CollisionRender.IsVisible = false;
            plugin.AddRender(CollisionFile.CollisionRender);
            if (isSwitch)
                CollisionFile.SetAsSwitchKcl();

            BgenvFile = new BgenvPlugin();
            BgenvFile.FileInfo = new Toolbox.Core.File_Info();
            BgenvFile.FileInfo.FileName = "course.bgenv";

            if (!isSwitch)
            {
                MapTexture = new BflimTexture();
                ((BflimTexture)MapTexture).InitDefault();
                MapTexture.Name = "course_maptexture";
            }

            MapCamera = new MapCamera();

            if (isSwitch)
                MapCamera.IsBigEndian = false;
            if (isSwitch)
                CourseDefinition.SetAsSwitchByaml();

            CourseDefinition.Objs = new List<Obj>();
            //Spawn
            CourseDefinition.Objs.Add(new Obj() { ObjId = 6003, });
            //Skybox
            CourseDefinition.Objs.Add(new Obj() { ObjId = 7023, });
        }

        public void Load(CourseMuuntPlugin plugin, string folder, string byamlFile, string workingDir)
        {
            Instance = this;
            Plugin = plugin;

            SLink.SoundLink.Init();

            BfresLoader.TargetShader = typeof(TurboNXRender);

       //     LoadCourseEffects(GetFile("course.ptcl"));

            LoadMapUint(byamlFile);

            //Load either the project directory or working directory
            string GetFile(string filePath)
            {
                if (File.Exists($"{folder}\\{filePath}"))
                    return $"{folder}\\{filePath}";
                else
                    return $"{workingDir}\\{filePath}";
            }

            LoadColllsion(GetFile("course.kcl"));
            LoadColllsion(GetFile("course_kcl.szs")); //Compressed in deluxe
            LoadCourseModel(GetFile("course_model.szs"));
            LoadLightingResources(GetFile("course.bgenv"));
            LoadCameraParams(GetFile("course_mapcamera.bin"));
            LoadCameraTexture(GetFile("course_maptexture.bflim"));
            LoadCameraTextureDX(GetFile("course_maptexture.bntx"));

            if (DEBUG_PROBES)
            {
                LoadProbeLighting(GetFile("course.bglpbd"));
                LoadProbeLighting(GetFile("course_bglpbd.szs"));
            }
            if (CollisionFile.CollisionRender != null)
                Plugin.AddRender(CollisionFile.CollisionRender);
        }

        public void SetupLighting()
        {
            BgenvFile?.SetupLighting(BfresEditor, Plugin.Scene);
        }

        private void LoadCameraParams(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            MapCamera = new MapCamera(filePath, !CourseDefinition.IsSwitch);
        }

        private void LoadCameraTexture(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            MapTexture = new BflimTexture(filePath);
        }

        private void LoadCameraTextureDX(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            MapTexture = new BntxTexture(filePath);
            MapTexture.LoadRenderableTexture();
        }

        private void LoadMapUint(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            ProcessLoading.Instance.Update(15, 100, "Loading Course Byaml");

            CourseDefinition = new CourseDefinition(filePath);

            MapFieldAccessor.Setup(CourseDefinition);
        }

        public void LoadCourseModel(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            ProcessLoading.Instance.Update(80, 100, "Loading Course Model");

            var hash = MD5.Calculate(filePath);
            if (hash == ModelHash)
                return;

            ModelHash = hash;

            BfresEditor = (BFRES)STFileLoader.OpenFileFormat(filePath);

            var actor = new TurboLibrary.Actors.MapActor();
            actor.UpdateModel(BfresEditor.Renderer);
            Workspace.ActiveWorkspace.StudioSystem.AddActor(actor);
        }

        public void LoadColllsion(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            //Dispose and remove any currently used renderers
            if (CollisionFile.CollisionRender != null) {
                CollisionFile.CollisionRender?.Dispose();
                Plugin.RemoveRender(CollisionFile.CollisionRender);
                CollisionFile.CollisionRender = null;
            }

            //Load the collision file.
            ProcessLoading.Instance.Update(50, 100, "Loading Course Collision");

            CollisionFile.FileInfo.FilePath = filePath;
            CollisionFile.FileInfo.FileName = System.IO.Path.GetFileName(filePath);

            if (filePath.EndsWith(".szs")) //Deluxe compresses collision files
            {
                CollisionFile.FileInfo.Compression = new Toolbox.Core.Yaz0();
                CollisionFile.Load(new MemoryStream(YAZ0.Decompress(filePath)));
            }
            else
                CollisionFile.Load(File.OpenRead(filePath));
            //Hide collision by default
            CollisionFile.CollisionRender.IsVisible = false;
        }

        private void LoadLightingResources(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            ProcessLoading.Instance.Update(90, 100, "Loading Course Lighting");

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                BgenvFile = new BgenvPlugin() { FileInfo = new Toolbox.Core.File_Info() { FilePath = filePath } };
                BgenvFile.FileInfo.FileName = System.IO.Path.GetFileName(filePath);
                BgenvFile.Load(stream);
            }
        }

        public void LoadCourseEffects(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            var elink = GlobalSettings.GetContentPath("common\\effect\\elink.bin");
            EffectLibrary.EffectManager.Instance.LoadElink(elink);

            ProcessLoading.Instance.Update(95, 100, "Loading Course Effects");

            var ptcl = new EffectLibrary.PTCL(filePath);
            EffectLibrary.EffectManager.AddPtclResource(ptcl.FileHeader);

            foreach (var actor in StudioSystem.Instance.Actors)
                if (actor.GetType() == typeof(EffectLibrary.EffectRenderer))
                    return;

            StudioSystem.Instance.AddActor(new EffectLibrary.EffectRenderer());
        }

        private void LoadProbeLighting(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            if (filePath.EndsWith(".szs")) //Deluxe compresses collision files
                AGraphicsLibrary.ProbeMapManager.Prepare(YAZ0.Decompress(File.ReadAllBytes(filePath)));
            else
                AGraphicsLibrary.ProbeMapManager.Prepare(File.ReadAllBytes(filePath));
        }

        public void SaveLightingResources()
        {
            if (BgenvFile == null)
                return;

            ImguiFileDialog dlg = new ImguiFileDialog();
            dlg.SaveDialog = true;
            dlg.FileName = "course.bgenv";
            dlg.AddFilter(".bgenv", "bgenv");
            if (dlg.ShowDialog()) {
                BgenvFile.Save(File.OpenWrite(dlg.FilePath));
            }
        }
    }
}
