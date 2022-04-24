using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using BfresLibrary.GX2;
using BfresLibrary;
using MapStudio.UI;

namespace CafeLibrary
{
    public class RenderStateEditor
    {
        public static void Render(FMAT material)
        {
            bool edited = false;

            var renderState = material.Material.RenderState;
            if (ImGui.CollapsingHeader("Culling", ImGuiTreeNodeFlags.DefaultOpen))
            {
                edited |= ImGuiHelper.ComboFromEnum<FMAT.CullMode>("Cull Mode", material, "CullState");
            }
            if (ImGui.CollapsingHeader("Alpha Control", ImGuiTreeNodeFlags.DefaultOpen))
            {
                edited |= ImGuiHelper.ComboFromEnum<RenderStateFlagsMode>("Render State", renderState, "FlagsMode");
                edited |= ImGuiHelper.InputFromBoolean("Alpha Test Enabled", renderState.AlphaControl, "AlphaTestEnabled");
                edited |= ImGuiHelper.ComboFromEnum<GX2CompareFunction>("Depth Function", renderState.AlphaControl, "AlphaFunc");
                edited |= ImGuiHelper.InputFromFloat("Alpha Reference", renderState, "AlphaRefValue");
            }
            if (ImGui.CollapsingHeader("Blend Control", ImGuiTreeNodeFlags.DefaultOpen))
            {
                edited |= ImGuiHelper.ComboFromEnum<RenderStateFlagsBlendMode>("Blend Mode", renderState, "FlagsBlendMode");

                ImGui.LabelText("Color Calculation", CreateBlendMethod(
                    renderState.BlendControl.ColorSourceBlend,
                    renderState.BlendControl.ColorCombine,
                    renderState.BlendControl.ColorDestinationBlend));

                edited |= ImGuiHelper.ComboFromEnum<GX2BlendFunction>("Color Source", renderState.BlendControl, "ColorSourceBlend");
                edited |= ImGuiHelper.ComboFromEnum<GX2BlendCombine>("Color Combine", renderState.BlendControl, "ColorCombine");
                edited |= ImGuiHelper.ComboFromEnum<GX2BlendFunction>("Color Destination", renderState.BlendControl, "ColorDestinationBlend");

                ImGui.LabelText("Alpha Calculation", CreateBlendMethod(
                    renderState.BlendControl.AlphaSourceBlend,
                    renderState.BlendControl.AlphaCombine,
                    renderState.BlendControl.AlphaDestinationBlend));

                edited |= ImGuiHelper.ComboFromEnum<GX2BlendFunction>("Alpha Source", renderState.BlendControl, "AlphaSourceBlend");
                edited |= ImGuiHelper.ComboFromEnum<GX2BlendCombine>("Alpha Combine", renderState.BlendControl, "AlphaCombine");
                edited |= ImGuiHelper.ComboFromEnum<GX2BlendFunction>("Alpha Destination", renderState.BlendControl, "AlphaDestinationBlend");

                ImGuiHelper.InputFromUint("Blend Target", renderState, "BlendTarget");
            }
            if (ImGui.CollapsingHeader("Color Control", ImGuiTreeNodeFlags.DefaultOpen))
            {
                edited |= ImGuiHelper.InputFromBoolean("ColorBuffer Enabled", renderState.ColorControl, "ColorBufferEnabled");
                edited |= ImGuiHelper.InputFromBoolean("MultiWrite Enabled", renderState.ColorControl, "MultiWriteEnabled");
                edited |= ImGuiHelper.InputFromByte("Blend Enable Mask", renderState.ColorControl, "BlendEnableMask");
                edited |= ImGuiHelper.ComboFromEnum<GX2LogicOp>("Logic Op", renderState.ColorControl, "LogicOp");
            }
            if (ImGui.CollapsingHeader("Depth Control", ImGuiTreeNodeFlags.DefaultOpen))
            {
                edited |= ImGuiHelper.InputFromBoolean("Depth Test Enabled", renderState.DepthControl, "DepthTestEnabled");
                edited |= ImGuiHelper.InputFromBoolean("Depth Write Enabled", renderState.DepthControl, "DepthWriteEnabled");
                edited |= ImGuiHelper.ComboFromEnum<GX2CompareFunction>("Depth Function", renderState.DepthControl, "DepthFunc");
            }

            if (edited)
                ReloadMaterial(material);
        }

        static void ReloadMaterial(FMAT mat)
        {
            mat.Material.RenderState.PolygonControl.CullBack = mat.CullBack;
            mat.Material.RenderState.PolygonControl.CullFront = mat.CullFront;

            mat.UpdateRenderState();
        }

        static string CreateBlendMethod(
         GX2BlendFunction src,
         GX2BlendCombine op,
         GX2BlendFunction dst)
        {
            string source = ConvertFunc(src);
            string dest = ConvertFunc(dst);
            if (op == GX2BlendCombine.Add) return $"{source} + {dest}";
            else if (op == GX2BlendCombine.SourceMinusDestination) return $"{source} - {dest}";
            else if (op == GX2BlendCombine.DestinationMinusSource) return $"{dest} - {source}";
            else if (op == GX2BlendCombine.Maximum) return $"min({source}, {dest})";
            else if (op == GX2BlendCombine.Minimum) return $"max({source}, {dest})";

            return $"";
        }

        static string ConvertFunc(GX2BlendFunction func)
        {
            switch (func)
            {
                case GX2BlendFunction.OneMinusDestinationAlpha: return "(1 - Dst.A)";
                case GX2BlendFunction.OneMinusDestinationColor: return "(1 - Dst.RGB)";
                case GX2BlendFunction.OneMinusConstantAlpha: return "(1 - Const.A)";
                case GX2BlendFunction.OneMinusConstantColor: return "(1 - Const.RGB)";
                case GX2BlendFunction.OneMinusSource1Color: return "(1 - Src1.RGB)";
                case GX2BlendFunction.OneMinusSource1Alpha: return "(1 - Src1.A)";
                case GX2BlendFunction.OneMinusSourceColor: return "(1 - Src.RGB)";
                case GX2BlendFunction.OneMinusSourceAlpha: return "(1 - Src.A)";
                case GX2BlendFunction.ConstantAlpha: return "Const.A";
                case GX2BlendFunction.ConstantColor: return "Const.RGB";
                case GX2BlendFunction.DestinationColor: return "Dst.RGB";
                case GX2BlendFunction.DestinationAlpha: return "Dst.A";
                case GX2BlendFunction.Source1Alpha: return "Src1.A";
                case GX2BlendFunction.Source1Color: return "Src1.RGB";
                case GX2BlendFunction.SourceColor: return "Src.RGB";
                case GX2BlendFunction.SourceAlpha: return "Src.A";
                case GX2BlendFunction.SourceAlphaSaturate: return "(Saturate(Src.A))";
                case GX2BlendFunction.One: return "1";
                case GX2BlendFunction.Zero: return "0";
                default:
                    return "(Unk Op)";
            }
        }
    }
}
