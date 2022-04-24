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
    public class EffectAreaEditor : AreaEditor
    {
        public override string Name => TranslationSource.GetText("EFFECT_AREAS");

        public override NodeBase Root { get; set; } = new NodeBase(TranslationSource.GetText("EFFECT_AREAS")) { HasCheckBox = true };

        public EffectAreaEditor(CourseMuuntPlugin editor, List<EffectArea> areas)
        {
            Root.Icon = MapEditorIcons.EFFECT_AREA.ToString();
            Root.IconColor = MapEditorIcons.EFFECT_AREA_COLOR;

            MapEditor = editor;
            ReloadMenuItems();
            Init(areas);

            var addMenu = new MenuItemModel(this.Name, () => PlaceNewArea());
            GLContext.ActiveContext.Scene.MenuItemsAdd.Add(addMenu);
        }

        public override PrmObject Create() { return new EffectArea(); }

        public override void ReloadEditor()
        {
            Root.Header = TranslationSource.GetText("EFFECT_AREAS");
            ReloadMenuItems();

            foreach (AreaRender render in Renderers)
                render.CanSelect = true;
        }

        void Init(List<EffectArea> areas)
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
            course.EffectAreas = new List<EffectArea>();

            foreach (AreaRender render in Renderers)
            {
                var obj = (EffectArea)render.UINode.Tag;
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

                course.EffectAreas.Add(obj);
            }
        }

        private AreaRender PlaceNewArea()
        {
            var render = Create(new EffectArea());

            //Get the default placements for our new object
            EditorUtility.SetObjectPlacementPosition(render.Transform);

            return render;
        }

        public override AreaRender Create(PrmObject obj)
        {
            var area = obj as EffectArea;

            var render = new AreaRender(Root, new Vector4(0, 1, 1, 1));
            render.UINode.Header = GetDisplayName(area, 0);
            render.UINode.Tag = area;
            render.UINode.Icon = Root.Icon;
            render.UINode.IconColor = Root.IconColor;
            render.AddCallback += delegate
            {
                Renderers.Add(render);
            };
            render.RemoveCallback += delegate
            {
                Renderers.Remove(render);
            };
            ((EditableObjectNode)render.UINode).UIProperyDrawer += delegate
            {
                EffectSWEditor.Render(area, "EffectSW");
                ImguiBinder.LoadProperties(area, Workspace.ActiveWorkspace.GetSelected());
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
            return render;
        }

        public string GetDisplayName(EffectArea area, int index)
        {
            return $"Area_{index}";
        }
    }
}
