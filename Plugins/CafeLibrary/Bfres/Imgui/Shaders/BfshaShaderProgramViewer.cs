using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using ImGuiNET;
using System.IO;
using MapStudio.UI;
using CafeLibrary.Rendering;

namespace CafeLibrary
{
    public class BfshaShaderProgramViewer
    {
        static string VertexShaderSource;
        static string FragShaderSource;

        static string vertexShaderPath;
        static string fragmentShaderPath;

        static string selectedStage = "Vertex";
        static MemoryEditor MemoryEditor = new MemoryEditor();

        public static void Render(FMAT material)
        {
            var renderer = material.MaterialAsset as BfshaRenderer;

            if (ImGui.BeginCombo("Stage", selectedStage))
            {
                if (ImGui.Selectable("Vertex"))
                {
                    selectedStage = "Vertex";
                }
                if (ImGui.Selectable("Pixel"))
                {
                    selectedStage = "Pixel";
                }
                ImGui.EndCombo();
            }

            ImGui.BeginTabBar("menu_shader1");
            if (ImguiCustomWidgets.BeginTab("menu_shader1", $"Shader Code"))
            {
                if (renderer.GLShaderInfo != null)
                    LoadShaderStageCode(material);
                ImGui.EndTabItem();
            }
            if (ImguiCustomWidgets.BeginTab("menu_shader1", "Shader Info"))
            {
                if (ImGui.BeginChild("ShaderInfoC"))
                {
                    LoadShaderInfo(material);
                }
                ImGui.EndChild();
                ImGui.EndTabItem();
            }

            if (renderer.GLShaderInfo != null)
            {
                if (ImguiCustomWidgets.BeginTab("menu_shader1", "Shader Constants"))
                {
                    if (selectedStage == "Vertex")
                    {
                        var constants = renderer.GLShaderInfo.VertexConstants;
                        if (constants != null)
                            MemoryEditor.Draw(constants, constants.Length);
                    }
                    if (selectedStage == "Pixel")
                    {
                        var constants = renderer.GLShaderInfo.PixelConstants;
                        if (constants != null)
                            MemoryEditor.Draw(constants, constants.Length);
                    }
                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }

        static void LoadShaderInfo(FMAT material)
        {
            var renderer = material.MaterialAsset as BfshaRenderer;
            if (renderer.ProgramPasses.Count == 0)
                return;

            var shader = renderer.ShaderModel;
            var program = renderer.ProgramPasses[0];
            var variation = shader.GetShaderVariation(program);

            if (selectedStage == "Vertex")
            {
                if (ImGui.CollapsingHeader("Attributes", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    for (int i = 0; i < shader.Attributes.Count; i++) {
                        if (program.HasAttribute(i))
                            ImGui.Text($"In {shader.Attributes.GetKey(i)} Location {shader.Attributes[i].Location}");
                    }
                }
                if (ImGui.CollapsingHeader("Samplers", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    for (int i = 0; i < program.SamplerLocations.Length; i++)
                    {
                        var sampler = shader.Samplers.GetKey(i);
                        if (program.SamplerLocations[i].VertexLocation != -1)
                            ImGui.Text($"Sampler {sampler} Location {program.SamplerLocations[i].FragmentLocation}");
                    }
                }
                if (ImGui.CollapsingHeader("Uniform Blocks", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    for (int i = 0; i < program.UniformBlockLocations.Length; i++)
                    {
                        var block = shader.UniformBlocks.GetKey(i);
                        if (program.UniformBlockLocations[i].VertexLocation != -1)
                            ImGui.Text($"UniformBlock {block} Location {program.UniformBlockLocations[i].VertexLocation}");
                    }
                }

                if (variation != null)
                    ShowReflectionData(variation.BinaryProgram.ShaderReflection.VertexShaderCode);
            }
            if (selectedStage == "Pixel")
            {
                if (ImGui.CollapsingHeader("Samplers", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    for (int i = 0; i < program.SamplerLocations.Length; i++)
                    {
                        var sampler = shader.Samplers.GetKey(i);
                        if (program.SamplerLocations[i].FragmentLocation != -1)
                            ImGui.Text($"Sampler {sampler} Location {program.SamplerLocations[i].FragmentLocation}");
                    }
                }
                if (ImGui.CollapsingHeader("Uniform Blocks", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    for (int i = 0; i < program.UniformBlockLocations.Length; i++)
                    {
                        var block = shader.UniformBlocks.GetKey(i);
                        if (program.UniformBlockLocations[i].FragmentLocation != -1) {
                            ImGui.Text($"UniformBlock {block} Location {program.UniformBlockLocations[i].FragmentLocation}");

                            foreach (var uniform in shader.UniformBlocks[i].Uniforms)
                            {
                                ImGui.Text($"Uniform {uniform.Key} Offset {uniform.Value.Offset} {uniform.Value.Name}");
                            }
                        }
                    }
                }

                if (variation != null)
                    ShowReflectionData(variation.BinaryProgram.ShaderReflection.PixelShaderCode);
            }
        }

        static void ShowReflectionData(BfshaLibrary.ShaderReflectionData reflectionData)
        {
            if (reflectionData == null)
                return;

            if (ImGui.CollapsingHeader("Reflection Uniform Blocks", ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var value in reflectionData.ShaderConstantBufferDictionary)
                    ImGui.Text($"{value}");
            }
            if (ImGui.CollapsingHeader("Reflection Samplers", ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var value in reflectionData.ShaderSamplerDictionary)
                    ImGui.Text($"{value}");
            }
            if (ImGui.CollapsingHeader("Reflection Inputs", ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var value in reflectionData.ShaderInputDictionary)
                    ImGui.Text($"{value}");
            }
            if (ImGui.CollapsingHeader("Reflection Outputs", ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var value in reflectionData.ShaderOutputDictionary)
                    ImGui.Text($"{value}");
            }
        }

        static void LoadShaderStageCode(FMAT material)
        {
            var renderer = material.MaterialAsset as BfshaRenderer;

            if (vertexShaderPath != renderer.GLShaderInfo.VertPath)
            {
                vertexShaderPath = renderer.GLShaderInfo.VertPath;
                VertexShaderSource = System.IO.File.ReadAllText(vertexShaderPath);
            }
            if (fragmentShaderPath != renderer.GLShaderInfo.FragPath)
            {
                fragmentShaderPath = renderer.GLShaderInfo.FragPath;
                FragShaderSource = System.IO.File.ReadAllText(fragmentShaderPath);
            }

            if (ImGui.BeginChild("stage_window"))
            {
                var size = ImGui.GetWindowSize();
                if (selectedStage == "Vertex")
                {
                    ImGui.InputTextMultiline("Vertex", ref VertexShaderSource, 4000, size);
                }
                if (selectedStage == "Pixel")
                {
                    ImGui.InputTextMultiline("Pixel", ref FragShaderSource, 4000, size);
                }
            }
            ImGui.EndChild();
        }
    }
}
