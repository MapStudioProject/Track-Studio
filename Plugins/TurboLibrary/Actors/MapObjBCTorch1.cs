using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EffectLibrary;
using OpenTK;

namespace TurboLibrary.Actors
{
    public class MapObjBCTorch1 : ActorModelBase
    {
        public MapObjBCTorch1()
        {

        }

        public override void Init()
        {
            var matrix = Transform.TransformMatrix;
            EffectManager.Instance.EmitDirect(ref EffectHandle[0], "BCTorch1", "Torch", matrix);
        }

        public override void Calc()
        {
            //Frustum check
            bool inFrustum = true;
            if (FrustumRenderCheck != null)
                inFrustum = FrustumRenderCheck.InFrustum;
            //visiblity check
            bool isVisible = Visible && inFrustum;

            if (EffectHandle[0] != null)
                EffectHandle[0].Visible = isVisible;

            if (!isVisible || EffectHandle[0] == null)
                return;

            //The actor transform for converting to world space. 
            var transform = Transform.TransformMatrix;
            EffectHandle[0].emitterSet.SetMatrix(transform);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
