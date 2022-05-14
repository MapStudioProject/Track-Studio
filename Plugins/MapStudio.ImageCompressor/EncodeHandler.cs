using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;

namespace MapStudio.ImageCompressor
{
    public class EncodeHandler : ITextureDecoder
    {
        public bool CanEncode(TexFormat format) {
            return FormatList.ContainsKey(format);
        }

        public bool IsSupportedPlatform() {
            return true;
        }
        
        public bool Decode(TexFormat format, byte[] input, int width, int height, out byte[] output)
        {
            output = null;
            if (format.ToString().StartsWith("BC"))
                output = DecompressBlock(input, width, height, format);

            return output != null;
        }

        public bool Encode(TexFormat format, byte[] input, int width, int height, out byte[] output)
        {
            output = null;
            if (format.ToString().StartsWith("BC"))
                output = EncodeBlock(input, width, height, format);

            return output != null;
        }

        private byte[] DecompressBlock(byte[] input, int width, int height, TexFormat format)
        {
            var compformat = FormatList[format];

            var decoder = new BcDecoder();
            var colors = decoder.DecodeRaw(new System.IO.MemoryStream(input), width, height, compformat);

            byte[] output = new byte[colors.Length * 4];

            for (int i = 0; i < colors.Length; i++) {
                int offset = i * 4;

                output[offset]     = colors[i].r;
                output[offset + 1] = colors[i].g;
                output[offset + 1] = colors[i].b;
                output[offset + 1] = colors[i].a;
            }
            return output;
        }

        private byte[] EncodeBlock(byte[] input, int width, int height, TexFormat format)
        {
            var compformat = FormatList[format];

            var encoder = new BcEncoder();
            encoder.OutputOptions.Format = compformat;
            encoder.OutputOptions.Quality = CompressionQuality.Balanced;

          /*  if (Runtime.CompressionQuality == Runtime.CompQuality.Fast)
                encoder.OutputOptions.Quality = CompressionQuality.Fast;
            if (Runtime.CompressionQuality == Runtime.CompQuality.BestQuality)
                encoder.OutputOptions.Quality = CompressionQuality.BestQuality;*/

            var colors = encoder.EncodeToRawBytes(input, width, height, PixelFormat.Rgba32);
            return colors[0];
        }

        class SynchronousProgress<T> : IProgress<T>
        {
            private readonly Action<T> handler;

            public SynchronousProgress(Action<T> handler)
            {
                this.handler = handler;
            }

            public void Report(T value)
            {
                handler(value);
            }
        }

        private Dictionary<TexFormat, CompressionFormat> FormatList = new Dictionary<TexFormat, CompressionFormat>()
        {
            { TexFormat.BC1_UNORM, CompressionFormat.Bc1WithAlpha },
            { TexFormat.BC1_SRGB, CompressionFormat.Bc1WithAlpha },
            { TexFormat.BC2_UNORM, CompressionFormat.Bc2 },
            { TexFormat.BC2_SRGB, CompressionFormat.Bc2 },
            { TexFormat.BC3_UNORM, CompressionFormat.Bc3 },
            { TexFormat.BC3_SRGB, CompressionFormat.Bc3 },
            { TexFormat.BC4_UNORM, CompressionFormat.Bc4 },
            { TexFormat.BC4_SNORM, CompressionFormat.Bc4 },
            { TexFormat.BC5_UNORM, CompressionFormat.Bc5 },
            { TexFormat.BC5_SNORM, CompressionFormat.Bc5 },
         //   { TexFormat.BC6H_SF16, CompressionFormat.Bc6 },
        //   { TexFormat.BC6H_UF16, CompressionFormat.Bc6 },
            { TexFormat.BC7_SRGB, CompressionFormat.Bc7 },
            { TexFormat.BC7_UNORM, CompressionFormat.Bc7 },
        };
    }
}
