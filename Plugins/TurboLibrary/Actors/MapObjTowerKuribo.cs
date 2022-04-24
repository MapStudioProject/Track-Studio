using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.Animations;
using CafeLibrary.Rendering;
using BfresLibrary;
using OpenTK;
using GLFrameworkEngine;

namespace TurboLibrary.Actors
{
    public class MapObjTowerKuribo : ActorModelBase
    {
        public float StackCount => Parameters[0];

        public BfresSkeletalAnim IdleAnimation { get; set; }

        public MapObjTowerKuribo() {
        }

        public override void Init()
        {

        }

        public override void Draw(GLContext context)
        {
            //Todo we need a better way of handling this
            //Drawing is still drawn outside the actor atm
         //   if (Render.Models.Count != StackCount)
             //   UpdateModelStack();
        }

        public void UpdateModelStack()
        {
            //Make sure the spawn count is higher than 0
            if (StackCount < 1)
                return;

            //Make sure the model has models adjusted based on the stack count
            //Cache the first model then stack each one on top with a different offset
            var modelCache = (BfresModelRender)Render.Models.FirstOrDefault();

            Vector3 offset = new Vector3();

            Render.Models.Clear();
            for (int i = 0; i < StackCount; i++) {
                if (i != 0)  {
                    var cached = BfresModelRender.CreateCache(modelCache);
                    Render.Models.Add(cached);
                }
                else
                    Render.Models.Add(modelCache);

                var transform = Matrix4.CreateTranslation(offset);
                ((BfresModelRender)Render.Models[i]).ModelTransform = transform;

                offset += new Vector3(0, 15, 0) * Render.Transform.Scale;
            }
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
