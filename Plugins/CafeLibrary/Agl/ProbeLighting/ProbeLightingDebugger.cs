using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapStudio.UI;
using AGraphicsLibrary;
using ImGuiNET;
using UIFramework;

namespace CafeLibrary
{
    public class ProbeLightingDebugger : Window
    {
        public override string Name => "ProbeLightingDebugger";

        static LightProbeMgr.ProbeInfo Info;

        public static void SetProbeInfo(LightProbeMgr.ProbeInfo probe)
        {
            Info = probe;
        }

        public override void Render()
        {
            if (Info == null || Info.shData == null)
                return;

            //ImGui.Text(Info.Position.ToString());

            var data = LightProbeMgr.ConvertSH2RGB(Info.shData);

            if (ImGui.CollapsingHeader("Volume Info", ImGuiTreeNodeFlags.DefaultOpen))
            {
                for (int i = 0; i < Info.Volumes.Length; i++)
                {
                    if (Info.Volumes[i] == null)
                        continue;

                    ImGui.Text($"Volume_{i} VoxelState: {Info.Volumes[i].VoxelState}");
                    ImGui.Text($"Volume_{i} VoxelIndex: {Info.Volumes[i].VoxelIndex}");
                    ImGui.Text($"Volume_{i} VoxelIndices: {string.Join(',', Info.Volumes[i].VoxelIndices)}");
                    ImGui.Text($"Volume_{i} DataIndices (8 probes around): {string.Join(',', Info.Volumes[i].DataIndices)}");
                }
            }
            if (ImGui.CollapsingHeader("SH Data", ImGuiTreeNodeFlags.DefaultOpen))
            {
                for (int i = 0; i < 7; i++)
                    ImGui.Text(data[i].ToString());
            }
        }
    }
}
