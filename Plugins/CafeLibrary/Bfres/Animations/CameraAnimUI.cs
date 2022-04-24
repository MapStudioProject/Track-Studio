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
    class CameraAnimUI
    {
        public static TreeNode ReloadTree(TreeNode Root, BfresCameraAnim anim, SceneAnim sceneAnim)
        {
            if (sceneAnim == null)
                return null;

            Root.Header = anim.Name;
            Root.CanRename = true;
            Root.OnHeaderRenamed += delegate
            {
                string previousName = anim.UINode.Header;

                anim.UINode.Header = Root.Header;
                anim.CameraAnim.Name = Root.Header;
                if (anim.SceneAnim.CameraAnims.ContainsKey(previousName))
                {
                    anim.SceneAnim.CameraAnims.RemoveKey(previousName);
                    anim.SceneAnim.CameraAnims.Add(anim.CameraAnim.Name, anim.CameraAnim);
                }
            };
            Root.Tag = anim;
            Root.IsExpanded = true;
            Root.Icon = anim.UINode.Icon;
            Root.ContextMenus.Add(new MenuItem("Rename", () => Root.ActivateRename = true));

            SetGroupNode(Root, anim, (BfresCameraAnim.CameraAnimGroup)anim.AnimGroups[0]);

            return Root;
        }

        public static void SetGroupNode(TreeNode cam, BfresCameraAnim anim, BfresCameraAnim.CameraAnimGroup group)
        {
            cam.Tag = group;
            cam.CanRename = true;
            cam.OnHeaderRenamed += delegate
            {
                group.Name = cam.Header;
            };
            cam.ContextMenus.Add(new MenuItem(""));
            cam.ContextMenus.Add(new MenuItem("Delete", () =>
            {
                foreach (var child in cam.Children)
                {
                    if (child is AnimationTree.GroupNode)
                        ((AnimationTree.GroupNode)child).OnGroupRemoved?.Invoke(child, EventArgs.Empty);
                }

                //Remove from animation
                anim.AnimGroups.Remove(group);
                //Remove from UI
                anim.Root.Children.Remove(cam);
            }));

            if (!anim.CameraAnim.Flags.HasFlag(CameraAnimFlags.EulerZXY))
            {
                cam.AddChild(GetTrackNode(anim, group.PositionX, "PositionX"));
                cam.AddChild(GetTrackNode(anim, group.PositionY, "PositionY"));
                cam.AddChild(GetTrackNode(anim, group.PositionZ, "PositionZ"));
                cam.AddChild(GetTrackNode(anim, group.RotationX, "LookAtX"));
                cam.AddChild(GetTrackNode(anim, group.RotationY, "LookAtY"));
                cam.AddChild(GetTrackNode(anim, group.RotationZ, "LookAtZ"));
                cam.AddChild(GetTrackNode(anim, group.Twist, "Twist"));
            }
            else
            {
                cam.AddChild(GetTrackNode(anim, group.PositionX, "PositionX"));
                cam.AddChild(GetTrackNode(anim, group.PositionY, "PositionY"));
                cam.AddChild(GetTrackNode(anim, group.PositionZ, "PositionZ"));
                cam.AddChild(GetTrackNode(anim, group.RotationX, "RotationX"));
                cam.AddChild(GetTrackNode(anim, group.RotationY, "RotationY"));
                cam.AddChild(GetTrackNode(anim, group.RotationZ, "RotationZ"));
            }
            cam.AddChild(GetTrackNode(anim, group.FieldOfView, "FieldOfView"));
            cam.AddChild(GetTrackNode(anim, group.ClipNear, "ClipNear"));
            cam.AddChild(GetTrackNode(anim, group.ClipFar, "ClipFar"));
        }

        static AnimationTree.TrackNode GetTrackNode(BfresCameraAnim anim, BfresAnimationTrack track, string name)
        {
            track.Name = name;

            var trackNode = new AnimationTree.TrackNode(anim, track);
            trackNode.Tag = track;
            trackNode.Icon = '\uf1b2'.ToString();
            return trackNode;
        }
    }
}
