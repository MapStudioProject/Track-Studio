using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.Animations;
using BfresLibrary;
using BfresLibrary.Helpers;
using Syroot.Maths;
using CafeLibrary.Rendering;

namespace CafeLibrary
{
    public class SkeletalAnimConverter
    {
        public static SkeletalAnim ConvertAnimation(BfresSkeletalAnim anim)
        {
            SkeletalAnim fska = new SkeletalAnim();
            ConvertAnimation(anim, fska);
            return fska;
        }

        public static void ConvertAnimation(BfresSkeletalAnim anim, SkeletalAnim target)
        {
            //Use helper classes to generate skeletal anim
            SkeletalAnimHelper animation = new SkeletalAnimHelper();
            animation.Name = anim.Name;
            animation.Path = "";
            animation.FrameCount = (int)anim.FrameCount;
            animation.Baked = false;
            animation.FlagsRotate = SkeletalAnimFlagsRotate.EulerXYZ;
            animation.FlagsScale = SkeletalAnimFlagsScale.Maya;
            animation.BoneAnims = new List<BoneAnimHelper>();
            animation.UseDegrees = false;
            foreach (BfresSkeletalAnim.BoneAnimGroup group in anim.AnimGroups)
            {
                var boneAnim = new BoneAnimHelper();
                boneAnim.Name = group.Name;
                animation.BoneAnims.Add(boneAnim);
                boneAnim.SegmentScaleCompensate = group.UseSegmentScaleCompensate;

                //var matrix = group.BaseMatrix;
                var pos = new Vector3F(
                    group.Translate.X.GetFrameValue(0),
                    group.Translate.Y.GetFrameValue(0),
                    group.Translate.Z.GetFrameValue(0));
                var rot = new Vector4F(
                    group.Rotate.X.GetFrameValue(0),
                    group.Rotate.Y.GetFrameValue(0),
                    group.Rotate.Z.GetFrameValue(0),
                    group.Rotate.W.HasKeys ? group.Rotate.W.GetFrameValue(0) : 1.0f);
                var scale = new Vector3F(
                    group.Scale.X.HasKeys ? group.Scale.X.GetFrameValue(0) : 1.0f,
                    group.Scale.Y.HasKeys ? group.Scale.Y.GetFrameValue(0) : 1.0f,
                    group.Scale.Z.HasKeys ? group.Scale.Z.GetFrameValue(0) : 1.0f);

                boneAnim.BaseData = new BaseDataHelper()
                {
                    Translate = pos,
                    Rotate = rot,
                    Scale = scale,
                };

                boneAnim.UseBaseRotation = true;
                boneAnim.UseBaseScale = true;
                boneAnim.UseBaseTranslation = true;
                boneAnim.Curves = new List<CurveAnimHelper>();

                if (group.Translate.X.KeyFrames.Count > 1) boneAnim.Curves.Add(ConvertCurve(group.Translate.X, "PositionX"));
                if (group.Translate.Y.KeyFrames.Count > 1) boneAnim.Curves.Add(ConvertCurve(group.Translate.Y, "PositionY"));
                if (group.Translate.Z.KeyFrames.Count > 1) boneAnim.Curves.Add(ConvertCurve(group.Translate.Z, "PositionZ"));

                if (group.Rotate.X.KeyFrames.Count > 1) boneAnim.Curves.Add(ConvertCurve(group.Rotate.X, "RotateX"));
                if (group.Rotate.Y.KeyFrames.Count > 1) boneAnim.Curves.Add(ConvertCurve(group.Rotate.Y, "RotateY"));
                if (group.Rotate.Z.KeyFrames.Count > 1) boneAnim.Curves.Add(ConvertCurve(group.Rotate.Z, "RotateZ"));
                if (group.Rotate.W.KeyFrames.Count > 1) boneAnim.Curves.Add(ConvertCurve(group.Rotate.W, "RotateW"));

                if (group.Scale.X.KeyFrames.Count > 1) boneAnim.Curves.Add(ConvertCurve(group.Scale.X, "ScaleX"));
                if (group.Scale.Y.KeyFrames.Count > 1) boneAnim.Curves.Add(ConvertCurve(group.Scale.Y, "ScaleY"));
                if (group.Scale.Z.KeyFrames.Count > 1) boneAnim.Curves.Add(ConvertCurve(group.Scale.Z, "ScaleZ"));
            }
            SkeletalAnimHelper.FromStruct(target, animation);
        }

        static CurveAnimHelper ConvertCurve(BfresAnimationTrack track, string target)
        {
            CurveAnimHelper curve = new CurveAnimHelper();
            curve.KeyFrames = new Dictionary<float, object>();
            curve.Scale = track.Scale;
            curve.Offset = track.Offset;
            curve.Target = target;
            curve.KeyType = track.KeyType;
            curve.FrameType = track.FrameType;

            if (track.InterpolationType == STInterpoaltionType.Hermite)
            {
                curve.Interpolation = AnimCurveType.Cubic;
                foreach (STKeyFrame key in track.KeyFrames)
                {
                    if (key is STHermiteKeyFrame)
                    {
                        curve.KeyFrames.Add(key.Frame, new HermiteKey()
                        {
                            In = ((STHermiteKeyFrame)key).TangentIn,
                            Out = ((STHermiteKeyFrame)key).TangentOut,
                            Value = (float)key.Value,
                        });
                    }
                    else
                    {
                        curve.KeyFrames.Add(key.Frame, new HermiteKey()
                        {
                            Value = (float)key.Value,
                        });
                    }
                }
            }
            else
            {
                curve.Interpolation = AnimCurveType.Linear;
                foreach (STKeyFrame key in track.KeyFrames)
                {
                    curve.KeyFrames.Add(key.Frame, new KeyFrame()
                    {
                        Value = (float)key.Value,
                    });
                }
            }

            //Get max frame value
            float frame = curve.KeyFrames.Max(x => x.Key);
            if (frame < byte.MaxValue) curve.FrameType = AnimCurveFrameType.Byte;
            else if (frame < ushort.MaxValue) curve.FrameType = AnimCurveFrameType.Decimal10x5;

            return curve;
        }

        public enum AnimTarget
        {
            ScaleX = 0x4,
            ScaleY = 0x8,
            ScaleZ = 0xC,
            PositionX = 0x10,
            PositionY = 0x14,
            PositionZ = 0x18,
            RotateX = 0x20,
            RotateY = 0x24,
            RotateZ = 0x28,
            RotateW = 0x2C,
        }
    }
}
