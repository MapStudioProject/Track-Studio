using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.Animations;
using Toolbox.Core;
using BfresLibrary;
using GLFrameworkEngine;
using OpenTK;
using MapStudio.UI;
using Toolbox.Core.ViewModels;
using UIFramework;

namespace CafeLibrary.Rendering
{
    public class BfresCameraAnim : CameraAnimation, IEditableAnimation
    {
        public NodeBase UINode;

        //Root for animation tree
        public TreeNode Root { get; set; } = new TreeNode();

        public ResFile ResFile;
        public CameraAnim CameraAnim;
        public SceneAnim SceneAnim;

        public BfresCameraAnim(ResFile resFile, SceneAnim sceneAnim, CameraAnim anim)
        {
            SceneAnim = sceneAnim;
            CameraAnim = anim;
            ResFile = resFile;

            UINode = new NodeBase(anim.Name);
            UINode.Tag = this;
            UINode.Icon = '\uf03d'.ToString();
            
            CanPlay = false; //Default all animations to not play unless toggled in list
            Reload(anim);

            Root = CameraAnimUI.ReloadTree(Root, this, sceneAnim);
        }

        public BfresCameraAnim(CameraAnim anim)
        {
            UINode = new NodeBase(anim.Name);
            UINode.Tag = this;

            CanPlay = false; //Default all animations to not play unless toggled in list
            Reload(anim);
        }

        public override void NextFrame(GLContext context)
        {
            var group = AnimGroups[0] as CameraAnimGroup;
            var camera = context.Camera;

            var posX = group.PositionX.GetFrameValue(this.Frame) * GLContext.PreviewScale;
            var posY = group.PositionY.GetFrameValue(this.Frame) * GLContext.PreviewScale;
            var posZ = group.PositionZ.GetFrameValue(this.Frame) * GLContext.PreviewScale;
            var rotX = group.RotationX.GetFrameValue(this.Frame);
            var rotY = group.RotationY.GetFrameValue(this.Frame);
            var rotZ = group.RotationZ.GetFrameValue(this.Frame);
            var twist = group.Twist.GetFrameValue(this.Frame);
            var near = group.ClipNear.GetFrameValue(this.Frame);
            var far = group.ClipFar.GetFrameValue(this.Frame);
            var fov = group.FieldOfView.GetFrameValue(this.Frame);
            var aspect = group.AspectRatio.GetFrameValue(this.Frame);

            camera.SetKeyframe(CameraAnimationKeys.PositionX, posX);
            camera.SetKeyframe(CameraAnimationKeys.PositionY, posY);
            camera.SetKeyframe(CameraAnimationKeys.PositionZ, posZ);
            camera.SetKeyframe(CameraAnimationKeys.Near, near);
            camera.SetKeyframe(CameraAnimationKeys.Far, far);
            camera.SetKeyframe(CameraAnimationKeys.FieldOfView, fov);
            camera.SetKeyframe(CameraAnimationKeys.Distance, 0);
            camera.RotationLookat = group.IsLookat;

            if (group.IsLookat)
            {
                camera.SetKeyframe(CameraAnimationKeys.RotationX, 0);
                camera.SetKeyframe(CameraAnimationKeys.RotationY, 0);
                camera.SetKeyframe(CameraAnimationKeys.RotationZ, 0);
                camera.SetKeyframe(CameraAnimationKeys.Twist, twist);
                //XYZ values for eye target placement
                camera.SetKeyframe(CameraAnimationKeys.TargetX, rotX * GLContext.PreviewScale);
                camera.SetKeyframe(CameraAnimationKeys.TargetY, rotY * GLContext.PreviewScale);
                camera.SetKeyframe(CameraAnimationKeys.TargetZ, rotZ * GLContext.PreviewScale);
            }
            else
            {
                camera.SetKeyframe(CameraAnimationKeys.RotationX, rotX);
                camera.SetKeyframe(CameraAnimationKeys.RotationY, rotY);
                camera.SetKeyframe(CameraAnimationKeys.RotationZ, rotZ);
                camera.SetKeyframe(CameraAnimationKeys.Twist, 0);
                camera.SetKeyframe(CameraAnimationKeys.TargetX, 0);
                camera.SetKeyframe(CameraAnimationKeys.TargetY, 0);
                camera.SetKeyframe(CameraAnimationKeys.TargetZ, 0);
            }
            GLContext.ActiveContext.UpdateViewport = true;

            camera.UpdateMatrices();
        }

        public void Reload(CameraAnim anim)
        {
            Name = anim.Name;
            FrameCount = anim.FrameCount;
            FrameRate = 60.0f;
            Loop = anim.Flags.HasFlag(CameraAnimFlags.Looping);

            CameraAnimGroup group = new CameraAnimGroup();
            group.Name = anim.Name;
            group.IsOrtho = !anim.Flags.HasFlag(CameraAnimFlags.Perspective);
            group.IsLookat = !anim.Flags.HasFlag(CameraAnimFlags.EulerZXY);

            group.PositionX.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Position.X));
            group.PositionY.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Position.Y));
            group.PositionZ.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Position.Z));
            group.RotationX.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Rotation.X));
            group.RotationY.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Rotation.Y));
            group.RotationZ.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Rotation.Z));
            group.Twist.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.Twist));
            group.ClipNear.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.ClipNear));
            group.ClipFar.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.ClipFar));
            group.AspectRatio.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.AspectRatio));
            group.FieldOfView.KeyFrames.Add(new STKeyFrame(0, anim.BaseData.FieldOfView));

            AnimGroups.Clear();
            AnimGroups.Add(group);
            for (int i = 0; i < anim.Curves.Count; i++)
            {
                var curve = anim.Curves[i];
                switch ((CameraAnimDataOffset)curve.AnimDataOffset)
                {
                    case CameraAnimDataOffset.PositionX: BfresAnimations.GenerateKeys(group.PositionX, curve); break;
                    case CameraAnimDataOffset.PositionY: BfresAnimations.GenerateKeys(group.PositionY, curve); break;
                    case CameraAnimDataOffset.PositionZ: BfresAnimations.GenerateKeys(group.PositionZ, curve); break;
                    case CameraAnimDataOffset.RotationX: BfresAnimations.GenerateKeys(group.RotationX, curve); break;
                    case CameraAnimDataOffset.RotationY: BfresAnimations.GenerateKeys(group.RotationY, curve); break;
                    case CameraAnimDataOffset.RotationZ: BfresAnimations.GenerateKeys(group.RotationZ, curve); break;
                    case CameraAnimDataOffset.Twist: BfresAnimations.GenerateKeys(group.Twist, curve); break;
                    case CameraAnimDataOffset.ClipNear: BfresAnimations.GenerateKeys(group.ClipNear, curve); break;
                    case CameraAnimDataOffset.ClipFar: BfresAnimations.GenerateKeys(group.ClipFar, curve); break;
                    case CameraAnimDataOffset.AspectRatio: BfresAnimations.GenerateKeys(group.AspectRatio, curve); break;
                    case CameraAnimDataOffset.FieldOFView: BfresAnimations.GenerateKeys(group.FieldOfView, curve); break;
                }
            }
        }

        public class CameraAnimGroup : STAnimGroup
        {
            public BfresAnimationTrack ClipNear = new BfresAnimationTrack();
            public BfresAnimationTrack ClipFar = new BfresAnimationTrack();
            public BfresAnimationTrack AspectRatio = new BfresAnimationTrack();
            public BfresAnimationTrack FieldOfView = new BfresAnimationTrack();
            public BfresAnimationTrack PositionX = new BfresAnimationTrack();
            public BfresAnimationTrack PositionY = new BfresAnimationTrack();
            public BfresAnimationTrack PositionZ = new BfresAnimationTrack();

            public BfresAnimationTrack RotationX = new BfresAnimationTrack();
            public BfresAnimationTrack RotationY = new BfresAnimationTrack();
            public BfresAnimationTrack RotationZ = new BfresAnimationTrack();

            public BfresAnimationTrack Twist = new BfresAnimationTrack();

            public bool IsLookat = false;
            public bool IsOrtho = false;
        }
    }
}
