using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using MapStudio.UI;
using GLFrameworkEngine;
using Toolbox.Core.ViewModels;

namespace TurboLibrary.MuuntEditor
{
    public class ReplayCameraUI
    {
        public static void Render(ReplayCamera camera)
        {
            if (camera.Type == ReplayCamera.CameraMode.KartPathSearch || camera.Type == ReplayCamera.CameraMode.OnlineSpectator_PathSearch)
            {
                if (ImGui.CollapsingHeader("Paths", ImGuiTreeNodeFlags.DefaultOpen)) {
                    ImguiCustomWidgets.ObjectLinkSelector(TranslationSource.GetText("RELATIVE_PATH"), camera, "Path");
                }
            }
        }
    }
}
