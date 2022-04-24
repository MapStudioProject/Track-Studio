using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;

namespace TurboLibrary.Actors
{
    public class DemoCameraDirector : ActorBase
    {
        public State CameraState = State.Default;

        public IntroCamera ActiveIntroCamera = null;

        public DemoCameraDirector()
        {

        }

        public override void Calc()
        {
            base.Calc();

            if (CameraState == State.IntroCamera)
                UpdateDemoCamera();
        }

        public void UpdateDemoCamera() {
            if (ActiveIntroCamera == null)
                return;

            var path = ActiveIntroCamera.Path;
            var lookAt = ActiveIntroCamera.LookAtPath;

            var camera = GLContext.ActiveContext.Camera;

            var point = new ByamlVector3F();
            var lookPoint = new ByamlVector3F();

            camera.SetKeyframe(CameraAnimationKeys.PositionX, point.X);
            camera.SetKeyframe(CameraAnimationKeys.PositionY, point.Y);
            camera.SetKeyframe(CameraAnimationKeys.PositionZ, point.Z);
            camera.SetKeyframe(CameraAnimationKeys.TargetX, lookPoint.X);
            camera.SetKeyframe(CameraAnimationKeys.TargetY, lookPoint.Y);
            camera.SetKeyframe(CameraAnimationKeys.TargetZ, lookPoint.Z);
        }

        public enum State
        {
            Default,
            IntroCamera,
            ReplayCamera,
        }
    }
}
