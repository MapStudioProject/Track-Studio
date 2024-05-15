using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using ImGuiNET;
using BfresLibrary;
using System.Reflection;
using MapStudio.UI;
using CafeLibrary.Rendering;
using GLFrameworkEngine;

namespace CafeLibrary
{
    public class MaterialParameter
    {
        static bool drag = true;

        public static void Reset()
        {
            OriginalValues.Clear();
        }

        static List<int> selectedIndices = new List<int>();

        static bool limitUniformsUsedByShaderCode = true;

        static float columnSize1;
        static float columnSize2;
        static float columnSize3;

        public static void Render(FMAT material)
        {
            ShaderInfo shaderInfo = null;
            if (material.MaterialAsset is BfshaRenderer)
            {
                //Reload the uniform list (only does it once)
                ((BfshaRenderer)material.MaterialAsset).ReloadUniformList();
                shaderInfo = ((BfshaRenderer)material.MaterialAsset).GLShaderInfo;
            }

            if (ImGui.Button($"   {IconManager.COPY_ICON}   Copy To Clipboard"))
            {
                //Create a material copy to turn as json
                Material copiedMat = new Material();
                copiedMat.Name = material.Name;
                copiedMat.ShaderParams = material.Material.ShaderParams;
                //Turn to json and copy to clipboard
                var json = BfresLibrary.TextConvert.MaterialConvert.ToJson(copiedMat);
                ImGui.SetClipboardText(json);
            }
            ImGui.SameLine();
            if (ImGui.Button($"   {IconManager.PASTE_ICON}   Paste To Clipboard"))
            {
                var json = ImGui.GetClipboardText();
                var mat = BfresLibrary.TextConvert.MaterialConvert.FromJson(json);
                if (mat != null && mat.ShaderParams != null)
                {
                    //Instead of replacing all we just want to swap out existing values
                    foreach (var param in mat.ShaderParams)
                    {
                        if (material.Material.ShaderParams.ContainsKey(param.Key))
                        {
                            //Ensure param data types match incase other param types change between files.
                            if (material.Material.ShaderParams[param.Key].Type == param.Value.Type)
                                material.Material.ShaderParams[param.Key].DataValue = param.Value.DataValue;
                        }
                    }
                    material.UpdateMaterialBlock();
                    GLContext.ActiveContext.UpdateViewport = true;
                }
                else
                    TinyFileDialog.MessageBoxErrorOk("No parameters copied to clipboard!");
            }

            if (shaderInfo != null)
                ImGui.Checkbox("Display Only Used Uniforms From Shader", ref limitUniformsUsedByShaderCode);

            if (OriginalValues.Count == 0)
            {
                foreach (var param in material.Material.ShaderParams)
                    OriginalValues.Add(param.Key, param.Value.DataValue);
            }

            LoadHeaders();

            ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg]);

            if (ImGui.BeginChild("PARAM_LIST"))
            {
                int index = 0;
                foreach (var param in material.Material.ShaderParams.Values)
                {
                    if (limitUniformsUsedByShaderCode && shaderInfo != null &&
                        !shaderInfo.UsedVertexStageUniforms.Contains(param.Name) &&
                        !shaderInfo.UsedPixelStageUniforms.Contains(param.Name) &&
                        param.Name != "gsys_area_env_index_diffuse")
                        continue;

                    if (material.AnimatedParams.ContainsKey(param.Name))
                    {
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0, 0.5f, 0, 1));
                        LoadParamColumns(material, material.AnimatedParams[param.Name], index++, true);
                        ImGui.PopStyleColor();
                    }
                    else
                        LoadParamColumns(material, param, index++);
                }
            }
            ImGui.EndChild();
            ImGui.PopStyleColor();
        }

        static void LoadHeaders()
        {
            ImGui.Columns(3);
            if (ImGui.Selectable("Name"))
            {

            }
            columnSize1 = ImGui.GetColumnWidth();
            ImGui.NextColumn();
            if (ImGui.Selectable("Value"))
            {
            }
            columnSize2 = ImGui.GetColumnWidth();
            ImGui.NextColumn();
            if (ImGui.Selectable("Colors (If Used)"))
            {
            }
            columnSize3 = ImGui.GetColumnWidth();
            ImGui.Separator();
            ImGui.Columns(1);
        }

        static void LoadParamColumns(FMAT material, ShaderParam param, int index, bool animated = false)
        {
            ImGui.Columns(3);

            if (selectedIndices.Contains(index))
            {
                ImGui.Columns(1);
                if (ImGui.CollapsingHeader(param.Name, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    LoadParamUI(material, param, $"##{param.Name}", drag);

                    if (ImGui.BeginPopupContextItem("paramPopup", ImGuiPopupFlags.MouseButtonRight))
                    {
                        if (ImGui.MenuItem("Insert Keyframe"))
                            material.TryInsertParamAnimKey(param);

                        ImGui.EndPopup();
                    }

                    if (OriginalValues[param.Name] != param.DataValue)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button("Reset"))
                        {
                            param.DataValue = OriginalValues[param.Name];
                            material.OnParamUpdated(param);

                            GLFrameworkEngine.GLContext.ActiveContext.UpdateViewport = true;
                        }
                    }
                }
                ImGui.Columns(3);
            }
            else
            {
                if (animated)
                {
                    ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0, 0.5f, 0, 1));
                    ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
                }

                ImGui.SetColumnWidth(0, columnSize1);
                ImGui.SetColumnWidth(1, columnSize2);
                ImGui.SetColumnWidth(2, columnSize3);

                if (ImGui.Selectable(param.Name, selectedIndices.Contains(index), ImGuiSelectableFlags.SpanAllColumns))
                {
                    selectedIndices.Clear();
                    selectedIndices.Add(index);
                }

                ImGui.NextColumn();
                ImGui.Text(GetDataString(param));
                ImGui.NextColumn();

                if (animated)
                {
                    ImGui.PopStyleColor();
                    ImGui.PopStyleVar();
                }

                if (param.Type == ShaderParamType.Float4)
                {
                    if (param.Name.Contains("color") || param.Name.Contains("Color"))
                        ImGuiHelper.InputFloatsFromColor4Button("", param, "DataValue", ImGuiColorEditFlags.AlphaPreviewHalf | ImGuiColorEditFlags.HDR);
                }
                else if (param.Type == ShaderParamType.Float3)
                {
                    if (param.Name.Contains("color") || param.Name.Contains("Color"))
                        ImGuiHelper.InputFloatsFromColor3Button("", param, "DataValue", ImGuiColorEditFlags.HDR);
                }

                ImGui.NextColumn();

                ImGui.Columns(1);
            }
        }

        static Dictionary<string, object> OriginalValues = new Dictionary<string, object>();

        static void LoadParamUI(FMAT material, ShaderParam param, string label = "", bool drag = false)
        {
            bool updated = false;
            switch (param.Type)
            {
                case ShaderParamType.Bool:
                    updated |= ImGuiHelper.InputFromBoolean(label, param, "DataValue"); break;
                case ShaderParamType.Int: 
                    updated |= ImGuiHelper.InputFromInt(label, param, "DataValue", 1, drag); break;
                case ShaderParamType.UInt:
                    updated |= ImGuiHelper.InputFromUint(label, param, "DataValue", 1, drag); break;
                case ShaderParamType.Float: 
                    updated |= ImGuiHelper.InputFromFloat(label, param, "DataValue", drag); break;
                case ShaderParamType.Float2:
                    updated |= ImGuiHelper.InputFloatsFromVector2(label, param, "DataValue", drag);
                    break;
                case ShaderParamType.Float3:
                    {
                        if (param.Name.Contains("color") || param.Name.Contains("Color"))
                            updated |= ImGuiHelper.InputFloatsFromColor3(label, param, "DataValue");
                        else
                            updated |= ImGuiHelper.InputFloatsFromVector3(label, param, "DataValue", drag);
                    }
                    break;
                case ShaderParamType.Float4:
                    {
                        if (param.Name.Contains("color") || param.Name.Contains("Color"))
                            updated |= ImGuiHelper.InputFloatsFromColor4(label, param, "DataValue", ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.AlphaPreviewHalf);
                        else
                            updated |= ImGuiHelper.InputFloatsFromVector4(label, param, "DataValue", drag);
                    }
                    break;
                case ShaderParamType.Srt2D:
                    {
                        Srt2D value = (Srt2D)param.DataValue;
                        var pos = new Vector2(value.Translation.X, value.Translation.Y);
                        var scale = new Vector2(value.Scaling.X, value.Scaling.Y);
                        var rot = value.Rotation;

                        bool edited0 = ImGui.DragFloat2("Scale", ref scale);
                        bool edited1 = ImGui.DragFloat("Rotate", ref rot, 0.1f);
                        bool edited2 = ImGui.DragFloat2("Translate", ref pos);
                        if (edited0 || edited1 || edited2)
                        {
                            param.DataValue = new Srt2D()
                            {
                                Scaling = new Syroot.Maths.Vector2F(scale.X, scale.Y),
                                Translation = new Syroot.Maths.Vector2F(pos.X, pos.Y),
                                Rotation = rot,
                            };
                            updated = true;
                        }
                    }
                    break;
                case ShaderParamType.TexSrt:
                case ShaderParamType.TexSrtEx:
                    {
                        TexSrt value = (TexSrt)param.DataValue;
                        var pos = new Vector2(value.Translation.X, value.Translation.Y);
                        var scale = new Vector2(value.Scaling.X, value.Scaling.Y);
                        var rot = value.Rotation;

                        var mode = value.Mode;
                        ImguiCustomWidgets.ComboScrollable("Mode", value.Mode.ToString(), ref mode, () =>
                        {
                            param.DataValue = new TexSrt()
                            {
                                Mode = mode,
                                Scaling = new Syroot.Maths.Vector2F(scale.X, scale.Y),
                                Translation = new Syroot.Maths.Vector2F(pos.X, pos.Y),
                                Rotation = rot,
                            };
                            GLContext.ActiveContext.UpdateViewport = true;
                            material.BatchEditParams(param);
                            material.OnParamUpdated(param, true);
                        });
                        bool edited0 = ImGui.DragFloat2("Scale", ref scale);
                        bool edited1 = ImGui.DragFloat("Rotate", ref rot, 0.1f);
                        bool edited2 = ImGui.DragFloat2("Translate", ref pos);
                        if (edited0 || edited1 || edited2)
                        {
                            param.DataValue = new TexSrt()
                            {
                                Mode = mode,
                                Scaling = new Syroot.Maths.Vector2F(scale.X, scale.Y),
                                Translation = new Syroot.Maths.Vector2F(pos.X, pos.Y),
                                Rotation = rot,
                            };
                            updated = true;
                        }
                    }
                    break;
            }
            if (updated) {
                GLContext.ActiveContext.UpdateViewport = true;
                material.BatchEditParams(param);
                material.OnParamUpdated(param, true);
            }
        }

        static string GetDataString(ShaderParam Param)
        {
            switch (Param.Type)
            {
                case ShaderParamType.Float:
                case ShaderParamType.UInt:
                    return Param.DataValue.ToString();
                case ShaderParamType.Float2:
                case ShaderParamType.Float3:
                case ShaderParamType.Float4:
                    return string.Join(",", (float[])Param.DataValue);
                case ShaderParamType.Srt2D:
                    {
                        var texSrt = (Srt2D)Param.DataValue;
                        return $"{texSrt.Scaling.X} {texSrt.Scaling.Y} {texSrt.Rotation} {texSrt.Translation.X} {texSrt.Translation.Y}";
                    }
                case ShaderParamType.TexSrt:
                case ShaderParamType.TexSrtEx:
                    {
                        var texSrt = (TexSrt)Param.DataValue;
                        return $"{texSrt.Mode} {texSrt.Scaling.X} {texSrt.Scaling.Y} {texSrt.Rotation} {texSrt.Translation.X} {texSrt.Translation.Y}";
                    }
                default:
                    return Param.DataValue.ToString();
            }
        }
    }
}
