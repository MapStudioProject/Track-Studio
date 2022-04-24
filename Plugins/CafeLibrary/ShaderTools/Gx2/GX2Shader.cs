using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Toolbox.Core.IO;
using System.Runtime.InteropServices;

namespace CafeLibrary.Rendering
{
    public class GX2Shader
    {
        public Stream Data { get; set; }

        public byte[] DataBytes
        {
            get
            {
                using (var binaryReader = new FileReader(Data, true))
                {
                    return binaryReader.ReadBytes((int)binaryReader.Length);
                }
            }
        }

        public virtual void Write(FileWriter writer)
        {

        }
    }

    public class GX2VertexShader : GX2Shader
    {
        GX2VertexShaderStuct ShaderRegsHeader;

        public List<GX2UniformBlock> UniformBlocks = new List<GX2UniformBlock>();
        public List<GX2UniformVar> Uniforms = new List<GX2UniformVar>();
        public List<GX2AttributeVar> Attributes = new List<GX2AttributeVar>();
        public List<GX2SamplerVar> Samplers = new List<GX2SamplerVar>();
        public List<GX2LoopVar> Loops = new List<GX2LoopVar>();

        public GX2VertexShader(FileReader reader, uint version) {

            long pos = reader.Position;
            ShaderRegsHeader = reader.ReadStruct<GX2VertexShaderStuct>();

            uint size = reader.ReadUInt32();
            uint dataOffset = reader.ReadUInt32();

            Data = new SubStream(reader.BaseStream, pos, dataOffset + size);

            uint mode = reader.ReadUInt32();
            uint uniformBlockCount = reader.ReadUInt32();
            uint uniformBlocksOffset = reader.ReadUInt32();
            uint uniformVarCount = reader.ReadUInt32();
            uint uniformVarsOffset = reader.ReadUInt32();
            uint padding1 = reader.ReadUInt32();
            uint padding2 = reader.ReadUInt32();

            uint loopVarCount = reader.ReadUInt32();
            uint loopVarsOffset = reader.ReadUInt32();
            uint samplerVarCount = reader.ReadUInt32();
            uint samplerVarsOffset = reader.ReadUInt32();
            uint attribVarCount = reader.ReadUInt32();
            uint attribVarsOffset = reader.ReadUInt32();
            byte hasStreamOut = reader.ReadByte();
            uint streamOutStride = reader.ReadUInt32();

            reader.SeekBegin(uniformVarsOffset);
            for (int i = 0; i < uniformVarCount; i++)
                Uniforms.Add(new GX2UniformVar(reader));

            reader.SeekBegin(uniformBlocksOffset);
            for (int i = 0; i < uniformBlockCount; i++)
                UniformBlocks.Add(new GX2UniformBlock(reader));

            reader.SeekBegin(attribVarsOffset);
            for (int i = 0; i < attribVarCount; i++)
                Attributes.Add(new GX2AttributeVar(reader));

            reader.SeekBegin(samplerVarsOffset);
            for (int i = 0; i < samplerVarCount; i++)
                Samplers.Add(new GX2SamplerVar(reader));

            reader.SeekBegin(loopVarsOffset);
            for (int i = 0; i < loopVarCount; i++)
                Loops.Add(new GX2LoopVar(reader));
        }
    }

    public class GX2PixelShader : GX2Shader
    {
        GX2PixelShaderStuct ShaderRegsHeader;

        public List<GX2UniformBlock> UniformBlocks = new List<GX2UniformBlock>();
        public List<GX2UniformVar> Uniforms = new List<GX2UniformVar>();
        public List<GX2SamplerVar> Samplers = new List<GX2SamplerVar>();
        public List<GX2LoopVar> Loops = new List<GX2LoopVar>();

        public GX2PixelShader(FileReader reader, uint version)
        {
            long pos = reader.Position;
            ShaderRegsHeader = reader.ReadStruct<GX2PixelShaderStuct>();

            uint size = reader.ReadUInt32();
            uint dataOffset = reader.ReadUInt32();
            uint mode = reader.ReadUInt32();

            Data = new SubStream(reader.BaseStream, pos, dataOffset + size);

            uint uniformBlockCount = reader.ReadUInt32();
            uint uniformBlocksOffset = reader.ReadUInt32();
            uint uniformVarCount = reader.ReadUInt32();
            uint uniformVarsOffset = reader.ReadUInt32();
            uint padding1 = reader.ReadUInt32();
            uint padding2 = reader.ReadUInt32();
            uint loopVarCount = reader.ReadUInt32();
            uint loopVarsOffset = reader.ReadUInt32();
            uint samplerVarCount = reader.ReadUInt32();
            uint samplerVarsOffset = reader.ReadUInt32();

            reader.SeekBegin(uniformVarsOffset);
            for (int i = 0; i < uniformVarCount; i++)
                Uniforms.Add(new GX2UniformVar(reader));

            reader.SeekBegin(uniformBlocksOffset);
            for (int i = 0; i < uniformBlockCount; i++)
                UniformBlocks.Add(new GX2UniformBlock(reader));

            reader.SeekBegin(samplerVarsOffset);
            for (int i = 0; i < samplerVarCount; i++)
                Samplers.Add(new GX2SamplerVar(reader));

            reader.SeekBegin(loopVarsOffset);
            for (int i = 0; i < loopVarCount; i++)
                Loops.Add(new GX2LoopVar(reader));
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class GX2VertexShaderStuct
    {
        public uint sq_pgm_resources_vs;
        public uint vgt_primitiveid_en;
        public uint spi_vs_out_config;
        public uint num_spi_vs_out_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public uint[] spi_vs_out_id;

        public uint pa_cl_vs_out_cntl;
        public uint sq_vtx_semantic_clear;
        public uint num_sq_vtx_semantic;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public uint[] sq_vtx_semantic;

        public uint vgt_strmout_buffer_en;
        public uint vgt_vertex_reuse_block_cntl;
        public uint vgt_hos_reuse_depth;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class GX2PixelShaderStuct
    {
        public uint sq_pgm_resources_ps;
        public uint sq_pgm_exports_ps;
        public uint spi_ps_in_control_0;
        public uint spi_ps_in_control_1;
        public uint num_spi_ps_input_cntl;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public uint[] spi_ps_input_cntls;

        public uint cb_shader_mask;
        public uint cb_shader_control;
        public uint db_shader_control;
        public uint spi_input_z;
    }

    public class GX2UniformVar
    {
        public string Name { get; set; }
        public GX2ShaderVarType Type { get; set; }
        public uint Count { get; set; }
        public uint Offset { get; set; }
        public uint BlockIndex { get; set; }

        public GX2UniformVar() { }

        public GX2UniformVar(FileReader reader)
        {
            Name = reader.ReadNameOffset(false, typeof(uint));
            Type = reader.ReadEnum<GX2ShaderVarType>(false);
            Count = reader.ReadUInt32();
            Offset = reader.ReadUInt32();
            BlockIndex = reader.ReadUInt32();
        }
    }

    public class GX2SamplerVar
    {
        public string Name { get; set; }
        public GX2SamplerVarType Type { get; set; }
        public uint Location { get; set; }

        public GX2SamplerVar() { }

        public GX2SamplerVar(FileReader reader)
        {
            Name = reader.ReadNameOffset(false, typeof(uint));
            Type = reader.ReadEnum< GX2SamplerVarType>(false);
            Location = reader.ReadUInt32();
        }
    }

    public class GX2UniformBlock
    {
        public string Name { get; set; }
        public uint Offset { get; set; }
        public uint Size { get; set; }

        public GX2UniformBlock() { }

        public GX2UniformBlock(FileReader reader)
        {
            Name = reader.ReadNameOffset(false, typeof(uint));
            Offset = reader.ReadUInt32();
            Size = reader.ReadUInt32();
        }
    }

    public class GX2AttributeVar
    {
        public string Name { get; set; }
        public GX2ShaderVarType Type { get; set; }
        public uint Count { get; set; }
        public int Location { get; set; }

        public GX2AttributeVar() { }

        public GX2AttributeVar(FileReader reader)
        {
            Name = reader.ReadNameOffset(false, typeof(uint));
            Type = reader.ReadEnum<GX2ShaderVarType>(true);
            Count = reader.ReadUInt32();
            Location = reader.ReadInt32();
        }

        public uint GetStreamCount() {
            return GX2ShaderHelper.GetStreamCount(Type);
        }
    }

    public class GX2LoopVar
    {
        public uint Offset { get; set; }
        public uint Value { get; set; }

        public GX2LoopVar() { }

        public GX2LoopVar(FileReader reader)
        {
            Offset = reader.ReadUInt32();
            Value = reader.ReadUInt32();
        }
    }

    public enum GX2SamplerVarType
    {
        SAMPLER_1D = 0,
        SAMPLER_2D = 1,
        SAMPLER_3D = 2,
        SAMPLER_CUBE = 4,
        SAMPLER_2D_SHADOW = 6,
        SAMPLER_2D_ARRAY = 10,
        SAMPLER_2D_ARRAY_SHADOW = 12,
        SAMPLER_CUBE_ARRAY = 13,
    }

    public enum GX2ShaderVarType
    {
        INT = 2,
        FLOAT = 4,
        FLOAT2 = 9,
        FLOAT3 = 10,
        FLOAT4 = 11,
        INT2 = 15,
        INT3 = 16,
        INT4 = 17,
        MATRIX4X4 = 28,
    }
}
