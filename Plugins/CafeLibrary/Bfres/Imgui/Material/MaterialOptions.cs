using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafeLibrary.Rendering;
using ImGuiNET;

namespace CafeLibrary
{
    public class MaterialOptions
    {
        static List<int> SelectedIndices = new List<int>();

        static Dictionary<string, string> LoadedOptions = new Dictionary<string, string>();
        static bool filter_defaults = true;

        public static void Reset()
        {
            LoadedOptions.Clear();
        }

        public static void Render(FMAT material)
        {
            if (LoadedOptions.Count == 0)
            {
                foreach (var op in material.ShaderOptions)
                    LoadedOptions.Add(op.Key, op.Value);
            }

            if (material.MaterialAsset is BfshaRenderer)
            {
                ImGui.Checkbox("Filter Default Options", ref filter_defaults);
            }

            RenderHeader(material);
            ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg]);

            if (ImGui.BeginChild("OPTION_LIST"))
            {
                ImGui.Columns(2);

                int index = 0;
                foreach (var option in LoadedOptions)
                {
                    if (filter_defaults && material.MaterialAsset is BfshaRenderer)
                    {
                        var m = material.MaterialAsset as BfshaRenderer;
                        if (m.ShaderModel.StaticOptions.ContainsKey(option.Key))
                        {
                            string value = option.Value;
                            if (value == "False") value = "0";
                            if (value == "True") value = "1";

                            var op = m.ShaderModel.StaticOptions[option.Key];
                            if (value == op.choices[(int)op.defaultIndex])
                                continue;
                        }
                    }

                    if (SelectedIndices.Contains(index))
                    {
                        if (ImGui.CollapsingHeader(option.Key, ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            string value = option.Value;
                            if (ImGui.InputText("", ref value, 100)) {
                                //Users should not edit these
                              //  material.ShaderOptions[option.Key] = value;
                            }
                        }
                    }
                    else if (ImGui.Selectable(option.Key, SelectedIndices.Contains(index)))
                    {
                        SelectedIndices.Clear();
                        SelectedIndices.Add(index);
                    }
                    ImGui.NextColumn();
                    ImGui.Text(option.Value);
                    ImGui.NextColumn();
                    index++;
                }
            }
            ImGui.EndChild();
            ImGui.PopStyleColor();

            ImGui.Columns(1);
        }

        static void RenderHeader(FMAT material)
        {
            ImGui.Columns(2);
            if (ImGui.Selectable("Name"))
            {
                LoadedOptions.Clear();

                foreach (var op in material.ShaderOptions.OrderBy(x => x.Key))
                    LoadedOptions.Add(op.Key, op.Value);
            }
            ImGui.NextColumn();
            if (ImGui.Selectable("Value"))
            {
                LoadedOptions.Clear();

                foreach (var op in material.ShaderOptions.OrderByDescending(x => x.Value))
                    LoadedOptions.Add(op.Key, op.Value);
            }
            ImGui.Separator();
            ImGui.Columns(1);
        }
    }
}
