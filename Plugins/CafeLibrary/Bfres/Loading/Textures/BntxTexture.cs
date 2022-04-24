using System;
using System.Collections.Generic;
using System.Text;
using Syroot.NintenTools.NSW.Bntx;
using Syroot.NintenTools.NSW.Bntx.GFX;
using Toolbox.Core;
using Toolbox.Core.Imaging;
using MapStudio.UI;

namespace CafeLibrary.Rendering
{
    public class BntxTexture : STGenericTexture, IDragDropNode, IRenamableNode
    {
        /// <summary>
        /// The parent BNTX binary container used to store the texture.
        /// </summary>
        public BntxFile BntxFile;

        /// <summary>
        /// The texture instance used in the binary file.
        /// </summary>
        public Texture Texture;

        public override TexFormat[] SupportedFormats => new TexFormat[]
{
            TexFormat.RGBA8_UNORM,
            TexFormat.RGBA8_SRGB,
            TexFormat.BC1_UNORM,
            TexFormat.BC1_SRGB,
            TexFormat.BC2_UNORM,
            TexFormat.BC2_SRGB,
            TexFormat.BC3_UNORM,
            TexFormat.BC3_SRGB,
            TexFormat.BC4_UNORM,
            TexFormat.BC4_SNORM,
            TexFormat.BC5_UNORM,
            TexFormat.BC5_SNORM,
            TexFormat.BC6H_UF16,
            TexFormat.BC6H_SF16,
            TexFormat.BC7_UNORM,
            TexFormat.BC7_SRGB,
        };

        public void Renamed(string text) {
            Texture.Name = text;
        }

        public BntxTexture() { }

        public BntxTexture(string filePath)
        {
            BntxFile = new BntxFile(filePath);
            Texture = BntxFile.Textures[0];
            ReloadImage();
        }

        public BntxTexture(BntxFile bntx, Texture tex)
        {
            Texture = tex;
            BntxFile = bntx;
            ReloadImage();
        }

        public void ReloadImage()
        {
            Name = Texture.Name;
            Width = Texture.Width;
            Height = Texture.Height;
            MipCount = Texture.MipCount;
            ArrayCount = Texture.ArrayLength;
            DisplayProperties = Texture;
            Platform = new SwitchSwizzle(FormatList[Texture.Format])
            {
                BlockHeightLog2 = (uint)Texture.BlockHeightLog2,
                Target = BntxFile.PlatformTarget != "NX  " ? 0 : 1,
            };

            RedChannel = SetChannel(Texture.ChannelRed);
            GreenChannel = SetChannel(Texture.ChannelGreen);
            BlueChannel = SetChannel(Texture.ChannelBlue);
            AlphaChannel = SetChannel(Texture.ChannelAlpha);

            if (Texture.SurfaceDim == SurfaceDim.Dim2DArray)
                SurfaceType = STSurfaceType.Texture2D_Array;

            if (Name.EndsWith("_st1"))
                SurfaceType = STSurfaceType.Texture2D_Array;
        }

        public void UpdateChannelSelectors()
        {
            Texture.ChannelRed = GetChannel(RedChannel);
            Texture.ChannelGreen = GetChannel(GreenChannel);
            Texture.ChannelBlue = GetChannel(BlueChannel);
            Texture.ChannelAlpha = GetChannel(AlphaChannel);
        }

        public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0, int DepthLevel = 0) {

            //Update the channels in the event the user may have adjusted them in the properties
            RedChannel = SetChannel(Texture.ChannelRed);
            GreenChannel = SetChannel(Texture.ChannelGreen);
            BlueChannel = SetChannel(Texture.ChannelBlue);
            AlphaChannel = SetChannel(Texture.ChannelAlpha);
            
            return Texture.TextureData[ArrayLevel][MipLevel];
        }

        private STChannelType SetChannel(ChannelType channelType)
        {
            if (channelType == ChannelType.Red) return STChannelType.Red;
            else if (channelType == ChannelType.Green) return STChannelType.Green;
            else if (channelType == ChannelType.Blue) return STChannelType.Blue;
            else if (channelType == ChannelType.Alpha) return STChannelType.Alpha;
            else if (channelType == ChannelType.Zero) return STChannelType.Zero;
            else return STChannelType.One;
        }

        private ChannelType GetChannel(STChannelType channelType)
        {
            if (channelType == STChannelType.Red) return ChannelType.Red;
            if (channelType == STChannelType.Green) return ChannelType.Green;
            if (channelType == STChannelType.Blue) return ChannelType.Blue;
            if (channelType == STChannelType.Alpha) return ChannelType.Alpha;
            if (channelType == STChannelType.Zero) return ChannelType.Zero;
            else return ChannelType.One;
        }

        public override void SetImageData(List<byte[]> imageData, uint width, uint height, int arrayLevel = 0)
        {
            throw new NotImplementedException();
        }

        static Dictionary<SurfaceFormat, TexFormat> FormatList = new Dictionary<SurfaceFormat, TexFormat>()
        {
            { SurfaceFormat.R8_G8_B8_A8_UNORM, TexFormat.RGBA8_UNORM },
            { SurfaceFormat.R8_G8_B8_A8_SRGB, TexFormat.RGBA8_SRGB },
            { SurfaceFormat.R8_G8_B8_A8_SNORM, TexFormat.RGBA8_SNORM },

            { SurfaceFormat.R11_G11_B10_UINT, TexFormat.RG11B10_FLOAT },
            { SurfaceFormat.R11_G11_B10_UNORM, TexFormat.RG11B10_FLOAT },
            { SurfaceFormat.A1_B5_G5_R5_UNORM, TexFormat.BGR5A1_UNORM },
            { SurfaceFormat.A4_B4_G4_R4_UNORM, TexFormat.BGRA4_UNORM },
            { SurfaceFormat.R10_G10_B10_A2_UNORM, TexFormat.RGBB10A2_UNORM },
            { SurfaceFormat.R16_G16_B16_A16_UNORM, TexFormat.RGBA16_UNORM },
            { SurfaceFormat.R16_G16_UNORM, TexFormat.RG16_UNORM },
            { SurfaceFormat.R16_UINT, TexFormat.R16_UINT },
            { SurfaceFormat.R16_UNORM, TexFormat.R16_UNORM },
            { SurfaceFormat.R24_G8_UNORM, TexFormat.R24G8_TYPELESS },
            { SurfaceFormat.R32_G32_B32_A32_UNORM, TexFormat.RGBA32_TYPELESS },
            { SurfaceFormat.R32_G32_B32_UNORM, TexFormat.RGB32_TYPELESS },
            { SurfaceFormat.R32_G32_UNORM, TexFormat.RG32_TYPELESS },
            { SurfaceFormat.R32_G8_X24_UNORM, TexFormat.R32G8X24_TYPELESS },
            { SurfaceFormat.R32_UNORM, TexFormat.R32_TYPELESS },
            { SurfaceFormat.R4_G4_B4_A4_UNORM, TexFormat.RGBA4_UNORM },
            { SurfaceFormat.R4_G4_UNORM, TexFormat.RG4_UNORM },
            { SurfaceFormat.R5_G5_B5_A1_UNORM, TexFormat.RGB5A1_UNORM },
            { SurfaceFormat.R5_G6_B5_UNORM, TexFormat.RGB565_UNORM },
            { SurfaceFormat.R8_G8_SNORM, TexFormat.RG8_SNORM },
            { SurfaceFormat.R8_G8_UNORM, TexFormat.RG8_UNORM },
            { SurfaceFormat.R8_UNORM, TexFormat.R8_UNORM },
            { SurfaceFormat.R9_G9_B9_E5_UNORM, TexFormat.RGB9E5_SHAREDEXP },
            { SurfaceFormat.D32_FLOAT_S8X24_UINT, TexFormat.D32_FLOAT_S8X24_UINT },

            { SurfaceFormat.BC1_UNORM, TexFormat.BC1_UNORM },
            { SurfaceFormat.BC1_SRGB, TexFormat.BC1_SRGB },
            { SurfaceFormat.BC2_UNORM, TexFormat.BC2_UNORM },
            { SurfaceFormat.BC2_SRGB, TexFormat.BC2_SRGB },
            { SurfaceFormat.BC3_UNORM, TexFormat.BC3_UNORM },
            { SurfaceFormat.BC3_SRGB, TexFormat.BC3_SRGB },
            { SurfaceFormat.BC4_UNORM, TexFormat.BC4_UNORM },
            { SurfaceFormat.BC4_SNORM, TexFormat.BC4_SNORM },
            { SurfaceFormat.BC5_UNORM, TexFormat.BC5_UNORM },
            { SurfaceFormat.BC5_SNORM, TexFormat.BC5_SNORM },
            { SurfaceFormat.BC6_FLOAT, TexFormat.BC6H_SF16 },
            { SurfaceFormat.BC6_UFLOAT, TexFormat.BC6H_UF16 },
            { SurfaceFormat.BC7_UNORM, TexFormat.BC7_UNORM },
            { SurfaceFormat.BC7_SRGB, TexFormat.BC7_SRGB },

            { SurfaceFormat.ASTC_4x4_UNORM, TexFormat.ASTC_4x4_UNORM },
            { SurfaceFormat.ASTC_5x4_UNORM, TexFormat.ASTC_5x4_UNORM },
            { SurfaceFormat.ASTC_5x5_UNORM, TexFormat.ASTC_5x5_UNORM },
            { SurfaceFormat.ASTC_6x5_UNORM, TexFormat.ASTC_6x5_UNORM },
            { SurfaceFormat.ASTC_6x6_UNORM, TexFormat.ASTC_6x6_UNORM },
            { SurfaceFormat.ASTC_8x5_UNORM, TexFormat.ASTC_8x5_UNORM },
            { SurfaceFormat.ASTC_8x6_UNORM, TexFormat.ASTC_8x6_UNORM },
            { SurfaceFormat.ASTC_8x8_UNORM, TexFormat.ASTC_8x8_UNORM },
            { SurfaceFormat.ASTC_10x5_UNORM, TexFormat.ASTC_10x5_UNORM },
            { SurfaceFormat.ASTC_10x6_UNORM, TexFormat.ASTC_10x6_UNORM },
            { SurfaceFormat.ASTC_10x8_UNORM, TexFormat.ASTC_10x8_UNORM },
            { SurfaceFormat.ASTC_10x10_UNORM, TexFormat.ASTC_10x10_UNORM },
            { SurfaceFormat.ASTC_12x10_UNORM, TexFormat.ASTC_12x10_UNORM },
            { SurfaceFormat.ASTC_12x12_UNORM, TexFormat.ASTC_12x12_UNORM },

            { SurfaceFormat.ASTC_4x4_SRGB, TexFormat.ASTC_4x4_SRGB },
            { SurfaceFormat.ASTC_5x4_SRGB, TexFormat.ASTC_5x4_SRGB },
            { SurfaceFormat.ASTC_5x5_SRGB, TexFormat.ASTC_5x5_SRGB },
            { SurfaceFormat.ASTC_6x5_SRGB, TexFormat.ASTC_6x5_SRGB },
            { SurfaceFormat.ASTC_6x6_SRGB, TexFormat.ASTC_6x6_SRGB },
            { SurfaceFormat.ASTC_8x5_SRGB, TexFormat.ASTC_8x5_SRGB },
            { SurfaceFormat.ASTC_8x6_SRGB, TexFormat.ASTC_8x6_SRGB },
            { SurfaceFormat.ASTC_8x8_SRGB, TexFormat.ASTC_8x8_SRGB },
            { SurfaceFormat.ASTC_10x5_SRGB, TexFormat.ASTC_10x5_SRGB },
            { SurfaceFormat.ASTC_10x6_SRGB, TexFormat.ASTC_10x6_SRGB },
            { SurfaceFormat.ASTC_10x8_SRGB, TexFormat.ASTC_10x8_SRGB },
            { SurfaceFormat.ASTC_10x10_SRGB, TexFormat.ASTC_10x10_SRGB },
            { SurfaceFormat.ASTC_12x10_SRGB, TexFormat.ASTC_12x10_SRGB },
            { SurfaceFormat.ASTC_12x12_SRGB, TexFormat.ASTC_12x12_SRGB },
        };
    }
}
