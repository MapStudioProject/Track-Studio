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
    public class MaterialAnimConverter
    {
        public static void ConvertAnimation(BfresMaterialAnim anim, MaterialAnim animTarget)
        {
            //Use helper classes to generate material anim
            MaterialAnimHelper animation = new MaterialAnimHelper();
            animation.Name = anim.Name;
            animation.Path = "";
            animation.FrameCount = (int)anim.FrameCount;
            animation.Baked = false;
            animation.MaterialAnims = new List<MaterialDataAnimHelper>();
            animation.Loop = anim.Loop;
            animation.TextureList = anim.TextureList.ToList();

            foreach (STAnimGroup group in anim.AnimGroups)
            {
                var matAnim = new MaterialDataAnimHelper();
                matAnim.Name = group.Name;
                animation.MaterialAnims.Add(matAnim);

                matAnim.Samplers = new List<SamplerAnimHelper>();
                matAnim.Params = new List<ShaderParamAnimHelper>();

                foreach (var track in group.GetTracks())
                {
                    if (track.KeyFrames.Count == 0)
                        continue;

                    if (track is BfresMaterialAnim.SamplerTrack)
                    {
                        var samplerTrack = track as BfresMaterialAnim.SamplerTrack;

                        var samplerAnimHelper = new SamplerAnimHelper();
                        samplerAnimHelper.Name = track.Name;
                        samplerAnimHelper.Constant = (ushort)track.KeyFrames[0].Value;

                        ((BfresAnimationTrack)track).Offset = 0;
                        ((BfresAnimationTrack)track).Scale = 1f;

                        if (track.KeyFrames.Count > 1)
                            samplerAnimHelper.Curve = ConvertCurve((BfresAnimationTrack)track, 0);

                        matAnim.Samplers.Add(samplerAnimHelper);
                    }
                }

                foreach (var subGroup in group.SubAnimGroups)
                {
                    if (subGroup is BfresMaterialAnim.ParamAnimGroup) {
                        var paramGroup = (BfresMaterialAnim.ParamAnimGroup)subGroup;
                        var paramAnimHelper = new ShaderParamAnimHelper();
                        paramAnimHelper.Name = subGroup.Name;
                        paramAnimHelper.Curves = new List<CurveAnimHelper>();
                        paramAnimHelper.Constants = new List<AnimConstant>();
                        matAnim.Params.Add(paramAnimHelper);

                        foreach (BfresMaterialAnim.ParamTrack track in paramGroup.GetTracks())
                        {
                            if (string.IsNullOrEmpty(track.Name))
                                continue;

                            if (track.KeyFrames.Count > 1)
                                paramAnimHelper.Curves.Add(ConvertCurve(track, track.ValueOffset));
                            else if (track.KeyFrames.Count == 1)
                            {
                                DWord value = track.KeyFrames[0].Value;
                                if (track.IsInt32)
                                    value = (int)track.KeyFrames[0].Value;

                                paramAnimHelper.Constants.Add(new AnimConstant()
                                {
                                    Value = value,
                                    AnimDataOffset = track.ValueOffset,
                                });
                            }
                        }
                    }
                }
            }

            MaterialAnimHelper.FromStruct(animTarget, animation);
        }

        static CurveAnimHelper ConvertCurve(BfresAnimationTrack track, uint target)
        {
            CurveAnimHelper curve = new CurveAnimHelper();
            curve.KeyFrames = new Dictionary<float, object>();
            curve.Scale = track.Scale;
            curve.Offset = track.Offset;
            curve.Target = target.ToString();
            curve.WrapMode = $"{track.PreWrap}, {track.PostWrap}";

            if (track.InterpolationType == STInterpoaltionType.Hermite)
            {
                curve.Interpolation = AnimCurveType.Cubic;
                foreach (STHermiteKeyFrame key in track.KeyFrames)
                {
                    if (curve.KeyFrames.ContainsKey(key.Frame))
                        curve.KeyFrames.Remove(key.Frame);

                    curve.KeyFrames.Add(key.Frame, new HermiteKey()
                    {
                        In = key.TangentIn,
                        Out = key.TangentOut,
                        Value = (float)key.Value,
                    });
                }
            }
            else if (track.InterpolationType == STInterpoaltionType.Linear)
            {
                curve.Interpolation = AnimCurveType.Linear;
                for (int i = 0; i < track.KeyFrames.Count; i++)
                {
                    var key = track.KeyFrames[i];

                    if (curve.KeyFrames.ContainsKey(key.Frame))
                        curve.KeyFrames.Remove(key.Frame);

                    float delta = 0;
                    if (i < track.KeyFrames.Count - 1)
                        delta = track.KeyFrames[i + 1].Value - key.Value;

                    // if (key is STLinearKeyFrame)
                    //     delta = ((STLinearKeyFrame)key).Delta;

                    curve.KeyFrames.Add(key.Frame, new LinearKeyFrame()
                    {
                        Value = (float)key.Value,
                        Delta = delta,
                    });
                }
            }
            else if (track.InterpolationType == STInterpoaltionType.Step)
            {
                curve.Interpolation = AnimCurveType.StepInt;
                for (int i = 0; i < track.KeyFrames.Count; i++)
                {
                    curve.KeyFrames.Add(track.KeyFrames[i].Frame, new KeyFrame()
                    {
                        Value = (int)track.KeyFrames[i].Value,
                    });
                }
            }

            curve.KeyType = track.KeyType;
            curve.FrameType = track.FrameType;

            //Get max frame value
            float frame = curve.KeyFrames.Max(x => x.Key);
            if (frame < byte.MaxValue) curve.FrameType = AnimCurveFrameType.Byte;
           // else if (frame < ushort.MaxValue) curve.FrameType = AnimCurveFrameType.Decimal10x5;

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
