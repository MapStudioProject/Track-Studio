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
    public class ClipEditor
    {
        public static NodeBase Setup(CourseDefinition course, ClipAreaEditor clipAreaEditor)
        {
            NodeBase clipsFolder = new NodeBase("Clips");

            //Setup clip tree
            clipsFolder.Icon = '\uf0c4'.ToString();
            //Adding clip
            clipsFolder.ContextMenus.Add(new MenuItemModel(TranslationSource.GetText("ADD"), () =>
            {
                //Prepare new clip instance if not present
                if (course.ClipPattern == null)
                    course.ClipPattern = new ClipPattern();

                //Add a new clip
                var clip = new AreaFlag();
                course.ClipPattern.AreaFlag.Add(clip);
                //Add to tree/ui
                ClipEditor.AddNode(clipsFolder, clip, course, clipAreaEditor);
            }));
            if (course.ClipPattern != null)
            {
                //Add to tree/ui
                foreach (var clip in course.ClipPattern.AreaFlag)
                    ClipEditor.AddNode(clipsFolder, clip, course, clipAreaEditor);
            }
            return clipsFolder;
        }

        public static void AddNode(NodeBase parent, Clip clip, CourseDefinition course, ClipAreaEditor clipAreaEditor)
        {
            var clipNode = new NodeBase($"Clip");
            //Clip display name
            clipNode.GetHeader += delegate
            {
                return $"Clip {clipNode.Index}";
            };
            //Clip remove
            clipNode.ContextMenus.Add(new MenuItemModel(TranslationSource.GetText("REMOVE"), () =>
            {
                parent.Children.Remove(clipNode);
                course.ClipPattern.AreaFlag.Remove((AreaFlag)clip);
            }));
            clipNode.Icon = parent.Icon;
            clipNode.Tag = clip;
            clipNode.TagUI.UIDrawer += delegate
            {
                Render(clip);
            };
            clipNode.OnSelected += delegate
            {
                if (!clipNode.IsSelected)
                {
                    MapFieldAccessor.Instance.ClipIndex = -1;
                    GLContext.ActiveContext.UpdateViewport = true;
                    //Disable sub mesh drawing
                    CafeLibrary.Rendering.BfresMeshRender.DISPLAY_SUB_MESH = false;
                    return;
                }

                //Update active clip index for cull viewing
                var clipData = TurboSystem.Instance.MapFieldAccessor.ClipAreaAccessor;
                MapFieldAccessor.Instance.ClipIndex = clipNode.Index;
                //Enable sub mesh drawing to view culling
                CafeLibrary.Rendering.BfresMeshRender.DISPLAY_SUB_MESH = true;

                foreach (var area in clipAreaEditor.Renderers)
                {
                    area.IsVisible = false;
                    if (clip.Areas.Contains(((IRenderNode)area).UINode.Tag))
                    {
                        ((ITransformableObject)area).CanSelect = true;
                        area.IsVisible = true;
                    }
                }
            };
            parent.AddChild(clipNode);
        }

        public static void Render(Clip clip)
        {
            if (ImGui.CollapsingHeader("Clip Areas", ImGuiTreeNodeFlags.DefaultOpen))
            {
                bool add = ImGui.Button($"   {IconManager.ADD_ICON}   ");

                if (add)
                    clip.Areas.Add(null);

                int removeIndex = -1;
                for (int i = 0; i < clip.Areas.Count; i++)
                {
                    bool remove = ImGui.Button($"   {IconManager.DELETE_ICON}   ##removeCamera{i}");
                    ImGui.SameLine();

                    if (remove)
                        removeIndex = i;

                    var camera = clip.Areas[i];
                    DrawLinkUI($"Area##areaSelect{i}", clip, i, camera);
                }

                if (removeIndex != -1)
                    clip.Areas.RemoveAt(removeIndex);
            }
        }

        static void DrawLinkUI(string text, Clip clip, int index, ClipArea area)
        {
            ObjectLinkSelector(TranslationSource.GetText(text), clip, index, area, GLContext.ActiveContext.Scene.Objects);
        }

        static bool ObjectLinkSelector(string label, Clip clip, int index, ClipArea area, IEnumerable<object> drawables, EventHandler onObjectLink = null)
        {
            bool edited = false;
            string name = FindRenderName(area, drawables);

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
                        if (tag == null || area == tag)
                            return;
                        //Make sure the tag property matches with the target property needed as linked
                        if (tag.GetType() == typeof(ClipArea))
                        {
                            clip.Areas[index] = tag as ClipArea;
                            clip.IsEdited = true;
                            ((IDrawable)picked).IsVisible = true;
                            ((ITransformableObject)picked).CanSelect = true;
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
                if (ImGui.Selectable(TranslationSource.GetText("NONE"), area == null))
                {
                    clip.Areas[index] = null;
                    clip.IsEdited = true;
                    edited = true;
                }

                foreach (var render in drawables)
                {
                    if (render is IRenderNode && ((IRenderNode)render).UINode.Tag is ClipArea)
                    {
                        var node = ((IRenderNode)render).UINode;
                        bool isSelected = node.Tag == area;
                        if (ImGui.Selectable($"{node.Header}", isSelected))
                        {
                            clip.Areas[index] = node.Tag as ClipArea;
                            clip.IsEdited = true;
                            ((IDrawable)render).IsVisible = true;
                            ((ITransformableObject)render).CanSelect = true;
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