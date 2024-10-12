using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using ImGuiNET;
using MapStudio.UI;
using BfresLibrary.GX2;

namespace CafeLibrary
{
    public class ModelImportSettings
    {
        //Global attribute toggles
        public AttributeSettings Position = new AttributeSettings(true, GX2AttribFormat.Format_32_32_32_Single, PositionFormats);
        public AttributeSettings Normal = new AttributeSettings(true, GX2AttribFormat.Format_10_10_10_2_SNorm, NormalsFormats);
        public AttributeSettings UVs = new AttributeSettings(true, GX2AttribFormat.Format_16_16_Single, UVFormats);
        public AttributeSettings Colors = new AttributeSettings(true, GX2AttribFormat.Format_8_8_8_8_UNorm, ColorFormats);
        public AttributeSettings Tangent = new AttributeSettings(true, GX2AttribFormat.Format_8_8_8_8_SNorm, TangentFormats);
        public AttributeSettings Bitangent = new AttributeSettings(true, GX2AttribFormat.Format_8_8_8_8_SNorm, BiTangentFormats);
        public AttributeSettings BoneWeights = new AttributeSettings(true, GX2AttribFormat.Format_8_8_8_8_UNorm, WeightFormats);
        public AttributeSettings BoneIndices = new AttributeSettings(true, GX2AttribFormat.Format_8_8_8_8_UInt, BoneIndicesFormats);

        public bool ResetUVParams = true;
        public bool ReseColorParams = true;

        public bool Replacing = false;
        public bool FlipUVs = false;
        public bool Rotate90 = false;
        public bool RotateNeg90 = false;
        public bool EnableLODs = false;
        public bool EnableSubMesh = false;
        public bool ImportBones = false;

        public bool RecalculateNormals = false;
        public bool OverrideVertexColors = false;

        public bool ReplaceMatchingMeshes = true;
        public bool KeepOrginalMaterialsOnReplace = false;

        public int LODCount = 2;

        public Vector4 ColorOverride = Vector4.One;

        public List<MeshSettings> Meshes = new List<MeshSettings>();
        public List<string> Materials = new List<string>();

        public string MaterialPresetName = "";

        public MaterialPresetWindow PresetWindow = new MaterialPresetWindow();

        public class MeshSettings
        {
            public string Name { get; set; }
            public string MaterialName;
            public string ImportedMaterial;
            public BfresLibrary.Material MaterialInstance;

            public bool CombineUVs;

            public bool UseCustomAttributeSettings;

            public string PresetName { get; set; }

            public bool KeepTextures = true;

            public IONET.Core.Model.IOMesh MeshData;

            public string MaterialRawFile;

            public uint UVLayerCount;

            public int SkinCount;

            public ushort BoneIndex = 0; //single binded bone

            public bool[] UseTexCoord { get; set; } = new bool[] { true, true, true, true };
            public bool[] UseColor { get; set; } = new bool[] { true, true, true, true };

            public AttributeSettings Position = new AttributeSettings(true, GX2AttribFormat.Format_32_32_32_Single, PositionFormats);
            public AttributeSettings Normal = new AttributeSettings(true, GX2AttribFormat.Format_10_10_10_2_SNorm, NormalsFormats);
            public AttributeSettings UVs = new AttributeSettings(true, GX2AttribFormat.Format_16_16_Single, UVFormats);
            public AttributeSettings UV_Layers = new AttributeSettings(true, GX2AttribFormat.Format_32_32_Single, UVFormats);
            public AttributeSettings Colors = new AttributeSettings(true, GX2AttribFormat.Format_8_8_8_8_UNorm, ColorFormats);
            public AttributeSettings Tangent = new AttributeSettings(true, GX2AttribFormat.Format_8_8_8_8_SNorm, TangentFormats);
            public AttributeSettings Bitangent = new AttributeSettings(true, GX2AttribFormat.Format_8_8_8_8_SNorm, BiTangentFormats);
            public AttributeSettings BoneWeights = new AttributeSettings(true, GX2AttribFormat.Format_8_8_8_8_UNorm, WeightFormats);
            public AttributeSettings BoneIndices = new AttributeSettings(true, GX2AttribFormat.Format_8_8_8_8_UInt, BoneIndicesFormats);

            public List<AttributeInfo> AttributeLayout = new List<AttributeInfo>();

            public uint CalculateVertexBufferSize()
            {
                uint size = 0;
                size += GetFormatStride(Position.Format);
                if (Normal.Enable) size += GetFormatStride(Normal.Format);
                if (UVs.Enable) size += GetFormatStride(UVs.Format) * UVLayerCount;
                if (Colors.Enable) size += GetFormatStride(Colors.Format);
                if (Tangent.Enable) size += GetFormatStride(Tangent.Format);
                if (Bitangent.Enable) size += GetFormatStride(Bitangent.Format);
                if (BoneWeights.Enable) size += GetFormatStride(BoneWeights.Format);
                if (BoneIndices.Enable) size += GetFormatStride(BoneIndices.Format);
                return size * (uint)MeshData.Vertices.Count;
            }
        }

        public class AttributeInfo
        {
            public string Name = "";
            public byte BufferIndex = 0;

            public AttributeInfo(string name, byte index)
            {
                Name = name;
                BufferIndex = index;
            }
        }

        public class AttributeSettings
        {
            public GX2AttribFormat Format = GX2AttribFormat.Format_32_32_32_Single;
            public GX2AttribFormat[] FormatList;
            public bool Enable = false;

            public AttributeSettings(bool enabled, GX2AttribFormat format, GX2AttribFormat[] formatList)
            {
                Enable = enabled;
                Format = format;
                FormatList = formatList;
            }

            public AttributeSettings(bool enabled, GX2AttribFormat format)
            {
                Enable = enabled;
                Format = format;
            }
        }

        static GX2AttribFormat[] PositionFormats = new GX2AttribFormat[]
        {
            GX2AttribFormat.Format_32_32_32_Single,
            GX2AttribFormat.Format_16_16_16_16_Single,
            GX2AttribFormat.Format_16_16_16_16_SNorm,
            GX2AttribFormat.Format_8_8_8_8_SNorm,
        };

        //Normalized values should include mostly signed formats
        static GX2AttribFormat[] NormalsFormats = new GX2AttribFormat[]
       {
            GX2AttribFormat.Format_10_10_10_2_SNorm,
            GX2AttribFormat.Format_32_32_32_Single,
            GX2AttribFormat.Format_16_16_16_16_Single,
            GX2AttribFormat.Format_16_16_16_16_SNorm,
            GX2AttribFormat.Format_8_8_8_8_SNorm,
       };

        static GX2AttribFormat[] UVFormats = new GX2AttribFormat[]
        {
            GX2AttribFormat.Format_32_32_Single,
            GX2AttribFormat.Format_16_16_Single,
            GX2AttribFormat.Format_16_16_SNorm,
            GX2AttribFormat.Format_16_16_UNorm,
            GX2AttribFormat.Format_8_8_SNorm,
            GX2AttribFormat.Format_8_8_UNorm,
        };

        //Normalized values should include mostly signed formats
        static GX2AttribFormat[] TangentFormats = new GX2AttribFormat[]
        {
            GX2AttribFormat.Format_32_32_32_Single,
            GX2AttribFormat.Format_16_16_16_16_Single,
            GX2AttribFormat.Format_16_16_16_16_SNorm,
            GX2AttribFormat.Format_10_10_10_2_SNorm,
            GX2AttribFormat.Format_8_8_8_8_SNorm,
        };

        //Normalized values should include mostly signed formats
        static GX2AttribFormat[] BiTangentFormats = new GX2AttribFormat[]
        {
            GX2AttribFormat.Format_32_32_32_Single,
            GX2AttribFormat.Format_16_16_16_16_Single,
            GX2AttribFormat.Format_16_16_16_16_SNorm,
            GX2AttribFormat.Format_10_10_10_2_SNorm,
            GX2AttribFormat.Format_8_8_8_8_SNorm,
        };

        static GX2AttribFormat[] ColorFormats = new GX2AttribFormat[]
        {
            GX2AttribFormat.Format_32_32_32_Single,
            GX2AttribFormat.Format_16_16_16_16_Single,
            GX2AttribFormat.Format_16_16_16_16_SNorm,
            GX2AttribFormat.Format_16_16_16_16_UNorm,
            GX2AttribFormat.Format_8_8_8_8_SNorm,
            GX2AttribFormat.Format_8_8_8_8_UNorm,
        };

        //Only use 4 element types for weights/indices. These will be automatically adjusted afterwards based on the skin count
        static GX2AttribFormat[] WeightFormats = new GX2AttribFormat[]
         {
            GX2AttribFormat.Format_32_32_32_32_Single,
            GX2AttribFormat.Format_16_16_16_16_UNorm,
            GX2AttribFormat.Format_8_8_8_8_UNorm,
         };

        //Only use 4 element types for weights/indices. These will be automatically adjusted afterwards based on the skin count
        static GX2AttribFormat[] BoneIndicesFormats = new GX2AttribFormat[]
         {
            GX2AttribFormat.Format_32_32_32_32_UInt,
            GX2AttribFormat.Format_16_16_16_16_UInt,
            GX2AttribFormat.Format_8_8_8_8_UInt,
         };

        public static uint GetFormatStride(GX2AttribFormat format)
        {
            switch (format)
            {
                case GX2AttribFormat.Format_32_32_32_32_Single:
                    return 16;
                case GX2AttribFormat.Format_32_32_32_Single:
                    return 12;
                case GX2AttribFormat.Format_32_32_Single:
                    return 8;
                case GX2AttribFormat.Format_16_16_16_16_Single:
                    return 8;
                case GX2AttribFormat.Format_16_16_Single:
                    return 8;
                case GX2AttribFormat.Format_16_16_16_16_SNorm:
                case GX2AttribFormat.Format_16_16_16_16_UNorm:
                    return 8;
                case GX2AttribFormat.Format_16_16_SNorm:
                case GX2AttribFormat.Format_16_16_UNorm:
                    return 4;//(ushort / 65535f)
                case GX2AttribFormat.Format_8_8_8_8_SNorm:
                    return 4;
                case GX2AttribFormat.Format_8_8_SNorm:
                case GX2AttribFormat.Format_8_8_UNorm:
                    return 2; // (byte / 255f)
                case GX2AttribFormat.Format_10_10_10_2_SNorm:
                    return 4;
                case GX2AttribFormat.Format_32_32_32_32_UInt:
                    return 16;
                case GX2AttribFormat.Format_16_16_16_16_UInt:
                    return 8;
                case GX2AttribFormat.Format_8_8_8_8_UInt:
                    return 4;
            }
            return 4;
        }

        public static string GetAttributeName(string attribute)
        {
            switch (attribute)
            {
                case "_p0": return "Position";
                case "_n0": return "Normal";
                case "_c0": return "Color";
                case "_b0": return "Bitangent";
                case "_t0": return "Tangent";
                case "_u0": return "Tex Coord 0";
                case "_u1": return "Tex Coord 1";
                case "_u2": return "Tex Coord 2";
                case "_w0": return "Bone Weights";
                case "_i0": return "Bone Indices";
            }

            return attribute;
        }

        public static string GetFormatName(GX2AttribFormat format)
        {
            switch (format)
            {
                case GX2AttribFormat.Format_32_32_32_32_Single:
                case GX2AttribFormat.Format_32_32_32_Single:
                case GX2AttribFormat.Format_32_32_Single:
                    return "Float";
                case GX2AttribFormat.Format_16_16_16_16_Single:
                case GX2AttribFormat.Format_16_16_Single:
                    return "Half Float";
                case GX2AttribFormat.Format_16_16_16_16_SNorm:
                case GX2AttribFormat.Format_16_16_SNorm:
                    return "SNorm16"; //(short / 32767f)
                case GX2AttribFormat.Format_16_16_16_16_UNorm:
                case GX2AttribFormat.Format_16_16_UNorm:
                    return "UNorm16"; //(ushort / 65535f)
                case GX2AttribFormat.Format_8_8_8_8_SNorm:
                case GX2AttribFormat.Format_8_8_SNorm:
                    return "SNorm8"; // (sbyte / 127f)
                case GX2AttribFormat.Format_8_8_8_8_UNorm:
                case GX2AttribFormat.Format_8_8_UNorm:
                    return "UNorm8"; // (byte / 255f)
                case GX2AttribFormat.Format_10_10_10_2_SNorm:
                    return "SNorm10_10_10_2";
                case GX2AttribFormat.Format_32_32_32_32_UInt:
                    return "UInt32";
                case GX2AttribFormat.Format_16_16_16_16_UInt:
                    return "UShort";
                case GX2AttribFormat.Format_8_8_8_8_UInt:
                    return "Byte";
            }
            return format.ToString();
        }

        public void Render()
        {
            if (!string.IsNullOrEmpty(PluginConfig.MaterialPreset) && string.IsNullOrEmpty(MaterialPresetName))
                MaterialPresetName = System.IO.Path.GetFileNameWithoutExtension(PluginConfig.MaterialPreset);

            string materialPresetLbl = string.IsNullOrEmpty(MaterialPresetName) ? "None" : MaterialPresetName;

            ImGui.Text($"Import Settings");

            ImGui.Columns(2);
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Material Preset:");
            ImGui.NextColumn();

            if (ImGui.Button(materialPresetLbl, new Vector2(ImGui.GetColumnWidth(), 23)))
            {

            }
            ImGui.NextColumn();

            ImGui.Columns(1);
        }
    }
}
