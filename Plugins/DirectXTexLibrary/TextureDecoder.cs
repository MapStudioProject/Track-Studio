using System;
using Toolbox.Core;
using System.Runtime.InteropServices;
using DirectXTexNet;

namespace DirectXTexLibrary
{
    public class TextureDecoder : ITextureDecoder
    {
        public bool CanEncode(TexFormat format)
        {
            return true;
        }

        public bool IsSupportedPlatform() {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows); 
        }

        public bool Decode(TexFormat format, byte[] input, int width, int height, out byte[] output)
        {
            if (format.ToString().StartsWith("BC"))
                output = DecompressBlock(input, width, height, (DXGI_FORMAT)format);
            else
                output = DecodePixelBlock(input, width, height, (DXGI_FORMAT)format);

            return output != null;
        }

        public bool Encode(TexFormat format, byte[] input, int width, int height, out byte[] output)
        {
            if (format.ToString().StartsWith("BC"))
                output = CompressBlock(input, width, height, (DXGI_FORMAT)format, false);
            else
                output = EncodePixelBlock(input, width, height, (DXGI_FORMAT)format);

            return output != null;
        }

        public static unsafe byte[] CompressBlock(Byte[] data, int width, int height, DXGI_FORMAT format, bool multiThread, float AlphaRef = 0.5f, STCompressionMode CompressionMode = STCompressionMode.Normal)
        {
            long inputRowPitch = width * 4;
            long inputSlicePitch = width * height * 4;

            if (data.Length == inputSlicePitch)
            {
                byte* buf;
                buf = (byte*)Marshal.AllocHGlobal((int)inputSlicePitch);
                Marshal.Copy(data, 0, (IntPtr)buf, (int)inputSlicePitch);

                DirectXTexNet.Image inputImage = new DirectXTexNet.Image(
                    width, height, DXGI_FORMAT.R8G8B8A8_UNORM, inputRowPitch,
                    inputSlicePitch, (IntPtr)buf, null);

                TexMetadata texMetadata = new TexMetadata(width, height, 1, 1, 1, 0, 0,
                    DXGI_FORMAT.R8G8B8A8_UNORM, TEX_DIMENSION.TEXTURE2D);

                ScratchImage scratchImage = TexHelper.Instance.InitializeTemporary(
                    new DirectXTexNet.Image[] { inputImage }, texMetadata, null);

                var compFlags = TEX_COMPRESS_FLAGS.DEFAULT;

                if (multiThread)
                    compFlags |= TEX_COMPRESS_FLAGS.PARALLEL;

                if (format == DXGI_FORMAT.BC7_UNORM ||
                    format == DXGI_FORMAT.BC7_UNORM_SRGB ||
                    format == DXGI_FORMAT.BC7_TYPELESS)
                {
                    if (CompressionMode == STCompressionMode.Fast)
                        compFlags |= TEX_COMPRESS_FLAGS.BC7_QUICK;
                }

                if (format == DXGI_FORMAT.BC1_UNORM_SRGB ||
                format == DXGI_FORMAT.BC2_UNORM_SRGB ||
                format == DXGI_FORMAT.BC3_UNORM_SRGB ||
                format == DXGI_FORMAT.BC7_UNORM_SRGB ||
                format == DXGI_FORMAT.R8G8B8A8_UNORM_SRGB)
                {
                    compFlags |= TEX_COMPRESS_FLAGS.SRGB;
                }

                using (var comp = scratchImage.Compress((DXGI_FORMAT)format, compFlags, 0.5f))
                {
                    long outRowPitch;
                    long outSlicePitch;
                    TexHelper.Instance.ComputePitch((DXGI_FORMAT)format, width, height, out outRowPitch, out outSlicePitch, CP_FLAGS.NONE);

                    byte[] result = new byte[outSlicePitch];
                    Marshal.Copy(comp.GetImage(0).Pixels, result, 0, result.Length);

                    inputImage = null;
                    scratchImage.Dispose();


                    return result;
                }
            }
            return null;
        }
        public static unsafe byte[] DecompressBlock(Byte[] data, int width, int height, DXGI_FORMAT format)
        {
            long inputRowPitch;
            long inputSlicePitch;
            TexHelper.Instance.ComputePitch((DXGI_FORMAT)format, width, height, out inputRowPitch, out inputSlicePitch, CP_FLAGS.NONE);

            DXGI_FORMAT FormatDecompressed;

            if (format.ToString().Contains("SRGB"))
                FormatDecompressed = DXGI_FORMAT.R8G8B8A8_UNORM_SRGB;
            else
                FormatDecompressed = DXGI_FORMAT.R8G8B8A8_UNORM;

            byte* buf;
            buf = (byte*)Marshal.AllocHGlobal((int)inputSlicePitch);
            Marshal.Copy(data, 0, (IntPtr)buf, (int)inputSlicePitch);

            DirectXTexNet.Image inputImage = new DirectXTexNet.Image(
                width, height, (DXGI_FORMAT)format, inputRowPitch,
                inputSlicePitch, (IntPtr)buf, null);

            TexMetadata texMetadata = new TexMetadata(width, height, 1, 1, 1, 0, 0,
                (DXGI_FORMAT)format, TEX_DIMENSION.TEXTURE2D);

            ScratchImage scratchImage = TexHelper.Instance.InitializeTemporary(
                new DirectXTexNet.Image[] { inputImage }, texMetadata, null);

            using (var decomp = scratchImage.Decompress(0, FormatDecompressed))
            {
                byte[] result = new byte[4 * width * height];
                Marshal.Copy(decomp.GetImage(0).Pixels, result, 0, result.Length);

                inputImage = null;
                scratchImage.Dispose();

                return result;
            }
        }
        public static unsafe byte[] DecodePixelBlock(Byte[] data, int width, int height, DXGI_FORMAT format, float AlphaRef = 0.5f)
        {
            if (format == DXGI_FORMAT.R8G8B8A8_UNORM)
            {
                byte[] result = new byte[data.Length];
                Array.Copy(data, result, data.Length);
                return result;
            }

            return Convert(data, width, height, (DXGI_FORMAT)format, DXGI_FORMAT.R8G8B8A8_UNORM);
        }
        public static unsafe byte[] EncodePixelBlock(Byte[] data, int width, int height, DXGI_FORMAT format, float AlphaRef = 0.5f)
        {
            if (format == DXGI_FORMAT.R8G8B8A8_UNORM || format == DXGI_FORMAT.R8G8B8A8_UNORM_SRGB)
                return data;

            return Convert(data, width, height, DXGI_FORMAT.R8G8B8A8_UNORM, (DXGI_FORMAT)format);
        }

        public static unsafe byte[] Convert(Byte[] data, int width, int height, DXGI_FORMAT inputFormat, DXGI_FORMAT outputFormat)
        {
            long inputRowPitch;
            long inputSlicePitch;
            TexHelper.Instance.ComputePitch(inputFormat, width, height, out inputRowPitch, out inputSlicePitch, CP_FLAGS.NONE);

            if (data.Length == inputSlicePitch)
            {
                byte* buf;
                buf = (byte*)Marshal.AllocHGlobal((int)inputSlicePitch);
                Marshal.Copy(data, 0, (IntPtr)buf, (int)inputSlicePitch);

                DirectXTexNet.Image inputImage = new DirectXTexNet.Image(
                    width, height, inputFormat, inputRowPitch,
                    inputSlicePitch, (IntPtr)buf, null);

                TexMetadata texMetadata = new TexMetadata(width, height, 1, 1, 1, 0, 0,
                    inputFormat, TEX_DIMENSION.TEXTURE2D);

                ScratchImage scratchImage = TexHelper.Instance.InitializeTemporary(
                    new DirectXTexNet.Image[] { inputImage }, texMetadata, null);

                var convFlags = TEX_FILTER_FLAGS.DEFAULT;

                if (inputFormat == DXGI_FORMAT.B8G8R8A8_UNORM_SRGB ||
                 inputFormat == DXGI_FORMAT.B8G8R8X8_UNORM_SRGB ||
                 inputFormat == DXGI_FORMAT.R8G8B8A8_UNORM_SRGB)
                {
                    convFlags |= TEX_FILTER_FLAGS.SRGB;
                }

                using (var decomp = scratchImage.Convert(0, outputFormat, convFlags, 0.5f))
                {
                    long outRowPitch;
                    long outSlicePitch;
                    TexHelper.Instance.ComputePitch(outputFormat, width, height, out outRowPitch, out outSlicePitch, CP_FLAGS.NONE);

                    byte[] result = new byte[outSlicePitch];
                    Marshal.Copy(decomp.GetImage(0).Pixels, result, 0, result.Length);

                    inputImage = null;
                    scratchImage.Dispose();


                    return result;
                }
            }
            return null;
        }
    }
}
