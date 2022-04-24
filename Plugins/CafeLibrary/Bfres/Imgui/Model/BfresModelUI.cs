using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using MapStudio.UI;
namespace CafeLibrary
{
    public class BfresModelUI
    {
        public BfresModelUI()
        {

        }

        public void Render(FMDL fmdl)
        {
            ImGui.BeginTabBar("model_tab");
            if (ImguiCustomWidgets.BeginTab("model_tab", "Model Data"))
            {
                DrawModelData(fmdl);
                ImGui.EndTabItem();
            }
            if (ImguiCustomWidgets.BeginTab("model_tab", "User Data"))
            {
                UserDataInfoEditor.Render(fmdl.Model.UserData);
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }

        private void DrawModelData(FMDL fmdl)
        {
            ImGuiHelper.InputFromText(TranslationSource.GetText("NAME"), fmdl, "Name", 0x1000);
            ImGuiHelper.InputFromText(TranslationSource.GetText("PATH"), fmdl.Model, "Path", 0x1000);

            ImGui.Text($"Total Vertex Count: {fmdl.Model.TotalVertexCount}");
        }
    }
}
