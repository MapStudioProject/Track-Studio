using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.Animations;
using BfresLibrary;
using CafeLibrary.Rendering;

namespace TurboLibrary.Actors
{
    public class MapObjItemBox : ActorModelBase
    {
        BfresSkeletalAnim IdleAnimation { get; set; }

        public MapObjItemBox()
        {

        }

        public override void Init()
        {
            if (Render == null) return;

            //Add an addtional hardcoded skeletal animation for idle animations
           // IdleAnimation = CreateItemBoxIdle(Render.Name);
          //  ToggleMeshes(Render);
        }

        private void ToggleMeshes(BfresRender render)
        {
            foreach (var model in render.Models)
            {
                foreach (BfresMeshRender mesh in model.MeshList)
                {
                    if (mesh.Name.StartsWith("DoubleItemBox"))
                        mesh.IsVisible = false;
                }
            }
        }

        public override List<STAnimation> GetAnimations()
        {
            List<STAnimation> animations = new List<STAnimation>();
            if (Render == null) return animations;

            animations.Add(IdleAnimation);
            animations.AddRange(Render.SkeletalAnimations);
            animations.AddRange(Render.MaterialAnimations);
            return animations;
        }

        public override void ResetAnimation() {
            if (Render == null) return;

            Render.ResetAnim();
        }

        public static BfresSkeletalAnim CreateItemBoxIdle(string modelName)
        {
            var ska = new SkeletalAnim();
            ska.Name = "Idle";
            ska.FrameCount = 240;
            ska.FlagsAnimSettings = SkeletalAnimFlags.Looping;
            ska.FlagsRotate = SkeletalAnimFlagsRotate.EulerXYZ;

            //Here we will animate 2 bones, "ItemBox" and "ItemBoxFont"

            float angle = OpenTK.MathHelper.DegreesToRadians(0);
            float full = OpenTK.MathHelper.DegreesToRadians(360);

            var boneAnim = new BoneAnim();
            boneAnim.Name = "ItemBox";
            boneAnim.FlagsBase |= BoneAnimFlagsBase.Translate;
            boneAnim.FlagsBase |= BoneAnimFlagsBase.Scale;
            boneAnim.FlagsBase |= BoneAnimFlagsBase.Rotate;
            boneAnim.BaseData = new BoneAnimData()
            {
                Translate = new Syroot.Maths.Vector3F(0, 20, 0),
                Rotate = new Syroot.Maths.Vector4F(angle, 0, 0, 1),
                Scale = new Syroot.Maths.Vector3F(1, 1, 1),
            };

            //The item box rotates
            float[] rotationXValues = new float[2] { angle, full };
            float[] rotationYValues = new float[2] { angle, full };
            float[] rotationZValues = new float[2] { angle, angle };

            float[] rotFrames = new float[2] { 0, 240 };

            boneAnim.Curves.Add(BfresAnimations.CreateLinearCurve(rotFrames, rotationXValues, (int)BfresSkeletalAnim.TrackType.XROT));
            boneAnim.Curves.Add(BfresAnimations.CreateLinearCurve(rotFrames, rotationYValues, (int)BfresSkeletalAnim.TrackType.YROT));
            boneAnim.Curves.Add(BfresAnimations.CreateLinearCurve(rotFrames, rotationZValues, (int)BfresSkeletalAnim.TrackType.ZROT));
            ska.BoneAnims.Add(boneAnim);

            boneAnim = new BoneAnim();
            boneAnim.Name = "ItemBoxFont";
            boneAnim.FlagsBase |= BoneAnimFlagsBase.Translate;
            boneAnim.FlagsBase |= BoneAnimFlagsBase.Scale;
            boneAnim.FlagsBase |= BoneAnimFlagsBase.Rotate;
            boneAnim.BaseData = new BoneAnimData()
            {
                Translate = new Syroot.Maths.Vector3F(0, 20, 0),
                Rotate = new Syroot.Maths.Vector4F(0, 0, 0, 1),
                Scale = new Syroot.Maths.Vector3F(1, 1, 1),
            };

            //The item box font scales
            float[] scaleValues = new float[3] { 1, 0.7f, 1 };
            float[] scaleFrames = new float[3] { 0, 30, 60 };

            boneAnim.Curves.Add(BfresAnimations.CreateLinearCurve(scaleFrames, scaleValues, (int)BfresSkeletalAnim.TrackType.XSCA));
            boneAnim.Curves.Add(BfresAnimations.CreateLinearCurve(scaleFrames, scaleValues, (int)BfresSkeletalAnim.TrackType.YSCA));
            boneAnim.Curves.Add(BfresAnimations.CreateLinearCurve(scaleFrames, scaleValues, (int)BfresSkeletalAnim.TrackType.ZSCA));
            ska.BoneAnims.Add(boneAnim);

            BfresSkeletalAnim anim = new BfresSkeletalAnim(ska, modelName);
            anim.NextFrame();
            return anim;
        }
    }
}
