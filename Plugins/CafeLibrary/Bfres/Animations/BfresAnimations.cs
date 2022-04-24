using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.Animations;
using BfresLibrary;

namespace CafeLibrary.Rendering
{
    public class BfresAnimations
    {
        public static AnimCurve CreateLinearCurve(float[] frames, float[] values, int offset)
        {
            var curve = new AnimCurve();
            curve.AnimDataOffset = (uint)offset;
            curve.CurveType = AnimCurveType.Linear;
            curve.Frames = frames;
            curve.Keys = new float[values.Length, 2];
            curve.Offset = 0;
            curve.Scale = 1;
            curve.StartFrame = 0;
            curve.EndFrame = frames.LastOrDefault();
            curve.FrameType = AnimCurveFrameType.Single;
            curve.PostWrap = WrapMode.Repeat;
            curve.PreWrap = WrapMode.Repeat;
            for (int i = 0; i < values.Length; i++)
                curve.Keys[i, 0] = values[i];
            return curve;
        }

        public static void GenerateKeys(BfresAnimationTrack track, AnimCurve curve,
            bool valuesAsInts = false)
        {
            //Use the curve's post wrap.
            track.Offset = curve.Offset;
            track.Scale = curve.Scale;
            track.FrameType = curve.FrameType;
            track.KeyType = curve.KeyType;
            track.PostWrap = curve.PostWrap;
            track.PreWrap = curve.PreWrap;

            track.WrapMode = STLoopMode.Clamp;
            if (curve.PostWrap == WrapMode.Repeat)
                track.WrapMode = STLoopMode.Repeat;
            if (curve.PostWrap == WrapMode.Mirror)
                track.WrapMode = STLoopMode.Mirror;
            //Todo not sure what type this one is
            //Possibly wrapping from the last value used
            if (curve.PostWrap == (WrapMode)3)
                track.WrapMode = STLoopMode.Cumulative;

            float valueScale = curve.Scale > 0 ? curve.Scale : 1;

            for (int i = 0; i < curve.Frames.Length; i++)
            {
                var frame = curve.Frames[i];
                if (frame == 0 && track.KeyFrames.Any(x => x.Frame == 0))
                    track.RemoveKey(0);

                switch (curve.CurveType)
                {
                    case AnimCurveType.Cubic:
                        {
                            track.InterpolationType = STInterpoaltionType.Hermite;
                            //Important to not offset the other 3 values, just the first one!
                            var value = curve.Keys[i, 0] * valueScale + curve.Offset;
                            var slopes = GetSlopes(curve, i);

                            track.KeyFrames.Add(new STHermiteKeyFrame()
                            {
                                Frame = frame,
                                Value = value,
                                TangentIn = slopes[0],
                                TangentOut = slopes[1],
                            });
                        }
                        break;
                    case AnimCurveType.Linear:
                        {
                            track.InterpolationType = STInterpoaltionType.Linear;
                            var value = curve.Keys[i, 0] * valueScale + curve.Offset;
                            var delta = curve.Keys[i, 1] * valueScale;
                            track.KeyFrames.Add(new STLinearKeyFrame()
                            {
                                Frame = frame,
                                Value = value,
                                Delta = delta,
                            });
                        }
                        break;
                    case AnimCurveType.StepBool:
                        {
                            track.InterpolationType = STInterpoaltionType.Step;
                            track.KeyFrames.Add(new STKeyFrame()
                            {
                                Frame = frame,
                                Value = curve.KeyStepBoolData[i] ? 1 : 0,
                            });
                        }
                        break;
                    default:
                        {
                            track.InterpolationType = STInterpoaltionType.Step;
                            var value = curve.Keys[i, 0] + curve.Offset;
                            if (valuesAsInts)
                                value = (int)curve.Keys[i, 0] + curve.Offset;

                            track.KeyFrames.Add(new STKeyFrame()
                            {
                                Frame = frame,
                                Value = value,
                            });
                        }
                        break;
                }
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
    }
}
