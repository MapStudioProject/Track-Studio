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
    public class BoneVisAnimConverter
    {
        public static VisibilityAnim ConvertAnimation(BfresVisibilityAnim anim)
        {
            VisibilityAnim fvis = new VisibilityAnim();
            ConvertAnimation(anim, fvis);
            return fvis;
        }

        //Calculates a unique hash to determine any edits to convert it on save
        public static int CalculateHash(BfresVisibilityAnim anim)
        {
            int hash = 0;

            foreach (BfresVisibilityAnim.BoneAnimGroup group in anim.AnimGroups)
            {
                hash |= group.Track.InterpolationType.GetHashCode();
                hash |= group.Track.PostWrap.GetHashCode();
                hash |= group.Track.PreWrap.GetHashCode();

                foreach (var key in group.Track.KeyFrames)
                {
                    hash ^= key.Frame.GetHashCode();
                    hash ^= key.Value.GetHashCode();
                }
            }
            return hash;
        }

        public static void ConvertAnimation(BfresVisibilityAnim anim, VisibilityAnim target)
        {
            target.Name = anim.Name;
            target.Loop = anim.Loop;
            target.FrameCount = (int)anim.FrameCount;

            target.BindIndices = new ushort[anim.AnimGroups.Count];
            target.BaseDataList = new bool[anim.AnimGroups.Count];
            target.Names = new string[anim.AnimGroups.Count];

            for (int i = 0; i < target.BindIndices.Length; i++)
                target.BindIndices[i] = ushort.MaxValue;

            target.Curves.Clear();
            for (int i = 0; i < anim.AnimGroups.Count; i++)
            {
                target.Names[i] = anim.AnimGroups[i].Name;

                var group = anim.AnimGroups[i] as BfresVisibilityAnim.BoneAnimGroup;
                if (group.Track.KeyFrames.Count == 0)
                    continue;

                target.BaseDataList[i] = group.Track.KeyFrames[0].Value != 0;

                if (group.Track.KeyFrames.Count > 1)
                    target.Curves.Add(ConvertCurve(group.Track, i));
            }
        }

        static AnimCurve ConvertCurve(BfresAnimationTrack track, int index)
        {
            AnimCurve animCurve = new AnimCurve();

            animCurve.AnimDataOffset = (uint)index;
            animCurve.CurveType = AnimCurveType.StepBool;
            animCurve.Frames = track.KeyFrames.Select(x => x.Frame).ToArray();
            animCurve.KeyStepBoolData = track.KeyFrames.Select(x => x.Value != 0).ToArray();
            animCurve.Delta = 0;
            animCurve.Scale = 1E-45f;
            animCurve.StartFrame = 0;
            animCurve.EndFrame = track.KeyFrames.LastOrDefault().Frame;
            animCurve.KeyType = AnimCurveKeyType.Single;
            animCurve.FrameType = AnimCurveFrameType.Single;
            //Get max frame value
            float frame = track.KeyFrames.Max(x => x.Frame);
            if (frame < byte.MaxValue) animCurve.FrameType = AnimCurveFrameType.Byte;
            else if (frame < ushort.MaxValue) animCurve.FrameType = AnimCurveFrameType.Decimal10x5;

            return animCurve;
        }
    }
}
