using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using TurboLibrary;
using GLFrameworkEngine;
using MapStudio.UI;

namespace TurboLibrary.MuuntEditor
{
    public class IntroCameraKeyEditor : UIFramework.Window, IToolWindowDrawer
    {
        AnimationPlayer Player;

        IntroCamera EditableCamera = null;
        IntroCamera SelectedCamera = null;

        CameraAnimation AnimationHandle;
        IntroCameraDisplay AnimationDisplay;

        bool displayAnimationPath = true;

        public override string Name => TranslationSource.GetText("INTRO_CAMERA_EDITOR");

        public IntroCameraKeyEditor()
        {
            Opened = false;
            AnimationDisplay = new IntroCameraDisplay();
            Player = new AnimationPlayer();
            Player.OnFrameChanged += CycleThroughAnimationByOrder;
        }

        public override void Render()
        {
            if (GLContext.ActiveContext != null) {
                var scene = GLContext.ActiveContext.Scene;
                if (!scene.Objects.Contains(AnimationDisplay))
                    scene.AddRenderObject(AnimationDisplay);
            }

            var mapEditor = Workspace.ActiveWorkspace.ActiveEditor as CourseMuuntPlugin;
            var courseFile = mapEditor.Resources.CourseDefinition;

            //Display animation player button
            DrawAnimPlayer();
            //Editable camera UI
            if (EditableCamera != null) {
                ImGui.SameLine();
                ImGui.Checkbox("Display Path", ref displayAnimationPath);

                if (AnimationDisplay.IsVisible != displayAnimationPath) {
                    AnimationDisplay.IsVisible = displayAnimationPath;
                    GLContext.ActiveContext.UpdateViewport = true;
                }

                RenderEditorUI();
                return;
            }
            //Add a new camera if there is less than 3
            bool hasMaxIntroCamera = courseFile.IntroCameras?.Count == 3;

            if (hasMaxIntroCamera) {
                ImGui.Text("Max amount of cameras reached for creating!");
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
            }

            if (ImGui.Button("Add Camera") && !hasMaxIntroCamera)
            {
                if (Player.IsPlaying)
                {
                    Player.Stop();

                    var viewCamera = GLContext.ActiveContext.Camera;
                    viewCamera.ResetAnimations();
                }

                if (courseFile.IntroCameras == null)
                    courseFile.IntroCameras = new List<IntroCamera>();

                CreateCamera(courseFile);
                return;
            }

            if (hasMaxIntroCamera)
                ImGui.PopStyleColor();

            ImGui.SameLine();
            if (ImGui.Button("Remove Camera"))
            {
                //Nothing selected so skip
                if (SelectedCamera == null)
                    return;

                Player.Stop();
                courseFile.IntroCameras.Remove(SelectedCamera);
                //Remove paths
                if (SelectedCamera.Path != null)
                    courseFile.Paths.Remove(SelectedCamera.Path);
                if (SelectedCamera.LookAtPath != null)
                    courseFile.Paths.Remove(SelectedCamera.LookAtPath);

                foreach (var editor in mapEditor.Editors)
                {
                    if (editor is RailPathEditor<Path, PathPoint>)
                    {
                        ((RailPathEditor<Path, PathPoint>)editor).Remove(SelectedCamera.Path);
                        ((RailPathEditor<Path, PathPoint>)editor).Remove(SelectedCamera.LookAtPath);
                    }
                    if (editor is IntroCameraEditor)
                    {
                        ((IntroCameraEditor)editor).Init(courseFile.IntroCameras);
                    }
                }

                SelectedCamera = null;
            }

            if (courseFile.IntroCameras == null)
                return;

            foreach (var camera in courseFile.IntroCameras)
            {
                bool isSelected = SelectedCamera == camera;
                if (ImGui.Selectable($"Camera {camera.Num}", isSelected))
                {
                    SelectedCamera = camera;
                    PreparePlayer();
                }

                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0)) {
                    EditableCamera = camera;
                    selectedPointIndex = -1;
                }
            }
        }

        private void DrawAnimPlayer()
        {
            if (Player.IsPlaying)
            {
                if (ImGui.Button("Stop")) {
                    Player.Stop();

                    var viewCamera = GLContext.ActiveContext.Camera;
                    viewCamera.ResetAnimations();
                }
            }
            else
            {
                if (ImGui.Button("Play")) {
                    PreparePlayer();
                    Player.Play();
                }
            }
        }

        //Make the animations switch to the next when the animation player finishes playing one.
        private void CycleThroughAnimationByOrder(object sender, EventArgs e)
        {
            var anim = Player.CurrentAnimations.FirstOrDefault();
            if (anim == null)
                return;

            if (Player.CurrentFrame >= anim.FrameCount - 1)
            {
                var mapEditor = Workspace.ActiveWorkspace.ActiveEditor as CourseMuuntPlugin;
                var courseFile = mapEditor.Resources.CourseDefinition;

                //Get the currently selected camera.
                var selectedIndex = courseFile.IntroCameras.IndexOf(this.SelectedCamera);
                //Select the next camera in the list if the amount is supported
                if (selectedIndex + 1 < courseFile.IntroCameras.Count)
                    SelectedCamera = courseFile.IntroCameras[selectedIndex + 1];
                else //If it is the last one then use the first camera
                    SelectedCamera = courseFile.IntroCameras.FirstOrDefault();
                //Reload the player with the selection changed
                PreparePlayer();
            }
        }

        private int selectedPointIndex = -1;

        private void RenderEditorUI() {
            var camera = EditableCamera;
            var path = camera.Path;
            var lookAtPath = camera.LookAtPath;

            AnimationDisplay.Path = path;
            AnimationDisplay.LookPath = lookAtPath;

            //Create a list of points for the user to edit in

            //For the editor we need to insert the current camera position
            //Then the lookat position based on the camera direction and an offset distance

            int index = path.Points.Count;
            bool edit = false;

            ImGuiHelper.BeginBoldText();
            ImGui.Text($"Camera: {camera.Num}");

            ImGui.BulletText("Move camera to desired location.");
            ImGui.BulletText("Add or update an existing point to capture the current camera placement.");
            ImGuiHelper.EndBoldText();

            if (selectedPointIndex != -1)
            {
                bool changedSpeed = false;

                float lookAtSpeed = path.Points[selectedPointIndex].Prm1;
                float pointSpeed = lookAtPath.Points[selectedPointIndex].Prm1;

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Move/Look Speed");
                ImGui.SameLine();

                ImGui.PushItemWidth(100);
                changedSpeed |= ImGui.InputFloat("##MoveSpeed", ref pointSpeed);
                ImGui.SameLine();
                changedSpeed |= ImGui.InputFloat("##LookSpeed", ref lookAtSpeed);
                ImGui.PopItemWidth();

                if (changedSpeed)
                {
                    path.Points[selectedPointIndex].Prm1 = lookAtSpeed;
                    lookAtPath.Points[selectedPointIndex].Prm1 = pointSpeed;
                }
            }
            else
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("");
            }

            if (ImGui.Button($"Add Point")) {
                edit = true;
                index = path.Points.Count;

                path.Points.Add(new PathPoint());
                lookAtPath.Points.Add(new PathPoint());

                if (selectedPointIndex == -1)
                    selectedPointIndex = 0;
            }

            if (selectedPointIndex != -1)
            {
                ImGui.SameLine();
                //Apply edit with the selectable point
                if (ImGui.Button($"Update Point"))
                {
                    edit = true;
                    index = selectedPointIndex;
                }

                ImGui.SameLine();
                //Removeable points
                if (ImGui.Button($"Remove Point"))
                {
                    path.Points.RemoveAt(selectedPointIndex);
                    lookAtPath.Points.RemoveAt(selectedPointIndex);

                    if (selectedPointIndex >= path.Points.Count)
                        selectedPointIndex -= 1;

                    //Reset if no points exist
                    if (path.Points.Count == 0)
                        selectedPointIndex = -1;

                    GLContext.ActiveContext.UpdateViewport = true;
                }
            }

            //Allow individual points to be adjustable if needed
            for (int i = 0; i < path.Points.Count; i++)
            {
                var isSelected = selectedPointIndex == i;
                if (ImGui.Selectable($"Point_{i}", isSelected)) {
                    selectedPointIndex = i;
                }

                //Update the camera view to display the camera capture on double click
                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
                {
                    var lookAtPoint = lookAtPath.Points[i];
                    var point = path.Points[i];

                    var pos = new OpenTK.Vector3(
                        point.Translate.X,
                        point.Translate.Y,
                        point.Translate.Z);
                    var lookAtPos = new OpenTK.Vector3(
                                   lookAtPoint.Translate.X,
                                   lookAtPoint.Translate.Y,
                                   lookAtPoint.Translate.Z);

                    var viewCamera = GLContext.ActiveContext.Camera;
                    viewCamera.TargetPosition = pos;
                    viewCamera.RotateFromLookat(pos, lookAtPos);
                    GLContext.ActiveContext.UpdateViewport = true;
                }
            }

            //Edit a point at a specified index
            if (edit)
            {
                var viewCamera = GLContext.ActiveContext.Camera;
                var pos = viewCamera.GetViewPostion();
                var lookAtPos = viewCamera.GetLookAtPostion();

                path.Points[index] = new PathPoint()
                {
                    Translate = new ByamlVector3F(pos.X, pos.Y, pos.Z),
                    Rotate = new ByamlVector3F(),
                    Scale = new ByamlVector3F(1, 1, 1),
                    Prm1 = 200,
                };
                lookAtPath.Points[index] = new PathPoint()
                {
                    Translate = new ByamlVector3F(lookAtPos.X, lookAtPos.Y, lookAtPos.Z),
                    Rotate = new ByamlVector3F(),
                    Scale = new ByamlVector3F(1, 1, 1),
                    Prm1 = 200,
                };
                GLContext.ActiveContext.UpdateViewport = true;
            }

            if (ImGui.Button("Finish")) {
                EditableCamera = null;
                AnimationDisplay.IsVisible = false;
                GLContext.ActiveContext.UpdateViewport = true;

                var mapEditor = Workspace.ActiveWorkspace.ActiveEditor as CourseMuuntPlugin;

                foreach (var editor in mapEditor.Editors) {
                    if (editor is RailPathEditor<Path, PathPoint>) {
                        ((RailPathEditor<Path, PathPoint>)editor).ReloadPath(camera.Path);
                        ((RailPathEditor<Path, PathPoint>)editor).ReloadPath(camera.LookAtPath);
                    }
                    if (editor is IntroCameraEditor)
                    {
                        var courseFile = mapEditor.Resources.CourseDefinition;
                       ((IntroCameraEditor)editor).Init(courseFile.IntroCameras);
                    }
                }
            }
        }

        private void CreateCamera(CourseDefinition courseFile) {

            IntroCamera camera = null;

            //Add the cameras by order number. The user can remove cameras out of order
            if (!courseFile.IntroCameras.Any(x => x.Num == 1))
                camera = IntroCamera.CreateFirst();
            else if (!courseFile.IntroCameras.Any(x => x.Num == 2))
                camera = IntroCamera.CreateSecond();
            else if (!courseFile.IntroCameras.Any(x => x.Num == 3))
                camera = IntroCamera.CreateThird();
            else
                return;

            //Select and edit the new camera
            EditableCamera = camera;
            SelectedCamera = camera;

            //Create 2 path objects
            camera.LookAtPath = new Path();
            camera.Path = new Path();
            //Add them to the file.
            if (courseFile.Paths == null)
                courseFile.Paths = new List<Path>();

            courseFile.IntroCameras.Add(camera);
            courseFile.Paths.Add(camera.Path);
            courseFile.Paths.Add(camera.LookAtPath);

            //Reorder the cameras by number
            courseFile.IntroCameras = courseFile.IntroCameras.OrderBy(x => x.Num).ToList();
            selectedPointIndex = -1;
        }

        private void PreparePlayer()
        {
            if (SelectedCamera == null)
                return;

            var mapEditor = Workspace.ActiveWorkspace.ActiveEditor as CourseMuuntPlugin;
            var courseFile = mapEditor.Resources.CourseDefinition;
            foreach (var editor in mapEditor.Editors)
            {
                if (editor is RailPathEditor<Path, PathPoint>) {
                    ((RailPathEditor<Path, PathPoint>)editor).OnSave(courseFile);
                }
            }

            var camera = SelectedCamera;
            if (camera.Path == null || camera.Path.Points.Count == 0)
                return;

            AnimationHandle = new CameraAnimation(camera);
            AnimationHandle.OnNextFrame += (s, e) =>
            {
                CameraAnimation cameraAnim = (CameraAnimation)s;
                var group = (CameraLookatGroup)cameraAnim.Group;
                float pointX = group.TranslateX.GetFrameValue(cameraAnim.Frame);
                float pointY = group.TranslateY.GetFrameValue(cameraAnim.Frame);
                float pointZ = group.TranslateZ.GetFrameValue(cameraAnim.Frame);
                float lookatX = group.LookatTranslateX.GetFrameValue(cameraAnim.Frame);
                float lookatY = group.LookatTranslateY.GetFrameValue(cameraAnim.Frame);
                float lookatZ = group.LookatTranslateZ.GetFrameValue(cameraAnim.Frame);
                float fov = group.FieldOfView.GetFrameValue(cameraAnim.Frame);

                var viewCamera = GLContext.ActiveContext.Camera;
                viewCamera.RotationLookat = true;
                viewCamera.SetKeyframe(CameraAnimationKeys.PositionX, pointX);
                viewCamera.SetKeyframe(CameraAnimationKeys.PositionY, pointY);
                viewCamera.SetKeyframe(CameraAnimationKeys.PositionZ, pointZ);
                viewCamera.SetKeyframe(CameraAnimationKeys.TargetX, lookatX);
                viewCamera.SetKeyframe(CameraAnimationKeys.TargetY, lookatY);
                viewCamera.SetKeyframe(CameraAnimationKeys.TargetZ, lookatZ);
                viewCamera.SetKeyframe(CameraAnimationKeys.FieldOfView, OpenTK.MathHelper.DegreesToRadians(fov));
            };
            Player.AddAnimation(AnimationHandle, "");
        }
    }

    class IntroCameraDisplay : IDrawable
    {
        LineRender PathRender;
        LineRender LookPathRender;

        SphereRender SphereRender;

        public Path Path;
        public Path LookPath;

        StandardMaterial LineMaterial = new StandardMaterial();

        public bool IsVisible { get; set; } = true;

        public void Init()
        {
            PathRender = new LineRender(OpenTK.Graphics.OpenGL.PrimitiveType.LineStrip);
            LookPathRender = new LineRender(OpenTK.Graphics.OpenGL.PrimitiveType.LineStrip);
            SphereRender = new SphereRender(0.1f);
        }

        public void DrawModel(GLContext context, Pass pass)
        {
            if (PathRender == null)
                Init();

            if (Path == null) return;

            List<OpenTK.Vector3> lookPoints = new List<OpenTK.Vector3>();
            List<OpenTK.Vector3> points = new List<OpenTK.Vector3>();

            for (int i = 0; i < Path.Points.Count; i++)
            {
                var point = Path.Points[i];
                var lookAtPoint = LookPath.Points[i];

                points.Add(new OpenTK.Vector3(
                    point.Translate.X,
                    point.Translate.Y,
                    point.Translate.Z));
                lookPoints.Add(new OpenTK.Vector3(
                    lookAtPoint.Translate.X,
                    lookAtPoint.Translate.Y,
                    lookAtPoint.Translate.Z));

                SphereRender.DrawSolid(context, OpenTK.Matrix4.CreateTranslation(points[i]), OpenTK.Vector4.One);
                SphereRender.DrawSolid(context, OpenTK.Matrix4.CreateTranslation(lookPoints[i]), OpenTK.Vector4.One);
            }

            LineMaterial.Render(context);

            PathRender.Draw(points, true);
            LookPathRender.Draw(lookPoints, true);
        }

        public void Dispose()
        {

        }
    }
}
