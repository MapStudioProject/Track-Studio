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
using CafeLibrary.Rendering;

namespace TurboLibrary.MuuntEditor
{
    public class ClipAreaEditor : AreaEditor
    {
        private const uint MAX_CLIP_AREAS = 64;

        public override string Name => TranslationSource.GetText("CLIP_AREAS");

        public override NodeBase Root { get; set; } = new NodeBase(TranslationSource.GetText("CLIP_AREAS")) { HasCheckBox = true };
        public ClipAreaEditor(CourseMuuntPlugin editor, List<ClipArea> areas) 
        {
            Root.Icon = MapEditorIcons.CLIP_AREA.ToString();
            Root.IconColor = MapEditorIcons.CLIP_AREA_COLOR;

            MapEditor = editor;
            ReloadMenuItems();
            Init(areas);

            var addMenu = new MenuItemModel(this.Name, () => PlaceNewArea());
            GLContext.ActiveContext.Scene.MenuItemsAdd.Add(addMenu);
        }

        public override PrmObject Create() { return new ClipArea(); }

        public override void ReloadEditor()
        {
            Root.Header = TranslationSource.GetText("CLIP_AREAS");
            ReloadMenuItems();

            foreach (AreaRender render in Renderers)
                render.CanSelect = true;
        }

        void Init(List<ClipArea> areas)
        {
            Root.Children.Clear();
            Renderers.Clear();

            //Load the current tree list
            for (int i = 0; i < areas?.Count; i++)
                Add(Create(areas[i]));

            if (Root.Children.Any(x => x.IsSelected))
                Root.IsExpanded = true;
        }

        public override void OnSave(CourseDefinition course)
        {
            course.ClipAreas = new List<ClipArea>();

            foreach (AreaRender render in Renderers)
            {
                var obj = (ClipArea)render.UINode.Tag;
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

                course.ClipAreas.Add(obj);
            }
        }

        private AreaRender PlaceNewArea()
        {
            var render = Create(new ClipArea());

            //Get the default placements for our new object
            EditorUtility.SetObjectPlacementPosition(render.Transform);

            return render;
        }

        public override void Add(AreaRender render, bool undo = false)
        {
            //Clips have a max limit of areas that can be inserted.
            if (Renderers.Count >= MAX_CLIP_AREAS) {
                TinyFileDialog.MessageBoxErrorOk($"Max amount of clip areas ({MAX_CLIP_AREAS}) reached!");
                return;
            }

            MapEditor.AddRender(render, undo);
        }

        public override AreaRender Create(PrmObject obj)
        {
            var area = obj as ClipArea;

            var render = new AreaRender(Root, new Vector4(0.2f, 0.2f, 0.2f, 1));
            render.UINode.Header = GetDisplayName(area, 0);
            render.UINode.Tag = area;
            render.UINode.Icon = Root.Icon;
            render.UINode.IconColor = Root.IconColor;
            //Remove the area from the course def for clip previewing
            render.RemoveCallback += delegate
            {
                Renderers.Remove(render);

                if (MapEditor.MapLoader.CourseDefinition.ClipAreas == null)
                    MapEditor.MapLoader.CourseDefinition.ClipAreas = new List<ClipArea>();

                var clipAreas = MapEditor.MapLoader.CourseDefinition.ClipAreas;
                if (clipAreas.Contains(area))
                    clipAreas.Remove(area);
            };
            //Add the area from the course def for clip previewing
            render.AddCallback += delegate
            {
                Renderers.Add(render);

                if (MapEditor.MapLoader.CourseDefinition.ClipAreas == null)
                    MapEditor.MapLoader.CourseDefinition.ClipAreas = new List<ClipArea>();

                var clipAreas = MapEditor.MapLoader.CourseDefinition.ClipAreas;
                if (!clipAreas.Contains(area))
                    clipAreas.Add(area);
            };

            this.Root.Children.CollectionChanged += delegate
            {
                render.UINode.Header = GetDisplayName(area, render.UINode.Index);
            };
            //Update the render transform
            render.Transform.Position = new OpenTK.Vector3(
                area.Translate.X,
                area.Translate.Y,
                area.Translate.Z);
            render.Transform.RotationEulerDegrees = new OpenTK.Vector3(
                area.RotateDegrees.X,
                area.RotateDegrees.Y,
                area.RotateDegrees.Z);
            render.Transform.Scale = new OpenTK.Vector3(
                area.Scale.X,
                area.Scale.Y,
                area.Scale.Z);
            render.Transform.UpdateMatrix(true);
            area.PropertyChanged += delegate
            {
                int index = render.UINode.Index;
                render.UINode.Header = GetDisplayName(area, index);
                GLContext.ActiveContext.UpdateViewport = true;
            };
            render.Transform.TransformUpdated += delegate
            {
                var obj = (ClipArea)render.UINode.Tag;
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
            };
            return render;
        }

        public string GetDisplayName(ClipArea area, int index)
        {
            return $"Area_{index}";
        }
    }
}

