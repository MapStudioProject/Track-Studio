using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using AGraphicsLibrary;
using GLFrameworkEngine;
using ImGuiNET;
using MapStudio.UI;

namespace TurboLibrary.LightingEditor
{
    public class LightMapEditor
    {
        public string ActiveFile = "";

        bool UPDATE_LIGHTMAP_TEX = false;

        public void Render(GLContext context)
        {
            var lightMapList = LightingEngine.LightSettings.Resources.LightMapFiles;
            if (!lightMapList.ContainsKey(ActiveFile))
                ActiveFile = "";

            if (string.IsNullOrEmpty(ActiveFile) && lightMapList.Count > 0)
                ActiveFile = lightMapList.Keys.FirstOrDefault();

            if (ImGui.BeginCombo("Change File", ActiveFile))
            {
                foreach (var file in lightMapList)
                {
                    bool selected = ActiveFile == file.Key;
                    if (ImGui.Selectable(file.Key, selected))
                        ActiveFile = file.Key;

                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            ImGui.SameLine();
            bool add = ImGui.Button("+");
            ImGui.SameLine();
            bool remove = ImGui.Button("-");

            if (add)
            {

            }
            if (remove)
            {

            }
            if (string.IsNullOrEmpty(ActiveFile))
            {
                ImGui.Text("Select a light map .bagllmap to edit.");
                return;
            }
            ShowPropertyUI(context, lightMapList[ActiveFile]);
        }

        private int selectedIndex;

        private void ShowPropertyUI(GLContext context, AglLightMap lightmap)
        {
            ImGui.Columns(2);

            for (int i = 0; i < lightmap.LightAreas.Count; i++)
            {
                if (ImGui.Selectable(lightmap.LightAreas[i].Settings.Name, selectedIndex == i))
                    selectedIndex = i;
            }

            if (lightmap.Lightmaps.Count == 0)
            {
                var envFile = LightingEngine.LightSettings.Resources.EnvFiles["course_area.baglenv"];
                string name = lightmap.LightAreas[selectedIndex].Settings.Name;
                lightmap.GenerateLightmap(context, envFile, name);
            }

            ImGui.NextColumn();

            if (selectedIndex != -1)
            {
                if (ImGui.BeginChild("lightData"))
                {
                    DisplayLightMap(context, lightmap, selectedIndex);

                    int index = 0;
                    foreach (var lightSource in lightmap.LightAreas[selectedIndex].Lights)
                    {
                        if (string.IsNullOrEmpty(lightSource.Name))
                            continue;

                        if (ImGui.CollapsingHeader(lightSource.Name, ImGuiTreeNodeFlags.DefaultOpen)) {
                            ShowLightSourceUI(lightmap, lightSource, index++);
                        }
                    }
                }
                ImGui.EndChild();
            }

            ImGui.NextColumn();

            ImGui.Columns(1);
        }

        private void DisplayLightMap(GLContext context, AglLightMap lightmap, int index)
        {
            string name = lightmap.LightAreas[index].Settings.Name;

            bool update = !lightmap.LightAreas[index].Initialized;
            if (update || UPDATE_LIGHTMAP_TEX)
            {
                var envFile = LightingEngine.LightSettings.Resources.EnvFiles["course_area.baglenv"];

                lightmap.GenerateLightmap(context, envFile, name);
                UPDATE_LIGHTMAP_TEX = false;
            }

            var cubemap = lightmap.Lightmaps[name];
            var tex = EquirectangularRender.CreateTextureRender(cubemap, 0, 0, update);
            ImGui.Image((IntPtr)tex.ID, new Vector2(512, 512 / 3));
        }

        string[] LightTypes => new string[] { "AmbientLight", "DirectionalLight", "HemisphereLight" };

        private void ShowLightSourceUI(AglLightMap lmap, LightEnvObject lightSource, int index)
        {
            var envFile = LightingEngine.LightSettings.Resources.EnvFiles["course_area.baglenv"];

 
            if (ImGui.BeginCombo($"Name##{index}", lightSource.Name))
            {
                switch (lightSource.Type)
                {
                    case "HemisphereLight":
                        foreach (var light in envFile.HemisphereLights)
                        {
                            bool selected = lightSource.Name == light.Name;
                            if (ImGui.Selectable(light.Name, selected))
                            {
                                lightSource.Name = light.Name;
                                UPDATE_LIGHTMAP_TEX = true;
                            }

                            if (selected)
                                ImGui.SetItemDefaultFocus();
                        }
                    break;
                    case "DirectionalLight":
                        foreach (var light in envFile.DirectionalLights)
                        {
                            bool selected = lightSource.Name == light.Name;
                            if (ImGui.Selectable(light.Name, selected))
                            {
                                lightSource.Name = light.Name;
                                UPDATE_LIGHTMAP_TEX = true;
                            }

                            if (selected)
                                ImGui.SetItemDefaultFocus();
                        }
                        break;
                    case "AmbientLight":
                        foreach (var light in envFile.AmbientLights)
                        {
                            bool selected = lightSource.Name == light.Name;
                            if (ImGui.Selectable(light.Name, selected))
                            {
                                lightSource.Name = light.Name;
                                UPDATE_LIGHTMAP_TEX = true;
                            }

                            if (selected)
                                ImGui.SetItemDefaultFocus();
                        }
                        break;
                }


                ImGui.EndCombo();
            }

            if (ImGui.BeginCombo($"Type##{index}", lightSource.Type))
            {
                foreach (var type in LightTypes)
                {
                    bool selected = ActiveFile == type;
                    if (ImGui.Selectable(type, selected))
                        lightSource.Type = type;

                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            DrawEnvLightSettings(lightSource.Type, lightSource.Name, index);

            if (ImGui.BeginCombo($"Lookup Table##{index}", lightSource.LutName))
            {
                foreach (var lut in lmap.LutTable)
                {
                    bool selected = ActiveFile == lut.Name;
                    if (ImGui.Selectable(lut.Name, selected)) {
                        lightSource.LutName = lut.Name;
                        UPDATE_LIGHTMAP_TEX = true;
                    }
                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
        }

        private void DrawEnvLightSettings(string type, string name, int index)
        {
            string id = $"##{index}";

            var envFile = LightingEngine.LightSettings.Resources.EnvFiles["course_area.baglenv"];
            switch (type)
            {
                case "HemisphereLight":
                    var hemi = envFile.HemisphereLights.FirstOrDefault(x => x.Name == name);
                    if (hemi != null)
                        UPDATE_LIGHTMAP_TEX |= EnvironmentEditor.RenderProperies(hemi, id);
                    break;
                case "DirectionalLight":
                    var dirLight = envFile.DirectionalLights.FirstOrDefault(x => x.Name == name);
                    if (dirLight != null)
                        UPDATE_LIGHTMAP_TEX |= EnvironmentEditor.RenderProperies(dirLight, id);
                    break;
                case "AmbientLight":
                    var ambLight = envFile.AmbientLights.FirstOrDefault(x => x.Name == name);
                    if (ambLight != null)
                        UPDATE_LIGHTMAP_TEX |= EnvironmentEditor.RenderProperies(ambLight, id);
                    break;
            }
        }
    }
}
