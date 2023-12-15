using IONET.Core.Animation;
using IONET.Core.Model;
using IONET.Core.Skeleton;
using IONET.Core;
using IONET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfresLibrary;
using System.IO;
using System.Numerics;
using Toolbox.Core;

namespace CafeLibrary
{
    public class SkeletonAnimImporter
    {
        public class Settings
        {
            public bool Loop;
        }

        public static SkeletalAnim Import(STSkeleton skeleton, string filePath, Settings settings)
        {
            SkeletalAnim anim = new SkeletalAnim();
            anim.Name = Path.GetFileNameWithoutExtension(filePath);
            anim.Loop = settings.Loop;
            anim.FlagsScale = SkeletalAnimFlagsScale.Standard;
            anim.FlagsRotate = SkeletalAnimFlagsRotate.EulerXYZ;

            Import(anim, skeleton, filePath, settings);
            return anim;
        }

        public static void Import(SkeletalAnim fskaAnimation, STSkeleton skeleton, string filePath, Settings settings)
        {
            var scene = IOManager.LoadScene(filePath, new ImportSettings());
            var ioanim = scene.Animations.FirstOrDefault();
            if (ioanim == null)
                throw new Exception($"Failed to find animation in file!");

            //Set frame count either by the set end frame or a calulated frame counter from the key data
            fskaAnimation.FrameCount = (int)(ioanim.EndFrame != 0 ? ioanim.EndFrame : ioanim.GetFrameCount());
            fskaAnimation.BoneAnims.Clear();
            foreach (var anim in ioanim.Groups)
                fskaAnimation.BoneAnims.Add(CreateBoneAnim(anim, skeleton, settings));

            //Sort by traverse order, just to match more accurately
            Dictionary<string, int> boneOrder = new Dictionary<string, int>();

            void TraverseBones(STBone bone)
            {
                //Add bone to traverse order
                if (!boneOrder.ContainsKey(bone.Name))
                    boneOrder.Add(bone.Name, boneOrder.Count);
                //Go through each child
                foreach (var child in bone.Children)
                    TraverseBones(child);   
            }
            //Get traversal order starting from root
            foreach (var bone in skeleton.Bones.Where(x => x.Parent == null))
                    TraverseBones(bone);

            int GetOrderIndex(BoneAnim boneAnim) =>
                boneOrder.ContainsKey(boneAnim.Name) ? boneOrder[boneAnim.Name] : -1;

            fskaAnimation.BoneAnims = fskaAnimation.BoneAnims.OrderBy(x => GetOrderIndex(x)).ToList();


            //Bind indices that are set at runtime. 
            fskaAnimation.BindIndices = new ushort[fskaAnimation.BoneAnims.Count];
            for (int i = 0; i < fskaAnimation.BindIndices.Length; i++)
                fskaAnimation.BindIndices[i] = ushort.MaxValue;
        }

        static BoneAnim CreateBoneAnim(IOAnimation animation, STSkeleton skeleton, Settings settings)
        {
            //bone animation for bfres
            BoneAnim boneAnim = new BoneAnim();
            boneAnim.Name = animation.Name;

            //base defaults
            var basePosition = Syroot.Maths.Vector3F.Zero;
            var baseRotation = new Syroot.Maths.Vector4F(0, 0, 0, 1f);
            var baseScale = Syroot.Maths.Vector3F.One;

            //Set base pose
            //This is only used to set in base positions for existing keys
            var bone = skeleton.SearchBone(boneAnim.Name);
            if (bone != null)
            {
                basePosition = new Syroot.Maths.Vector3F(bone.Position.X, bone.Position.Y, bone.Position.Z);
                baseRotation = new Syroot.Maths.Vector4F(bone.EulerRotation.X, bone.EulerRotation.Y, bone.EulerRotation.Z, 1f);
                baseScale = new Syroot.Maths.Vector3F(bone.Scale.X, bone.Scale.Y, bone.Scale.Z);
            }

            //Set curves
            foreach (var track in animation.Tracks)
            {
                //insert into base pos instead if 1 frame
                if (track.KeyFrames.Count >= 1)
                {
                    switch (track.ChannelType)
                    {
                        case IOAnimationTrackType.PositionX: basePosition.X = track.KeyFrames[0].ValueF32; break;
                        case IOAnimationTrackType.PositionY: basePosition.Y = track.KeyFrames[0].ValueF32; break;
                        case IOAnimationTrackType.PositionZ: basePosition.Z = track.KeyFrames[0].ValueF32; break;
                        case IOAnimationTrackType.RotationEulerX: baseRotation.X = track.KeyFrames[0].ValueF32; break;
                        case IOAnimationTrackType.RotationEulerY: baseRotation.Y = track.KeyFrames[0].ValueF32; break;
                        case IOAnimationTrackType.RotationEulerZ: baseRotation.Z = track.KeyFrames[0].ValueF32; break;
                        case IOAnimationTrackType.ScaleX: baseScale.X = track.KeyFrames[0].ValueF32; break;
                        case IOAnimationTrackType.ScaleY: baseScale.Y = track.KeyFrames[0].ValueF32; break;
                        case IOAnimationTrackType.ScaleZ: baseScale.Z = track.KeyFrames[0].ValueF32; break;
                    }

                    //base flags
                    switch (track.ChannelType)
                    {
                        case IOAnimationTrackType.PositionX:
                        case IOAnimationTrackType.PositionY: 
                        case IOAnimationTrackType.PositionZ:
                            boneAnim.FlagsBase |= BoneAnimFlagsBase.Translate;
                            break;
                        case IOAnimationTrackType.RotationEulerX:
                        case IOAnimationTrackType.RotationEulerY:
                        case IOAnimationTrackType.RotationEulerZ:
                            boneAnim.FlagsBase |= BoneAnimFlagsBase.Rotate;
                            break;
                        case IOAnimationTrackType.ScaleX:
                        case IOAnimationTrackType.ScaleY:
                        case IOAnimationTrackType.ScaleZ:
                            boneAnim.FlagsBase |= BoneAnimFlagsBase.Scale;
                            break;
                    }
                }
                if (track.KeyFrames.Count > 1)
                {
                    switch (track.ChannelType)
                    {
                        case IOAnimationTrackType.PositionX: boneAnim.Curves.Add(CreateCurve(track, AnimTarget.PositionX)); break;
                        case IOAnimationTrackType.PositionY: boneAnim.Curves.Add(CreateCurve(track, AnimTarget.PositionY)); break;
                        case IOAnimationTrackType.PositionZ: boneAnim.Curves.Add(CreateCurve(track, AnimTarget.PositionZ)); break;
                        case IOAnimationTrackType.RotationEulerX: boneAnim.Curves.Add(CreateCurve(track, AnimTarget.RotateX)); break;
                        case IOAnimationTrackType.RotationEulerY: boneAnim.Curves.Add(CreateCurve(track, AnimTarget.RotateY)); break;
                        case IOAnimationTrackType.RotationEulerZ: boneAnim.Curves.Add(CreateCurve(track, AnimTarget.RotateZ)); break;
                        case IOAnimationTrackType.ScaleX: boneAnim.Curves.Add(CreateCurve(track, AnimTarget.ScaleX)); break;
                        case IOAnimationTrackType.ScaleY: boneAnim.Curves.Add(CreateCurve(track, AnimTarget.ScaleY)); break;
                        case IOAnimationTrackType.ScaleZ: boneAnim.Curves.Add(CreateCurve(track, AnimTarget.ScaleZ)); break;
                    }
                }
            }

            //Set curve flags
            foreach (var curve in boneAnim.Curves)
            {
                switch ((AnimTarget)curve.AnimDataOffset)
                {
                    case AnimTarget.PositionX: boneAnim.FlagsCurve |= BoneAnimFlagsCurve.TranslateX; break;
                    case AnimTarget.PositionY: boneAnim.FlagsCurve |= BoneAnimFlagsCurve.TranslateY; break;
                    case AnimTarget.PositionZ: boneAnim.FlagsCurve |= BoneAnimFlagsCurve.TranslateZ; break;
                    case AnimTarget.RotateX: boneAnim.FlagsCurve |= BoneAnimFlagsCurve.RotateX; break;
                    case AnimTarget.RotateY: boneAnim.FlagsCurve |= BoneAnimFlagsCurve.RotateY; break;
                    case AnimTarget.RotateZ: boneAnim.FlagsCurve |= BoneAnimFlagsCurve.RotateZ; break;
                    case AnimTarget.RotateW: boneAnim.FlagsCurve |= BoneAnimFlagsCurve.RotateW; break;
                    case AnimTarget.ScaleX: boneAnim.FlagsCurve |= BoneAnimFlagsCurve.ScaleX; break;
                    case AnimTarget.ScaleY: boneAnim.FlagsCurve |= BoneAnimFlagsCurve.ScaleY; break;
                    case AnimTarget.ScaleZ: boneAnim.FlagsCurve |= BoneAnimFlagsCurve.ScaleZ; break;
                }
            }

            bool animateTranslation = boneAnim.FlagsCurve.HasFlag(BoneAnimFlagsCurve.TranslateX) ||
                                      boneAnim.FlagsCurve.HasFlag(BoneAnimFlagsCurve.TranslateY) ||
                                      boneAnim.FlagsCurve.HasFlag(BoneAnimFlagsCurve.TranslateZ);

            bool animateRotation =   boneAnim.FlagsCurve.HasFlag(BoneAnimFlagsCurve.RotateX) ||
                                     boneAnim.FlagsCurve.HasFlag(BoneAnimFlagsCurve.RotateY) ||
                                     boneAnim.FlagsCurve.HasFlag(BoneAnimFlagsCurve.RotateZ) ||
                                     boneAnim.FlagsCurve.HasFlag(BoneAnimFlagsCurve.RotateW);

            bool animateScale =      boneAnim.FlagsCurve.HasFlag(BoneAnimFlagsCurve.ScaleX) ||
                                     boneAnim.FlagsCurve.HasFlag(BoneAnimFlagsCurve.ScaleY) ||
                                     boneAnim.FlagsCurve.HasFlag(BoneAnimFlagsCurve.ScaleZ);

            //Set base flags on what is animated
            if (animateTranslation)
                boneAnim.FlagsBase |= BoneAnimFlagsBase.Translate;
            if (animateScale)
                boneAnim.FlagsBase |= BoneAnimFlagsBase.Scale;
            if (animateRotation)
                boneAnim.FlagsBase |= BoneAnimFlagsBase.Rotate;

            //Set base transform
            boneAnim.BaseData = new BoneAnimData()
            {
                Translate = basePosition,
                Rotate = baseRotation,
                Scale = baseScale,
                Flags = 0,
            };

            return boneAnim;
        }

        static AnimCurve CreateCurve(IOAnimationTrack track, AnimTarget target)
        {
            //check if hermite keys are used
            bool isHermite = track.KeyFrames.Any(x => x is IOKeyFrameHermite);

            AnimCurve curve = new AnimCurve();
            curve.CurveType = AnimCurveType.Linear;
            //use cubic for hermite keys
            if (isHermite)
                curve.CurveType = AnimCurveType.Cubic;

            //Todo. Add options for short and sbyte using calculated scale/offset
            curve.KeyType = AnimCurveKeyType.Single;
            curve.Scale = 1f;
            curve.Offset = 0;

            //wrap modes
            curve.PreWrap = GetWrap(track.PreWrap);
            curve.PostWrap = GetWrap(track.PostWrap);

            //start frame
            curve.StartFrame = track.KeyFrames.Min(x => x.Frame);
            //end frame
            curve.EndFrame = track.KeyFrames.Max(x => x.Frame);
            //Target anim offset (used as enum type)
            curve.AnimDataOffset = (uint)target;

            //Get max frame value
            float frame = track.KeyFrames.Max(x => x.Frame);
            if (frame < byte.MaxValue) curve.FrameType = AnimCurveFrameType.Byte;
            else if (frame < ushort.MaxValue) curve.FrameType = AnimCurveFrameType.Decimal10x5;

            //Set a key and frame list
            var keys = track.KeyFrames.ToList();
            var frames = track.KeyFrames.Select(x => x.Frame).ToList();
            curve.Frames = frames.ToArray();

            //Prepare amount of key data to allocate by interpolation type
            curve.Keys = new float[keys.Count, 1];
            if (curve.CurveType == AnimCurveType.Cubic) curve.Keys = new float[keys.Count, 4];
            if (curve.CurveType == AnimCurveType.Linear) curve.Keys = new float[keys.Count, 2];

            for (int i = 0; i < keys.Count; i++)
            {
                switch (curve.CurveType)
                {
                    case AnimCurveType.Cubic:
                        var value =  keys[i].ValueF32;

                        //Get the hermite data if key is hermite
                        var hermiteKey = keys[i] as IOKeyFrameHermite;
                        var inSlope = hermiteKey != null ? hermiteKey.TangentSlopeInput : 0;
                        var outSlope = hermiteKey != null ? hermiteKey.TangentSlopeOutput : 0;

                        //Calculate the next key frame value, slope and time
                        float time = 0;
                        float nextValue = 0;
                        float nextInSlope = 0;
                        if (i < keys.Count - 1)
                        {
                            var nextFrame = frames[i + 1];

                            nextValue = keys[i + 1].ValueF32;
                            time = nextFrame - frames[i];

                            if (keys[i + 1] is IOKeyFrameHermite)
                                nextInSlope = ((IOKeyFrameHermite)keys[i + 1]).TangentSlopeInput;
                        }

                        //Generate 4 coef values for a cubic curve
                        float[] coefs = HermiteToCubicKey(
                            value, nextValue,
                            outSlope * time, nextInSlope * time);

                        curve.Keys[i, 0] = coefs[0];
                        if (time != 0)
                        {
                            curve.Keys[i, 1] = coefs[1];
                            curve.Keys[i, 2] = coefs[2];
                            curve.Keys[i, 3] = coefs[3];
                        }
                        break;
                    case AnimCurveType.Linear:
                        //Calculate delta between current and next value
                        //Todo including a way to insert custom delta might be ideal
                        var delta = 0f;
                        if (i < keys.Count - 1)
                            delta = keys[i + 1].ValueF32 - keys[i].ValueF32;

                        curve.Keys[i, 0] = keys[i].ValueF32;
                        curve.Keys[i, 1] = delta;
                        break;
                    default:
                        curve.Keys[i, 0] = keys[i].ValueF32;
                        break;
                }
            }

            if (curve.Keys.Length >= 2)
            {
                //difference in value between first and last key value
                var lastKey = curve.Keys[keys.Count - 1, 0];
                var firstKey = curve.Keys[0, 0];

                curve.Delta = lastKey - firstKey;
            }

            //Quantization by offset and scale
            for (int i = 0; i < keys.Count; i++)
            {
                curve.Keys[i, 0] -= curve.Offset;

                //Apply scale for cubic and linear curves only
                if (curve.CurveType == AnimCurveType.Cubic)
                {
                    if (curve.Scale != 0)
                    {
                        curve.Keys[i, 0] /= curve.Scale;
                        curve.Keys[i, 1] /= curve.Scale;
                        curve.Keys[i, 2] /= curve.Scale;
                        curve.Keys[i, 3] /= curve.Scale;
                    }
                }
                else if (curve.CurveType == AnimCurveType.Linear)
                {
                    if (curve.Scale != 0)
                    {
                        curve.Keys[i, 0] /= curve.Scale;
                        curve.Keys[i, 1] /= curve.Scale;
                    }
                }
            }

            return curve;
        }

        static BfresLibrary.WrapMode GetWrap(IOCurveWrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case IOCurveWrapMode.Cycle: return BfresLibrary.WrapMode.Repeat;
                case IOCurveWrapMode.CycleRelative: return BfresLibrary.WrapMode.Repeat;
                case IOCurveWrapMode.Oscillate: return BfresLibrary.WrapMode.Mirror;
                case IOCurveWrapMode.Constant: return BfresLibrary.WrapMode.Clamp;
                default:
                    return BfresLibrary.WrapMode.Clamp;
            }
        }

        static float[] HermiteToCubicKey(float p0, float p1, float s0, float s1)
        {
            float[] coefs = new float[4];
            coefs[3] = (p0 * 2) + (p1 * -2) + (s0 * 1) + (s1 * 1);
            coefs[2] = (p0 * -3) + (p1 * 3) + (s0 * -2) + (s1 * -1);
            coefs[1] = (p0 * 0) + (p1 * 0) + (s0 * 1) + (s1 * 0);
            coefs[0] = (p0 * 1) + (p1 * 0) + (s0 * 0) + (s1 * 0);
            return coefs;
        }

        enum AnimTarget
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
