using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.Animations;
using CafeLibrary.Rendering;
using BfresLibrary;

namespace TurboLibrary.Actors
{
    public class MapObjKuribo : ActorModelBase
    {
        public BfresSkeletalAnim IdleAnimation { get; set; }

        public MapObjKuribo() {
        }

        public override void Init()
        {

        }

        public override List<STAnimation> GetAnimations()
        {
            List<STAnimation> animations = new List<STAnimation>();
            animations.Add(Render.SkeletalAnimations.FirstOrDefault(x => x.Name == "WalkT"));
            animations.Add(Render.MaterialAnimations.FirstOrDefault(x => x.Name == "KuriboEye"));
            return animations;
        }

        public override void ResetAnimation() {
            Render.ResetAnim();
        }
    }   
}
