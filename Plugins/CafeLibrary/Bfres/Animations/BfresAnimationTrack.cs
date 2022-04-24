using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.Animations;
using BfresLibrary;
using Toolbox.Core;

namespace CafeLibrary.Rendering
{
    public class BfresAnimationTrack : STAnimationTrack, ICloneable
    {
        public DWord Offset { get; set; }

        [BindGUI("Scale", Category = "Quantization")]
        public float Scale { get; set; } = 1.0f;

        [BindGUI("Key Type", Category = "Quantization")]
        public AnimCurveKeyType KeyType { get; set; } = AnimCurveKeyType.Single;

        [BindGUI("Frame Type", Category = "Quantization")]
        public AnimCurveFrameType FrameType { get; set; } = AnimCurveFrameType.Single;

        [BindGUI("Pre Wrap")]
        public WrapMode PreWrap { get; set; } = BfresLibrary.WrapMode.Clamp;

        [BindGUI("Post Wrap")]
        public WrapMode PostWrap { get; set; } = BfresLibrary.WrapMode.Clamp;

        private float GetWrapFrame(float frame)
        {
            var lastFrame = KeyFrames.Last().Frame;
            if (WrapMode == STLoopMode.Clamp)
            {
                if (frame > lastFrame)
                    return lastFrame;
                else
                    return frame;
            }
            else if (WrapMode == STLoopMode.Repeat)
            {
                while (frame > lastFrame)
                    frame -= lastFrame;
                return frame;
            }
            return frame;
        }

        public virtual object Clone()
        {
            var track = new BfresAnimationTrack();
            Clone(track);
            return track;
        }

        public void Clone(BfresAnimationTrack track)
        {
            track.Scale = this.Scale;
            track.Offset = this.Offset;
            track.KeyType = this.KeyType;
            track.FrameType = this.FrameType;
            track.WrapMode = this.WrapMode;
            track.PostWrap = this.PostWrap;
            track.PreWrap = this.PreWrap;
            track.InterpolationType = this.InterpolationType;
            track.ChannelIndex = this.ChannelIndex;
            track.Name = this.Name;
            foreach (var key in this.KeyFrames)
                track.KeyFrames.Add(key.Clone());
        }
    }
}
