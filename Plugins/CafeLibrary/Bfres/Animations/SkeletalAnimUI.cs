using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIFramework;
using BfresLibrary;
using CafeLibrary.Rendering;
using MapStudio.UI;

namespace CafeLibrary
{
    class SkeletalAnimUI
    {
        public static TreeNode ReloadTree(TreeNode Root, BfresSkeletalAnim anim, ResFile resFile)
        {
            if (resFile == null)
                return null;

            Root.Icon = anim.UINode.Icon;
            Root.Header = anim.Name;
            Root.CanRename = true;
            Root.OnHeaderRenamed += delegate
            {
                anim.UINode.Header = Root.Header;
                anim.OnRenamed(Root.Header);
            };
            Root.Tag = anim;
            Root.IsExpanded = true;
            Root.ContextMenus.Add(new MenuItem("Add Bone", () =>
            {
                var boneGroup = new BfresSkeletalAnim.BoneAnimGroup();
                boneGroup.Name = "NewBone";
                anim.AnimGroups.Add(boneGroup);
                //Add to ui
                Root.AddChild(GetGroupNode(anim, boneGroup));
                anim.IsEdited = true;
            }));
            Root.ContextMenus.Add(new MenuItem("Rename", () => Root.ActivateRename = true));

            foreach (BfresSkeletalAnim.BoneAnimGroup group in anim.AnimGroups)
                Root.AddChild(GetGroupNode(anim, group));

            return Root;
        }

        public static TreeNode GetGroupNode(BfresSkeletalAnim anim, BfresSkeletalAnim.BoneAnimGroup group)
        {
            TreeNode boneNode = new TreeNode(group.Name);
            boneNode.IsExpanded = false;
            boneNode.Tag = group;
            boneNode.Icon = MapStudio.UI.IconManager.BONE_ICON.ToString();
            boneNode.CanRename = true;
            boneNode.OnHeaderRenamed += delegate
            {
                group.Name = boneNode.Header;
            };
            boneNode.ContextMenus.Add(new MenuItem("Rename", () => boneNode.ActivateRename = true));
            boneNode.ContextMenus.Add(new MenuItem(""));
            boneNode.ContextMenus.Add(new MenuItem("Delete", () =>
            {
                foreach (var child in boneNode.Children)
                {
                    if (child is AnimationTree.GroupNode)
                        ((AnimationTree.GroupNode)child).OnGroupRemoved?.Invoke(child, EventArgs.Empty);
                }

                //Remove from animation
                anim.AnimGroups.Remove(group);
                //Remove from UI
                anim.Root.Children.Remove(boneNode);
            }));

            boneNode.AddChild(GetTrackNode(anim, group.Translate.X, "Translate.X"));
            boneNode.AddChild(GetTrackNode(anim, group.Translate.Y, "Translate.Y"));
            boneNode.AddChild(GetTrackNode(anim, group.Translate.Z, "Translate.Z"));

            boneNode.AddChild(GetTrackNode(anim, group.Rotate.X, "Rotate.X", true));
            boneNode.AddChild(GetTrackNode(anim, group.Rotate.Y, "Rotate.Y", true));
            boneNode.AddChild(GetTrackNode(anim, group.Rotate.Z, "Rotate.Z", true));

            boneNode.AddChild(GetTrackNode(anim, group.Scale.X, "Scale.X"));
            boneNode.AddChild(GetTrackNode(anim, group.Scale.Y, "Scale.Y"));
            boneNode.AddChild(GetTrackNode(anim, group.Scale.Z, "Scale.Z"));

            if (group.Translate.GetTracks().Any(x => x.KeyFrames.Count > 1))
            {
    
            }
            if (group.Rotate.GetTracks().Any(x => x.KeyFrames.Count > 1))
            {
            
            }
            if (group.Scale.GetTracks().Any(x => x.KeyFrames.Count > 1))
            {

            }
            return boneNode;
        }

        static AnimationTree.TrackNode GetTrackNode(BfresSkeletalAnim anim, BfresAnimationTrack track, string name, bool degrees = false)
        {
            track.Name = name;
            if (degrees)
                return new AnimationTree.TrackNodeDegreesConversion(anim, track) { Tag = track, Icon = '\uf1b2'.ToString() };
            else
                return new AnimationTree.TrackNode(anim, track) { Tag = track, Icon = '\uf1b2'.ToString() };
        }

        class DegreesTrackNode
        {

        }
    }
}
