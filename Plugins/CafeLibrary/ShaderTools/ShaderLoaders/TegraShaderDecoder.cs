using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using Toolbox.Core;
using Toolbox.Core.IO;
using GLFrameworkEngine;
using System.Text;

namespace CafeLibrary.Rendering
{
    public class TegraShaderDecoder
    {
        public static Dictionary<string, ShaderProgram> GLShaderPrograms = new Dictionary<string, ShaderProgram>();

        public static ShaderInfo LoadShaderProgram(BfshaLibrary.ShaderModel shaderModel, BfshaLibrary.ShaderVariation variation)
        {
            var shaderData = variation.BinaryProgram.ShaderInfoData;

            var vertexData = GetShaderData(shaderData.VertexShaderCode);
            var fragData = GetShaderData(shaderData.PixelShaderCode);
            string fragHash = GetHashSHA1(fragData);
            string vertHash = GetHashSHA1(vertexData);

            string key = $"{vertHash}_{fragHash}";

            if (GLShaderPrograms.ContainsKey(key))
            {
                return new ShaderInfo()
                {
                    Program = GLShaderPrograms[key],
                    VertexConstants = GetConstants(shaderData.VertexShaderCode),
                    PixelConstants = GetConstants(shaderData.PixelShaderCode),
                    FragPath = $"ShaderCache/{fragHash}.frag",
                    VertPath = $"ShaderCache/{vertHash}.vert",
                };
            }

            if (!Directory.Exists($"ShaderCache"))
                Directory.CreateDirectory("ShaderCache");

            if (!File.Exists($"ShaderCache/{vertHash}.vert"))
            {
                File.WriteAllText($"ShaderCache/{vertHash}.vert",
                      DecompileShader(BfshaLibrary.ShaderType.VERTEX, vertexData));
            }
            if (!File.Exists($"ShaderCache/{fragHash}.frag"))
                File.WriteAllText($"ShaderCache/{fragHash}.frag",
                     DecompileShader(BfshaLibrary.ShaderType.PIXEL, fragData));

            //Load the source to opengl
            var program = new ShaderProgram(
                            new FragmentShader(File.ReadAllText($"ShaderCache/{fragHash}.frag")),
                            new VertexShader(File.ReadAllText($"ShaderCache/{vertHash}.vert")));

            GLShaderPrograms.Add(key, program);

            return new ShaderInfo()
            {
                Program = program,
                VertexConstants = GetConstants(shaderData.VertexShaderCode),
                PixelConstants = GetConstants(shaderData.PixelShaderCode),
                FragPath = $"ShaderCache/{fragHash}.frag",
                VertPath = $"ShaderCache/{vertHash}.vert",
            };
        }

        static string AppendPixelShaderCode(string code)
        {
            bool writtenExtraUniforms = false;

            var builder = new StringBuilder();

            var lines = code.Split('\n');
            int numLines = 0;
            foreach (var line in lines) {
                if (!writtenExtraUniforms && line.Contains("const int undef = 0;")) {
                    //Extra in tool uniforms for in tool functions (ie selection color)
                    builder.AppendLine("struct EXTRA_BLOCK");
                    builder.AppendLine("{");
                    builder.AppendLine("    vec4 selectionColor;");
                    builder.AppendLine("};");
                    builder.AppendLine("uniform EXTRA_BLOCK extraBlock;");

                    writtenExtraUniforms = true;
                }

                if (writtenExtraUniforms && line.Contains("    return;") && numLines >= lines.Length - 5) {
                    builder.AppendLine("    out_attr0.rgb = out_attr0.rgb * (1 - extraBlock.selectionColor.a) + extraBlock.selectionColor.rgb * extraBlock.selectionColor.a;");
                }

                builder.AppendLine(line);
                numLines++;
            }
            return builder.ToString();
        }

        //Hash algorithm for cached shaders. Make sure to only decompile unique/new shaders
        static string GetHashSHA1(byte[] data)
        {
            using (var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider()) {
                return string.Concat(sha1.ComputeHash(data).Select(x => x.ToString("X2")));
            }
        }

        //Gets the raw byte data and splits off uneeded parts
        static byte[] GetShaderData(BfshaLibrary.ShaderCodeData shaderData)
        {
            var data = ((BfshaLibrary.ShaderCodeDataBinary)shaderData).BinaryData;
            byte[] data1 = data[1].ToArray();

            return ByteUtils.SubArray(data1, 48, (uint)data1.Length - 48);
        }

        static byte[] GetConstants(BfshaLibrary.ShaderCodeData shaderData)
        {
            var data = ((BfshaLibrary.ShaderCodeDataBinary)shaderData).BinaryData;

            //Bnsh has 2 shader code sections. The first section has block info for constants
            using (var reader = new Toolbox.Core.IO.FileReader(data[0])) {
                reader.SeekBegin(1776);
                ulong ofsUnk = reader.ReadUInt64();
                uint lenByteCode = reader.ReadUInt32();
                uint lenConstData = reader.ReadUInt32();
                uint ofsConstBlockDataStart = reader.ReadUInt32();
                uint ofsConstBlockDataEnd = reader.ReadUInt32();
                return GetConstantsFromCode(data[1], ofsConstBlockDataStart, lenConstData);
            }
        }

        static byte[] GetConstantsFromCode(Stream shaderCode, uint offset, uint length)
        {
            using (var reader = new Toolbox.Core.IO.FileReader(shaderCode, true))
            {
                reader.SeekBegin(offset);
                return reader.ReadBytes((int)length);
            }
        }

        static string DecompileShader(BfshaLibrary.ShaderType shaderType, byte[] Data)
        {
            string translated = TegraShaderTranslator.Translate(Data);
            return translated;
        }
    }
}
