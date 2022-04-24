using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EffectLibrary;
using OpenTK;

namespace TurboLibrary.Actors
{
    public class MapObjWsFirering : ActorModelBase
    {
        public const int NUM_BALLS = 6;
        public const float DISTANCE = 100;
        public const float SPEED = 0.5f;

        public Handle[] Fireballs = new Handle[NUM_BALLS];

        public bool IsCounterClockwise => this.Parameters[0] == 1.0f;

        private float angle = 0;

        public MapObjWsFirering()
        {

        }

        public override void Init()
        {
            var matrix = Transform.TransformMatrix;

            for (int i = 0; i < Fireballs.Length; i++) {
                EffectManager.Instance.EmitDirect(ref Fireballs[i], "WsFirering", "WsFirering", matrix);
            }
        }

        public override void Calc()
        {
            for (int i = 0; i < Fireballs.Length; i++)
                if (Fireballs[i] != null)
                    Fireballs[i].Visible = this.Visible;

            if (!Visible)
                return;

            //The actor transform for converting to world space. 
            var transform = Transform.TransformMatrix;
            //Keep rotating along the Z axis
            float inputAngle = IsCounterClockwise ? angle-- : angle++;
            var rotationAnim = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(inputAngle * SPEED));

            for (int i = 0; i < NUM_BALLS; i++)
            {
                //Make sure the emitters are initialized and setup
                if (Fireballs[i]?.Init != true)
                    return;

                //Rotate the balls into a ring
                double angle = 2 * Math.PI * i / NUM_BALLS;

                Vector3 position = new Vector3(
                    MathF.Cos((float)angle),
                    MathF.Sin((float)angle), 0) * DISTANCE;

                //Animate the rotation
                position = Vector3.TransformPosition(position, rotationAnim);
                //Set the emitter matrix with the offset translation
                Fireballs[i].emitterSet.SetMatrix(Matrix4.CreateTranslation(position) * transform);
            }
        }

        public override void Dispose()
        {
            if (EffectManager.Instance == null)
                return;

            for (int i = 0; i < Fireballs.Length; i++)
                EffectManager.Instance.KillEmitterHandle(ref Fireballs[i]);
        }
    }
}
