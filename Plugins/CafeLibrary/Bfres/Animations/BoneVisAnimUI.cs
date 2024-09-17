using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIFramework;
using BfresLibrary;
using CafeLibrary.Rendering;
using MapStudio.UI;
using Toolbox.Core.Animations;
using ImGuiNET;
using static MapStudio.UI.AnimationTree;
using System.Numerics;
using static GLFrameworkEngine.RenderablePath;

namespace CafeLibrary
{
    class BoneVisAnimUI
    {
        public static TreeNode ReloadTree(TreeNode Root, BfresVisibilityAnim anim, VisibilityAnim visAnim)
        {
            if (visAnim == null)
                return null;

            Root.Header = anim.Name;
            Root.CanRename = true;
            Root.OnHeaderRenamed += delegate
            {
                string previousName = anim.UINode.Header;

                anim.UINode.Header = Root.Header;
                anim.VisibilityAnim.Name = Root.Header;
            };
            Root.Tag = anim;
            Root.IsExpanded = true;
            Root.Icon = anim.UINode.Icon;
            Root.IconColor = anim.UINode.IconColor;
            Root.ContextMenus.Add(new MenuItem("Rename", () => Root.ActivateRename = true));
            Root.ContextMenus.Add(new MenuItem("Add Bone", () =>
            {
                var boneGroup = new BfresVisibilityAnim.BoneAnimGroup();
                boneGroup.Name = "NewBone";
                boneGroup.Track.InterpolationType = STInterpoaltionType.Step;
                boneGroup.Track.Name = boneGroup.Name;
                anim.AnimGroups.Add(boneGroup);
                //Add to ui
                Root.AddChild(GetGroupNode(anim, boneGroup));
                anim.IsEdited = true;
            }));

            foreach (BfresVisibilityAnim.BoneAnimGroup group in anim.AnimGroups)
                Root.AddChild(GetGroupNode(anim, group));

            return Root;
        }

        public static TreeNode GetGroupNode(BfresVisibilityAnim anim, BfresVisibilityAnim.BoneAnimGroup group)
        {
            BoneVisTrack boneNode = new BoneVisTrack(anim, group.Track);
            boneNode.Header = group.Name;
            boneNode.IsExpanded = false;
            boneNode.Tag = group.Track;
            boneNode.Icon = MapStudio.UI.IconManager.BONE_ICON.ToString();
            boneNode.CanRename = true;
            boneNode.OnHeaderRenamed += delegate
            {
                //not changed
                if (group.Name == boneNode.Header)
                    return;

                //Dupe name
                if (anim.AnimGroups.Any(x => x.Name == boneNode.Header))
                {
                    TinyFileDialog.MessageBoxErrorOk($"Name {boneNode.Header} already exists!");
                    //revert
                    boneNode.Header = group.Name;
                    return;
                }

                group.Name = boneNode.Header;
                group.Track.Name = boneNode.Header;
            };
            boneNode.ContextMenus.Add(new MenuItem("Rename", () => boneNode.ActivateRename = true));
            boneNode.ContextMenus.Add(new MenuItem(""));
            boneNode.ContextMenus.Add(new MenuItem("Delete", () =>
            {
                //Remove from animation
                anim.AnimGroups.Remove(group);
                //Remove from UI
                boneNode.Parent.Children.Remove(boneNode);
            }));
            return boneNode;
        }

        public class BoneVisTrack : AnimationTree.TrackNode
        {
            public BoneVisTrack(STAnimation anim, STAnimationTrack track) : base(anim, track)
            {
            }

            public override void RenderNode()
            {
                ImGui.Text(this.Header);

                ImGui.NextColumn();

                var color = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
                //Display keyed values differently
                bool isKeyed = Track.KeyFrames.Any(x => x.Frame == Anim.Frame);
           
                ImGui.PushStyleColor(ImGuiCol.Text, color);

                //Display the current track value
                bool value = Track.GetFrameValue(Anim.Frame) != 0;
                //Span the whole column
                ImGui.PushItemWidth(ImGui.GetColumnWidth() - 3);
                bool edited = ImGui.Checkbox($"##{Track.Name}_frame", ref value);
                bool isActive = ImGui.IsItemDeactivated();

                ImGui.PopItemWidth();

                //Insert key value from current frame
                if (edited || (isActive && ImGui.IsKeyDown((int)ImGuiKey.Enter)))
                    InsertOrUpdateKeyValue(value ? 1f : 0f);

                ImGui.PopStyleColor();

                ImGui.NextColumn();
            }

            public override void RenderKeyTableUI()
            {
                ImGui.BeginColumns("##keyList", 2);
                for (int i = 0; i < Keys.Count; i++)
                {
                    float frame = Keys[i].Frame;
                    bool value = Keys[i].KeyFrame.Value == 0;

                    bool edited = false;
                    edited |= ImGui.DragFloat($"Frame##{i}", ref frame, 1, 0, Anim.FrameCount);
                    ImGui.NextColumn();

                    edited |= ImGui.Checkbox($"Value##{i}", ref value);
                    ImGui.NextColumn();

                    if (edited)
                    {
                        bool isFrameUpdated = Keys[i].Frame != frame;

                        Keys[i].Frame = frame;
                        Keys[i].KeyFrame.Value = value ? 0 : 1;

                       // if (isFrameUpdated)
                          //  OnFrameUpdated();
                    }
                }
                ImGui.EndColumns();
            }
        }
    }
}
