using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafeLibrary.Rendering;
using Toolbox.Core.Animations;
using GLFrameworkEngine;
using EffectLibrary;

namespace TurboLibrary.Actors
{
   public class ActorModelBase : ActorBase
    {
        public IFrustumCulling FrustumRenderCheck { get; set; }

        public BfresRender Render { get; private set; }

        public Handle[] EffectHandle = new Handle[1];

        public bool Visible { get; set; } = true;

        public GLTransform Transform { get; set; }

        public List<float> Parameters = new List<float>(8);

        public override void Init()
        {
            var matrix = Transform.TransformMatrix;
            if (EffectManager.Instance.HasAsset(this.Name))
            {
                EffectManager.Instance.EmitAction(Age, ref EffectHandle[0], this.Name, "EActionAlways", matrix);
                if (EffectHandle[0] == null)
                    EffectManager.Instance.EmitDirect(ref EffectHandle[0], this.Name, this.Name, matrix);
            }
        }

        public virtual void UpdateModel(BfresRender render) {
            if (render == null)
                return;

            FrustumRenderCheck = render;
            Render = render;
            Transform = render.Transform;
        }

        public override void BeginFrame()
        {
            UpdateCalc = true;

            if (Render == null)
                return;

            foreach (var model in Render.Models)
                model.ModelData.Skeleton.Updated = false;
        }

        public override void Calc()
        {
            //Frustum check
            bool inFrustum = true;
            if (FrustumRenderCheck != null)
                inFrustum = FrustumRenderCheck.InFrustum;
            //visiblity check
            bool isVisible = Visible && inFrustum;

            for (int i = 0; i < EffectHandle.Length; i++)
            {
                if (EffectHandle[i] != null)
                {
                    EffectHandle[i].Visible = isVisible;
                    if (isVisible)
                        EffectHandle[i].emitterSet.SetMatrix(this.Transform.TransformMatrix);
                }
            }

            if (!isVisible || Render == null)
                return;

            foreach (var anim in GetAnimations())
            {
                if (anim == null)
                    continue;

                anim.SetFrame(GetAnimationFrame(anim, anim.Frame));
                foreach (var model in Render.Models)
                {
                    if (anim is BfresSkeletalAnim) {
                        ((BfresSkeletalAnim)anim).NextFrame(model.ModelData.Skeleton);
                        MapStudio.UI.AnimationStats.SkeletalAnims += 1;
                    }
                    else
                        anim.NextFrame();
                }

                if (anim.Frame >= anim.FrameCount - 1 && !anim.Loop)
                    OnAnimationEnd(anim);

                anim.Frame++;
            }
        }

        private float GetAnimationFrame(STAnimation anim, float frame)
        {
            float animFrameNum = frame;

            if (anim.Loop)
            {
                //Loop current frame to 0 - frame count range
                var lastFrame = anim.FrameCount;
                while (animFrameNum > lastFrame)
                    animFrameNum -= lastFrame + 1;
            }

            return animFrameNum;
        }

        public virtual List<STAnimation> GetAnimations()
        {
            if (Render == null)
                return new List<STAnimation>();

            List<STAnimation> animations = new List<STAnimation>();
            animations.Add(Render.SkeletalAnimations.FirstOrDefault());
            animations.AddRange(Render.MaterialAnimations);
            return animations;
        }

        public virtual void OnAnimationEnd(STAnimation anim) { }

        public virtual void ResetAnimation()
        {
            Render?.ResetAnimations();
        }

        public override void Dispose()
        {
            if (EffectManager.Instance == null)
                return;

            for (int i = 0; i < EffectHandle.Length; i++) {
                if (EffectHandle[i] != null)
                    EffectManager.Instance.KillEmitterHandle(ref EffectHandle[i]);
            }
        }
    }
}
