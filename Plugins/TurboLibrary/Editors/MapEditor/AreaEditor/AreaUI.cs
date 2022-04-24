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
    public class AreaUI
    {
        public static void Render(Area area)
        {
            if (area.ReplayCameras == null)
                area.ReplayCameras = new List<ReplayCamera>();

            if (ImGui.CollapsingHeader("Replay Cameras", ImGuiTreeNodeFlags.DefaultOpen))
            {
                bool add = ImGui.Button($"   {IconManager.ADD_ICON}   ");

                if (add)
                    area.ReplayCameras.Add(null);

                int removeIndex = -1;
                for (int i = 0; i < area.ReplayCameras.Count; i++)
                {
                    bool remove = ImGui.Button($"   {IconManager.DELETE_ICON}   ##removeCamera{i}");
                    ImGui.SameLine();

                    if (remove) 
                        removeIndex = i;

                     var camera = area.ReplayCameras[i];
                    DrawLinkUI($"Camera##camSelect{i}", area, i, camera);
                }

                if (removeIndex != -1)
                    area.ReplayCameras.RemoveAt(removeIndex);
            }
        }

        static void DrawLinkUI(string text, Area area, int index, ReplayCamera camera)
        {
            ObjectLinkSelector(TranslationSource.GetText(text), area, index, camera, GLContext.ActiveContext.Scene.Objects);
        }

        static bool ObjectLinkSelector(string label, Area area, int index, ReplayCamera camera, IEnumerable<object> drawables, EventHandler onObjectLink = null)
        {
            bool edited = false;
            string name = FindRenderName(camera, drawables);

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
                        if (tag == null || camera == tag)
                            return;
                        //Make sure the tag property matches with the target property needed as linked
                        if (tag.GetType() == typeof(ReplayCamera))
                        {
                            area.ReplayCameras[index] = tag as ReplayCamera;
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
                if (ImGui.Selectable(TranslationSource.GetText("NONE"), camera == null))
                {
                    area.ReplayCameras[index] = null;
                    edited = true;
                }

                foreach (var render in drawables)
                {
                    if (render is IRenderNode && ((IRenderNode)render).UINode.Tag is ReplayCamera)
                    {
                        var node = ((IRenderNode)render).UINode;
                        bool isSelected = node.Tag == camera;
                        if (ImGui.Selectable($"{node.Header}", isSelected))
                        {
                            area.ReplayCameras[index] = node.Tag as ReplayCamera;
                            edited = true;
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
                if (render is IRenderNode && ((IRenderNode)render).UINode.Tag == obj)
                    return ((IRenderNode)render).UINode.Header;
            }
            return "";
        }
    }
}
