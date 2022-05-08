using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using MapStudio.UI;
using Toolbox.Core;
using Toolbox.Core.ViewModels;
using TurboLibrary.MuuntEditor;
using GLFrameworkEngine;
using OpenTK;
using TurboLibrary.LightingEditor;
using TurboLibrary.CollisionEditor;
using CafeLibrary.ModelConversion;
using CafeLibrary;
using ImGuiNET;

namespace TurboLibrary
{
    public class CourseMuuntPlugin : FileEditor, IFileFormat
    {
        public string[] Description => new string[] { "Byaml" };
        public string[] Extension => new string[] { "*.byaml" };

        /// <summary>
        /// Wether or not the file can be saved.
        /// </summary>
        public bool CanSave { get; set; } = true;

        /// <summary>
        /// Information of the loaded file.
        /// </summary>
        public File_Info FileInfo { get; set; }

        public MapLoader MapLoader;

        public IMunntEditor ActiveEditor { get; set; }
        public List<IMunntEditor> Editors = new List<IMunntEditor>();

        public MapLoader Resources;

        public override string SubEditor
        {
            get { return ActiveEditor.Name; }
            set
            {
                foreach (var editor in Editors)
                {
                    if (editor.Name == value)
                        UpdateMuuntEditor(editor);
                }
            }
        }

        public bool IsNewSwitch = false;

        private string model_path = "";

        public override Action RenderNewFileDialog => () =>
        {
            ImGui.Checkbox("Mario Kart 8 Deluxe Map", ref IsNewSwitch);

            ImguiCustomWidgets.FileSelector("Model File", ref model_path, new string[] { ".dae", ".fbx" });

            var size = new System.Numerics.Vector2(ImGui.GetWindowWidth() / 2, 23);
            if (ImGui.Button(TranslationSource.GetText("CANCEL"), size))
                DialogHandler.ClosePopup(false);

            ImGui.SameLine();
            if (ImGui.Button(TranslationSource.GetText("OK"), size))
                DialogHandler.ClosePopup(true);
        };

        public override List<string> SubEditors => Editors.Select(x => x.Name).ToList();

        public bool Identify(File_Info fileInfo, Stream stream) {
            //Just load maps from checking the byaml extension atm.
            return fileInfo.Extension == ".byaml";
        }

        private LightingEditorWindow LightingEditorWindow;
        private CourseUIEditor CourseUITool;

        private bool IsNewProject = false;

        public bool FilterEditorsInOutliner = false;

        FileEditorMode EditorMode = FileEditorMode.MapEditor;

        enum FileEditorMode
        {
            MapEditor,
            ModelEditor,
            CollisionEditor,
            MinimapEditor,
            LightingEditor,
        }

        //File roots

        public override bool CreateNew()
        {
            bool isSwitch = IsNewSwitch;

            IsNewProject = true;

            MapLoader = new MapLoader(this);
            MapLoader.Init(this, isSwitch, model_path);

            Root.Header = "course_muunt.byaml";
            Root.Tag = this;

            FileInfo.FileName = "course_muunt.byaml";

            Setup(MapLoader);

            return true;
        }

        public void LoadProject()
        {
            var projectFile = Workspace.Resources.ProjectFile;

            foreach (var editor in this.Editors)
            {
                if (editor.Name == projectFile.ActiveEditor)
                    SubEditor = editor.Name;
            }
            if (!string.IsNullOrEmpty(projectFile.SelectedWorkspace))
            {
                if (Enum.TryParse(typeof(FileEditorMode), projectFile.SelectedWorkspace, out object? mode))
                {
                    this.EditorMode = (FileEditorMode)mode;
                    ReloadEditorMode();
                }
            }
        }

        public void SaveProject(string folder)
        {
            Workspace.Resources.ProjectFile.ActiveEditor = this.ActiveEditor.Name;
            Workspace.Resources.ProjectFile.SelectedWorkspace = this.EditorMode.ToString();

            SaveProjectFile(folder, MapLoader.BfresEditor);
            SaveProjectFile(folder, MapLoader.CollisionFile);
            /*    SaveProjectFile(folder, MapLoader.BgenvFile);
                if (MapLoader.MapCamera != null)
                    MapLoader.MapCamera.Save($"{folder}\\course_mapcamera.bin");
                if (MapLoader.MapTexture != null && MapLoader.MapTexture is BflimTexture)
                    ((BflimTexture)MapLoader.MapTexture).Save(File.OpenWrite($"{folder}\\course_maptexture.bflim"));*/
        }

        private void SaveProjectFile(string folder, IFileFormat fileFormat)
        {
            if (fileFormat != null)
                fileFormat.Save(File.OpenWrite($"{folder}\\{fileFormat.FileInfo.FileName}"));
        }

        public override void PrintErrors()
        {
            //Update with editor data first
            foreach (var editor in this.Editors)
                editor.OnSave(Resources.CourseDefinition);

            ErrorLogger.CheckCourseErrors(MapLoader.CourseDefinition);
        }

        public void Load(Stream stream)
        {
            string workingDir = MapStudio.UI.Workspace.ActiveWorkspace.Resources.ProjectFile.WorkingDirectory;

            MapLoader = new MapLoader(this);
            MapLoader.Load(this, FileInfo.FolderPath, FileInfo.FilePath, workingDir);

            Setup(MapLoader);
            MapLoader.SetupLighting();
        }

        public void Save(Stream stream)
        {
            foreach (var editor in this.Editors)
                editor.OnSave(Resources.CourseDefinition);

            MapLoader.CourseDefinition.Save(stream);
        }

        private void DrawEditorList()
        {
            ImGui.PushItemWidth(230);
            var flags = ImGuiComboFlags.HeightLargest;
            var pos = ImGui.GetCursorPos();
            if (ImGui.BeginCombo("##editorMenu", $"", flags))
            {
                foreach (var editor in this.Editors)
                {
                    ImGuiHelper.IncrementCursorPosX(3);

                    bool select = editor == ActiveEditor;

                    ImGui.PushStyleColor(ImGuiCol.Text, editor.Root.IconColor);
                    ImGui.Text($"   {editor.Root.Icon}   ");
                    ImGui.PopStyleColor();

                    ImGui.SameLine();

                    if (ImGui.Selectable($"{editor.Name}", select, ImGuiSelectableFlags.SpanAllColumns))
                    {
                        SubEditor = editor.Name;
                        GLContext.ActiveContext.UpdateViewport = true;
                    }
                    if (select)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            if (ImGui.IsItemHovered()) //Check for combo box hover
            {
                var delta = ImGui.GetIO().MouseWheel;
                if (delta < 0) //Check for mouse scroll change going up
                {
                    int index = this.Editors.IndexOf(ActiveEditor);
                    if (index < this.Editors.Count - 1)
                    { //Shift upwards if possible
                        SubEditor = this.Editors[index + 1].Name;
                        GLContext.ActiveContext.UpdateViewport = true;
                    }
                }
                if (delta > 0) //Check for mouse scroll change going down
                {
                    int index = this.Editors.IndexOf(ActiveEditor);
                    if (index > 0)
                    { //Shift downwards if possible
                        SubEditor = this.Editors[index - 1].Name;
                        GLContext.ActiveContext.UpdateViewport = true;
                    }
                }
            }

            ImGui.SetCursorPos(pos);

            ImGui.AlignTextToFramePadding();
            ImGui.Text(" Tool:");
            ImGui.SameLine();

            ImGui.PushStyleColor(ImGuiCol.Text, ActiveEditor.Root.IconColor);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"    {ActiveEditor.Root.Icon}   ");
            ImGui.PopStyleColor();

            ImGui.SameLine();

            ImGui.AlignTextToFramePadding();
            ImGui.Text(ActiveEditor.Name);

            ImGui.PopItemWidth();
        }

        public void Setup(MapLoader mapResources)
        {
            var course = mapResources.CourseDefinition;

            Root.ContextMenus.Clear();
            Root.ContextMenus.Add(new MenuItemModel("Is MK8 Deluxe", () =>
            {
                course.IsSwitch = !course.IsSwitch;
                Root.ContextMenus[0].IsChecked = course.IsSwitch;

            }, "", course.IsSwitch));

            if (mapResources.BfresEditor == null)
            {
                MapLoader.BfresEditor = new BFRES();
                MapLoader.BfresEditor.CreateNew();
                MapLoader.BfresEditor.FileInfo.FileName = "course_model.szs";
                MapLoader.BfresEditor.Root.Header = "course_model.szs";
            }
            mapResources.MapCamera.IsBigEndian = !course.IsSwitch;

            Workspace.OnProjectLoad += delegate
            {
                LoadProject();
            };
            Workspace.OnProjectSave += delegate
            {
                SaveProject(Workspace.Resources.ProjectFolder);
            };
            Workspace.Outliner.SelectionChanged += (o, e) =>
            {
                var node = o as NodeBase;
                if (node == null || node.Parent == null || !node.IsSelected)
                    return;

                foreach (var editor in this.Editors)
                {
                    if (editor.Root == node.Parent || editor.Root == node.Parent.Parent)
                        UpdateMuuntEditor(editor, false);
                }
            };
            Workspace.ViewportWindow.DrawEditorDropdown += delegate
            {
                DrawEditorList();
            };

            _camera = GLContext.ActiveContext.Camera;

            Root.TagUI.Tag = mapResources.CourseDefinition;
            GLContext.ActiveContext.Scene = Scene;

            TurboSystem.Instance = new TurboSystem();
            TurboSystem.Instance.MapFieldAccessor.Setup(MapLoader.CourseDefinition);

            CafeLibrary.Rendering.BfresMeshRender.DisplaySubMeshFunc = (o) =>
            {
                var bounding = o as BoundingNode;
                if (ClipSubMeshCulling.IsInside(bounding.Box))
                    return false;

                return true;
            };

            //A little hack atm. Make the model pickable during XRay
            //So the user cannot select things like lap paths through the map model.
            //But always allow selection to go through during xray mode
            MapLoader.BfresEditor.Renderer.EnablePicking = () =>
            {
                if (ActiveEditor is CubePathEditor<LapPath, LapPathPoint>)
                    return !((CubePathEditor<LapPath, LapPathPoint>)ActiveEditor).IsXRAY;

                return true;
            };

            Resources = mapResources;

            //Make sure the collision render isn't null
            if (MapLoader.CollisionFile.CollisionRender == null)
                MapLoader.CollisionFile.CreateNew();

            //Load a custom map object category for the asset handler.
            Workspace.AddAssetCategory(new AssetViewMapObject());
            Workspace.AddAssetCategory(new AssetViewMapObjectVR());

            //Load custom window editors
            LightingEditorWindow = new LightingEditorWindow(Workspace, this);
            LightingEditorWindow.DockDirection = ImGuiNET.ImGuiDir.Right;
            LightingEditorWindow.SplitRatio = Workspace.PropertyWindow.SplitRatio;
            Workspace.Windows.Add(new IntroCameraKeyEditor());

            //Get the folder name to determine what course might be loaded
            string folderName = "";
            if (!string.IsNullOrEmpty(this.FileInfo.FolderPath))
                folderName = new System.IO.DirectoryInfo(this.FileInfo.FolderPath).Name;

            CourseUITool = new CourseUIEditor(folderName);
            CourseUITool.Opened = false;
            Workspace.Windows.Add(CourseUITool);

            //Load the editors
            var colorSettings = GlobalSettings.PathDrawer;

            //Section List

            //Course Info

            //Area
            //Clip, ClipArea, ClipPattern
            //Effect Area
            //Enemy Area
            //GCamera Path
            //Glide Path
            //Gravity Path
            //Intro Camera
            //Item Path
            //Jugem Path
            //Lap Path
            //Obj
            //Path
            //Pull Path
            //Replay Camera
            //Sound Obj

            //Deluxe Extras

            //Steer Assist Path
            //Ceiling Area
            //Prison Area
            //Current Area

            //Jugem Path
            //Clip, ClipArea, ClipPattern
            //Replay Camera
            //Intro Camera

            Editors.Add(new ObjectEditor(this, course.Objs));
            Editors.Add(new SoundObjEditor(this, course.SoundObjs));
            Editors.Add(new AreaEditor(this, course));
            Editors.Add(new ClipAreaEditor(this, course.ClipAreas));

            Editors.Add(new EffectAreaEditor(this, course.EffectAreas));
            Editors.Add(new IntroCameraEditor(this, course.IntroCameras));
            Editors.Add(new ReplayCameraEditor(this, course.ReplayCameras));

            Editors.Add(new PathEditor<EnemyPath, EnemyPathPoint>(this, TranslationSource.GetText("ENEMY_PATHS"), colorSettings.EnemyColor, course.EnemyPaths));
            Editors.Add(new PathEditor<ItemPath, ItemPathPoint>(this, TranslationSource.GetText("ITEM_PATHS"), colorSettings.ItemColor, course.ItemPaths));
            Editors.Add(new PathEditor<GlidePath, GlidePathPoint>(this, TranslationSource.GetText("GLIDER_PATHS"), colorSettings.GlideColor, course.GlidePaths));
            Editors.Add(new PathEditor<PullPath, PullPathPoint>(this, TranslationSource.GetText("PULL_PATHS"), colorSettings.PullColor, course.PullPaths));

            Editors.Add(new CubePathEditor<LapPath, LapPathPoint>(this, TranslationSource.GetText("LAP_PATHS"), colorSettings.LapColor, course.LapPaths));
            Editors.Add(new CubePathEditor<GravityPath, GravityPathPoint>(this, TranslationSource.GetText("GRAVITY_PATHS"), colorSettings.GravityColor, course.GravityPaths));
            Editors.Add(new CubePathEditor<GCameraPath, GCameraPathPoint>(this, TranslationSource.GetText("GRAVITY_CAMERA_PATHS"), colorSettings.GravityCameraColor, course.GCameraPaths));

            Editors.Add(new PathEditor<SteerAssistPath, SteerAssistPathPoint>(this, TranslationSource.GetText("STEER_ASSIST_PATHS"), colorSettings.SteerAssistColor, course.SteerAssistPaths));
         //   Editors.Add(new PathEditor<KillerPath, KillerPathPoint>(this, TranslationSource.GetText("KILLER_PATHS"), colorSettings.SteerAssistColor, course.KillerPaths));

            Editors.Add(new RailPathEditor<Path, PathPoint>(this, TranslationSource.GetText("RAIL_PATHS"), colorSettings.RailColor, course.Paths));
            Editors.Add(new RailPathEditor<ObjPath, ObjPathPoint>(this, TranslationSource.GetText("OBJECT_PATHS"), colorSettings.RailColor, course.ObjPaths));
            Editors.Add(new RailPathEditor<JugemPath, JugemPathPoint>(this, TranslationSource.GetText("LAKITU_PATHS"), colorSettings.RailColor, course.JugemPaths));
            //  Editors.Add(new RouteChangeEditor(this, course.RouteChanges));

            foreach (var editor in Editors)
                editor.MapEditor = this;

            if (Workspace.Resources.ProjectFile.ActiveWorkspaces.Count > 0)
            {
                foreach (var ed in Editors)
                    ed.IsActive = Workspace.Resources.ProjectFile.ActiveWorkspaces.Contains(ed.Name);
            }
            else
            {
                foreach (var ed in Editors)
                    ed.IsActive = false;

                Editors[0].IsActive = true;
            }

            NodeBase areaFolder = new NodeBase("Areas");
            NodeBase pathFolder = new NodeBase("Paths");
            NodeBase camFolder = new NodeBase("Cameras");
            NodeBase objFolder = new NodeBase("Objects");

            Root.Children.Clear();

            foreach (var editor in Editors)
            {
                if (editor is AreaEditor || editor is EffectAreaEditor || editor is ClipAreaEditor)
                    areaFolder.AddChild(editor.Root);
                else if (editor is IntroCameraEditor || editor is ReplayCameraEditor)
                    camFolder.AddChild(editor.Root);
                else if (editor is ObjectEditor || editor is SoundObjEditor || editor is RouteChangeEditor)
                    objFolder.AddChild(editor.Root);
                else
                    pathFolder.AddChild(editor.Root);
            }

            Root.AddChild(objFolder);
            Root.AddChild(camFolder);
            Root.AddChild(areaFolder);
            Root.AddChild(pathFolder);

            var clipFolder = ClipEditor.Setup(course, (ClipAreaEditor)Editors[3]);
            Root.AddChild(clipFolder);

            foreach (var ed in Editors)
            {
                ed.Root.OnSelected += delegate
                {
                    if (ActiveEditor == ed)
                        return;

                    ActiveEditor = ed;
                    UpdateMuuntEditor(ActiveEditor, false);
                };
                ed.Root.OnChecked += delegate
                {
                    foreach (var render in ed.Renderers)
                        render.IsVisible = ed.Root.IsChecked;
                };
            }

            //Set the active editor as the map object one.
            ActiveEditor = Editors.FirstOrDefault();
            UpdateMuuntEditor(ActiveEditor);

            if (MapLoader.BfresEditor != null)
            {
                MapLoader.BfresEditor.Root.Header = MapLoader.BfresEditor.FileInfo.FileName;
                MapLoader.BfresEditor.Scene.Init();
                MapLoader.BfresEditor.Renderer.MeshPicking = false;

                foreach (var ob in MapLoader.BfresEditor.Scene.Objects)
                    Scene.AddRenderObject(ob);

                foreach (var ob in MapLoader.BfresEditor.Scene.Objects)
                    MapLoader.MapCamera.Editor.AddRender(ob);
            }

            Workspace.WorkspaceTools.Add(new MenuItemModel(
               $"   {'\uf279'}    Map Editor", () =>
               {
                   EditorMode = FileEditorMode.MapEditor;
                   ReloadEditorMode();
               }));
            Workspace.WorkspaceTools.Add(new MenuItemModel(
               $"   {'\uf6d1'}    Model Editor", () =>
               {
                   EditorMode = FileEditorMode.ModelEditor;
                   ReloadEditorMode();
               }));
            Workspace.WorkspaceTools.Add(new MenuItemModel(
                $"   {'\uf5e1'}    Collision Editor", () =>
                {
                    EditorMode = FileEditorMode.CollisionEditor;
                    ReloadEditorMode();
                }));
            Workspace.WorkspaceTools.Add(new MenuItemModel(
                $"   {IconManager.CAMERA_ICON}    Minimap Editor", () =>
                {
                    EditorMode = FileEditorMode.MinimapEditor;
                    ReloadEditorMode();
                }));
            Workspace.WorkspaceTools.Add(new MenuItemModel(
                $"   {'\uf5fd'}    UI Editor", () =>
                {
                    CourseUITool.Opened = !CourseUITool.Opened;
                }));

            
            //TODO this will be added in the future when the data can be made from scatch
            /*  Workspace.MainWindowMenuItems.Add(new MenuItemModel(
                  "Lighting Edit", () =>
                  {
                      EditorMode = FileEditorMode.LightingEditor;
                      ReloadEditorMode();
                  }));*/
        }

        public override void DrawViewportMenuBar() {
            ActiveEditor.DrawEditMenuBar();
        }

        public override void DrawHelpWindow()
        {
            base.DrawHelpWindow();

            if (ImGuiNET.ImGui.CollapsingHeader("Editors", ImGuiNET.ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGuiHelper.BoldTextLabel("Alt + Click", "Add.");
                ImGuiHelper.BoldTextLabel("Del", "Delete.");
                ImGuiHelper.BoldTextLabel("Hold Ctrl", "Multi select.");
            }

            ActiveEditor?.DrawHelpWindow();
        }

        private Camera _camera;

        public override void AfterLoaded()
        {
            if (this.IsNewProject)
            {
                this.EditorMode = FileEditorMode.ModelEditor;
                ReloadEditorMode();
                Workspace.ActiveWorkspaceTool = Workspace.WorkspaceTools[1];
            }
        }

        private void ReloadEditorMode()
        {
            GLContext.ActiveContext.Camera = _camera;

            Workspace.Outliner.DeselectAll();
            Workspace.Outliner.Nodes.Clear();

            //Defaults
            CollisionRender.DisplayCollision = false;
            MapLoader.BfresEditor.Renderer.MeshPicking = false;
            MapLoader.CollisionFile.CollisionRender.IsVisible = false;
            MapLoader.CollisionFile.CollisionRender.CanSelect = false;

            if (this.EditorMode == FileEditorMode.MapEditor) {
                Workspace.ActiveEditor = this;
                Workspace.SetupActiveEditor(this);
                Workspace.ReloadEditors();
            }
            else if (this.EditorMode == FileEditorMode.CollisionEditor)
            {
                MapLoader.CollisionFile.CollisionRender.IsVisible = true;
                MapLoader.CollisionFile.CollisionRender.CanSelect = true;

                Workspace.ActiveEditor = MapLoader.CollisionFile;
                Workspace.SetupActiveEditor(MapLoader.CollisionFile);
                Workspace.ReloadEditors();
            }
            else if (this.EditorMode == FileEditorMode.LightingEditor)
            {
                Workspace.ActiveEditor = MapLoader.BgenvFile;
                Workspace.SetupActiveEditor(MapLoader.BgenvFile);
                Workspace.ReloadEditors();
            }
            else if (this.EditorMode == FileEditorMode.MinimapEditor)
            {
                MapLoader.MapCamera.Editor.Load(MapLoader.MapCamera, MapLoader.MapTexture);
                MapLoader.MapCamera.Editor.ReloadEditor(GLContext.ActiveContext);

                Workspace.ActiveEditor = MapLoader.MapCamera.Editor;
                Workspace.SetupActiveEditor(MapLoader.MapCamera.Editor);
                Workspace.ReloadEditors();
            }
            else {
                MapLoader.BfresEditor.Renderer.MeshPicking = true;
                Workspace.ActiveEditor = MapLoader.BfresEditor;
                Workspace.SetupActiveEditor(MapLoader.BfresEditor);
                Workspace.ReloadEditors();
            }
            GLContext.ActiveContext.UpdateViewport = true;
        }

        public override List<MenuItemModel> GetViewportMenuIcons()
        {
            List<MenuItemModel> menus = new List<MenuItemModel>();

            if (LightingEditorWindow != null)
            {
                menus.Add(new MenuItemModel($" {IconManager.LIGHT_ICON} ", () =>
                {
                    LightingEditorWindow.Opened = !LightingEditorWindow.Opened;
                    Workspace.ActiveWorkspace.ReloadViewportMenu();

                }, "LIGHTING_WINDOW", LightingEditorWindow.Opened));
            }
            
            return menus;
        }

        public override List<MenuItemModel> GetViewMenuItems()
        {
            List<MenuItemModel> menus = new List<MenuItemModel>();

            if (MapLoader.BfresEditor != null)
                menus.AddRange(MapLoader.BfresEditor.GetViewMenuItems());

            menus.Add(new MenuItemModel($"      {IconManager.MESH_ICON}      Show Collision", () =>
            {
                CollisionRender.Overlay = false;
                CollisionRender.DisplayCollision = !CollisionRender.DisplayCollision;
                if (MapLoader.BfresEditor != null)
                    MapLoader.BfresEditor.Renderer.IsVisible = !CollisionRender.DisplayCollision;

                Workspace.ActiveWorkspace.ReloadViewportMenu();
                GLContext.ActiveContext.UpdateViewport = true;
            }, "DISPLAY_COLLISION", CollisionRender.DisplayCollision));

            menus.Add(new MenuItemModel($"      {IconManager.MODEL_ICON}      Show Collision Overlay", () =>
            {
                CollisionRender.Overlay = !CollisionRender.Overlay;
                CollisionRender.DisplayCollision = false;
                if (MapLoader.BfresEditor != null)
                    MapLoader.BfresEditor.Renderer.IsVisible = true;

                Workspace.ActiveWorkspace.ReloadViewportMenu();
                GLContext.ActiveContext.UpdateViewport = true;
            }, "COLLISION_OVERLAY", CollisionRender.Overlay));
            return menus;
        }

        public override List<MenuItemModel> GetFilterMenuItems()
        {
            List<MenuItemModel> items = new List<MenuItemModel>();
            items.Add(new MenuItemModel("Filter Nodes By Editor", (sender, e)=>
            {
                FilterEditorsInOutliner = ((MenuItemModel)sender).IsChecked;
                ReloadOutliner(false);
            }, "", FilterEditorsInOutliner)
            { CanCheck = true });
            return items;
        }

        public void AutoGenerateCollision()
        {
            var models = MapLoader.BfresEditor.ModelFolder.Models.FirstOrDefault();
            if (models == null) {
                TinyFileDialog.MessageBoxInfoOk("No models found in scene!");
                return;
            }

            //Turn into an exportable scene for collision conversion
            var scene = BfresModelExporter.FromGeneric(models.ResFile, models.Model);
            MapLoader.CollisionFile.ImportCollision(scene);
        }

        public override List<MenuItemModel> GetEditMenuItems()
        {
            var items = new List<MenuItemModel>();
            //Import menus for collision
            items.AddRange(MapLoader.CollisionFile.GetEditMenuItems());
            items.Add(new MenuItemModel("        Auto Generate Collision From Bfres", AutoGenerateCollision));
            return items;
        }

        Vector3 previousPosition = Vector3.Zero;

        public override void DrawToolWindow()
        {
            if (ImGuiNET.ImGui.CollapsingHeader("Transform Scene"))
            {
                var transform = MapLoader.BfresEditor.Renderer.Transform;
                if (ImGuiHelper.InputTKVector3("Position", transform, "Position"))
                {
                    transform.UpdateMatrix(true);

                    if (previousPosition == Vector3.Zero)
                        previousPosition = transform.Position;

                    Vector3 positionDelta = transform.Position - previousPosition;

                    previousPosition = transform.Position;

                    MapLoader.CollisionFile.CollisionRender.Transform.TransformMatrix = transform.TransformMatrix;
                    MapLoader.CollisionFile.UpdateTransformedVertices = true;

                    //Transform each byaml object
                    foreach (var editor in this.Editors)
                    {
                        foreach (ITransformableObject render in editor.Renderers)
                        {
                            if (render is RenderablePath)
                            {
                                ((RenderablePath)render).TranslateDelta(positionDelta);
                            }
                            else
                            {
                                render.Transform.Position += positionDelta;
                                render.Transform.UpdateMatrix(true);
                            }
                        }
                    }
                }
            }

            if (ActiveEditor != null)
                ActiveEditor.ToolWindowDrawer?.Render();
        }

        private void UpdateMuuntEditor(IMunntEditor editor, bool filterVisuals = true)
        {
            //Enable/Disable "Active" editors which determine to use shorts ie alt mouse click to spawn
            foreach (var ed in Editors)
            {
                ed.IsActive = ed == editor;
                foreach (var render in ed.Renderers)
                {
                    if (render is RenderablePath)
                        ((RenderablePath)render).IsActive = ed.IsActive;
                }
            }
            ActiveEditor = editor;

            Workspace.ActiveWorkspace.ReloadEditors();
            ReloadOutliner(filterVisuals);
        }

        public void ReloadOutliner(bool filterVisuals)
        {
            //   if (FilterEditorsInOutliner)
            //   Root.Children.Clear();

            if (filterVisuals)
            {
             //   Workspace.Outliner.DeselectAll();
                Scene.DeselectAll(GLContext.ActiveContext);

                foreach (var editor in Editors)
                    HideRenders(editor);
            }
            foreach (var editor in Editors)
            {
                if (editor.IsActive)
                {
                    editor.ReloadEditor();
                  //  if (FilterEditorsInOutliner)
                    //    Root.AddChild(editor.Root);
                }

                if (filterVisuals)
                    editor.Root.IsChecked = editor.IsActive;
                if (filterVisuals && editor.IsActive)
                {
                    foreach (var render in editor.Renderers)
                        render.IsVisible = true;
                }
            }

            GLContext.ActiveContext.UpdateViewport = true;
        }

        public override List<UIFramework.DockWindow> PrepareDocks()
        {
            List<UIFramework.DockWindow> windows = new List<UIFramework.DockWindow>();
            windows.Add(Workspace.Outliner);
            if (LightingEditorWindow != null)
                windows.Add(LightingEditorWindow);
            windows.Add(Workspace.PropertyWindow);
            windows.Add(Workspace.ConsoleWindow);
            windows.Add(Workspace.AssetViewWindow);
            windows.Add(Workspace.HelpWindow);
            windows.Add(Workspace.ToolWindow);
            windows.Add(Workspace.ViewportWindow);
            return windows;
        }

        public override bool OnFileDrop(string filePath)
        {
            if (filePath.EndsWith(".kcl"))
            {
                MapLoader.LoadColllsion(filePath);
                return true;
            }
            if (MapLoader.CollisionFile.OnFileDrop(filePath))
            {
                return true;
            }
            return false;
        }

        public override void AssetViewportDrop(AssetItem item, Vector2 screenCoords)
        {
            var asset = item as MapObjectAsset;
            if (asset == null)
                return;

            ((ObjectEditor)Editors[0]).OnAssetViewportDrop(asset.ObjID, screenCoords);
        }

        private void HideRenders(IMunntEditor editor)
        {
            foreach (var render in editor.Renderers)
            {
                render.IsVisible = false;
                if (render is IEditModeObject)
                {
                    Scene.DisableEditMode((ITransformableObject)render);
                    foreach (var part in ((IEditModeObject)render).Selectables)
                        part.CanSelect = false;
                }
                else
                    ((ITransformableObject)render).CanSelect = false;
            }
        }

        public override void OnMouseMove(MouseEventInfo mouseInfo)
        {
            foreach (var editor in this.Editors.Where(x => x.IsActive))
                editor.OnMouseMove(mouseInfo);
        }

        public override void OnMouseDown(MouseEventInfo mouseInfo)
        {
            foreach (var editor in this.Editors.Where(x => x.IsActive))
                editor.OnMouseDown(mouseInfo);
        }

        public override void OnMouseUp(MouseEventInfo mouseInfo)
        {
            foreach (var editor in this.Editors.Where(x => x.IsActive))
                editor.OnMouseUp(mouseInfo);
        }

        public override void OnKeyDown(KeyEventInfo keyInfo)
        {
            foreach (var editor in this.Editors.Where(x => x.IsActive))
                editor.OnKeyDown(keyInfo);
        }
    }
}
