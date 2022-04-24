using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.Animations;
using CafeLibrary.Rendering;
using BfresLibrary;
using OpenTK;
using TurboLibrary;

namespace TurboLibrary.Actors
{
    public class MapObjHhStatue : ActorModelBase
    {
        public List<STAnimation> Animations = new List<STAnimation>();
        public List<STAnimation> SpawnAnimations = new List<STAnimation>();

        public EffectLibrary.Handle EmitterHandle;

        public MapObjHhStatue() {
        }

        public override void Calc() {
            if (Render == null)
                return;

            if (SpawnAnimations.Count > 0) {
                Animations.Clear();
                Animations.AddRange(SpawnAnimations);
                SpawnAnimations.Clear();
            }

            var anim = Animations.FirstOrDefault();
            if (anim != null)
            {
                if (anim.Name == "Attack" && anim.Frame == 36)
                {
                    //Attach to the model matrix
                    var matrix = Render.Transform.TransformMatrix;
                    EffectLibrary.EffectManager.Instance.EmitDirect(ref EmitterHandle, "HhStatue", "Attack", matrix);
                }
            }

            base.Calc();

            animationAge++;
        }

        const float AttackDelay = 0;
        const float LiftDelay = 0;

        float animationAge = 0;

        public override void OnAnimationEnd(STAnimation anim)
        {
            float frame = anim.Frame;
            float diff = animationAge - anim.FrameCount;
            switch (anim.Name)
            {
                case "Attack":
                    if (diff >= AttackDelay)
                        DisplayTeresa();
                    break;
                case "AttackSt":
                    if (diff >= LiftDelay) 
                        AttackWeapon();
                    break;
                case "TeresaAppear":
                    SetSkeletalAnimation("TeresaLoop");
                    break;
                case "TeresaLoop":
                    SetSkeletalAnimation("TeresaHide");
                    break;
                case "TeresaHide":
                    LiftWeapon();
                    break;
            }
        }

        public override void Init() {
            SetSkeletalAnimation("Attack");
        }

        public void AttackWeapon() {
            SetSkeletalAnimation("Attack");
        }

        public void LiftWeapon() {
            if (Render == null)
                return;

            //Hide teresa
            Render.ToggleMeshes("HhStatue__mTeresa", false);
            //Play the lift animation
            SetSkeletalAnimation("AttackSt");
        }

        public void DisplayTeresa()
        {
            if (Render == null)
                return;

            //Show teresa
            Render.ToggleMeshes("HhStatue__mTeresa", true);
            //Play the appear animation
            SetSkeletalAnimation("TeresaAppear");
        }

        public void SetSkeletalAnimation(string name) {
            if (Render == null)
                return;

            SpawnAnimations.Clear();
            SpawnAnimations.Add(Render.SkeletalAnimations.FirstOrDefault(x => x.Name == name));

            animationAge = 0;
            foreach (var anim in SpawnAnimations)
                anim.Frame = 0;
        }

        public override List<STAnimation> GetAnimations() => Animations;

        public override void ResetAnimation() {
            if (Render == null)
                return;

            Render.ResetAnim();
        }
    }   
}
