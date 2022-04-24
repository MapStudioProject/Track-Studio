using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using TurboLibrary;
using OpenTK;
using Toolbox.Core.ViewModels;

namespace TurboLibrary.MuuntEditor
{
    public class PathRender<TPath, TPoint> : RenderablePath
        where TPath : PathBase<TPath, TPoint>
        where TPoint : PathPointBase<TPath, TPoint>, new()
    {
        public override bool EditMode => true;

        public bool AutoGroup = true;

        public NodeBase NodeFolder { get; set; }

        public TPath ActiveGroup;

        private List<PathBase<TPath, TPoint>> _groups;

        public PathRender(List<PathBase<TPath, TPoint>> groups, NodeBase node)
        {
            _groups = groups;
            NodeFolder = node;
            //ConnectHoveredPoints = false;

            if (groups != null)
                ActiveGroup = _groups.FirstOrDefault() as TPath;
            else
                _groups = new List<PathBase<TPath, TPoint>>();
        }

        public List<PathBase<TPath, TPoint>> GetGroups() => _groups;

        public void AddGroup(PathBase<TPath, TPoint> group)
        {
            if (!_groups.Contains(group))
                _groups.Add(group);

            ActiveGroup = group as TPath;
        }

        public void RemoveGroup(PathBase<TPath, TPoint> group)
        {
            if (group != null && _groups.Contains(group))
                _groups.Remove(group);

            if (ActiveGroup == group)
                ActiveGroup = _groups.LastOrDefault() as TPath;
        }

        public void RemoveByGroup(PathBase<TPath, TPoint> group)
        {
            _groups.Remove(group);

            var selected = PathPoints.Where(x => ((PathPoint<TPath, TPoint>)x).Group == group).ToList();
            if (selected.Count == 0)
                return;

            GLContext.ActiveContext.Scene.AddToUndo(new RevertableDelPointCollection(selected));

            foreach (var obj in selected)
                RemovePointReferences(obj);
            foreach (var obj in selected)
                RemovePoint(obj);

            GLContext.ActiveContext.UpdateViewport = true;
        }

        public override void OnPointAdded(RenderablePathPoint point) {
            FillTree();
            this.AddGroup(((PathPoint<TPath, TPoint>)point).Group);
        }

        public override void RemovePoint(RenderablePathPoint point)
        {
            base.RemovePoint(point);
            var pt = ((PathPoint<TPath, TPoint>)point);
            //Find the group the point uses
            var groupNode = NodeFolder.Children.FirstOrDefault(x => x.Tag == pt.Group);
            if (groupNode == null) {
                RemoveGroup(pt.Group);
                return;
            }

            //Remove the point from the group
            groupNode.Children.Remove(pt.UINode);
            //Remove the group from the path
            if (groupNode.Children.Count == 0)
                NodeFolder.Children.Remove(groupNode);

            //Group no longer in use
            if (NodeFolder.Children.FirstOrDefault(x => x.Tag == pt.Group) == null)
            {
                RemoveGroup(pt.Group);
                return;
            }
        }

        public override RenderablePathPoint CreatePoint(Vector3 position)
        {
            var point = new PathPoint<TPath, TPoint>(this, position);
            point.Transform.Scale = DefaultScale;
            point.Transform.UpdateMatrix(true);
            return point;
        }

        /// <summary>
        /// Assigns groups to current points and updates the current group list
        /// </summary>
        public void UpdatePointGroups()
        {
            List<PathBase<TPath, TPoint>> groups = new List<PathBase<TPath, TPoint>>();
            foreach (PathPoint<TPath, TPoint> point in PathPoints)
            {
                //No group is assigned
                if (point.Group == null)
                {
                    //If no auto grouping is set, make sure the group choosen is the active one
                    if (!AutoGroup) {
                        if (ActiveGroup == null) //Assign a new group if one does not exist
                        {
                            point.CreateGroup();
                            ActiveGroup = point.Group;
                        }
                        point.Group = this.ActiveGroup;
                    }
                    else //Auto group based on children info
                    {
                        if (point.Parents.Count > 0 && point.Parents[0].Children.Count > 1 || point.Parents.Count == 0)
                            point.CreateGroup();
                        else
                            point.AssignParentGroup(point.Parents[0]);
                    }
                    point.OnGroupUpdated();
                }
                //Add the group to the list
                if (!groups.Contains(point.Group))
                    groups.Add(point.Group);
            }
            this._groups = groups;
        }

        public virtual void RegroupSelected(TPath group)
        {
            foreach (PathPoint<TPath, TPoint> point in GetSelectedPoints()) {
                //Get the previous group child
                var groupNode = NodeFolder.Children.FirstOrDefault(x => x.Tag == point.Group);
                groupNode.Children.Remove(point.UINode);

                point.Tag.Path = group;
                point.Group = group;
            }
            FillTree();
        }

        public void FillTree()
        {
            UpdatePointGroups();

            NodeFolder.Children.Clear();
            foreach (var group in _groups)
            {
                var groupNode = new PathWrapper(group);
                SetupGroupNode(groupNode);

                if (group == ActiveGroup)
                    groupNode.IsExpanded = true;

                NodeFolder.AddChild(groupNode);
            }

            List<TPath> groups = new List<TPath>();
            for (int i = 0; i < PathPoints.Count; i++)
            {
                var point = PathPoints[i] as PathPoint<TPath, TPoint>;
                point.UINode.Tag = point.Tag;
                point.UINode.Header = $"Point{i}";
                MapEditorIcons.ReloadIcons(point.UINode, typeof(TPath));

                if (!groups.Contains(point.Group))
                {
                    if (point.Group == null)
                        point.Group = (TPath)Activator.CreateInstance(typeof(TPath));

                    //Reset the path list
                    point.Group.Points = new List<TPoint>();
                    //Add the group instance
                    groups.Add(point.Group);
                }

                //Add the group's point
                point.Group.Points.Add(point.Tag);

                //Get the parent node
                var groupNode = NodeFolder.Children.FirstOrDefault(x => x.Tag == point.Group);

                //Set a new group wrapper if null
                if (groupNode == null)
                {
                    groupNode = new PathWrapper(point.Group);
                    SetupGroupNode(groupNode);

                    NodeFolder.AddChild(groupNode);
                    _groups.Add(point.Group);
                }
                //Add the current point to the group if it isn't there
                if (!groupNode.Children.Contains(point.UINode))
                    groupNode.AddChild(point.UINode);
            }
        }

        private void SetupGroupNode(NodeBase groupNode)
        {
            groupNode.ContextMenus.Clear();
            groupNode.ContextMenus.Add(new MenuItemModel("Delete", () =>
            {
                MapStudio.UI.UIManager.ActionExecBeforeUIDraw += delegate
                {
                    //Batch edit
                    var selected = this.NodeFolder.Children.Where(x => x.IsSelected).ToList();
                    GLContext.ActiveContext.Scene.BeginUndoCollection();
                    foreach (var path in selected)
                        this.RemoveByGroup(path.Tag as TPath);
                    GLContext.ActiveContext.Scene.EndUndoCollection();
                };
            }));
            groupNode.Header = $"Path {NodeFolder.Children.Count}";
            MapEditorIcons.ReloadIcons(groupNode, typeof(TPath));

            //Quick UI to link gravity paths to lap paths
            if (typeof(TPath) == typeof(LapPath))
            {
                groupNode.TagUI.UIDrawer += delegate
                {
                    var lapPath = groupNode.Tag as LapPath;
                    LapPathUI.Render(lapPath);
                };
            }
            if (typeof(TPath) == typeof(GravityPath))
            {
                groupNode.TagUI.UIDrawer += delegate
                {
                    var lapPath = groupNode.Tag as GravityPath;
                    GravityPathUI.Render(lapPath);
                };
            }
        }

        /// <summary>
        /// Generates a list of paths usable for byaml conversion from the current renderer.
        /// </summary>
        /// <returns></returns>
        public virtual List<PathBase<TPath, TPoint>> GeneratePaths()
        {
            List<PathBase<TPath, TPoint>> groups = new List<PathBase<TPath, TPoint>>();

            //First go through all paths and get all the groups
            for (int i = 0; i < PathPoints.Count; i++)
            {
                var point = PathPoints[i] as PathPoint<TPath, TPoint>;
                var pt = point.Tag;
                pt.Translate = new ByamlVector3F(
                    point.Transform.Position.X,
                    point.Transform.Position.Y,
                    point.Transform.Position.Z);
                pt.Scale = new ByamlVector3F(
                    point.Transform.Scale.X,
                    point.Transform.Scale.Y,
                    point.Transform.Scale.Z);
                pt.Rotate = new ByamlVector3F(
                    point.Transform.RotationEuler.X,
                    point.Transform.RotationEuler.Y,
                    point.Transform.RotationEuler.Z);

                if (!groups.Contains(point.Group))
                {
                    //Reset the path list
                    point.Group.Points = new List<TPoint>();
                    //Add the group instance
                    groups.Add(point.Group);
                }

                //Add the group's point
                point.Group.Points.Add(point.Tag);
                //Add the children's points
                point.Tag.NextPoints = new List<TPoint>();
                foreach (PathPoint<TPath, TPoint> child in point.Children)
                    point.Tag.NextPoints.Add(child.Tag);

                //Add the parent points
                point.Tag.PreviousPoints = new List<TPoint>();
                foreach (PathPoint<TPath, TPoint> child in point.Parents)
                    point.Tag.PreviousPoints.Add(child.Tag);

            }
            return groups;
        }
    }
}
