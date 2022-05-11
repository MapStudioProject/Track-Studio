using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using KclLibrary;
using ImGuiNET;
using MapStudio.UI;
using Toolbox.Core;
using Toolbox.Core.ViewModels;

namespace TurboLibrary.CollisionEditor
{
    class CollisionPropertyUI
    {
        public static CollisionPresetData Preset;

        string CurrentPreset = "";

        KclPrism KclPrism;
        NodeBase Node;

        public CollisionPropertyUI(NodeBase node, KclPrism prism) {
            Node = node;
            KclPrism = prism;

            if (Preset != null)
            {
                if (Preset.MaterialPresets.ContainsKey(prism.CollisionFlags))
                    CurrentPreset = Preset.MaterialPresets[prism.CollisionFlags];
            }
        }

        public void Render()
        {
            int id = KclPrism.CollisionFlags;

            if (Preset == null)
            {
                CollisionPresetData.LoadPresets(Directory.GetFiles(System.IO.Path.Combine(Runtime.ExecutableDir,"Presets","Collision")));
                Preset = CollisionPresetData.CollisionPresets.FirstOrDefault();

                if (Preset.MaterialPresets.ContainsKey(KclPrism.CollisionFlags))
                    CurrentPreset = Preset.MaterialPresets[KclPrism.CollisionFlags];
            }

            ImGui.PushItemWidth(300);
            if (ImGui.BeginCombo(TranslationSource.GetText("PRESET"), CurrentPreset))
            {
                foreach (var mat in Preset.MaterialPresets)
                {
                    bool IsSelected = CurrentPreset == mat.Value;
                    if (ImGui.Selectable(mat.Value, IsSelected))
                    {
                        CurrentPreset = mat.Value;
                        KclPrism.CollisionFlags = mat.Key;
                    }
                    if (IsSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            ImGui.PopItemWidth();

            if (ImGui.DragInt(TranslationSource.GetText("ID"), ref id))
            {
                KclPrism.CollisionFlags = (ushort)id;
                Node.Header = $"Material {id.ToString("X4")}";

                if (Preset.MaterialPresets.ContainsKey(KclPrism.CollisionFlags))
                    CurrentPreset = Preset.MaterialPresets[KclPrism.CollisionFlags];
            }
        }
    }
}
