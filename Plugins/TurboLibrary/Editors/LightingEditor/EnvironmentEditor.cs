using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Numerics;
using AGraphicsLibrary;
using ImGuiNET;
using MapStudio.UI;
using Toolbox.Core;
using GLFrameworkEngine;

namespace TurboLibrary.LightingEditor
{
    public class EnvironmentEditor
    {
        private int selectedAreaIndex = 0;
        private string selectedPreset = "";
        private string savePreset = "";

        public bool Render(GLContext context)
        {
            bool propertyChanged = false;
            bool updateLightMap = false;

            if (ImGui.Button("Reset Fog (All Areas)"))
            {
                var courseArea = LightingEngine.LightSettings.Resources.EnvFiles["course_area.baglenv"];

                for (int i = 0; i < courseArea.FogObjects.Count; i++)
                {
                    //Reset fog to defaults
                    courseArea.FogObjects[i].Enable = false;
                    courseArea.FogObjects[i].Color = new STColor(1, 1, 1, 1);
                    courseArea.FogObjects[i].Direction = new Syroot.Maths.Vector3F(0, 0, -1);
                    courseArea.FogObjects[i].Start = 1000.0f;
                    courseArea.FogObjects[i].End = 10000.0f;
                }
                GLContext.ActiveContext.UpdateViewport = true;
            }
            ImGuiHelper.Tooltip("Removes fog in all areas.");

            if (ImGui.BeginCombo($"Preset", selectedPreset))
            {
                if (Directory.Exists("Presets\\Env"))
                {
                    foreach (var file in Directory.GetFiles("Presets\\Env"))
                    {
                        string name = System.IO.Path.GetFileNameWithoutExtension(file);
                        bool isSelected = name == selectedPreset;
                        if (ImGui.Selectable(name, isSelected)) {
                            selectedPreset = name;
                            LoadPreset(file);
                            updateLightMap = true;
                            propertyChanged = true;
                        }

                        if (isSelected)
                            ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }
            ImGui.SameLine();
            if (ImGui.Button("Save Preset")) {
                ImGui.OpenPopup("envpreset");
            }

            if (ImGui.BeginPopup("envpreset"))
            {
                ImGui.Text("Name"); ImGui.SameLine();
                ImGui.InputText("##Name", ref savePreset, 100);

                if (ImGui.Button("Save")) {
                    SavePreset(savePreset);
                    selectedPreset = savePreset;
                }
                ImGui.EndPopup();
            }

            //Env data is done per area.
            //The ideal way of making this functional, is having areas possible to highlight via a shader
            //Use a dropdown to select what area to use
            if (ImGui.BeginCombo($"Area", $"Area_{selectedAreaIndex}"))
            {
                //Prepare a debug draw for area indices
                CafeLibrary.Rendering.BfresRender.DrawDebugAreaID = true;

                for (int i = 0; i < GetAreaCount(); i++)
                {
                    bool isSelected = selectedAreaIndex == i;

                    ImGui.ColorButton("", GetAreaColor(i));
                    ImGui.SameLine();
                    if (ImGui.Selectable($"Area_{i}", isSelected)) {
                        selectedAreaIndex = i;
                    }

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            else
                CafeLibrary.Rendering.BfresRender.DrawDebugAreaID = false;

            propertyChanged |= DrawAreaUI(selectedAreaIndex);

            if (updateLightMap)
                LightingEngine.LightSettings.UpdateAllLightMaps(context);

            if (propertyChanged)
                context.UpdateViewport = true;

            return propertyChanged;
        }

        private void LoadPreset(string filePath)
        {
            var courseArea = LightingEngine.LightSettings.Resources.EnvFiles["course_area.baglenv"];
            courseArea.LoadFile(filePath);
        }

        private void SavePreset(string name)
        {
            if (!Directory.Exists($"{Runtime.ExecutableDir}\\Presets\\Env"))
                Directory.CreateDirectory($"{Runtime.ExecutableDir}\\Presets\\Env");

            var courseArea = LightingEngine.LightSettings.Resources.EnvFiles["course_area.baglenv"];
            courseArea.SaveFile($"{Runtime.ExecutableDir}\\Presets\\Env\\{name}.baglenv");
        }

        private Vector4 GetAreaColor(int index)
        {
            switch (index)
            {
                case 0: return new Vector4(1, 0, 0, 1);
                case 1: return new Vector4(0, 1, 0, 1);
                case 2: return new Vector4(0, 0, 1, 1);
                case 3: return new Vector4(1, 1, 0, 1);
                case 4: return new Vector4(0, 1, 1, 1);
                case 5: return new Vector4(1, 0, 1, 1);
            }
            return Vector4.Zero;
        }

        private bool DrawAreaUI(int index)
        {
            var courseArea = LightingEngine.LightSettings.Resources.EnvFiles["course_area.baglenv"];
            bool updateLightMap = false;
            bool updateViewport = false;

            var width = ImGui.GetWindowWidth();

            var color = ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg];
            ImGui.PushStyleColor(ImGuiCol.ChildBg, color);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

            ImGui.BeginChild("dirChild", new Vector2(width, 150));
            //Each area only uses one directional light source
            if (ImGui.CollapsingHeader("Sun Lights", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.BeginColumns("dirColumns", 5);
                ImGui.SetColumnWidth(0, 125);
                ImGui.SetColumnWidth(1, 80);
                ImGui.SetColumnWidth(2, 80);
                ImGui.SetColumnWidth(3, 80);

                ImGuiHelper.BeginBoldText();
                ImGui.Text("");
                ImGui.NextColumn();
                ImGui.Text("Color");
                ImGui.NextColumn();
                ImGui.Text("Backside");
                ImGui.NextColumn();
                ImGui.Text("Amount");
                ImGui.NextColumn();
                ImGui.Text("Direction");
                ImGui.NextColumn();
                ImGuiHelper.EndBoldText();

                updateLightMap |= RenderProperies(courseArea.DirectionalLights[0], $"##dir{0}");

                ImGui.EndColumns();
            }
            ImGui.EndChild();
            ImGui.BeginChild("hemiChild", new Vector2(width, 150));

            if (ImGui.CollapsingHeader("Hemi Lights", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.BeginColumns("hemiColumns", 4);
                ImGui.SetColumnWidth(0, 125);
                ImGui.SetColumnWidth(1, 80);
                ImGui.SetColumnWidth(2, 80);
                ImGui.SetColumnWidth(3, 80);

                ImGuiHelper.BeginBoldText();
                ImGui.Text("");
                ImGui.NextColumn();
                ImGui.Text("Sky");
                ImGui.NextColumn();
                ImGui.Text("Ground");
                ImGui.NextColumn();
                ImGui.Text("Amount");
                ImGui.NextColumn();
                ImGuiHelper.EndBoldText();

                for (int i = 0; i < courseArea.HemisphereLights.Count; i++)
                {
                    if (courseArea.HemisphereLights[i].Name != $"HemiLight_chara{index}" &&
                        courseArea.HemisphereLights[i].Name != $"HemiLight_course{index}")
                        continue;

                    updateLightMap |= RenderProperies(courseArea.HemisphereLights[i], $"##hemi{i}");
                }
                ImGui.EndColumns();
            }
            ImGui.EndChild();
            ImGui.BeginChild("fogChild", new Vector2(width, 150));

            if (ImGui.CollapsingHeader("Fog", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.BeginColumns("fogColumns", 4);
                ImGui.SetColumnWidth(0, 125);
                ImGui.SetColumnWidth(1, 80);
                ImGui.SetColumnWidth(2, 100);
                ImGui.SetColumnWidth(3, 100);

                ImGuiHelper.BeginBoldText();
                ImGui.Text("");
                ImGui.NextColumn();
                ImGui.Text("Color");
                ImGui.NextColumn();
                ImGui.Text("Start");
                ImGui.NextColumn();
                ImGui.Text("End");
                ImGui.NextColumn();
                ImGuiHelper.EndBoldText();

                for (int i = 0; i < courseArea.FogObjects.Count; i++)
                {
                    if (courseArea.FogObjects[i].Name != $"Fog_Main{index}" &&
                        courseArea.FogObjects[i].Name != $"Fog_Y{index}")
                        continue;

                    updateViewport |= RenderProperies(courseArea.FogObjects[i], $"##fog{i}");
                }
                ImGui.EndColumns();
            }
            ImGui.EndChild();
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();

            if (updateLightMap)
                LightingEngine.LightSettings.UpdateAllLightMaps(GLFrameworkEngine.GLContext.ActiveContext);

            return updateLightMap || updateViewport;
        }

        public static int GetAreaCount() => 5;

        public static bool RenderProperies(HemisphereLight hemi, string id)
        {
            bool edited = false;

            if (!hemi.Enable)
             {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0, 1, 0, 0.2f));
            }
            else {
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(1, 0, 0, 0.2f));
            }

            ImGui.AlignTextToFramePadding();
            bool toggle = ImGui.Selectable(hemi.Name);
            edited |= toggle;

            ImGui.NextColumn();

            edited |= EditColor($"##SkyColor", $"{id}_0", hemi, "SkyColor");
            ImGui.NextColumn();

            edited |= EditColor($"##GroundColor", $"{id}_1", hemi, "GroundColor");
            ImGui.NextColumn();

            ImGui.PushItemWidth(ImGui.GetColumnWidth());

            edited |= ImGuiHelper.InputFromFloat($"##Intensity{id}", hemi, "Intensity", true);
            ImGui.PopItemWidth();

            ImGui.NextColumn();

            if (!hemi.Enable)
                ImGui.PopStyleColor(2);
            else
                ImGui.PopStyleColor(1);

            if (toggle)
                hemi.Enable = !hemi.Enable;

            return edited;
        }

        public static bool RenderProperies(DirectionalLight dirLight, string id)
        {
            bool edited = false;

            if (!dirLight.Enable)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0, 1, 0, 0.2f));
            }
            else {
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(1, 0, 0, 0.2f));
            }

            ImGui.AlignTextToFramePadding();
            bool toggle = ImGui.Selectable($"{dirLight.Name}");
            edited |= toggle;

            ImGui.NextColumn();

            edited |= EditColor($"##Diffuse Color", $"{id}_0", dirLight, "DiffuseColor");
            ImGui.NextColumn();

            edited |= EditColor($"##Backside Color", $"{id}_1", dirLight, "BacksideColor");
            ImGui.NextColumn();

            ImGui.PushItemWidth(ImGui.GetColumnWidth());

            edited |= ImGuiHelper.InputFromFloat($"##Intensity{id}", dirLight, "Intensity", true);
            ImGui.PopItemWidth();

            ImGui.NextColumn();

            ImGui.PushItemWidth(ImGui.GetColumnWidth());
            edited |= EditVector3($"##VDirection{id}", dirLight, "Direction");
            ImGui.PopItemWidth();

            /* if (ImGui.Button("Edit"))
             {
                 ImGui.OpenPopup($"##Directionpop{id}");
             }

             if (ImGui.BeginPopup($"##Directionpop{id}"))
             {
                 edited |= EditVector3($"##VDirection{id}", dirLight, "Direction");

                 ImGui.EndPopup();
             }*/

            ImGui.NextColumn();

            if (!dirLight.Enable)
                ImGui.PopStyleColor(2);
            else
                ImGui.PopStyleColor(1);

            if (toggle)
                dirLight.Enable = !dirLight.Enable;

            return edited;
        }

        public static bool RenderProperies(AmbientLight ambLight, string id)
        {
            bool edited = false;

            edited |= ImGuiHelper.InputFromBoolean($"Enable{id}", ambLight, "Enable");

            edited |= EditColor($"Color", $"{id}_0", ambLight, "Color");
            edited |= ImGuiHelper.InputFromFloat($"Intensity{id}", ambLight, "Intensity");

            return edited;
        }

        public static bool RenderProperies(Fog fog, string id)
        {
            bool edited = false;

            if (!fog.Enable)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0, 1, 0, 0.2f));
            }
            else {
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(1, 0, 0, 0.2f));
            }

            ImGui.AlignTextToFramePadding();
            bool toggle = ImGui.Selectable(fog.Name);
            edited |= toggle;

            ImGui.NextColumn();

            edited |= EditColor($"##Color", $"{id}_0", fog, "Color");
            ImGui.NextColumn();

            ImGui.PushItemWidth(100);
            edited |= ImGuiHelper.InputFromFloat($"##Start{id}", fog, "Start", true);
            ImGui.NextColumn();

            ImGui.PopItemWidth();
            ImGui.PushItemWidth(100);

            edited |= ImGuiHelper.InputFromFloat($"##End{id}", fog, "End", true);
            ImGui.NextColumn();

            ImGui.PopItemWidth();

            if (!fog.Enable)
                ImGui.PopStyleColor(2);
            else
                ImGui.PopStyleColor(1);

            if (toggle)
                fog.Enable = !fog.Enable;

            return edited;
        }

        static bool EditColor(string label, string id, object obj, string properyName)
        {
            var input = obj.GetType().GetProperty(properyName);
            var inputValue = (STColor)input.GetValue(obj);
            var color = new Vector4(inputValue.R, inputValue.G, inputValue.B, inputValue.A);

            var flags = ImGuiColorEditFlags.HDR;

            if (ImGui.ColorButton($"##colorBtn{id}", color, flags, new Vector2(200, 22)))
            {
                ImGui.OpenPopup($"colorPicker{id}");
            }

            bool edited = false;
            if (ImGui.BeginPopup($"colorPicker{id}"))
            {
                if (ImGui.ColorPicker4("##picker", ref color, flags 
                    | ImGuiColorEditFlags.Float | ImGuiColorEditFlags.DisplayRGB
                    | ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.DisplayHSV))
                {
                    input.SetValue(obj, new STColor()
                    {
                        R = color.X,
                        G = color.Y,
                        B = color.Z,
                        A = color.W,
                    });
                    edited = true;
                }
                ImGui.EndPopup();
            }
            return edited;
        }

        static bool EditVector3(string label, object obj, string properyName)
        {
            var input = obj.GetType().GetProperty(properyName);
            var inputValue = (Syroot.Maths.Vector3F)input.GetValue(obj);
            var vec3 = new Vector3(inputValue.X, inputValue.Y, inputValue.Z);

            bool edited = ImGui.DragFloat3(label, ref vec3, 0.005f);
            if (edited)
            {
                input.SetValue(obj, new Syroot.Maths.Vector3F(vec3.X, vec3.Y, vec3.Z));
            }
            return edited;
        }
    }
}
