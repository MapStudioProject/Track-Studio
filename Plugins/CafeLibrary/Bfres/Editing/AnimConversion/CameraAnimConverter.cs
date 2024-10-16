using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.Animations;
using BfresLibrary;
using CafeLibrary.Rendering;

namespace CafeLibrary
{
    public class CameraAnimConverter
    {
        public static CameraAnim ConvertAnimation(BfresCameraAnim anim)
        {
            CameraAnim canm = new CameraAnim();
            ConvertAnimation(anim, canm);
            return canm;
        }

        //Calculates a unique hash to determine any edits to convert it on save
        public static int CalculateHash(BfresCameraAnim anim)
        {
            int hash = 0;

            var group = (BfresCameraAnim.CameraAnimGroup)anim.AnimGroups.FirstOrDefault();
            foreach (var track in group.GetTracks())
            {
                hash |= track.InterpolationType.GetHashCode();
                foreach (var key in track.KeyFrames)
                {
                    hash ^= key.Frame.GetHashCode();
                    hash ^= key.Value.GetHashCode();
                }
            }
            return hash;
        }

        public static void ConvertAnimation(BfresCameraAnim anim, CameraAnim target)
        {
            target.Name = anim.Name;
            if (anim.Loop)
                target.Flags |= CameraAnimFlags.Looping;
            else
                target.Flags &= ~CameraAnimFlags.Looping;

            target.FrameCount = (int)anim.FrameCount;

            var group = (BfresCameraAnim.CameraAnimGroup)anim.AnimGroups.FirstOrDefault();
            target.Curves.Clear();

            void TryAddCurve(BfresAnimationTrack track, CameraAnimDataOffset dataOffset)
            {
                if (track.KeyFrames.Count > 1) target.Curves.Add(ConvertCurve(track, dataOffset));
            }

            TryAddCurve(group.PositionX, CameraAnimDataOffset.PositionX);
            TryAddCurve(group.PositionY, CameraAnimDataOffset.PositionY);
            TryAddCurve(group.PositionZ, CameraAnimDataOffset.PositionZ);
            TryAddCurve(group.RotationX, CameraAnimDataOffset.RotationX);
            TryAddCurve(group.RotationY, CameraAnimDataOffset.RotationY);
            TryAddCurve(group.RotationZ, CameraAnimDataOffset.RotationZ);
            TryAddCurve(group.AspectRatio, CameraAnimDataOffset.AspectRatio);
            TryAddCurve(group.FieldOfView, CameraAnimDataOffset.FieldOFView);
            TryAddCurve(group.ClipFar, CameraAnimDataOffset.ClipFar);
            TryAddCurve(group.ClipNear, CameraAnimDataOffset.ClipNear);
            TryAddCurve(group.Twist, CameraAnimDataOffset.Twist);

            target.BaseData = new CameraAnimData()
            {
                Position = new Syroot.Maths.Vector3F(
                        group.PositionX.GetFrameValue(0),
                        group.PositionY.GetFrameValue(0),
                        group.PositionZ.GetFrameValue(0)),
                Rotation = new Syroot.Maths.Vector3F(
                        group.RotationX.GetFrameValue(0),
                        group.RotationY.GetFrameValue(0),
                        group.RotationZ.GetFrameValue(0)),
                Twist = group.AspectRatio.GetFrameValue(0),
                AspectRatio = group.AspectRatio.HasKeys ? group.AspectRatio.GetFrameValue(0) : 1f,
                ClipFar = group.ClipFar.HasKeys ? group.ClipFar.GetFrameValue(0) : 10000,
                ClipNear = group.ClipNear.HasKeys ? group.ClipNear.GetFrameValue(0) : 1,
                FieldOfView = group.FieldOfView.HasKeys ? group.FieldOfView.GetFrameValue(0) : 0.785398f,
            };
        }

        static AnimCurve ConvertCurve(BfresAnimationTrack track, CameraAnimDataOffset dataOffset)
        {
            AnimCurve animCurve = new AnimCurve();

            animCurve.AnimDataOffset = (uint)dataOffset;
            animCurve.CurveType = AnimCurveType.Linear;
            animCurve.Frames = track.KeyFrames.Select(x => x.Frame).ToArray();
            animCurve.KeyStepBoolData = track.KeyFrames.Select(x => x.Value != 0).ToArray();
            animCurve.Scale = track.Scale;
            animCurve.Offset = track.Offset;
            animCurve.EndFrame = track.KeyFrames.LastOrDefault().Frame;
            animCurve.KeyType = track.KeyType;
            animCurve.FrameType = track.FrameType;
            animCurve.StartFrame = track.StartFrame;
            animCurve.Delta = 0;

            if (track.InterpolationType == STInterpoaltionType.Hermite)
                animCurve.CurveType = AnimCurveType.Cubic;
            if (track.InterpolationType == STInterpoaltionType.Step)
                animCurve.CurveType = AnimCurveType.StepInt;

            var keys = track.KeyFrames.ToList();
            var frames = track.KeyFrames.Select(x => x.Frame).ToList();

            //Get max frame value
            float frame = track.KeyFrames.Max(x => x.Frame);
            if (frame < byte.MaxValue) animCurve.FrameType = AnimCurveFrameType.Byte;
            else if (frame < ushort.MaxValue) animCurve.FrameType = AnimCurveFrameType.Decimal10x5;

            animCurve.Frames = frames.ToArray();
            animCurve.Keys = new float[keys.Count, 1];

            if (animCurve.CurveType == AnimCurveType.Cubic) animCurve.Keys = new float[keys.Count, 4];
            if (animCurve.CurveType == AnimCurveType.Linear) animCurve.Keys = new float[keys.Count, 2];

            for (int i = 0; i < keys.Count; i++)
            {
                switch (animCurve.CurveType)
                {
                    case AnimCurveType.Cubic:

                        var hermiteKey = keys[i] as STHermiteKeyFrame;

                        float time = 0;
                        float value = hermiteKey.Value;
                        float outSlope = hermiteKey.TangentOut;
                        float nextValue = 0;
                        float nextInSlope = 0;
                        if (i < keys.Count - 1)
                        {
                            var nextKey = keys[i + 1] as STHermiteKeyFrame;
                            var nextFrame = frames[i + 1];

                            nextValue = nextKey.Value;
                            nextInSlope = nextKey.TangentIn;
                            time = nextFrame - frames[i];
                        }

                        float[] coefs = CurveAnimHelper.HermiteToCubicKey(
                        value, nextValue,
                        outSlope * time, nextInSlope * time);

                        animCurve.Keys[i, 0] = coefs[0];
                        if (time != 0)
                        {
                            animCurve.Keys[i, 1] = coefs[1];
                            animCurve.Keys[i, 2] = coefs[2];
                            animCurve.Keys[i, 3] = coefs[3];
                        }
                        break;
                    case AnimCurveType.Linear:
                        animCurve.Keys[i, 0] = keys[i].Value;
                        animCurve.Keys[i, 1] = 0;
                        //delta
                        if (i < keys.Count - 1)
                            animCurve.Keys[i, 1] = keys[i + 1].Value - keys[i].Value;
                        break;
                    default:
                        animCurve.Keys[i, 0] = keys[i].Value;
                        break;
                }
            }

            if (animCurve.Keys.Length >= 2)
            {
                var lastKey = animCurve.Keys[keys.Count - 1, 0];
                var firstKey = animCurve.Keys[0, 0];

                animCurve.Delta = lastKey - firstKey;
            }

            for (int i = 0; i < keys.Count; i++)
            {
                animCurve.Keys[i, 0] -= animCurve.Offset;

                //Apply scale for cubic and linear curves only
                if (animCurve.CurveType == AnimCurveType.Cubic)
                {
                    if (animCurve.Scale != 0)
                    {
                        animCurve.Keys[i, 0] /= animCurve.Scale;
                        animCurve.Keys[i, 1] /= animCurve.Scale;
                        animCurve.Keys[i, 2] /= animCurve.Scale;
                        animCurve.Keys[i, 3] /= animCurve.Scale;
                    }
                }
                else if (animCurve.CurveType == AnimCurveType.Linear)
                {
                    if (animCurve.Scale != 0)
                    {
                        animCurve.Keys[i, 0] /= animCurve.Scale;
                        animCurve.Keys[i, 1] /= animCurve.Scale;
                    }
                }
            }

            return animCurve;
        }
    }
}
