using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.ViewModels;
using ImGuiNET;
using GLFrameworkEngine;
using TurboLibrary;
using OpenTK;
using MapStudio.UI;

namespace TurboLibrary.MuuntEditor
{
    public class ReplayCameraEditor : IMunntEditor
    {
        public string Name => TranslationSource.GetText("REPLAY_CAMERAS");

        public CourseMuuntPlugin MapEditor { get; set; }

        public bool IsActive { get; set; } = false;

        public IToolWindowDrawer ToolWindowDrawer => null;

        public List<IDrawable> Renderers { get; set; } = new List<IDrawable>();

        public NodeBase Root { get; set; } = new NodeBase(TranslationSource.GetText("REPLAY_CAMERAS")) { HasCheckBox = true };
        public List<MenuItemModel> MenuItems { get; set; } = new List<MenuItemModel>();

        public List<NodeBase> GetSelected()
        {
            return Root.Children.Where(x => x.IsSelected).ToList();
        }

        public ReplayCameraEditor(CourseMuuntPlugin editor, List<ReplayCamera> objs)
        {
            Root.Icon = MapEditorIcons.REPLAY_CAMERA_ICON.ToString();

            MapEditor = editor;
            ReloadMenuItems();
            Init(objs);
        }

        public void DrawEditMenuBar()
        {

        }

        public void DrawHelpWindow() {
            if (ImGuiNET.ImGui.CollapsingHeader("Objects", ImGuiNET.ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGuiHelper.BoldTextLabel(InputSettings.INPUT.Scene.Create, "Create Object.");
            }
        }

        public void ReloadEditor()
        {
            Root.Header = TranslationSource.GetText("REPLAY_CAMERAS");
            ReloadMenuItems();

            foreach (EditableObject render in Renderers)
                render.CanSelect = true;
        }

        void Init(List<ReplayCamera> objs)
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
            course.ReplayCameras = new List<ReplayCamera>();

            foreach (EditableObject render in Renderers)
            {
                var obj = (ReplayCamera)render.UINode.Tag;
                obj.Translate = new ByamlVector3F(
                    render.Transform.Position.X,
                    render.Transform.Position.Y,
                    render.Transform.Position.Z);
                obj.RotateDegrees = new ByamlVector3F(
                    render.Transform.RotationEulerDegrees.X,
                    render.Transform.RotationEulerDegrees.Y,
                    render.Transform.RotationEulerDegrees.Z);
                obj.Scale = new ByamlVector3F(
                    render.Transform.Scale.X,
                    render.Transform.Scale.Y,
                    render.Transform.Scale.Z);

                course.ReplayCameras.Add(obj);
            }
        }

        public void OnMouseDown(MouseEventInfo mouseInfo) {
            bool isActive = Workspace.ActiveWorkspace.ActiveEditor.SubEditor == this.Name;

            if (isActive && KeyEventInfo.State.KeyAlt && mouseInfo.LeftButton == OpenTK.Input.ButtonState.Pressed)
                Add(PlaceNewObject(), true);
        }

        public void OnMouseUp(MouseEventInfo mouseInfo) { }
        public void OnMouseMove(MouseEventInfo mouseInfo) { }

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
        }

        public void OnKeyDown(KeyEventInfo keyInfo)
        {
            if (Root.IsSelected && keyInfo.IsKeyDown(InputSettings.INPUT.Scene.Create))
                Add(PlaceNewObject(), true);
        }

        public void ReloadMenuItems()
        {
        }

        public void RemoveSelected()
        {
            var selected = Renderers.Where(x => ((EditableObject)x).IsSelected).ToList().ToList();
            foreach (EditableObject obj in selected)
                Remove(obj, true);
        }

        private EditableObject PlaceNewObject()
        {
            var render = Create(new ReplayCamera());
            render.IsSelected = true;

            //Get the default placements for our new object
            EditorUtility.SetObjectPlacementPosition(render.Transform);

            return render;
        }

        private EditableObject Create(ReplayCamera obj)
        {
            var render = new TransformableObject(Root);
            render.UINode.Tag = obj;
            render.UINode.Icon = Root.Icon;
            UpdateCameraLinks(render);
            render.RemoveCallback += delegate
            {
                if (this.Renderers.Contains(render))
                    this.Renderers.Remove(render);
            };
            render.AddCallback += delegate
            {
                if (!this.Renderers.Contains(render))
                    this.Renderers.Add(render);
            };
            render.UINode.GetHeader = () =>
            {
                return $"Camera {render.UINode.Index}";
            };
            ((EditableObjectNode)render.UINode).UIProperyDrawer += delegate
            {
                ReplayCameraUI.Render(obj);
                ImguiBinder.LoadProperties(obj, Workspace.ActiveWorkspace.GetSelected());
            };
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
            obj.PropertyChanged += delegate
            {
            };
            return render;
        }

        private void UpdateCameraLinks(EditableObject render)
        {
            render.DestObjectLinks.Clear();

            var camera = render.UINode.Tag as ReplayCamera;
            foreach (var linkableObject in GLContext.ActiveContext.Scene.Objects)
            {
                if (linkableObject is RenderablePath)
                {
                    var path = linkableObject as RenderablePath;
                    TryFindPathLink(render, path, camera.Path);
                }
            }
        }

        private void TryFindPathLink(EditableObject render, RenderablePath path, object pathInstance)
        {
            if (pathInstance == null)
                return;

            var properties = path.UINode.Tag;
            if (properties == pathInstance)
                render.DestObjectLinks.Add(path);
        }
    }
}
