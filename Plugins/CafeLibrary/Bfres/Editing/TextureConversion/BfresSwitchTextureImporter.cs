using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BfresLibrary;
using Syroot.NintenTools.NSW.Bntx;
using Syroot.NintenTools.NSW.Bntx.GFX;
using Toolbox.Core;

namespace CafeLibrary
{
    public class BfresSwitchTextureImporter
    {
        public static TextureShared ImportSwitchDDS(BntxFile bntxFile, string name, Stream data)
        {
            var dds = new DDS(data);
            var ddsSurfaces = DDSHelper.GetArrayFaces(dds);
            return new BfresLibrary.Switch.SwitchTexture(bntxFile, ImportTexture(name,
                ddsSurfaces, dds.Platform.OutputFormat, dds.Width, dds.Height, dds.Depth, dds.MipCount, dds.ArrayCount));
        }

        public static TextureShared ImportSwitch(BntxFile bntxFile, string fileName)
        {
            string ext = Path.GetExtension(fileName);
            if (ext == ".bftex")
            {
                var texture = new Texture();
                texture.Import(fileName);
                return new BfresLibrary.Switch.SwitchTexture(bntxFile, texture);
            }
            if (ext == ".dds")
            {
                var dds = new DDS(fileName);
                var ddsSurfaces = DDSHelper.GetArrayFaces(dds);
                return new BfresLibrary.Switch.SwitchTexture(bntxFile, ImportTexture(fileName,
                    ddsSurfaces, dds.Platform.OutputFormat, dds.Width, dds.Height, dds.Depth, dds.MipCount, dds.ArrayCount));
            }
            if (ext == ".png" || ext == ".jpeg" || ext == ".bmp" || ext == ".gif" || ext == ".tga" || ext == ".jpg")
            {
            
            }
            
            return null;
        }

        public static TextureShared ImportSwitch(BntxFile bntxFile, string name, List<STGenericTexture.Surface> surfaces, TexFormat format, uint width, uint height, uint mipCount)
        {
            return new BfresLibrary.Switch.SwitchTexture(bntxFile, ImportTexture(name,
                surfaces, format, width, height, 1, mipCount, (uint)surfaces.Count));
        }

        static Texture ImportTexture( string fileName, List<STGenericTexture.Surface> surfaces,
            TexFormat format, uint width, uint height, uint depth, uint mipCount, uint arrayCount) {

            List<List<byte[]>> outputSurfaces = new List<List<byte[]>>();

            var swizzleHandler = new Toolbox.Core.Imaging.SwitchSwizzle(format);

            uint imageOffset = 0;
            for (int i = 0; i < arrayCount; i++)
            {
                var data = ByteUtils.CombineArray(surfaces[i].mipmaps.ToArray());
                List<byte[]> mipmaps = swizzleHandler.SwizzleSurfaceMipMaps(data, width, height, 1, mipCount, ref imageOffset);

                mipmaps[0] = ByteUtils.CombineArray(mipmaps.ToArray());

                //Add in alignment to the end of each array level
                if (arrayCount > 1)
                {
                    byte[] alignment = new byte[swizzleHandler.Alignment];
                    mipmaps[0] = mipmaps[0].Concat(alignment).ToArray();
                }

                outputSurfaces.Add(mipmaps);
            }

            Texture texture = new Texture();
            texture.Name = Path.GetFileNameWithoutExtension(fileName);
            texture.TextureData = outputSurfaces;
            texture.MipCount = mipCount;
            texture.ArrayLength = arrayCount;
            texture.BlockHeightLog2 = swizzleHandler.BlockHeightLog2;
            texture.ReadTextureLayout = (int)swizzleHandler.ReadTextureLayout;
            texture.Width = width;
            texture.Height = height;
            texture.Depth = 1;
            texture.Dim = Dim.Dim2D;
            texture.SurfaceDim = SurfaceDim.Dim2D;
            texture.Alignment = (int)swizzleHandler.Alignment;
            texture.MipOffsets = new long[texture.MipCount];
            texture.TileMode = (TileMode)swizzleHandler.TileMode;
            texture.Format = FormatList.FirstOrDefault(x => x.Value == swizzleHandler.OutputFormat).Key;
            texture.ChannelRed = ChannelType.Red;
            texture.ChannelGreen = ChannelType.Green;
            texture.ChannelBlue = ChannelType.Blue;
            texture.ChannelAlpha = ChannelType.Alpha;

            for (int i = 0; i < texture.MipCount - 1; i++)
                texture.MipOffsets[i] = swizzleHandler.MipOffsets[i];
            return texture;
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
