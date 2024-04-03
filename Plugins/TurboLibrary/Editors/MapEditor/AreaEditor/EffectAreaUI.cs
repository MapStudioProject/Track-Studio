using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using MapStudio.UI;

namespace TurboLibrary
{
    public class EffectSWEditor
    {
        public static void Render(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName);
            Type type = prop.PropertyType;
            Type nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                type = nullableType;

            var effectSW = (EffectArea.EffectSWFlags)prop.GetValue(obj);

            if (ImGui.CollapsingHeader("Effects List (ELink/XLink)", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var width = ImGui.GetWindowWidth();

                if (obj is CourseDefinition)
                    ImGuiHelper.BoldText("These effects display at all times.");
                else
                    ImGuiHelper.BoldText("These effects display when the player is inside the area box.");

                if (ImGui.BeginChild("effectList", new System.Numerics.Vector2(width, 80)))
                {
                    for (int i = 0; i < 0x10; i++)
                    {
                        var effect = (EffectArea.EffectSWFlags)(1 << i);
                        if (!effectSW.HasFlag(effect))
                            continue;

                        ImGui.Selectable(effect.ToString());
                    }
                }
                ImGui.EndChild();

                ImGui.PushItemWidth(400);
                if (ImGui.BeginCombo("Select", ""))
                {
                    for (int i = 0; i < 0x10; i++)
                    {
                        var effect = (EffectArea.EffectSWFlags)(1 << i);
                        if (!effectSW.HasFlag(effect))
                        {
                            bool add = ImGui.Button($"   {IconManager.ADD_ICON}   ##addEffect{i}");
                            ImGui.SameLine();

                            if (add)
                            {
                                prop.SetValue(obj, (EffectArea.EffectSWFlags)BitUtility.SetBit((uint)effectSW, i, true));
                            }
                        }
                        else
                        {
                            bool remove = ImGui.Button($"   {IconManager.DELETE_ICON}   ##removeEffect{i}");
                            ImGui.SameLine();

                            if (remove)
                                prop.SetValue(obj, (EffectArea.EffectSWFlags)BitUtility.SetBit((uint)effectSW, i, false));
                        }

                        if (ImGui.Selectable(effect.ToString(), effectSW.HasFlag(effect)))
                        {

                        }
                    }

                    ImGui.EndCombo();
                }
                ImGui.PopItemWidth();
            }
        }
    }
}
