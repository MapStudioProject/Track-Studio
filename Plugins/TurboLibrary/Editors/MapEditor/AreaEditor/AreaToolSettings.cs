using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using MapStudio.UI;
using GLFrameworkEngine;
using CafeLibrary.Rendering;

namespace TurboLibrary.MuuntEditor
{
    public class AreaToolSettings : IToolWindowDrawer
    {
        public void Render()
        {
            bool refreshScene = false;

            if (ImGui.CollapsingHeader(TranslationSource.GetText("VISUALS"), ImGuiTreeNodeFlags.DefaultOpen))
            {
                refreshScene |= ImGui.Checkbox($"{TranslationSource.GetText("DRAW_FILLED")}", ref AreaRender.DrawFilled);
                if (AreaRender.DrawFilled)
                    refreshScene |= ImGui.DragFloat($"{TranslationSource.GetText("TRANSPARENCY")}", ref AreaRender.Transparency, 0.01f, 0, 1);
            }
            if (refreshScene)
                GLContext.ActiveContext.UpdateViewport = true;
        }
    }
}
