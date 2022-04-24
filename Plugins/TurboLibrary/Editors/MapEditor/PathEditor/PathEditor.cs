using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurboLibrary;
using OpenTK;
using GLFrameworkEngine;
using Toolbox.Core.ViewModels;
using MapStudio.UI;

namespace TurboLibrary.MuuntEditor
{
    /// <summary>
    /// General purpose path editor.
    /// </summary>
    public class PathEditor<TPath, TPoint> : IMunntEditor
       where TPath : PathBase<TPath, TPoint>
       where TPoint : PathPointBase<TPath, TPoint>, new()
    {
        public string Name => _name;

        public CourseMuuntPlugin MapEditor { get; set; }

        private IToolWindowDrawer PathSettingsWindow;
        public IToolWindowDrawer ToolWindowDrawer => PathSettingsWindow;

        public List<IDrawable> Renderers { get; set; } = new List<IDrawable>();

        public NodeBase Root { get; set; }
        public List<MenuItemModel> MenuItems { get; set; }

        public bool IsActive { get; set; }

        private List<PathBase<TPath, TPoint>> _groups;
        private string _name;

        private PathRender<TPath, TPoint> Renderer;

        public PathEditor(CourseMuuntPlugin editor, string name, GlobalSettings.PathColor color,
                IEnumerable<PathBase<TPath, TPoint>> groups)
        {
            MapEditor = editor;
            _name = name;
            Root = new NodeBase(name) { HasCheckBox = true };
            MapEditorIcons.ReloadIcons(Root, typeof(TPath));

            if (groups != null)
                _groups = groups.ToList();

            Renderer = new PathRender<TPath, TPoint>(_groups, Root);
            Renderer.PointColor = new Vector4(color.PointColor.X, color.PointColor.Y, color.PointColor.Z, 1.0f);
            Renderer.ArrowColor = new Vector4(color.ArrowColor.X, color.ArrowColor.Y, color.ArrowColor.Z, 1.0f);
            Renderer.LineColor = new Vector4(color.LineColor.X, color.LineColor.Y, color.LineColor.Z, 1.0f);
            MapEditor.AddRender(Renderer);
            Renderer.PointAddedCallback += delegate
            {
                if (!Root.IsChecked)
                    Root.IsChecked = true;
            };
            PathSettingsWindow = new PathToolSettings<TPath, TPoint>(this, Renderer); 

            color.OnColorChanged += delegate
            {
                Renderer.PointColor = new Vector4(color.PointColor.X, color.PointColor.Y, color.PointColor.Z, 1.0f);
                Renderer.ArrowColor = new Vector4(color.ArrowColor.X, color.ArrowColor.Y, color.ArrowColor.Z, 1.0f);
                Renderer.LineColor = new Vector4(color.LineColor.X, color.LineColor.Y, color.LineColor.Z, 1.0f);
            };

            Renderers.Add(Renderer);

            ReloadTree();

            var addMenu = new MenuItemModel(this.Name, () => Renderer.AddSinglePoint());
            GLContext.ActiveContext.Scene.MenuItemsAdd.Add(addMenu);
        }

        public void DrawEditMenuBar() {
            bool changed = Workspace.ActiveWorkspace.ViewportWindow.DrawPathDropdown();
            if (changed) {
                Renderer.DeselectAll();
                Renderer.IsSelected = false;
            }

            bool refreshScene = false;

            var h = ImGuiNET.ImGui.GetWindowHeight();
            var size = new System.Numerics.Vector2(h, h);

            refreshScene |= ImguiCustomWidgets.ButtonToggle($"  {IconManager.XRAY}    ", ref Renderer.XRayMode, size);
            ImGuiHelper.Tooltip("XRAY");

            refreshScene |= ImguiCustomWidgets.ButtonToggle($"  {'\uf111'}    ", ref RenderablePath.DisplayPointSize, size);
            ImGuiHelper.Tooltip("DISPLAY_RADIUS");

            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.ADD_ICON}   ", "ADD", InputSettings.INPUT.Scene.Create))
                Renderer.AddSinglePoint();

            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.DELETE_ICON}   ", "REMOVE", InputSettings.INPUT.Scene.Delete))
                Renderer.RemoveSelected();

            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.DUPE_ICON}   ", "DUPLICATE", InputSettings.INPUT.Scene.Dupe))
                Renderer.DuplicateSelected();

            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.LINK_ICON}   ", "CONNECT", InputSettings.INPUT.Scene.Fill))
                Renderer.FillSelectedPoints();

            if (ImguiCustomWidgets.MenuItemTooltip($"   {IconManager.UNLINK_ICON}   ", "UNCONNECT"))
                Renderer.UnlinkSelectedPoints();

            ImGuiNET.ImGui.PushItemWidth(70);
            refreshScene |= ImGuiNET.ImGui.DragFloat($"{TranslationSource.GetText("POINT_SIZE")}", ref RenderablePath.PointSize, 0.1f, 0.1f);
            ImGuiNET.ImGui.PopItemWidth();

            if (refreshScene)
                GLContext.ActiveContext.UpdateViewport = true;
        }

        public void DrawHelpWindow() 
        {
            if (ImGuiNET.ImGui.CollapsingHeader("Paths", ImGuiNET.ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGuiHelper.BoldTextLabel(InputSettings.INPUT.Scene.Create, "Create Point.");
                ImGuiHelper.BoldTextLabel(InputSettings.INPUT.Scene.Extrude, "Extrude Point.");
                ImGuiHelper.BoldTextLabel(InputSettings.INPUT.Scene.Fill, "Fill Points.");
                ImGuiHelper.BoldTextLabel(InputSettings.INPUT.Scene.Insert, "Insert Between Points.");
            }
        }

        public void ReloadEditor()
        {
            Renderer.IsActive = true;
            foreach (var part in Renderer.PathPoints)
                part.CanSelect = true;
        }

        private void ReloadTree()
        {
            if (_groups == null)
                return;

            List<object> points = new List<object>();

            Renderer.PathPoints.Clear();
            Root.Children.Clear();  
            foreach (var group in _groups) {
                var node = new PathWrapper(group);
                Root.AddChild(node);

                foreach (var pt in group.Points) {
                    if (pt.Path == null)
                        continue;

                    points.Add(pt);

                    var pos = new Vector3(pt.Translate.X, pt.Translate.Y, pt.Translate.Z) * GLContext.PreviewScale;
                    var point = (PathPoint<TPath, TPoint>)Renderer.CreatePoint(pos);
                    if (pt.Scale.HasValue)
                        point.Transform.Scale = new Vector3(pt.Scale.Value.X, pt.Scale.Value.Y, pt.Scale.Value.Z);
                    point.Transform.UpdateMatrix(true);
                    point.Group = pt.Path;
                    point.Tag = pt;
                    point.UINode.Tag = pt;
                    node.AddChild(point.UINode);

                    Renderer.AddPoint(point);
                }
            }

            //Then connect each point from the next child
            int pointIndex = 0;
            for (int i = 0; i < _groups?.Count; i++) {
                foreach (var pt in _groups[i].Points) {
                    if (pt.NextPoints == null)
                        continue;

                    foreach (var nextPT in pt.NextPoints)
                    {
                        int childIndex = points.IndexOf(nextPT);
                        var child = Renderer.PathPoints[childIndex];

                        child.Parents.Add(Renderer.PathPoints[pointIndex]);
                        Renderer.PathPoints[pointIndex].Children.Add(child);
                    }
                    pointIndex++;
                }   
            }
            Renderer.FillTree();
        }

        public List<NodeBase> GetSelected()
        {
            var selected = new List<NodeBase>();
            foreach (PathPoint<TPath, TPoint> point in Renderer.PathPoints)
            {
                if (point.UINode.IsSelected)
                    selected.Add(point.UINode);
            }
            return selected;
        }

        public void Add(NodeBase node)
        {

        }

        public void Remove(NodeBase node)
        {

        }

        public void RemoveSelected() {
            Renderer.RemoveSelected();
        }

        public void OnMouseDown(MouseEventInfo mouseInfo) { }
        public void OnMouseUp(MouseEventInfo mouseInfo) { }
        public void OnMouseMove(MouseEventInfo mouseInfo) 
        {
        }

        public void OnKeyDown(KeyEventInfo keyInfo)
        {
            if (keyInfo.IsKeyDown(InputSettings.INPUT.Scene.Dupe))
            {
                var selected = this.Renderer.GetSelectedPoints();
                if (selected.Count == 1)
                    this.Renderer.DuplicateSelected();
            }
        }


        public void ConvertItemPaths<ConverPath, ConvertPoint>()
                     where ConverPath : PathBase<TPath, TPoint>
                    where ConvertPoint : PathPointBase<TPath, TPoint>, new()
        {
            /*   var itemPath = (PathEditor<ConverPath, ConvertPoint>)MapEditor.Editors.FirstOrDefault(x => x is PathEditor<ConverPath, ConvertPoint>);
               var paths = itemPath.SavePaths<ItemPath>();
               ConvertPath(paths);*/
        }

        public void OnSave(CourseDefinition course)
        {
            if (typeof(TPath) == typeof(EnemyPath)) course.EnemyPaths = SavePaths<EnemyPath>();
            if (typeof(TPath) == typeof(ItemPath))  course.ItemPaths = SavePaths<ItemPath>();
            if (typeof(TPath) == typeof(GlidePath)) course.GlidePaths = SavePaths<GlidePath>();
            if (typeof(TPath) == typeof(PullPath)) course.PullPaths = SavePaths<PullPath>();
            if (typeof(TPath) == typeof(SteerAssistPath)) course.SteerAssistPaths = SavePaths<SteerAssistPath>();
        }

        public List<T> SavePaths<T>()
        {
            if (Renderer.PathPoints.Count == 0)
                return null;
            else
                return Renderer.GeneratePaths().OfType<T>().ToList();
        }

        public void ConvertPaths<CPath, CPoint>()
            where CPath : PathBase<CPath, CPoint>
            where CPoint : PathPointBase<CPath, CPoint>, new()
        {
            //Find editor to path type
            var editor = this.MapEditor.Editors.FirstOrDefault(x => x is PathEditor<CPath, CPoint>);
            if (editor == null)
                return;

            var paths = ((PathEditor<CPath, CPoint>)editor).SavePaths<CPath>().OfType<PathBase<CPath, CPoint>>().ToList();
            ConvertPath(paths);
        }

        private void ConvertPath<CPath, CPoint>(List<PathBase<CPath,CPoint>> paths)
            where CPath : PathBase<CPath, CPoint>
            where CPoint : PathPointBase<CPath, CPoint>, new()
        {
            if (_groups == null) _groups = new List<PathBase<TPath, TPoint>>();

            _groups.Clear();
            foreach (var path in paths)
            {
                var newGroup = (PathBase<TPath, TPoint>)Activator.CreateInstance(typeof(TPath));
                _groups.Add(newGroup);

                foreach (var pt in path.Points)
                {
                    var newPoint = (PathPointBase<TPath, TPoint>)Activator.CreateInstance(typeof(TPoint));
                    newPoint.Path = (TPath)newGroup;
                    newPoint.NextPoints = new List<TPoint>();
                    newPoint.PreviousPoints = new List<TPoint>();
                    newPoint.Translate = pt.Translate;
                    newPoint.Scale = pt.Scale;
                    newPoint.Rotate = pt.Rotate;
                }
            }
            //Setup connections
            for (int i = 0; i < paths.Count; i++)
            {
                for (int j = 0; j < paths[i].Points.Count; j++)
                {
                    var srcPoint = (PathPointBase<CPath, CPoint>)paths[i].Points[j];
                    var dstPoint = (PathPointBase<TPath, TPoint>)_groups[i].Points[j];
                    foreach (var nextPt in srcPoint.NextPoints)
                    {
                        int groupIndex = paths.IndexOf(nextPt.Path);
                        int ptIndex = paths[groupIndex].Points.IndexOf(nextPt);
                        dstPoint.NextPoints.Add(_groups[groupIndex].Points[ptIndex]);
                    }
                    foreach (var prevPt in srcPoint.PreviousPoints)
                    {
                        int groupIndex = paths.IndexOf(prevPt.Path);
                        int ptIndex = paths[groupIndex].Points.IndexOf(prevPt);
                        dstPoint.PreviousPoints.Add(_groups[groupIndex].Points[ptIndex]);
                    }
                }
            }
            ReloadTree();
            GLContext.ActiveContext.UpdateViewport = true;
        }
    }
}
