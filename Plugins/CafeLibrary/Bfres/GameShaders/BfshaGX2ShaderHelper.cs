using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Toolbox.Core.IO;
using BfshaLibrary;
using BfshaLibrary.WiiU;

namespace CafeLibrary.Rendering
{
    public class BfshaGX2ShaderHelper
    {
        public static byte[] CreateVertexShader(ShaderModel shaderModel, ResShaderProgram program)
        {
            var mem = new MemoryStream();
            using (var writer = new FileWriter(mem)) {
                var header = program.GX2VertexData;
                writer.Write(header.Regs);
                writer.Write((uint)header.Data.Length);
                long _dataOfsPos = writer.Position;
                writer.Write(uint.MaxValue); //Data offset reserved
                writer.Write(header.Mode);

                var samplers = GetSamplers(shaderModel, program, false).OrderBy(x => x.Location).ToList();
                var blocks = GetUniformBlocks(shaderModel, program, false).OrderByDescending(x => x.Offset).ToList();
                var uniforms = GetUniforms(shaderModel, program, false).OrderBy(x => x.BlockIndex).ToList();
                var attributes = GetAttributes(shaderModel, program).OrderBy(x => x.Location).ToList();
                var loops = GetLoops(shaderModel, program);

                long headerStart = writer.Position;
                WriteOffsetCount(writer, blocks.Count);
                WriteOffsetCount(writer, uniforms.Count);
                WriteOffsetCount(writer, 0);
                WriteOffsetCount(writer, samplers.Count);
                WriteOffsetCount(writer, loops.Count);
                WriteOffsetCount(writer, attributes.Count);
                writer.Write(shaderModel.MaxRingItemSize);
                writer.Write(new byte[36]);

                long uniformBlocksPos = SatisfyOffset(writer, headerStart + 4);
                for (int i = 0; i < blocks.Count; i++)
                {
                    writer.Write(uint.MaxValue); //Name offset reserved
                    writer.Write(blocks[i].Offset);
                    writer.Write(blocks[i].Size);
                }
                long uniformsPos = SatisfyOffset(writer, headerStart + 12);
                for (int i = 0; i < uniforms.Count; i++)
                {
                    writer.Write(uint.MaxValue); //Name offset reserved
                    writer.Write((uint)uniforms[i].Type);
                    writer.Write(uniforms[i].Count);
                    writer.Write(uniforms[i].Offset);
                    writer.Write(uniforms[i].BlockIndex);
                }
                long samplersPos = SatisfyOffset(writer, headerStart + 28);
                for (int i = 0; i < samplers.Count; i++)
                {
                    writer.Write(uint.MaxValue); //Name offset reserved
                    writer.Write((uint)uniforms[i].Type);
                    writer.Write(samplers[i].Location);
                }
                SatisfyOffset(writer, headerStart + 36);
                for (int i = 0; i < loops.Count; i++)
                {
                    writer.Write(loops[i].Offset);
                    writer.Write(loops[i].Value);
                }
                long attributesPos = SatisfyOffset(writer, headerStart + 44);
                for (int i = 0; i < attributes.Count; i++)
                {
                    writer.Write(uint.MaxValue); //Name offset reserved
                    writer.Write((uint)attributes[i].Type);
                    writer.Write(attributes[i].Count);
                    writer.Write(attributes[i].Location);
                }

                //Write string table
                for (int i = 0; i < blocks.Count; i++) {
                    SatisfyOffset(writer, (uniformBlocksPos + i * 12));
                    writer.WriteString(blocks[i].Name);
                }
                for (int i = 0; i < uniforms.Count; i++) {
                    SatisfyOffset(writer, (uniformsPos + i * 20));
                    writer.WriteString(uniforms[i].Name);
                }
                for (int i = 0; i < samplers.Count; i++) {
                    SatisfyOffset(writer, (samplersPos + i * 12));
                    writer.WriteString(samplers[i].Name);
                }
                for (int i = 0; i < attributes.Count; i++) {
                    SatisfyOffset(writer, (attributesPos + i * 16));
                    writer.WriteString(attributes[i].Name);
                }

                writer.Align(128);

                SatisfyOffset(writer, _dataOfsPos);
                writer.Write(header.Data.ToArray());
            }
            return mem.ToArray();
        }

        public static byte[] CreatePixelShader(ShaderModel shaderModel, ResShaderProgram program)
        {
            var mem = new MemoryStream();
            using (var writer = new FileWriter(mem))
            {
                var header = program.GX2PixelData;
                writer.Write(header.Regs);
                writer.Write((uint)header.Data.Length);
                long _dataOfsPos = writer.Position;
                writer.Write(uint.MaxValue); //Data offset reserved
                writer.Write(header.Mode);

                var samplers = GetSamplers(shaderModel, program, true).OrderBy(x => x.Location).ToList();
                var blocks = GetUniformBlocks(shaderModel, program, true).OrderByDescending(x => x.Offset).ToList();
                var uniforms = GetUniforms(shaderModel, program, true).OrderBy(x => x.BlockIndex).ToList();
                var loops = GetLoops(shaderModel, program);

                long headerStart = writer.Position;
                WriteOffsetCount(writer, blocks.Count);
                WriteOffsetCount(writer, uniforms.Count);
                WriteOffsetCount(writer, 0);
                WriteOffsetCount(writer, loops.Count);
                WriteOffsetCount(writer, samplers.Count);
                writer.Write(shaderModel.MaxRingItemSize);
                writer.Write(0);
                writer.Write(new uint[4]
                {
                    0, 0, 6064, 2,
                });
                writer.Write(new byte[28]);

                long uniformBlocksPos = SatisfyOffset(writer, headerStart + 4);
                for (int i = 0; i < blocks.Count; i++) {
                    writer.Write(uint.MaxValue); //Name offset reserved
                    writer.Write(blocks[i].Offset);
                    writer.Write(blocks[i].Size);
                }
                long uniformsPos = SatisfyOffset(writer, headerStart + 12);
                for (int i = 0; i < uniforms.Count; i++) {
                    writer.Write(uint.MaxValue); //Name offset reserved
                    writer.Write((uint)uniforms[i].Type);
                    writer.Write(uniforms[i].Count);
                    writer.Write(uniforms[i].Offset);
                    writer.Write(uniforms[i].BlockIndex);
                }
                SatisfyOffset(writer, headerStart + 28);
                for (int i = 0; i < loops.Count; i++) {
                    writer.Write(uint.MaxValue); //Name offset reserved
                    writer.Write(loops[i].Offset);
                    writer.Write(loops[i].Value);
                }
                long samplersPos = SatisfyOffset(writer, headerStart + 36);
                for (int i = 0; i < samplers.Count; i++) {
                    if (samplers[i].Name == "_b1" && samplers[i].Type == GX2SamplerVarType.SAMPLER_2D_ARRAY)
                        samplers[i].Type = GX2SamplerVarType.SAMPLER_2D;

                    writer.Write(uint.MaxValue); //Name offset reserved
                    writer.Write((uint)samplers[i].Type);
                    writer.Write(samplers[i].Location);
                }
                //Write string table
                for (int i = 0; i < blocks.Count; i++) {
                    SatisfyOffset(writer, (uniformBlocksPos + i * 12));
                    writer.WriteString(blocks[i].Name);
                }
                for (int i = 0; i < uniforms.Count; i++) {
                    SatisfyOffset(writer, (uniformsPos + i * 20));
                    writer.WriteString(uniforms[i].Name);
                }
                for (int i = 0; i < samplers.Count; i++) {
                    SatisfyOffset(writer, (samplersPos + i * 12));
                    writer.WriteString(samplers[i].Name);
                }

                writer.Align(128);

                SatisfyOffset(writer, _dataOfsPos);
                writer.Write(header.Data.ToArray());
            }
            return mem.ToArray();
        }

        static void WriteOffsetCount(FileWriter writer, int count)
        {
            writer.Write(count);
            writer.Write(0); //Data offset reserved
        }

        static long SatisfyOffset(FileWriter writer, long pos)
        {
            long currentPos = writer.Position;
            using (writer.TemporarySeek(pos, SeekOrigin.Begin))
            {
                writer.Write((uint)currentPos);
            }
            return currentPos;
        }

        private static List<GX2UniformVar> GetUniforms(ShaderModel shaderModel, ResShaderProgram program, bool isFragment)
        {
            List<GX2UniformVar> uniforms = new List<GX2UniformVar>();
            for (int i = 0; i < program.UniformBlockLocations.Length; i++)
            {
                int location = isFragment ? program.UniformBlockLocations[i].FragmentLocation :
                                           program.UniformBlockLocations[i].VertexLocation;

                if (location != -1)
                {
                    foreach (var uniform in shaderModel.UniformBlocks[i].Uniforms)
                    {
                        uniforms.Add(new GX2UniformVar()
                        {
                            Name = uniform.Key,
                            Offset = (uint)(uniform.Value.Offset - 1) / 4,
                            BlockIndex = (uint)uniform.Value.BlockIndex,
                            Count = uniform.Value.GX2Count,
                            Type = (GX2ShaderVarType)uniform.Value.GX2Type,
                        });
                    }
                }
            }
            return uniforms;
        }

        private static List<GX2UniformBlock> GetUniformBlocks(ShaderModel shaderModel, ResShaderProgram program, bool isFragment)
        {
            List<GX2UniformBlock> blocks = new List<GX2UniformBlock>();
            for (int i = 0; i < program.UniformBlockLocations.Length; i++)
            {
                int location = isFragment ? program.UniformBlockLocations[i].FragmentLocation :
                                           program.UniformBlockLocations[i].VertexLocation;

                if (location != -1)
                    blocks.Add(new GX2UniformBlock()
                    {
                        Name = shaderModel.UniformBlocks.GetKey(i),
                        Offset = (uint)location,
                        Size = shaderModel.UniformBlocks[i].Size,
                    });
            }
            return blocks;
        }

        private static List<GX2SamplerVar> GetSamplers(ShaderModel shaderModel, ResShaderProgram program, bool isFragment)
        {
            List<GX2SamplerVar> samplers = new List<GX2SamplerVar>();
            for (int i = 0; i < program.SamplerLocations.Length; i++)
            {
                int location = isFragment ? program.SamplerLocations[i].FragmentLocation :
                                           program.SamplerLocations[i].VertexLocation;

                Console.WriteLine($"{shaderModel.Samplers.GetKey(i)} SamplerVarType {(GX2SamplerVarType)shaderModel.Samplers[i].GX2Type}");

                if (location != -1)
                {
                    var type = (GX2SamplerVarType)shaderModel.Samplers[i].GX2Type;
                   // if (type == GX2SamplerVarType.SAMPLER_CUBE || type == GX2SamplerVarType.SAMPLER_CUBE_ARRAY)
                    //    type = (GX2SamplerVarType)5;

                    samplers.Add(new GX2SamplerVar()
                    {
                        Name = shaderModel.Samplers.GetKey(i),
                        Location = (uint)location,
                        Type = type,
                    });
                }
            }
            return samplers;
        }

        private static List<GX2AttributeVar> GetAttributes(ShaderModel shaderModel, ResShaderProgram program)
        {
            List<GX2AttributeVar> attributes = new List<GX2AttributeVar>();
            foreach (var att in shaderModel.Attributes)
            {
                attributes.Add(new GX2AttributeVar()
                {
                    Name = att.Key,
                    Location = att.Value.Location,
                    Type = (GX2ShaderVarType)att.Value.GX2Type,
                    Count = 0, //SHARCFB is 0
                });
            }
            return attributes;
        }

        private static List<GX2LoopVar> GetLoops(ShaderModel shaderModel, ResShaderProgram program)
        {
            //Todo figure these out
            List<GX2LoopVar> loops = new List<GX2LoopVar>();
            return loops;
        }
    }
}
