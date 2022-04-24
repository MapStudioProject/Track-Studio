using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.Animations;
using OpenTK;
using TurboLibrary;

namespace TurboLibrary.MuuntEditor
{
    public class CameraAnimation : STAnimation
    {
        public CameraGroup Group = new CameraGroup();
        public EventHandler OnNextFrame;

        public object PropertyObject;

        public void Reload(ReplayCamera camera) {
            ReloadReplayCamera(camera);
        }

        public CameraAnimation(IntroCamera introCamera) {
            introCamera.PropertyChanged += delegate {
                ReloadIntroCamera(introCamera);
            };

            ReloadIntroCamera(introCamera);
        }

        public void ReloadReplayCamera(ReplayCamera camera)
        {
            if (Group.TranslateX.KeyFrames.Count > 0)
                return;

            PropertyObject = camera;
            FrameCount = 400;

            var group = new CameraGroupRollPitchYaw();

            if (camera.Path != null)
            {
                var paths = camera.Path;
                var timeStep = 400 / paths.Points.Count;
                for (int i = 0; i < paths.Points.Count; i++)
                {
                    float currentFrame = i * timeStep;

                    var point = paths.Points[i];
                    group.TranslateX.KeyFrames.Add(new STKeyFrame(currentFrame, camera.Translate.X + point.Translate.X));
                    group.TranslateY.KeyFrames.Add(new STKeyFrame(currentFrame, camera.Translate.Y + point.Translate.Y));
                    group.TranslateZ.KeyFrames.Add(new STKeyFrame(currentFrame, camera.Translate.Z + point.Translate.Z));
                    group.Roll.KeyFrames.Add(new STKeyFrame(currentFrame, MathHelper.DegreesToRadians(camera.Roll)));
                    group.Pitch.KeyFrames.Add(new STKeyFrame(currentFrame, MathHelper.DegreesToRadians(camera.Pitch)));
                    group.Yaw.KeyFrames.Add(new STKeyFrame(currentFrame, MathHelper.DegreesToRadians(camera.Yaw)));
                }
            }
            else
            {
                group.TranslateX.KeyFrames.Add(new STKeyFrame(0, camera.Translate.X));
                group.TranslateY.KeyFrames.Add(new STKeyFrame(0, camera.Translate.Y));
                group.TranslateZ.KeyFrames.Add(new STKeyFrame(0, camera.Translate.Z));
                group.Roll.KeyFrames.Add(new STKeyFrame(0, MathHelper.DegreesToRadians(camera.Roll)));
                group.Pitch.KeyFrames.Add(new STKeyFrame(0, MathHelper.DegreesToRadians(camera.Pitch)));
                group.Yaw.KeyFrames.Add(new STKeyFrame(0, MathHelper.DegreesToRadians(camera.Yaw)));
            }

            Group = group;
        }

        public void ReloadIntroCamera(IntroCamera introCamera)
        {
            if (Group.TranslateX.KeyFrames.Count > 0 || introCamera.Path.Points.Count == 0)
                return;

            PropertyObject = introCamera;
            FrameCount = introCamera.Time;

            var paths = introCamera.Path;
            var lookatPaths = introCamera.LookAtPath;

            var group = new CameraLookatGroup();

            var timeStep = introCamera.Time / paths.Points.Count;

            var points = InterpolatePoints(paths, introCamera.Time, timeStep);
            var lookAtpoints = InterpolatePoints(lookatPaths, introCamera.Time, timeStep);

            float endFovFrame = introCamera.FovySpeed * 60;

            //Start FOV
            group.FieldOfView.KeyFrames.Add(new STKeyFrame(0, introCamera.Fovy));
            //End FOV
            group.FieldOfView.KeyFrames.Add(new STKeyFrame(endFovFrame, introCamera.Fovy2));
            //Path to move camera
            group.TranslateX = points[0];
            group.TranslateY = points[1];
            group.TranslateZ = points[2];
            //Path to look at
            group.LookatTranslateX = lookAtpoints[0];
            group.LookatTranslateY = lookAtpoints[1];
            group.LookatTranslateZ = lookAtpoints[2];
            Group = group;
        }

        public override void NextFrame() {
            OnNextFrame?.Invoke(this, EventArgs.Empty);
        }

        public STAnimationTrack[] InterpolatePoints(Path path, int time, int step)
        {
            var interpolation = STInterpoaltionType.Linear;
            if (path.RailType == Path.RailInterpolation.Bezier)
                interpolation = STInterpoaltionType.Bezier;

            var tracks = new STAnimationTrack[3];
            tracks[0] = new STAnimationTrack() { InterpolationType = interpolation };
            tracks[1] = new STAnimationTrack() { InterpolationType = interpolation };
            tracks[2] = new STAnimationTrack() { InterpolationType = interpolation };

            float progress = 0.0f;
            for (int i = 0; i < path.Points.Count; i++)
            {
                var point = path.Points[i];
                if (i < path.Points.Count - 1)
                {
                    var nextPt = path.Points[i + 1];
                    var dist = (nextPt.Translate - point.Translate);
                    var length = new Vector3(dist.X, dist.Y, dist.Z).Length;

                }

                //The speed of the path to the dest point (0 for the last point)
                var speed = path.Points[i].Prm1;

                int currentFrame = (i * step);
                if (i == path.Points.Count - 1)
                    currentFrame = time;

                if (path.RailType == Path.RailInterpolation.Bezier)
                {
                    tracks[0].KeyFrames.Add(new STBezierKeyFrame(currentFrame,
                         point.Translate.X,
                         point.ControlPoints[0].X,
                         point.ControlPoints[1].X));
                    tracks[1].KeyFrames.Add(new STBezierKeyFrame(currentFrame,
                         point.Translate.Y,
                         point.ControlPoints[0].Y,
                         point.ControlPoints[1].Y));
                    tracks[2].KeyFrames.Add(new STBezierKeyFrame(currentFrame,
                         point.Translate.Z,
                         point.ControlPoints[0].Z,
                         point.ControlPoints[1].Z));
                }
                else
                {
                    tracks[0].KeyFrames.Add(new STKeyFrame(currentFrame, point.Translate.X));
                    tracks[1].KeyFrames.Add(new STKeyFrame(currentFrame, point.Translate.Y));
                    tracks[2].KeyFrames.Add(new STKeyFrame(currentFrame, point.Translate.Z));
                }
                progress += speed;
            }
            return tracks;
        }
    }

    public class CameraGroup : STAnimGroup
    {
        public STAnimationTrack TranslateX = new STAnimationTrack();
        public STAnimationTrack TranslateY = new STAnimationTrack();
        public STAnimationTrack TranslateZ = new STAnimationTrack();
        public STAnimationTrack FieldOfView = new STAnimationTrack();

        public CameraGroup()
        {
            TranslateX.InterpolationType = STInterpoaltionType.Linear;
            TranslateY.InterpolationType = STInterpoaltionType.Linear;
            TranslateZ.InterpolationType = STInterpoaltionType.Linear;
            FieldOfView.InterpolationType = STInterpoaltionType.Linear;
        }
    }

    public class CameraGroupRollPitchYaw : CameraGroup
    {
        public STAnimationTrack Roll = new STAnimationTrack();
        public STAnimationTrack Pitch = new STAnimationTrack();
        public STAnimationTrack Yaw = new STAnimationTrack();

        public CameraGroupRollPitchYaw() : base()
        {
            Roll.InterpolationType = STInterpoaltionType.Linear;
            Pitch.InterpolationType = STInterpoaltionType.Linear;
            Yaw.InterpolationType = STInterpoaltionType.Linear;
        }
    }

    public class CameraLookatGroup : CameraGroup
    {
        public STAnimationTrack LookatTranslateX = new STAnimationTrack();
        public STAnimationTrack LookatTranslateY = new STAnimationTrack();
        public STAnimationTrack LookatTranslateZ = new STAnimationTrack();

        public override List<STAnimationTrack> GetTracks()
        {
            return new List<STAnimationTrack>() {
                TranslateX, TranslateY, TranslateZ,
             LookatTranslateX, LookatTranslateY, LookatTranslateZ,};
        }

        public CameraLookatGroup() : base()
        {
            LookatTranslateX.InterpolationType = STInterpoaltionType.Linear;
            LookatTranslateY.InterpolationType = STInterpoaltionType.Linear;
            LookatTranslateZ.InterpolationType = STInterpoaltionType.Linear;
        }
    }
}
