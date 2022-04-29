using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using MapStudio.UI;
using GLFrameworkEngine;
using TurboLibrary;

namespace TurboLibrary.MuuntEditor
{
    public class PathToolSettings<TPath, TPoint> : IToolWindowDrawer
        where TPath : PathBase<TPath, TPoint>
        where TPoint : PathPointBase<TPath, TPoint>, new()
    {
        PathRender<TPath, TPoint> PathRender;
        PathEditor<TPath, TPoint> PathEditor;

        public PathToolSettings(PathEditor<TPath, TPoint> editor, PathRender<TPath, TPoint> render) {
            PathEditor = editor;
            PathRender = render;
        }

        public void Render()
        {
            var width = ImGui.GetWindowWidth();

            if (ImGui.BeginChild("convertMenu", new System.Numerics.Vector2(width, 50)))
            {
                RenderConvertEditor();
            }
            ImGui.EndChild();

            if (ImGui.BeginChild("groupChild1", new System.Numerics.Vector2(width, 200)))
            {
                RenderGroupEditor();
            }
            ImGui.EndChild();
        }

        private void RenderGroupEditor()
        {
            if (ImGui.CollapsingHeader(TranslationSource.GetText("GROUPS"), ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Auto Group", ref PathRender.AutoGroup);

                if (ImGui.Button(TranslationSource.GetText("SELECT"))) { SelectGroupPoints(true); }
                ImGui.SameLine();
                if (ImGui.Button(TranslationSource.GetText("DESELECT"))) { SelectGroupPoints(false); }
                ImGui.SameLine();
                if (ImGui.Button(TranslationSource.GetText("ADD"))) { PathRender.AddGroup((TPath)Activator.CreateInstance(typeof(TPath))); }
                ImGui.SameLine();
                if (ImGui.Button(TranslationSource.GetText("REMOVE"))) { PathRender.RemoveByGroup(PathRender.ActiveGroup); PathRender.FillTree(); }
                ImGui.SameLine();
                if (ImGui.Button(TranslationSource.GetText("GROUP_SELECTED"))) { PathRender.RegroupSelected(PathRender.ActiveGroup); }

                int index = 0;
                foreach (TPath group in PathRender.GetGroups())
                {
                    var selected = PathRender.ActiveGroup == group;
                    if (ImGui.Selectable($"{TranslationSource.GetText("GROUP")} {index++}", selected)) {
                        PathRender.ActiveGroup = group;
                    }
                }
            }
        }

        private void RenderConvertEditor()
        {
            if (ImGui.CollapsingHeader(TranslationSource.GetText("CONVERT"), ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (typeof(TPath) == typeof(EnemyPath))
                {
                    if (ImGui.Button("From Item Paths"))
                        PathEditor.ConvertPaths<ItemPath, ItemPathPoint>();
                }
                if (typeof(TPath) == typeof(ItemPath))
                {
                    if (ImGui.Button("From Enemy Paths"))
                        PathEditor.ConvertPaths<EnemyPath, EnemyPathPoint>();
                }
                if (typeof(TPath) == typeof(SteerAssistPath))
                {
                    if (ImGui.Button("From Enemy Paths"))
                        PathEditor.ConvertPaths<EnemyPath, EnemyPathPoint>();
                }
            }
        }

        private void SelectGroupPoints(bool toggle)
        {
            if (PathRender.ActiveGroup == null)
                return;

            var points = PathRender.PathPoints.Where(x => ((PathPoint<TPath, TPoint>)x).Group == PathRender.ActiveGroup).ToList();
            if (points.Count == 0)
                return;

            foreach (var point in points)
                point.IsSelected = toggle;
            GLContext.ActiveContext.UpdateViewport = true;
        }
    }
}
