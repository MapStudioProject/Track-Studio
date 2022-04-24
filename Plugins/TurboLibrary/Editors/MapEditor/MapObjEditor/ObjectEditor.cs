using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Toolbox.Core.ViewModels;
using ImGuiNET;
using GLFrameworkEngine;
using OpenTK;
using MapStudio.UI;
using CafeLibrary.Rendering;
using TurboLibrary.Actors;
using Toolbox.Core;
using GLFrameworkEngine.UI;

namespace TurboLibrary.MuuntEditor
{
    /// <summary>
    /// Represents an editor to load, edit and save map objects.
    /// </summary>
    public class ObjectEditor : IMunntEditor, UIEditToolMenu
    {
        public string Name => TranslationSource.GetText("OBJECT_EDITOR");

        public CourseMuuntPlugin MapEditor { get; set; }

        public bool IsActive { get; set; } = false;

        public IToolWindowDrawer ToolWindowDrawer => new MapObjectToolMenu(this);

        public List<IDrawable> Renderers { get; set; } = new List<IDrawable>();

        public NodeBase Root { get; set; } = new NodeBase(TranslationSource.GetText("MAP_OBJECTS")) { HasCheckBox = true };
        public List<MenuItemModel> MenuItems { get; set; } = new List<MenuItemModel>();

        int SpawnObjectID = 1018;

        public List<NodeBase> GetSelected()
        {
            return Root.Children.Where(x => x.IsSelected).ToList();
        }

        static bool initIcons = false;
        //Loads the icons for map objects (once on init)
        static void InitIcons()
        {
            if (initIcons)
                return;

            initIcons = true;

            //Load icons for map objects
            if (Directory.Exists($"{Runtime.ExecutableDir}\\Lib\\Images\\MapObjects"))
            {
                foreach (var imageFile in Directory.GetFiles($"{Runtime.ExecutableDir}\\Lib\\Images\\MapObjects"))
                {
                    IconManager.LoadTextureFile(imageFile, 32, 32);
                }
            }
        }

        public ObjectEditor(CourseMuuntPlugin editor, List<Obj> objs)
        {
            MapEditor = editor;
            InitIcons();

            Root.Icon = MapEditorIcons.OBJECT_ICON.ToString();

            Init(objs);

            GlobalSettings.LoadDataBase();

            var addMenu = new MenuItemModel("ADD_OBJECT", AddObjectMenuAction);
            var commonItemsMenu = new MenuItemModel("OBJECTS");
            commonItemsMenu.MenuItems.Add(new MenuItemModel("COIN", () => AddObject(1018, true)));

            GLContext.ActiveContext.Scene.MenuItemsAdd.Add(addMenu);
            GLContext.ActiveContext.Scene.MenuItemsAdd.Add(commonItemsMenu);

            MenuItems.AddRange(GetEditMenuItems());
        }

        public List<MenuItemModel> GetToolMenuItems()
        {
            List<MenuItemModel> items = new List<MenuItemModel>();
            return items;
        }

        MapObjectSelector ObjectSelector;

        public void DrawEditMenuBar()
        {
            DrawObjectSpawner();

            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.ADD_ICON}   ", "ADD", InputSettings.INPUT.Scene.Create)) {
                AddObjectMenuAction();
            }
            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.DELETE_ICON}   ", "REMOVE", InputSettings.INPUT.Scene.Delete)) {
                MapEditor.Scene.BeginUndoCollection();
                RemoveSelected();
                MapEditor.Scene.EndUndoCollection();
            }
            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.COPY_ICON}   ", "COPY", InputSettings.INPUT.Scene.Copy)) {
                CopySelected();
            }
            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.PASTE_ICON}   ", "PASTE", InputSettings.INPUT.Scene.Paste)) {
                PasteSelected();
            }
        }

        public void DrawHelpWindow() {
            if (ImGuiNET.ImGui.CollapsingHeader("Objects", ImGuiNET.ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGuiHelper.BoldTextLabel(InputSettings.INPUT.Scene.Create, "Create Object.");
            }
        }

        private void DrawObjectSpawner()
        {
            //Selector popup window instance
            if (ObjectSelector == null) {
                var objects = GlobalSettings.ObjDatabase.Values.OrderBy(x => x.Label).ToList();
                ObjectSelector = new MapObjectSelector(objects);
                ObjectSelector.CloseOnSelect = true;
                //Update current spawn id when selection is changed
                ObjectSelector.SelectionChanged += delegate {
                    SpawnObjectID = ObjectSelector.GetSelectedID();
                };
            }
            //Current spawnable
            string resName = Obj.GetResourceName(SpawnObjectID);
            var pos = ImGui.GetCursorScreenPos();

            //Make the window cover part of the viewport
            var viewportHeight = GLContext.ActiveContext.Height;
            var spawnPopupHeight = viewportHeight;

            ImGui.SetNextWindowPos(new System.Numerics.Vector2(pos.X, pos.Y + 27));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, spawnPopupHeight));

            //Render popup window when opened
            var flags = ImGuiWindowFlags.NoScrollbar;
            if (ImGui.BeginPopup("spawnPopup", ImGuiWindowFlags.Popup | flags)) {

                if (ImGui.BeginChild("spawnableChild", new System.Numerics.Vector2(300, spawnPopupHeight), false, flags)) {
                    ObjectSelector.Render(false);
                }
                ImGui.EndChild();
                ImGui.EndPopup();
            }

            //Menu to open popup
            ImGui.PushItemWidth(150);
            if (ImGui.BeginCombo("##spawnableCB", resName))
            {
                ImGui.EndCombo();
            }
            if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(0))
            {
                if (ImGui.IsPopupOpen("spawnPopup"))
                    ImGui.CloseCurrentPopup();
                else
                {
                    ImGui.OpenPopup("spawnPopup");
                    ObjectSelector.SetSelectedID(SpawnObjectID);
                }
            }
            ImGui.PopItemWidth();
        }

        public List<MenuItemModel> GetEditMenuItems()
        {
            List<MenuItemModel> items = new List<MenuItemModel>();
            items.Add(new MenuItemModel($"   {IconManager.ADD_ICON}   ", AddObjectMenuAction));

            bool hasSelection = GetSelected().Count > 0;

            items.Add(new MenuItemModel($"   {IconManager.COPY_ICON}   ", CopySelected) { IsEnabled = hasSelection, ToolTip = $"Copy ({InputSettings.INPUT.Scene.Copy})" });
            items.Add(new MenuItemModel($"   {IconManager.PASTE_ICON}   ", PasteSelected) { IsEnabled = hasSelection, ToolTip = $"Paste ({InputSettings.INPUT.Scene.Paste})" });
            items.Add(new MenuItemModel($"   {IconManager.DELETE_ICON}   ", () =>
            {
                GLContext.ActiveContext.Scene.DeleteSelected();
            })
            { IsEnabled = hasSelection, ToolTip = $" Delete ({InputSettings.INPUT.Scene.Delete})" });

            return items;
        }

        public void ReloadEditor()
        {
            Root.Header = TranslationSource.GetText("MAP_OBJECTS");

            foreach (EditableObject render in Renderers)
            {
                UpdateObjectLinks(render);

                render.CanSelect = true;

                if (((Obj)render.UINode.Tag).IsSkybox)
                    render.CanSelect = false;
            }
        }

        void Init(List<Obj> objs)
        {
            Root.Children.Clear();
            Renderers.Clear();

            //Load the current tree list
            for (int i = 0; i < objs?.Count; i++)
                Add(Create(objs[i]));

            if (Root.Children.Any(x => x.IsSelected))
                Root.IsExpanded = true;
        }

        public void OnSave(CourseDefinition course)
        {
            course.Objs = new List<Obj>();

            foreach (EditableObject render in Renderers)
            {
                var obj = (Obj)render.UINode.Tag;
                obj.Translate = new ByamlVector3F(
                    render.Transform.Position.X,
                    render.Transform.Position.Y,
                    render.Transform.Position.Z);
                obj.Rotate = new ByamlVector3F(
                    render.Transform.RotationEuler.X,
                    render.Transform.RotationEuler.Y,
                    render.Transform.RotationEuler.Z);
                obj.Scale = new ByamlVector3F(
                    render.Transform.Scale.X,
                    render.Transform.Scale.Y,
                    render.Transform.Scale.Z);
                course.Objs.Add(obj);
            }
        }

        public void OnMouseDown(MouseEventInfo mouseInfo)
        {
            bool isActive = Workspace.ActiveWorkspace.ActiveEditor.SubEditor == this.Name;

            if (isActive && KeyEventInfo.State.KeyAlt && mouseInfo.LeftButton == OpenTK.Input.ButtonState.Pressed)
                AddObject(SpawnObjectID);
        }
        public void OnMouseUp(MouseEventInfo mouseInfo) 
        {
        }
        public void OnMouseMove(MouseEventInfo mouseInfo)
        {
        }

        public void Add(EditableObject render, bool undo = false)
        {
            MapEditor.AddRender(render, undo);
        }

        public void Remove(EditableObject render, bool undo = false)
        {
            MapEditor.RemoveRender(render, undo);
        }

        /// <summary>
        /// When an object asset is drag and dropped into the viewport.
        /// </summary>
        public void OnAssetViewportDrop(int id, Vector2 screenPosition)
        {
            var context = GLContext.ActiveContext;

            Quaternion rotation = Quaternion.Identity;
            //Spawn by drag/drop coordinates in 3d space.
            Vector3 position = context.ScreenToWorld(screenPosition.X, screenPosition.Y, 100);
            //Face the camera
            if (MapStudio.UI.GlobalSettings.Current.Asset.FaceCameraAtSpawn)
                rotation = Quaternion.FromEulerAngles(0, -context.Camera.RotationY, 0);
            //Drop to collision if used.
            if (context.EnableDropToCollision)
            {
                Quaternion rotateByDrop = Quaternion.Identity;
                CollisionDetection.SetObjectToCollision(context, context.CollisionCaster, screenPosition, ref position, ref rotateByDrop);
                if (context.TransformTools.TransformSettings.RotateFromNormal)
                    rotation *= rotateByDrop;
            }
            //Add the object with the dropped id and set the transform
            var render = AddObject(id);
            var obj = render.UINode.Tag as Obj;
            //Ignore selecting and setting SRT for skybox
            if (obj.IsSkybox)
                render.IsSelected = false;
            else //Set the dropped place based on where the current mouse is.
            {
                render.Transform.Position = position;
                render.Transform.Rotation = rotation;
                render.Transform.UpdateMatrix(true);
                render.UINode.IsSelected = true;
            }

            this.MapEditor.Scene.SelectionUIChanged?.Invoke(render.UINode, EventArgs.Empty);

            //Update the SRT tool if active
            GLContext.ActiveContext.TransformTools.UpdateOrigin();

            //Force the editor to display
            if (!IsActive)
            {
                IsActive = true;
                ((CourseMuuntPlugin)Workspace.ActiveWorkspace.ActiveEditor).ReloadOutliner(true);
            }
        }

        public void OnKeyDown(KeyEventInfo keyInfo)
        {
            bool isActive = Workspace.ActiveWorkspace.ActiveEditor.SubEditor == this.Name;

            if (isActive && !keyInfo.KeyCtrl && keyInfo.IsKeyDown(InputSettings.INPUT.Scene.Create))
                AddObject(SpawnObjectID);
            if (keyInfo.IsKeyDown(InputSettings.INPUT.Scene.Copy) && GetSelected().Count > 0)
                CopySelected();
            if (keyInfo.IsKeyDown(InputSettings.INPUT.Scene.Paste))
                PasteSelected();
            if (keyInfo.IsKeyDown(InputSettings.INPUT.Scene.Dupe))
            {
                CopySelected();
                PasteSelected();
                copied.Clear();
            }
        }

        public void ExportModel()
        {
            ImguiFileDialog dlg = new ImguiFileDialog();
            dlg.SaveDialog = true;
            dlg.AddFilter(".dae", ".dae");
            if (dlg.ShowDialog())
                ExportModel(dlg.FilePath);
        }

        public void ExportModel(string filePath)
        {
            var scene = new IONET.Core.IOScene();
            foreach (ITransformableObject render in this.Renderers.Where(x => ((ITransformableObject)x).IsSelected))
            {
                var obj = ((IRenderNode)render).UINode.Tag as Obj;
                var modelFilePath = Obj.FindFilePath(Obj.GetResourceName(obj.ObjId));
                if (!File.Exists(modelFilePath))
                    continue;

                var resFile = new BfresLibrary.ResFile(modelFilePath);
                var model = CafeLibrary.ModelConversion.BfresModelExporter.FromGeneric(resFile, resFile.Models[0]);
                foreach (var mesh in model.Models[0].Meshes)
                    mesh.Transform = Matrix4Extension.ToNumerics(render.Transform.TransformMatrix);

                scene.Models.AddRange(model.Models);
                scene.Materials.AddRange(model.Materials);

                string folder = System.IO.Path.GetDirectoryName(filePath);
                foreach (var tex in resFile.Textures.Values)
                {
                    if (tex is BfresLibrary.WiiU.Texture)
                    {
                        var ftex = new FtexTexture(resFile, (BfresLibrary.WiiU.Texture)tex);
                        ftex.Export($"{folder}\\{tex.Name}.png", new TextureExportSettings());
                    }
                }
            }
            IONET.IOManager.ExportScene(scene, filePath);
        }

        List<IDrawable> copied = new List<IDrawable>();

        public void CopySelected()
        {
            var selected = Renderers.Where(x => ((EditableObject)x).IsSelected).ToList();

            copied.Clear();
            copied = selected;
        }

        public void PasteSelected()
        {
            GLContext.ActiveContext.Scene.DeselectAll(GLContext.ActiveContext);

            foreach (EditableObject ob in copied)
            {
                var obj = ob.UINode.Tag as Obj;
                var duplicated = Create(obj.Clone());
                duplicated.Transform.Position = ob.Transform.Position;
                duplicated.Transform.Scale = ob.Transform.Scale;
                duplicated.Transform.Rotation = ob.Transform.Rotation;
                duplicated.Transform.UpdateMatrix(true);
                duplicated.IsSelected = true;

                Add(duplicated, true);
            }
        }

        public void RemoveSelected()
        {
            var selected = Renderers.Where(x => ((EditableObject)x).IsSelected).ToList();
            foreach (EditableObject ob in selected)
                Remove(ob, true);
        }

        private EditableObject Create(Obj obj)
        {
            string name = GetResourceName(obj);
            EditableObject render = new TransformableObject(Root);

            var filePath = Obj.FindFilePath(Obj.GetResourceName(obj.ObjId));

            //Don't load it for now if the model is already cached. It should load up instantly
            //TODO should use a less intrusive progress bar (like top/bottom of the window)
            if (!DataCache.ModelCache.ContainsKey(filePath) && File.Exists(filePath))
            {
                ProcessLoading.Instance.IsLoading = true;
                ProcessLoading.Instance.Update(0, 100, $"Loading model {name}");
            }

            //Open a bfres resource if one exist.
            if (System.IO.File.Exists(filePath))
                render = new BfresRender(filePath, Root);
            else if (name == "WaterBox") //Water boxes will have a custom area display.
                render = new WaterBoxRender(Root);
            if (name == "Start") //Start objects display with a grid.
                render = new StartObjRender(Root);

            //Toggle models to use
            if (render is BfresRender)
            {
                if (GlobalSettings.ObjDatabase.ContainsKey(obj.ObjId))
                {
                    //Obj requires specific model to display
                    string modelName = GlobalSettings.ObjDatabase[obj.ObjId].ModelName;
                    if (!string.IsNullOrEmpty(modelName))
                    {
                        foreach (var model in ((BfresRender)render).Models)
                        {
                            if (model.Name != modelName)
                                model.IsVisible = false;
                        }
                    }
                }
            }

            if (ProcessLoading.Instance.IsLoading)
            {
                ProcessLoading.Instance.Update(100, 100, $"Finished loading model {name}");
                ProcessLoading.Instance.IsLoading = false;
            }

            //Set the UI label and property tag
            render.UINode.Header = GetNodeHeader(obj);
            render.UINode.Tag = obj;
            render.UINode.ContextMenus.Add(new MenuItemModel("EXPORT", () => ExportModel()));
            //Set custom UI properties
            ((EditableObjectNode)render.UINode).UIProperyDrawer += delegate
            {
                if (ImGui.CollapsingHeader(TranslationSource.GetText("EDIT"), ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if (ImGui.Button(TranslationSource.GetText("CHANGE_OBJECT"))) {
                        EditObjectMenuAction();
                    }
                }

                var gui = new MapObjectUI();
                gui.Render(obj, Workspace.ActiveWorkspace.GetSelected().Select(x => x.Tag));
            };

            //Icons for map objects
            if (IconManager.HasIcon($"{Runtime.ExecutableDir}\\Images\\MapObjects\\{name}.png"))
            {
                render.UINode.Icon = $"{Runtime.ExecutableDir}\\Images\\MapObjects\\{name}.png";
                //A sprite drawer for displaying distant objects
                //Todo this is not used currently and may need improvements
                render.SpriteDrawer = new SpriteDrawer();
            }
            else
                render.UINode.Icon = "Node";

            //Disable selection on skyboxes
            render.CanSelect = !obj.IsSkybox;

            //Map object actors for animation purposes
            var ActorInfo = ActorFactory.GetActorEntry(name);
            if (ActorInfo is ActorModelBase)
            {
                //Actors attach renderers for animating
                if (render is BfresRender)
                    ((ActorModelBase)ActorInfo).UpdateModel((BfresRender)render);
                if (render is IFrustumCulling)
                    ((ActorModelBase)ActorInfo).FrustumRenderCheck = (IFrustumCulling)render;
                //Actors specific parameters
                ((ActorModelBase)ActorInfo).Parameters = obj.Params;
                //Actor transform. Necessary for spawning non bfres actor models.
                ((ActorModelBase)ActorInfo).Transform = ((ITransformableObject)render).Transform;
                ((ActorModelBase)ActorInfo).Visible = render.IsVisible;
                render.VisibilityChanged += delegate
                {
                    ((ActorModelBase)ActorInfo).Visible = render.IsVisible;
                };
            }
            ActorInfo.Init();

            render.AddCallback += delegate
            {
                Renderers.Add(render);
                StudioSystem.Instance.AddActor(ActorInfo);
            };
            render.RemoveCallback += delegate
            {
                //Remove actor data on disposing the object.
                Renderers.Remove(render);
                StudioSystem.Instance.RemoveActor(ActorInfo);
                ActorInfo?.Dispose();
            };

            //Custom frustum culling.
            //Map objects should just cull one big box rather than individual meshes.
            if (render is BfresRender)
                ((BfresRender)render).FrustumCullingCallback = () => {
                    if (!obj.IsSkybox)
                        ((BfresRender)render).UseDrawDistance = true;

                    return FrustumCullObject((BfresRender)render);
                }   ;
            
            //Render links
            UpdateObjectLinks(render);

            //Update the render transform
            render.Transform.Position = new OpenTK.Vector3(
                obj.Translate.X,
                obj.Translate.Y,
                obj.Translate.Z);
            render.Transform.RotationEulerDegrees = new OpenTK.Vector3(
                obj.RotateDegrees.X,
                obj.RotateDegrees.Y,
                obj.RotateDegrees.Z);
            render.Transform.Scale = new OpenTK.Vector3(
                obj.Scale.X,
                obj.Scale.Y,
                obj.Scale.Z);
            render.Transform.UpdateMatrix(true);

            //Updates for property changes
            obj.PropertyChanged += delegate
            {
                render.UINode.Header = GetNodeHeader(obj);
                string objName = GetResourceName(obj);

                if (IconManager.HasIcon($"{Runtime.ExecutableDir}\\Images\\MapObjects\\{objName}.png"))
                    render.UINode.Icon = $"{Runtime.ExecutableDir}\\Images\\MapObjects\\{objName}.png";
                else
                    render.UINode.Icon = "Node";

                UpdateObjectLinks(render);

                //Update actor parameters into the actor class
                ((ActorModelBase)ActorInfo).Parameters = obj.Params;

                //Update the view if properties are updated.
                GLContext.ActiveContext.UpdateViewport = true;
            };
            return render;
        }

        private void UpdateObjectLinks(EditableObject render)
        {
            render.DestObjectLinks.Clear();

            var obj = render.UINode.Tag as Obj;
            foreach (var linkableObject in GLContext.ActiveContext.Scene.Objects)
            {
                if (linkableObject is RenderablePath) {
                    var path = linkableObject as RenderablePath;
                    TryFindPathLink(render, path, obj.Path, obj.PathPoint);
                    TryFindPathLink(render, path, obj.LapPath, obj.LapPathPoint);
                    TryFindPathLink(render, path, obj.ObjPath, obj.ObjPathPoint);
                    TryFindPathLink(render, path, obj.EnemyPath1, null);
                    TryFindPathLink(render, path, obj.EnemyPath2, null);
                    TryFindPathLink(render, path, obj.ItemPath1, null);
                    TryFindPathLink(render, path, obj.ItemPath2, null);

                }
                else if (linkableObject is EditableObject)
                {
                    var editObj = linkableObject as EditableObject;
                    TryFindObjectLink(render, editObj, obj.ParentArea);
                    TryFindObjectLink(render, editObj, obj.ParentObj);
                }
            }
        }

        private void TryFindObjectLink(EditableObject render, EditableObject obj, object objInstance)
        {
            if (objInstance == null)
                return;

            if (obj.UINode.Tag == objInstance)
                render.DestObjectLinks.Add(obj);
        }

        private void TryFindPathLink(EditableObject render, RenderablePath path, object pathInstance, object pointInstance)
        {
            if (pathInstance == null)
                return;

            var properties = path.UINode.Tag;
            if (properties == pathInstance)
            {
                foreach (var point in path.PathPoints)
                {
                    if (point.UINode.Tag == pointInstance) {
                        render.DestObjectLinks.Add(point);
                        return;
                    }
                }
                if (path.PathPoints.Count > 0)
                    render.DestObjectLinks.Add(path.PathPoints.FirstOrDefault());
            }
        }

        //Object specific frustum cull handling
        private bool FrustumCullObject(BfresRender render)
        {
            if (render.Models.Count == 0)
                return false;

            var transform = render.Transform;
            var context = GLContext.ActiveContext;

            var bounding = render.BoundingNode;
            bounding.UpdateTransform(transform.TransformMatrix);
            if (!context.Camera.InFustrum(bounding))
                return false;

            if (render.IsSelected)
                return true;

          //  if (render.UseDrawDistance)
            //    return context.Camera.InRange(transform.Position, 6000000);

            return true;
        }

        private EditableObject AddObject(int id, bool spawnAtCursor = false)
        {
            //Force added sky boxes to edit existing if possible
            if (GlobalSettings.ObjDatabase.ContainsKey(id) && GlobalSettings.ObjDatabase[id].VR)
            {
                EditableObject skyboxRender = null;
                foreach (EditableObject render in this.Renderers)
                {
                    var obj = render.UINode.Tag as Obj;
                    if (obj.IsSkybox)
                        skyboxRender = render;
                }
                if (skyboxRender != null)
                    return EditObject(skyboxRender, id);
            }
            var rend = Create(new Obj() { ObjId = id });
            Add(rend, true);

            var ob = rend.UINode.Tag as Obj;

            //Reset parameters to defaults
            if (ParamDatabase.ParameterDefaults.ContainsKey(ob.ObjId))
                ob.Params = ParamDatabase.ParameterDefaults[ob.ObjId].ToList();

            //Define some parameters based on the existing objects in the scene

            //These types (start objects) are indexed individually with a unique index.
            //They should spawn based on the placed index used
            if (ob.ObjId == 6002) {
               int index = Root.Children.Where(x => ((Obj)x.Tag).ObjId == ob.ObjId).ToList().Count;
                ob.Params[7] = index;
            }
            if (ob.ObjId == 8008) {
                int index = Root.Children.Where(x => ((Obj)x.Tag).ObjId == ob.ObjId).ToList().Count;
                ob.Params[0] = index;
            }

            GLContext.ActiveContext.Scene.DeselectAll(GLContext.ActiveContext);

            //Get the default placements for our new object
            EditorUtility.SetObjectPlacementPosition(rend.Transform, spawnAtCursor);
            rend.UINode.IsSelected = true;
            return rend;
        }

        private EditableObject EditObject(EditableObject render, int id)
        {
            int index = render.UINode.Index;
            var obj = render.UINode.Tag as Obj;
            obj.ObjId = id;

            //Remove the previous renderer
            GLContext.ActiveContext.Scene.RemoveRenderObject(render);

            //Create a new object with the current ID
            var editedRender = Create(obj);
            Add(editedRender);

            //Reset parameters to defaults
            if (ParamDatabase.ParameterDefaults.ContainsKey(obj.ObjId))
                obj.Params = ParamDatabase.ParameterDefaults[obj.ObjId].ToList();

            //Keep the same node order
            Root.Children.Remove(editedRender.UINode);
            Root.Children.Insert(index, editedRender.UINode);

            editedRender.Transform.Position = render.Transform.Position;
            editedRender.Transform.Scale = render.Transform.Scale;
            editedRender.Transform.Rotation = render.Transform.Rotation;
            editedRender.Transform.UpdateMatrix(true);

            editedRender.UINode.IsSelected = true;

            //Skybox updated, change the cubemap
            if (obj.IsSkybox)
                Workspace.ActiveWorkspace.Resources.UpdateCubemaps = true;

            //Undo operation
            GLContext.ActiveContext.Scene.AddToUndo(new ObjectEditUndo(this, render, editedRender));

            return editedRender;
        }

        private string GetResourceName(Obj obj)
        {
            GlobalSettings.LoadDataBase();

            //Load through an in tool list if the database isn't loaded
            string name = GlobalSettings.ObjectList.ContainsKey(obj.ObjId) ? $"{GlobalSettings.ObjectList[obj.ObjId]}" : obj.ObjId.ToString();

            //Use object database instead if exists
            if (GlobalSettings.ObjDatabase.ContainsKey(obj.ObjId))
                name = GlobalSettings.ObjDatabase[obj.ObjId].ResNames.FirstOrDefault();

            return name;
        }

        private string GetNodeHeader(Obj obj)
        {
            string name = GlobalSettings.ObjectList.ContainsKey(obj.ObjId) ? $"{GlobalSettings.ObjectList[obj.ObjId]}" : obj.ObjId.ToString();
            //Use object database instead if exists
            if (GlobalSettings.ObjDatabase.ContainsKey(obj.ObjId))
                name = GlobalSettings.ObjDatabase[obj.ObjId].Label;

            //Start Ex parameter spawn index
            if (obj.ObjId == 8008)
                name += $" ({obj.Params[0]})";
            //Test start parameter spawn index
            if (obj.ObjId == 6002)
                name += $" ({obj.Params[7]})";

            if (obj.ParentObj != null)
                name += $"    {IconManager.MODEL_ICON}    ";
            if (obj.ParentArea != null)
                name += $"    {IconManager.RECT_SCALE_ICON}    ";
            if (obj.Path != null)
                name += $"    {IconManager.PATH_ICON}    ";
            if (obj.ObjPath != null)
                name += $"    {IconManager.ANIM_PATH_ICON}    ";

            return name;
        }

        private void AddObjectMenuAction()
        {
            var objects = GlobalSettings.ObjDatabase.Values.OrderBy(x => x.Label).ToList();

            MapObjectSelector selector = new MapObjectSelector(objects);
            MapStudio.UI.DialogHandler.Show(TranslationSource.GetText("SELECT_OBJECT"), 400, 800, () =>
            {
                selector.Render();
            }, (result) =>
            {
                var id = selector.GetSelectedID();
                if (!result || id == 0)
                    return;

                AddObject(id, true);
            });
        }

        private void EditObjectMenuAction()
        {
            var selected = GetSelected().ToList();
            if (selected.Count == 0)
                return;

            var objects = GlobalSettings.ObjDatabase.Values.OrderBy(x => x.Label).ToList();

            MapObjectSelector selector = new MapObjectSelector(objects);
            MapStudio.UI.DialogHandler.Show(TranslationSource.GetText("SELECT_OBJECT"), 400, 800, () =>
            {
                selector.Render();
            }, (result) =>
            {
            var id = selector.GetSelectedID();
            if (!result || id == 0)
                return;

            var renders = selected.Select(x => ((EditableObjectNode)x).Object).ToList();

                GLContext.ActiveContext.Scene.BeginUndoCollection();
                foreach (EditableObjectNode ob in selected)
                {
                    int previousID = ((Obj)ob.Tag).ObjId;

                    var render = EditObject(ob.Object, id);
                }
                GLContext.ActiveContext.Scene.EndUndoCollection();
            });
        }

        class ObjectEditUndo : IRevertable
        {
            private List<ObjectInfo> Objects = new List<ObjectInfo>();

            public ObjectEditUndo(List<ObjectInfo> objects)
            {
                this.Objects = objects;
            }

            public ObjectEditUndo(ObjectEditor editor, EditableObject previousObj, EditableObject newObj)
            {
                Objects.Add(new ObjectInfo(editor, previousObj, newObj));
            }

            public IRevertable Revert()
            {
                var redoList = new List<ObjectInfo>();
                foreach (var info in Objects)
                {
                    redoList.Add(new ObjectInfo(info.Editor, info.NewRender, info.PreviousRender));

                    info.Editor.Remove(info.NewRender);
                    info.Editor.Add(info.PreviousRender);

                    //Skybox updated, change the cubemap
                    if (((Obj)info.NewRender.UINode.Tag).IsSkybox)
                        Workspace.ActiveWorkspace.Resources.UpdateCubemaps = true;
                }
                return new ObjectEditUndo(redoList);
            }

            public class ObjectInfo
            {
                public EditableObject PreviousRender;
                public EditableObject NewRender;

                public ObjectEditor Editor;

                public ObjectInfo(ObjectEditor editor, EditableObject previousObj, EditableObject newObj)
                {
                    Editor = editor;
                    PreviousRender = previousObj;
                    NewRender = newObj;
                }
            }
        }
    }
}
