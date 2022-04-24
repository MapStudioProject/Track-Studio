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
    public class LapPathUI
    {
        public static void Render(LapPath path)
        {
            if (path.GravityPaths == null)
                path.GravityPaths = new List<GravityPath>();

            if (ImGui.CollapsingHeader("Gravity Paths", ImGuiTreeNodeFlags.DefaultOpen))
            {
                bool add = ImGui.Button($"   {IconManager.ADD_ICON}   ");

                if (add)
                    path.GravityPaths.Add(null);

                int removeIndex = -1;
                for (int i = 0; i < path.GravityPaths.Count; i++)
                {
                    bool remove = ImGui.Button($"   {IconManager.DELETE_ICON}   ##removePath{i}");
                    ImGui.SameLine();

                    if (remove)
                        removeIndex = i;

                    var gravityPath = path.GravityPaths[i];
                    DrawLinkUI($"Path", path, i, gravityPath);
                }

                if (removeIndex != -1)
                    path.GravityPaths.RemoveAt(removeIndex);
            }
        }

        static void DrawLinkUI(string text, LapPath path, int index, GravityPath gravityPath)
        {
            ObjectLinkSelector(TranslationSource.GetText(text), path, index, gravityPath, GLContext.ActiveContext.Scene.Objects);
        }

        static bool ObjectLinkSelector(string label, LapPath lapPath, int index, GravityPath path, IEnumerable<object> drawables, EventHandler onObjectLink = null)
        {
            bool edited = false;
            string name = FindRenderName(path, drawables);

            if (ImGui.Button($"  {IconManager.EYE_DROPPER_ICON}  ##{label}{index}"))
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
                        if (tag.GetType() == typeof(GravityPath)) {
                            lapPath.GravityPaths[index] = tag as GravityPath;
                        }
                        if (tag.GetType() == typeof(GravityPathPoint)) {
                            var pt = tag as GravityPathPoint;
                            lapPath.GravityPaths[index] = pt.Path;
                        }
                    }
                };
            }
            //Set the tooltip
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(TranslationSource.GetText("EYE_DROPPER_TOOL"));

            ImGui.SameLine();
            if (ImGui.BeginCombo($"{label}##{label}{index}", name))
            {
                if (ImGui.Selectable(TranslationSource.GetText("NONE"), path == null))
                {
                    lapPath.GravityPaths[index] = null;
                    edited = true;
                }

                foreach (var render in drawables)
                {
                    if (render is CubePathRender<GravityPath, GravityPathPoint>)
                    {
                        var renderer = render as CubePathRender<GravityPath, GravityPathPoint>;
                        foreach (var group in renderer.NodeFolder.Children)
                        {
                            bool isSelected = group.Tag == path;
                            if (ImGui.Selectable($"{group.Header}", isSelected))
                            {
                                lapPath.GravityPaths[index] = group.Tag as GravityPath;
                                edited = true;
                            }
                        }
                    }
                }
                ImGui.EndCombo();
            }
            return edited;
        }

        static CubePathRender<GravityPath, GravityPathPoint> TargetRender;

        static string FindRenderName(object obj, IEnumerable<object> drawables)
        {
            if (obj == null)
                return "";

            foreach (var render in drawables)
            {
                if (render is CubePathRender<GravityPath, GravityPathPoint>)
                {
                    var renderer = render as CubePathRender<GravityPath, GravityPathPoint>;
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
