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
using CafeLibrary.Rendering;

namespace TurboLibrary.MuuntEditor
{
    public class AreaEditor : IMunntEditor
    {
        public virtual string Name => TranslationSource.GetText("AREAS");

        public CourseMuuntPlugin MapEditor { get; set; }

        public bool IsActive { get; set; } = false;

        private AreaToolSettings ToolSettings = new AreaToolSettings();
        public IToolWindowDrawer ToolWindowDrawer => ToolSettings;

        public List<IDrawable> Renderers { get; set; } = new List<IDrawable>();

        public virtual NodeBase Root { get; set; } = new NodeBase(TranslationSource.GetText("AREAS")) { HasCheckBox = true };
        public List<MenuItemModel> MenuItems { get; set; } = new List<MenuItemModel>();

        List<IDrawable> copied = new List<IDrawable>();

        public List<NodeBase> GetSelected()
        {
            return Root.Children.Where(x => x.IsSelected).ToList();
        }

        public AreaEditor() { }

        public AreaEditor(CourseMuuntPlugin editor, CourseDefinition course)
        {
            MapEditor = editor;

            ReloadMenuItems();
            Init(course);

            Root.Icon = MapEditorIcons.AREA_BOX.ToString();
            Root.IconColor = MapEditorIcons.AREA_BOX_COLOR;

            var addMenu = new MenuItemModel(this.Name);
            GLContext.ActiveContext.Scene.MenuItemsAdd.Add(addMenu);
            addMenu.MenuItems.Add(new MenuItemModel("SOUND_AREA", () => PlaceNewArea(new Area(AreaType.Audio))));
            addMenu.MenuItems.Add(new MenuItemModel("CAMERA_AREA", () => PlaceNewArea(new Area(AreaType.Camera))));
            addMenu.MenuItems.Add(new MenuItemModel("CEILING_AREA", () => PlaceNewArea(new Area(AreaType.Ceiling_DeluxeOnly))));
            addMenu.MenuItems.Add(new MenuItemModel("CURRENT_AREA", () => PlaceNewArea(new Area(AreaType.Current_DeluxeOnly))));
            addMenu.MenuItems.Add(new MenuItemModel("PRISON_AREA", () => PlaceNewArea(new Area(AreaType.Prison_DeluxeOnly))));
        }

        public void DrawEditMenuBar()
        {
            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.ADD_ICON}   ", "ADD", InputSettings.INPUT.Scene.Create))
            {
                Add(PlaceNewArea(Create()), true);
            }
            if (ImguiCustomWidgets.MenuItemTooltip($"   {'\uf044'}   ", "CREATE"))
            {
                HandleBoxTool();
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

        public virtual PrmObject Create() => new Area();

        public virtual void ReloadEditor()
        {
            Root.Header = TranslationSource.GetText("AREAS");
            ReloadMenuItems();

            foreach (AreaRender render in Renderers)
                render.CanSelect = true;
        }

        void Init(CourseDefinition course)
        {
            Root.Children.Clear();
            Renderers.Clear();

            //Load the current tree list
            for (int i = 0; i < course.Areas?.Count; i++)
                Add(Create(course.Areas[i]));
            for (int i = 0; i < course.CeilingAreas?.Count; i++)
                Add(Create(course.CeilingAreas[i]));
            for (int i = 0; i < course.CurrentAreas?.Count; i++)
                Add(Create(course.CurrentAreas[i]));
            for (int i = 0; i < course.PrisonAreas?.Count; i++)
                Add(Create(course.PrisonAreas[i]));

            if (Root.Children.Any(x => x.IsSelected))
                Root.IsExpanded = true;
        }

        public virtual void OnSave(CourseDefinition course)
        {
            course.Areas = new List<Area>();
            course.PrisonAreas = new List<PrisonArea>();
            course.CurrentAreas = new List<CurrentArea>();
            course.CeilingAreas = new List<CeilingArea>();

            foreach (AreaRender render in Renderers)
            {
                var obj = (Area)render.UINode.Tag;
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

                //Update unit id for linking with project data
                if (obj is PrisonArea)
                    course.PrisonAreas.Add((PrisonArea)obj);
                else if (obj is CurrentArea)
                    course.CurrentAreas.Add((CurrentArea)obj);
                else if (obj is CeilingArea)
                    course.CeilingAreas.Add((CeilingArea)obj);
                else
                    course.Areas.Add(obj);
            }
        }

        public void OnMouseDown(MouseEventInfo mouseInfo) {
            bool isActive = Workspace.ActiveWorkspace.ActiveEditor.SubEditor == this.Name;

            if (isActive && KeyEventInfo.State.KeyAlt && mouseInfo.LeftButton == OpenTK.Input.ButtonState.Pressed)
                Add(PlaceNewArea(Create()), true);
        }

        public void OnMouseUp(MouseEventInfo mouseInfo) { }
        public void OnMouseMove(MouseEventInfo mouseInfo) { }

        public virtual void Add(AreaRender render, bool undo = false)
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
            bool isActive = Workspace.ActiveWorkspace.ActiveEditor.SubEditor == this.Name;

            if (isActive && keyInfo.IsKeyDown(InputSettings.INPUT.Scene.Create))
                Add(PlaceNewArea(Create()), true);
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

        public void HandleBoxTool()
        {
            var tool = GLContext.ActiveContext.BoxCreationTool;
            tool.IsActive = true;

            tool.BoxCreated = null;
            tool.BoxCreated += delegate
            {
                var area = Create(Create());
                area.Transform.Rotation = tool.GetRotation();
                area.Transform.Scale = tool.GetScale() * 0.02f;
                area.Transform.Position = tool.GetCenter() - (new Vector3(0, 50, 0) * area.Transform.Scale);
                area.Transform.UpdateMatrix(true);
                Add(area, true);
            };
        }

        public void ReloadMenuItems()
        {
        }

        public void RemoveSelected()
        {
            var selected = Renderers.Where(x => ((AreaRender)x).IsSelected).ToList();
            foreach (EditableObject obj in selected)
                Remove(obj, true);
        }

        private AreaRender PlaceNewArea(PrmObject area)
        {
            var render = Create(area);

            //Get the default placements for our new object
            EditorUtility.SetObjectPlacementPosition(render.Transform);

            return render;
        }

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
                var obj = ob.UINode.Tag as ICloneable;
                var duplicated = Create((PrmObject)obj.Clone());
                duplicated.Transform.Position = ob.Transform.Position;
                duplicated.Transform.Scale = ob.Transform.Scale;
                duplicated.Transform.Rotation = ob.Transform.Rotation;
                duplicated.Transform.UpdateMatrix(true);
                duplicated.IsSelected = true;

                Add(duplicated, true);
            }
        }

        public virtual AreaRender Create(PrmObject obj)
        {
            var area = obj as Area;

            var render = new AreaRender(Root, GetAreaColor(area));
            render.UINode.Header = GetDisplayName(area, render.UINode.Index);
            render.UINode.Tag = area;
            render.AddCallback += delegate
            {
                Renderers.Add(render);
            };
            render.RemoveCallback += delegate
            {
                Renderers.Remove(render);
            };
            ReloadIcon(render.UINode, area);

            ((EditableObjectNode)render.UINode).UIProperyDrawer += delegate
            {
                if (area.AreaType == AreaType.Roam)
                    ImguiCustomWidgets.ObjectLinkSelector(TranslationSource.GetText("RELATIVE_OBJECT"), render.UINode.Tag, "Obj");
                else if (area.AreaType == AreaType.Pull)
                    ImguiCustomWidgets.ObjectLinkSelector(TranslationSource.GetText("RELATIVE_PULL_PATH"), render.UINode.Tag, "PullPath");
                else if (area.AreaType == AreaType.Camera)
                {
                    AreaUI.Render(area);
                }
                else
                    ImguiCustomWidgets.ObjectLinkSelector(TranslationSource.GetText("RELATIVE_RAIL_PATH"), render.UINode.Tag, "Path");

                ImguiBinder.LoadProperties(area, Workspace.ActiveWorkspace.GetSelected());
            };
            //Update the index on collection changed
            this.Root.Children.CollectionChanged += delegate
            {
                render.UINode.Header = GetDisplayName(area, render.UINode.Index);
            };
            //Update the render transform
            render.Transform.Position = new OpenTK.Vector3(
                area.Translate.X,
                area.Translate.Y,
                area.Translate.Z);
            render.Transform.RotationEulerDegrees = new OpenTK.Vector3(
                area.RotateDegrees.X,
                area.RotateDegrees.Y,
                area.RotateDegrees.Z);
            render.Transform.Scale = new OpenTK.Vector3(
                area.Scale.X,
                area.Scale.Y,
                area.Scale.Z);
            render.Transform.UpdateMatrix(true);
            area.PropertyChanged += delegate
            {
                int index = render.UINode.Index;
                render.UINode.Header = GetDisplayName(area, index);
                render.Color = GetAreaColor(area);
                ReloadIcon(render.UINode, area);
                GLContext.ActiveContext.UpdateViewport = true;
            };

            return render;
        }

        private void ReloadIcon(NodeBase node, Area area)
        {
            node.Icon = MapEditorIcons.AREA_BOX.ToString();
            var color = GetAreaColor(area);
            node.IconColor = new System.Numerics.Vector4(color.X, color.Y, color.Z, color.W);
        }

        public string GetDisplayName(Area area, int index)
        {
            return $"Area_{index} [{area.AreaType} {GetCameraInfo(area)}]";
        }

        private string GetCameraInfo(Area area)
        {
            if (area.AreaType == AreaType.Camera && area.ReplayCameras != null)
                return $"   { MapEditorIcons.REPLAY_CAMERA_ICON}   ";
            return "";
        }

        private Vector4 GetAreaColor(Area area)
        {
            if (area.AreaType == AreaType.Camera)
                return new Vector4(1, 0, 0, 1);
            if (area.AreaType == AreaType.Pull)
                return new Vector4(0, 1, 0, 1);
            if (area.AreaType == AreaType.Roam)
                return new Vector4(0, 0, 1, 1);
            if (area.AreaType == AreaType.Audio)
                return new Vector4(1, 1, 0, 1);
            if (area.AreaType == AreaType.Clip)
                return new Vector4(0.2f, 0.2f, 0.2f, 1);
            if (area.AreaType == AreaType.Ceiling_DeluxeOnly)
                return new Vector4(0.5f, 0.5f, 1, 1);
            if (area.AreaType == AreaType.Current_DeluxeOnly)
                return new Vector4(1, 0.5f, 0.5f, 1);
            if (area.AreaType == AreaType.Ceiling_DeluxeOnly)
                return new Vector4(0.5f, 1, 0.5f, 1);

            return new Vector4(1, 1, 1, 1);
        }
    }
}
