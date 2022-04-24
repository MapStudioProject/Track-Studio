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
    public class CubePathToolSettings<TPath, TPoint> : IToolWindowDrawer
        where TPath : PathBase<TPath, TPoint>
        where TPoint : PathPointBase<TPath, TPoint>, new()
    {
        CubePathRender<TPath, TPoint> PathRender;
        CubePathEditor<TPath, TPoint> PathEditor;

        public CubePathToolSettings(CubePathEditor<TPath, TPoint> editor, CubePathRender<TPath, TPoint> render) {
            PathRender = render;
            PathEditor = editor;
        }

        public void Render()
        {
            var width = ImGui.GetWindowWidth();
            if (ImGui.BeginChild("groupChild1", new System.Numerics.Vector2(width, 200)))
            {
                RenderGroupEditor();
            }
            ImGui.EndChild();
            if (ImGui.BeginChild("convChild1", new System.Numerics.Vector2(width, 200)))
            {
                RenderConvertEditor();
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
                if (typeof(TPath) == typeof(GravityPath))
                {
                    if (ImGui.Button("From Lap Paths "))
                        PathEditor.ConvertPaths<LapPath, LapPathPoint>();
                }
                if (typeof(TPath) == typeof(GCameraPath))
                {
                    if (ImGui.Button("From Lap Paths "))
                        PathEditor.ConvertPaths<LapPath, LapPathPoint>();
                    if (ImGui.Button("From Gravity Paths "))
                        PathEditor.ConvertPaths<GravityPath, GravityPathPoint>();
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
