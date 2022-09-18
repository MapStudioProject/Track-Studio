using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.ViewModels;
using ImGuiNET;
using GLFrameworkEngine;
using TurboLibrary;
using OpenTK;
using MapStudio.UI;

namespace TurboLibrary.MuuntEditor
{
    public class IntroCameraEditor : IMunntEditor
    {
        public string Name => TranslationSource.GetText("INTRO_CAMERAS");

        public CourseMuuntPlugin MapEditor { get; set; }

        public bool IsActive { get; set; } = false;

        private IToolWindowDrawer introCameraTool = new IntroCameraKeyEditor();
        public IToolWindowDrawer ToolWindowDrawer => introCameraTool;

        public List<IDrawable> Renderers { get; set; } = new List<IDrawable>();

        public NodeBase Root { get; set; } = new NodeBase(TranslationSource.GetText("INTRO_CAMERAS")) { HasCheckBox = true };
        public List<MenuItemModel> MenuItems { get; set; } = new List<MenuItemModel>();

        private IntroCameraKeyEditor KeyEditor = new IntroCameraKeyEditor();

        public List<NodeBase> GetSelected()
        {
            return Root.Children.Where(x => x.IsSelected).ToList();
        }

        public IntroCameraEditor(CourseMuuntPlugin editor, List<IntroCamera> objs)
        {
            MapEditor = editor;
            ReloadMenuItems();
            Init(objs);

            Root.Icon = IconManager.VIDEO_ICON.ToString(); 
        }

        public void DrawEditMenuBar() { }

        public void DrawHelpWindow() { }

        public void ReloadEditor()
        {
            Root.Header = TranslationSource.GetText("INTRO_CAMERAS");
            ReloadMenuItems();

            foreach (EditableObject render in Renderers)
                render.CanSelect = true;
        }

        public void Init(List<IntroCamera> objs)
        {
            foreach (var render in Renderers) {
                render?.Dispose();
            }

            Root.Children.Clear();
            Renderers.Clear();

            //Load the current tree list
            for (int i = 0; i < objs?.Count; i++)
                Add(Create(objs[i]));

            if (Root.Children.Any(x => x.IsSelected))
                Root.IsExpanded = true;
        }

        public void OnSave(CourseDefinition course)
        {
            course.IntroCameras = new List<IntroCamera>();

            foreach (EditableObject render in Renderers)
            {
                var obj = (IntroCamera)render.UINode.Tag;
                obj.Translate = new ByamlVector3F(
                    render.Transform.Position.X,
                    render.Transform.Position.Y,
                    render.Transform.Position.Z);
                obj.RotateDegrees = new ByamlVector3F(
                    render.Transform.RotationEulerDegrees.X,
                    render.Transform.RotationEulerDegrees.Y,
                    render.Transform.RotationEulerDegrees.Z);
                obj.Scale = new ByamlVector3F(
                    render.Transform.Scale.X,
                    render.Transform.Scale.Y,
                    render.Transform.Scale.Z);

                course.IntroCameras.Add(obj);
            }
        }

        public void OnMouseDown(MouseEventInfo mouseInfo) { }
        public void OnMouseUp(MouseEventInfo mouseInfo) { }
        public void OnMouseMove(MouseEventInfo mouseInfo) { }

        public void Add(EditableObject render, bool undo = false)
        {
            MapEditor.AddRender(render, undo);
        }

        public void Remove(EditableObject render, bool undo = false)
        {
            MapEditor.RemoveRender(render, undo);
        }

        /// <summary>
        /// When an object asset is drag and dropped into the viewport.
        /// </summary>
        public void OnAssetViewportDrop(int id, Vector2 screenPosition)
        {
        }

        public void OnKeyDown(KeyEventInfo keyInfo)
        {
            bool isActive = MapEditor.SubEditor == this.Name;

            if (isActive && keyInfo.IsKeyDown(InputSettings.INPUT.Scene.Create) && Root.Children.Count < 3)
                Add(Create(SetupNewIntroCamera()), true);
        }

        public void ReloadMenuItems()
        {
        }

        public void RemoveSelected()
        {
            var selected = Renderers.Where(x => ((EditableObject)x).IsSelected).ToList().ToList();
            foreach (EditableObject obj in selected)
                Remove(obj, true);
        }

        private IntroCamera SetupNewIntroCamera()
        {
            IntroCamera camera = IntroCamera.CreateFirst();
            if (Root.Children.Count == 1) camera = IntroCamera.CreateSecond();
            if (Root.Children.Count == 2) camera = IntroCamera.CreateThird();

            var position = EditorUtility.GetObjectPlacementPosition();
            camera.Translate = new ByamlVector3F(position.X, position.Y, position.Z);
            camera.Path = new Path();
            camera.LookAtPath = new Path();
            return camera;
        }

        private EditableObject Create(IntroCamera obj)
        {
            var courseFile = MapEditor.Resources.CourseDefinition;

            var render = new TransformableObject(Root);
            render.UINode.Header = $"Intro Camera {obj.Num}";
            render.UINode.Tag = obj;
            render.UINode.Icon = IconManager.VIDEO_ICON.ToString();

            //Make sure the course file is updated during edits as the camera key tool uses these
            render.RemoveCallback += delegate
            {
                if (courseFile.IntroCameras.Contains(obj))
                    courseFile.IntroCameras.Remove(obj);

                if (this.Renderers.Contains(render))
                    this.Renderers.Remove(render);

                //Remove references
                foreach (var editor in MapEditor.Editors)
                {
                    if (editor is RailPathEditor<Path, PathPoint>)
                    {
                        ((RailPathEditor<Path, PathPoint>)editor).Remove(obj.Path);
                        ((RailPathEditor<Path, PathPoint>)editor).Remove(obj.LookAtPath);
                    }
                }
            };
            render.AddCallback += delegate
            {
                if (courseFile.IntroCameras == null)
                    courseFile.IntroCameras = new List<IntroCamera>();

                if (!courseFile.IntroCameras.Contains(obj))
                    courseFile.IntroCameras.Add(obj);

                if (!this.Renderers.Contains(render))
                    this.Renderers.Add(render);

                //Reload references
                foreach (var editor in MapEditor.Editors)
                {
                    if (editor is RailPathEditor<Path, PathPoint>)
                    {
                        ((RailPathEditor<Path, PathPoint>)editor).ReloadPath(obj.Path);
                        ((RailPathEditor<Path, PathPoint>)editor).ReloadPath(obj.LookAtPath);
                    }
                }
            };
            //Update the render transform
            render.Transform.Position = new OpenTK.Vector3(
                obj.Translate.X,
                obj.Translate.Y,
                obj.Translate.Z);
            render.Transform.RotationEulerDegrees = new OpenTK.Vector3(
                obj.RotateDegrees.X,
                obj.RotateDegrees.Y,
                obj.RotateDegrees.Z);
            render.Transform.Scale = new OpenTK.Vector3(
                obj.Scale.X,
                obj.Scale.Y,
                obj.Scale.Z);
            render.Transform.UpdateMatrix(true);
            obj.PropertyChanged += delegate
            {
                render.UINode.Header = $"Intro Camera {obj.Num}";
            };
            return render;
        }
    }
}
