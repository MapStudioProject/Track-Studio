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
    public class RouteChangeEditor : IMunntEditor
    {
        public string Name => TranslationSource.GetText("ROUTE_CHANGE");

        public CourseMuuntPlugin MapEditor { get; set; }

        public bool IsActive { get; set; } = false;

        public IToolWindowDrawer ToolWindowDrawer => null;

        public List<IDrawable> Renderers { get; set; } = new List<IDrawable>();

        public NodeBase Root { get; set; } = new NodeBase(TranslationSource.GetText("ROUTE_CHANGE")) { HasCheckBox = true };
        public List<MenuItemModel> MenuItems { get; set; } = new List<MenuItemModel>();

        public List<NodeBase> GetSelected()
        {
            return Root.Children.Where(x => x.IsSelected).ToList();
        }

        public RouteChangeEditor(CourseMuuntPlugin editor, List<RouteChange> objs)
        {
            Root.Icon = '\uf4d7'.ToString();

            MapEditor = editor;
            ReloadMenuItems();
            Init(objs);
        }

        public void DrawEditMenuBar()
        {
            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.ADD_ICON}   ", "ADD", InputSettings.INPUT.Scene.Create))
            {
                Add(PlaceNewObject(), true);
            }
            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.DELETE_ICON}   ", "REMOVE", InputSettings.INPUT.Scene.Delete))
            {
                MapEditor.Scene.BeginUndoCollection();
                RemoveSelected();
                MapEditor.Scene.EndUndoCollection();
            }
            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.COPY_ICON}   ", "COPY", InputSettings.INPUT.Scene.Copy))
            {
                CopySelected();
            }
            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.PASTE_ICON}   ", "PASTE", InputSettings.INPUT.Scene.Paste))
            {
                PasteSelected();
            }
        }

        public void DrawHelpWindow() {
            if (ImGuiNET.ImGui.CollapsingHeader("Objects", ImGuiNET.ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGuiHelper.BoldTextLabel(InputSettings.INPUT.Scene.Create, "Create Object.");
            }
        }

        public void ReloadEditor()
        {
            Root.Header = TranslationSource.GetText("ROUTE_CHANGE");
            ReloadMenuItems();

            foreach (EditableObject render in Renderers)
                render.CanSelect = true;
        }

        void Init(List<RouteChange> objs)
        {
            Root.Children.Clear();
            Renderers.Clear();

            //Load the current tree list
            for (int i = 0; i < objs?.Count; i++)
                Add(Create(objs[i]));

            if (Root.Children.Any(x => x.IsSelected))
                Root.IsExpanded = true;

            var addMenu = new MenuItemModel(this.Name, () => PlaceNewObject());
            GLContext.ActiveContext.Scene.MenuItemsAdd.Add(addMenu);
        }

        public void OnSave(CourseDefinition course)
        {
            course.RouteChanges = new List<RouteChange>();

            foreach (EditableObject render in Renderers)
            {
                var obj = (RouteChange)render.UINode.Tag;
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

                course.RouteChanges.Add(obj);
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
            Renderers.Add(render);
            MapEditor.AddRender(render, undo);
        }

        public void Remove(EditableObject render, bool undo = false)
        {
            Renderers.Remove(render);
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
            bool isActive = Workspace.ActiveWorkspace.ActiveEditor.SubEditor == this.Name;

            if (isActive && keyInfo.IsKeyDown(InputSettings.INPUT.Scene.Create))
                Add(PlaceNewObject(), true);
        }

        private EditableObject PlaceNewObject()
        {
            var render = Create(new RouteChange());
            render.IsSelected = true;

            //Get the default placements for our new object
            EditorUtility.SetObjectPlacementPosition(render.Transform);

            return render;
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

        public void CopySelected()
        {

        }

        public void PasteSelected()
        {

        }

        private EditableObject Create(RouteChange obj)
        {
            var render = new TransformableObject(Root);
            render.UINode.Tag = obj;
            render.UINode.Icon = Root.Icon;
            render.UINode.Header = this.Root.Header;
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
    }
}
