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
    public class SkeletonAnimExporter
    {
        public static void Export(SkeletalAnim fskaAnimation, STSkeleton skeleton, string filePath)
        {
            if (skeleton  == null)
            {
                throw new Exception($"No skeleton found to export animation!");
            }

            IOAnimation anm = new IOAnimation();
            anm.Name = Path.GetFileNameWithoutExtension(filePath);
            anm.StartFrame = 0;
            anm.EndFrame = (float)fskaAnimation.FrameCount;

            bool isQuaternion = fskaAnimation.FlagsRotate == SkeletalAnimFlagsRotate.Quaternion;

            foreach (var bone in fskaAnimation.BoneAnims)
                anm.Groups.Add(CreateIOBoneAnimGroup(bone, isQuaternion));

            IOModel iomodel = new IOModel();
            iomodel.Name = "Skeleton";

            //Convert skeleton
            List<IOBone> bones = new List<IOBone>();
            foreach (var bone in skeleton.Bones)
            {
                IOBone iobone = new IOBone();
                iobone.Name = bone.Name;
                iobone.Scale = new Vector3(bone.Scale.X, bone.Scale.Y, bone.Scale.Z);
                iobone.RotationEuler = new Vector3(bone.EulerRotation.X, bone.EulerRotation.Y, bone.EulerRotation.Z);
                iobone.Translation = new Vector3(bone.Position.X, bone.Position.Y, bone.Position.Z);
                bones.Add(iobone);
            }
            //setup children and root
            for (int i = 0; i < bones.Count; i++)
            {
                var bone = bones[i];
                var parentIdx = skeleton.Bones[i].ParentIndex;
                if (parentIdx == -1)
                    iomodel.Skeleton.RootBones.Add(bone);
                else
                    bones[parentIdx].AddChild(bone);
            }

            IOScene scene = new IOScene();
            scene.Animations.Add(anm);
            scene.Models.Add(iomodel);
            scene.Name = anm.Name;

            IOManager.ExportScene(scene, filePath, new ExportSettings()
            {
                MayaAnimUseRadians = false,
            });
        }

        static IOAnimation CreateIOBoneAnimGroup(BoneAnim boneAnim, bool isQuaternion)
        {
            IOAnimation anim = new IOAnimation();
            anim.Name = boneAnim.Name;
            anim.UseSegmentScaleCompensate = boneAnim.ApplySegmentScaleCompensate;

           // if (isQuaternion)
          //      throw new Exception($"Quaternion animation export is not supported yet!");

            foreach (var curve in boneAnim.Curves)
            {
                switch ((AnimTarget)curve.AnimDataOffset)
                {
                    case AnimTarget.PositionX: anim.Tracks.Add(ConvertIOTrack(curve, IOAnimationTrackType.PositionX)); break;
                    case AnimTarget.PositionY: anim.Tracks.Add(ConvertIOTrack(curve, IOAnimationTrackType.PositionY)); break;
                    case AnimTarget.PositionZ: anim.Tracks.Add(ConvertIOTrack(curve, IOAnimationTrackType.PositionZ)); break;
                    case AnimTarget.RotateX: anim.Tracks.Add(ConvertIOTrack(curve, IOAnimationTrackType.RotationEulerX)); break;
                    case AnimTarget.RotateY: anim.Tracks.Add(ConvertIOTrack(curve, IOAnimationTrackType.RotationEulerY)); break;
                    case AnimTarget.RotateZ: anim.Tracks.Add(ConvertIOTrack(curve, IOAnimationTrackType.RotationEulerZ)); break;
                    case AnimTarget.ScaleX: anim.Tracks.Add(ConvertIOTrack(curve, IOAnimationTrackType.ScaleX)); break;
                    case AnimTarget.ScaleY: anim.Tracks.Add(ConvertIOTrack(curve, IOAnimationTrackType.ScaleY)); break;
                    case AnimTarget.ScaleZ: anim.Tracks.Add(ConvertIOTrack(curve, IOAnimationTrackType.ScaleZ)); break;
                }
            }
            if (boneAnim.FlagsBase.HasFlag(BoneAnimFlagsBase.Translate))
            {
                InsertBaseTrack(anim, IOAnimationTrackType.PositionX, boneAnim.BaseData.Translate.X);
                InsertBaseTrack(anim, IOAnimationTrackType.PositionY, boneAnim.BaseData.Translate.Y);
                InsertBaseTrack(anim, IOAnimationTrackType.PositionZ, boneAnim.BaseData.Translate.Z);
            }
            if (boneAnim.FlagsBase.HasFlag(BoneAnimFlagsBase.Rotate))
            {
                InsertBaseTrack(anim, IOAnimationTrackType.RotationEulerX, boneAnim.BaseData.Rotate.X);
                InsertBaseTrack(anim, IOAnimationTrackType.RotationEulerY, boneAnim.BaseData.Rotate.Y);
                InsertBaseTrack(anim, IOAnimationTrackType.RotationEulerZ, boneAnim.BaseData.Rotate.Z);
            }
            if (boneAnim.FlagsBase.HasFlag(BoneAnimFlagsBase.Scale))
            {
                InsertBaseTrack(anim, IOAnimationTrackType.ScaleX, boneAnim.BaseData.Scale.X);
                InsertBaseTrack(anim, IOAnimationTrackType.ScaleY, boneAnim.BaseData.Scale.Y);
                InsertBaseTrack(anim, IOAnimationTrackType.ScaleZ, boneAnim.BaseData.Scale.Z);
            }

            return anim;
        }

        static void InsertBaseTrack(IOAnimation anim, IOAnimationTrackType type, float value)
        {
            //check if a track exists
            var trackTarget = anim.Tracks.FirstOrDefault(x => x.ChannelType == type);
            if (trackTarget == null)
            {
                //if not, insert the track with base pose used
                IOAnimationTrack track = new IOAnimationTrack();
                track.ChannelType = type;
                anim.Tracks.Add(track);
                track.KeyFrames.Add(new IOKeyFrame() { Frame = 0, Value = value });
            }
            else //else insert the base pos to the first frame if no frame is at frame 0. 
            {
                if (!trackTarget.KeyFrames.Any(x => x.Frame == 0))
                {
                    trackTarget.KeyFrames.Add(new IOKeyFrame() { Frame = 0, Value = value });
                }
            }
        }

        static IOAnimationTrack ConvertIOTrack(AnimCurve curve, IOAnimationTrackType type)
        {
            IOAnimationTrack track = new IOAnimationTrack();
            track.ChannelType = type;
            track.PreWrap = GetWrap(curve.PreWrap);
            track.PostWrap = GetWrap(curve.PostWrap);

            float valueScale = curve.Scale > 0 ? curve.Scale : 1;
            for (int i = 0; i < curve.Frames.Length; i++)
            {
                var frame = curve.Frames[i];
                switch (curve.CurveType)
                {
                    case AnimCurveType.Cubic:
                        {
                            var value = curve.Keys[i, 0] * valueScale + curve.Offset;
                            var slopes = GetSlopes(curve, i);
                            track.KeyFrames.Add(new IOKeyFrameHermite()
                            {
                                Frame = frame,
                                Value = value,
                                TangentSlopeInput = slopes[0],
                                TangentSlopeOutput = slopes[1],
                            });
                        }
                        break;
                    case AnimCurveType.Linear:
                        {
                            var value = curve.Keys[i, 0] * valueScale + curve.Offset;
                            track.KeyFrames.Add(new IOKeyFrame()
                            {
                                Frame = frame,
                                Value = value,
                            });
                        }
                        break;
                    default:
                        {
                            var value = curve.Keys[i, 0] * valueScale + curve.Offset;
                            track.KeyFrames.Add(new IOKeyFrame()
                            {
                                Frame = frame,
                                Value = value,
                            });
                        }
                        break;
                }
            }
            return track;
        }

        static IOCurveWrapMode GetWrap(BfresLibrary.WrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case BfresLibrary.WrapMode.Repeat: return IOCurveWrapMode.Cycle;
                case BfresLibrary.WrapMode.Mirror: return IOCurveWrapMode.Oscillate;
                case BfresLibrary.WrapMode.Clamp: return IOCurveWrapMode.Constant;
                default:
                    return IOCurveWrapMode.Constant;
            }
        }

        //Method to extract the slopes from a cubic curve
        //Need to get the time, delta, out and next in slope values
        public static float[] GetSlopes(AnimCurve curve, float index)
        {
            float[] slopes = new float[2];
            if (curve.CurveType == AnimCurveType.Cubic)
            {
                float InSlope = 0;
                float OutSlope = 0;
                for (int i = 0; i < curve.Frames.Length; i++)
                {
                    var coef0 = curve.Keys[i, 0] * curve.Scale + curve.Offset;
                    var coef1 = curve.Keys[i, 1] * curve.Scale;
                    var coef2 = curve.Keys[i, 2] * curve.Scale;
                    var coef3 = curve.Keys[i, 3] * curve.Scale;
                    float time = 0;
                    float delta = 0;
                    if (i < curve.Frames.Length - 1)
                    {
                        var nextValue = curve.Keys[i + 1, 0] * curve.Scale + curve.Offset;
                        delta = nextValue - coef0;
                        time = curve.Frames[i + 1] - curve.Frames[i];
                    }

                    var slopeData = GetCubicSlopes(time, delta,
                        new float[4] { coef0, coef1, coef2, coef3, });

                    if (index == i)
                    {
                        OutSlope = slopeData[1];
                        return new float[2] { InSlope, OutSlope };
                    }

                    //The previous inslope is used
                    InSlope = slopeData[0];
                }
            }

            return slopes;
        }

        public static float[] GetCubicSlopes(float time, float delta, float[] coef)
        {
            float outSlope = coef[1] / time;
            float param = coef[3] - (-2 * delta);
            float inSlope = param / time - outSlope;
            return new float[2] { inSlope, coef[1] == 0 ? 0 : outSlope };
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
