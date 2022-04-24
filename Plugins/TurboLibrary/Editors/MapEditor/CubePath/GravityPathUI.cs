using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using MapStudio.UI;
using GLFrameworkEngine;
using Toolbox.Core.ViewModels;

namespace TurboLibrary.MuuntEditor
{
    public class GravityPathUI
    {
        public static void Render(GravityPath path)
        {
            if (path.GCameraPaths == null)
                path.GCameraPaths = new List<GCameraPath>();

            if (ImGui.CollapsingHeader("GCamera Paths", ImGuiTreeNodeFlags.DefaultOpen))
            {
                bool add = ImGui.Button($"   {IconManager.ADD_ICON}   ");

                if (add)
                    path.GCameraPaths.Add(null);

                int removeIndex = -1;
                for (int i = 0; i < path.GCameraPaths.Count; i++)
                {
                    bool remove = ImGui.Button($"   {IconManager.DELETE_ICON}   ##removePath{i}");
                    ImGui.SameLine();

                    if (remove) 
                        removeIndex = i;

                     var gravityCamPath = path.GCameraPaths[i];
                    DrawLinkUI($"Path", path, i, gravityCamPath);
                }

                if (removeIndex != -1)
                    path.GCameraPaths.RemoveAt(removeIndex);
            }
        }

        static void DrawLinkUI(string text, GravityPath path, int index, GCameraPath gravityCamPath)
        {
            ObjectLinkSelector(TranslationSource.GetText(text), path, index, gravityCamPath, GLContext.ActiveContext.Scene.Objects);
        }

        static bool ObjectLinkSelector(string label, GravityPath gravityPath, int index, GCameraPath path, IEnumerable<object> drawables, EventHandler onObjectLink = null)
        {
            bool edited = false;
            string name = FindRenderName(path, drawables);

            if (ImGui.Button($"  {IconManager.EYE_DROPPER_ICON}  ##{label}"))
            {
                EventHandler handler = null;

                GLContext.ActiveContext.PickingTools.UseEyeDropper = true;
                GLContext.ActiveContext.PickingTools.OnObjectPicked += handler = (sender, e) =>
                {
                    //Only call the event once so remove it after execute
                    GLContext.ActiveContext.PickingTools.OnObjectPicked -= handler;
                    //Check if the object is a node type to find the same tag
                    //Objects match via the data that they are attached to.
                    var picked = sender as ITransformableObject;
                    if (picked is IRenderNode)
                    {
                        var tag = ((IRenderNode)picked).UINode.Tag;
                        if (tag == null || path == tag)
                            return;
                        //Make sure the tag property matches with the target property needed as linked
                        if (tag.GetType() == typeof(GCameraPath)) {
                            gravityPath.GCameraPaths[index] = tag as GCameraPath;
                        }
                        if (tag.GetType() == typeof(GCameraPathPoint)) {
                            var pt = tag as GCameraPathPoint;
                            gravityPath.GCameraPaths[index] = pt.Path;
                        }
                    }
                };
            }
            //Set the tooltip
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(TranslationSource.GetText("EYE_DROPPER_TOOL"));

            ImGui.SameLine();
            if (ImGui.BeginCombo(label, name))
            {
                if (ImGui.Selectable(TranslationSource.GetText("NONE"), path == null))
                {
                    gravityPath.GCameraPaths[index] = null;
                    edited = true;
                }

                foreach (var render in drawables)
                {
                    if (render is CubePathRender<GCameraPath, GCameraPathPoint>)
                    {
                        var renderer = render as CubePathRender<GCameraPath, GCameraPathPoint>;
                        foreach (var group in renderer.NodeFolder.Children)
                        {
                            bool isSelected = group.Tag == path;
                            if (ImGui.Selectable($"{group.Header}", isSelected))
                            {
                                gravityPath.GCameraPaths[index] = group.Tag as GCameraPath;
                                edited = true;
                            }
                        }
                    }
                }
                ImGui.EndCombo();
            }
            return edited;
        }

        static string FindRenderName(object obj, IEnumerable<object> drawables)
        {
            if (obj == null)
                return "";

            foreach (var render in drawables)
            {
                if (render is CubePathRender<GCameraPath, GCameraPathPoint>)
                {
                    var renderer = render as CubePathRender<GCameraPath, GCameraPathPoint>;
                    foreach (var group in renderer.NodeFolder.Children)
                    {
                        if (group.Tag == obj)
                            return group.Header;
                    }
                    break;
                }
            }
            return "";
        }
    }
}
