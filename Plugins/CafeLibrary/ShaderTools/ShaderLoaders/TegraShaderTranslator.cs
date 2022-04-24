using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace CafeLibrary.Rendering
{
    public class TegraShaderTranslator
    {
        private static readonly Lazy<bool> _supportsFragmentShaderInterlock = new Lazy<bool>(() => HasExtension("GL_ARB_fragment_shader_interlock"));
        private static readonly Lazy<bool> _supportsFragmentShaderOrdering = new Lazy<bool>(() => HasExtension("GL_INTEL_fragment_shader_ordering"));

        private static readonly Lazy<bool> _supportsShaderBallot = new Lazy<bool>(() => HasExtension("GL_ARB_shader_ballot"));
        private static readonly Lazy<bool> _supportsImageLoadFormatted = new Lazy<bool>(() => HasExtension("GL_EXT_shader_image_load_formatted"));

        private static readonly Lazy<bool> _supportsTextureShadowLod = new Lazy<bool>(() => HasExtension("GL_EXT_texture_shadow_lod"));

        private static readonly Lazy<int> _maximumComputeSharedMemorySize = new Lazy<int>(() => GetLimit(All.MaxComputeSharedMemorySize));
        private static readonly Lazy<int> _storageBufferOffsetAlignment = new Lazy<int>(() => GetLimit(All.ShaderStorageBufferOffsetAlignment));

        private static bool _supportsNonConstantTextureOffset => _gpuVendor.Value == GpuVendor.Nvidia;

        private static readonly Lazy<GpuVendor> _gpuVendor = new Lazy<GpuVendor>(GetGpuVendor);

        public static void Prepare()
        {
            TranslationFlags flags = TranslationFlags.None;
            Translator.CreateContext(0, new GpuAccessor(new byte[0]), flags);
        }
        private class GpuAccessor : IGpuAccessor
        {
            private readonly byte[] _data;

            public GpuAccessor(byte[] data)
            {
                _data = data;
            }

            public T MemoryRead<T>(ulong address) where T : unmanaged
            {
                return MemoryMarshal.Cast<byte, T>(new ReadOnlySpan<byte>(_data).Slice((int)address))[0];
            }

            public int QueryHostStorageBufferOffsetAlignment() => _storageBufferOffsetAlignment.Value;
            public bool QueryHostSupportsFragmentShaderInterlock() => _supportsFragmentShaderInterlock.Value;
            public bool QueryHostSupportsFragmentShaderOrderingIntel() => _supportsFragmentShaderOrdering.Value;
            public bool QueryHostSupportsShaderBallot() => _supportsShaderBallot.Value;
            public bool QueryHostSupportsNonConstantTextureOffset() => _supportsNonConstantTextureOffset;
            public bool QueryHostSupportsImageLoadFormatted() => _supportsImageLoadFormatted.Value;
            public bool QueryHostSupportsTextureShadowLod() => _supportsTextureShadowLod.Value;
        }

        public static string Translate(byte[] data)
        {
            TranslationFlags flags = TranslationFlags.None;

            return Translator.CreateContext(0,
                 new GpuAccessor(data), flags).Translate(out _).Code;
        }

        private static int GetLimit(All name)
        {
            return GL.GetInteger((GetPName)name);
        }

        private static bool HasExtension(string name)
        {
            int numExtensions = GL.GetInteger(GetPName.NumExtensions);

            for (int extension = 0; extension < numExtensions; extension++)
            {
                if (GL.GetString(StringNameIndexed.Extensions, extension) == name)
                {
                    return true;
                }
            }
            return false;
        }

        private static GpuVendor GetGpuVendor()
        {
            string vendor = GL.GetString(StringName.Vendor).ToLower();

            if (vendor == "nvidia corporation")
            {
                return GpuVendor.Nvidia;
            }
            else if (vendor == "intel")
            {
                string renderer = GL.GetString(StringName.Renderer).ToLower();

                return renderer.Contains("mesa") ? GpuVendor.IntelUnix : GpuVendor.IntelWindows;
            }
            else if (vendor == "ati technologies inc." || vendor == "advanced micro devices, inc.")
            {
                return GpuVendor.AmdWindows;
            }
            else if (vendor == "amd" || vendor == "x.org")
            {
                return GpuVendor.AmdUnix;
            }
            else
            {
                return GpuVendor.Unknown;
            }
        }
        enum GpuVendor
        {
            Nvidia,
            IntelWindows,
            IntelUnix,
            AmdWindows,
            AmdUnix,
            Unknown,
        }
    }
}
