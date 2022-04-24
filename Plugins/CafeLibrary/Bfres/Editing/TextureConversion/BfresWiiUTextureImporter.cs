using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BfresLibrary;
using BfresLibrary.WiiU;
using BfresLibrary.GX2;
using Toolbox.Core;
using Toolbox.Core.WiiU;

namespace CafeLibrary
{
    public class BfresWiiUTextureImporter
    {
        public static TextureShared ImportWiiUDDS(ResFile resFile, string name, Stream data)
        {
            var dds = new DDS(data);
            var ddsSurfaces = DDSHelper.GetArrayFaces(dds);
            return ImportTexture(name,
                ddsSurfaces, dds.Platform.OutputFormat, dds.Width, dds.Height, dds.Depth, dds.MipCount, dds.ArrayCount);
        }


        public static TextureShared ImportWiiU(ResFile resFile, string fileName)
        {
            string ext = Path.GetExtension(fileName);
            if (ext == ".bftex")
            {
                var texture = new Texture();
                texture.Import(fileName, resFile);
                return texture;
            }
            if (ext == ".dds")
            {
                var dds = new DDS(fileName);
                var ddsSurfaces = DDSHelper.GetArrayFaces(dds);
                return ImportTexture(fileName,
                   ddsSurfaces, dds.Platform.OutputFormat, dds.Width, dds.Height, dds.Depth, dds.MipCount, dds.ArrayCount);
            }
            return null;
        }

        public static TextureShared ImportWiiU(string name, List<STGenericTexture.Surface> surfaces, TexFormat format, uint width, uint height, uint mipCount)
        {
            return ImportTexture(name,
                surfaces, format, width, height, 1, mipCount, (uint)surfaces.Count);
        }

        static Texture ImportTexture(string fileName, List<STGenericTexture.Surface> surfaces,
            TexFormat format, uint width, uint height, uint depth, uint mipCount, uint arrayCount)
        {

            List<byte[]> outputSurfaces = new List<byte[]>();
            List<byte[]> outputMipmaps = new List<byte[]>();

            var swizzleHandler = new Toolbox.Core.Imaging.WiiUSwizzle(format);
            swizzleHandler.TileMode = (Toolbox.Core.WiiU.GX2.GX2TileMode)4;
            swizzleHandler.SurfaceUse = Toolbox.Core.WiiU.GX2.GX2SurfaceUse.USE_TEXTURE;
            if (swizzleHandler.Format == GX2.GX2SurfaceFormat.INVALID)
                swizzleHandler.Format = GX2.GX2SurfaceFormat.TCS_R8_G8_B8_A8_SRGB;

            for (int i = 0; i < arrayCount; i++)
            {
                var data = ByteUtils.CombineArray(surfaces[i].mipmaps.ToArray());

                var NewSurface = GX2.CreateGx2Texture(data, fileName,
                      (uint)swizzleHandler.TileMode,
                      (uint)swizzleHandler.AAMode,
                      (uint)width,
                      (uint)height,
                      (uint)1,
                      (uint)swizzleHandler.Format,
                      (uint)swizzleHandler.Swizzle,
                      (uint)swizzleHandler.SurfaceDimension,
                      (uint)mipCount
                      );

                //Add in alignment to the end of each array level
                if (arrayCount > 1)
                {
                    byte[] alignment = new byte[(int)NewSurface.alignment];
                    NewSurface.data = NewSurface.data.Concat(alignment).ToArray();
                    if (NewSurface.mipData != null)
                        NewSurface.mipData = NewSurface.mipData.Concat(alignment).ToArray();
                }

                outputSurfaces.Add(NewSurface.data);
                outputMipmaps.Add(NewSurface.mipData);

                swizzleHandler.MipOffsets = NewSurface.mipOffset;
                swizzleHandler.Alignment = NewSurface.alignment;
                swizzleHandler.Pitch = NewSurface.pitch;
                swizzleHandler.TileMode = (Toolbox.Core.WiiU.GX2.GX2TileMode)NewSurface.tileMode;
            }

            Texture texture = new Texture();
            texture.Name = Path.GetFileNameWithoutExtension(fileName);
            texture.Data = ByteUtils.CombineArray(outputSurfaces.ToArray());
            texture.MipData = ByteUtils.CombineArray(outputMipmaps.ToArray());
            texture.MipCount = mipCount;
            texture.ArrayLength = arrayCount;
            texture.Depth = arrayCount;
            texture.Width = width;
            texture.Height = height;
            texture.Alignment = swizzleHandler.Alignment;
            texture.Swizzle = swizzleHandler.Swizzle;
            texture.Pitch = swizzleHandler.Pitch;
            texture.MipOffsets = new uint[texture.MipCount];
            texture.TileMode = (GX2TileMode)swizzleHandler.TileMode;
            texture.Use = (GX2SurfaceUse)swizzleHandler.SurfaceUse;
            texture.Format = FormatList.FirstOrDefault(x => x.Value == swizzleHandler.OutputFormat).Key;
            texture.CompSelR = GX2CompSel.ChannelR;
            texture.CompSelG = GX2CompSel.ChannelG;
            texture.CompSelB = GX2CompSel.ChannelB;
            texture.CompSelA = GX2CompSel.ChannelA;

            for (int i = 0; i < texture.MipCount - 1; i++)
                texture.MipOffsets[i] = swizzleHandler.MipOffsets[i];
            return texture;
        }

        static Dictionary<GX2SurfaceFormat, TexFormat> FormatList = new Dictionary<GX2SurfaceFormat, TexFormat>()
        {
            { GX2SurfaceFormat.TC_R8_UNorm, TexFormat.R8_UNORM },
            { GX2SurfaceFormat.TC_R8_G8_UNorm, TexFormat.RG8_UNORM },
            { GX2SurfaceFormat.TCS_R8_G8_B8_A8_UNorm, TexFormat.RGBA8_UNORM  },
            { GX2SurfaceFormat.TCS_R8_G8_B8_A8_SRGB, TexFormat.RGBA8_SRGB  },

            { GX2SurfaceFormat.TC_R4_G4_B4_A4_UNorm, TexFormat.RGBA4_UNORM },
            { GX2SurfaceFormat.T_R4_G4_UNorm, TexFormat.RG4_UNORM},
            { GX2SurfaceFormat.TCS_R5_G6_B5_UNorm,TexFormat.RGB565_UNORM },
            { GX2SurfaceFormat.TC_R5_G5_B5_A1_UNorm, TexFormat.RGB5A1_UNORM },
            { GX2SurfaceFormat.TC_A1_B5_G5_R5_UNorm,  TexFormat.BGR5A1_UNORM  },

            { GX2SurfaceFormat.TCD_R16_UNorm,  TexFormat.R16_UNORM },
            { GX2SurfaceFormat.TC_R16_G16_UNorm, TexFormat.RG16_UNORM  },

            { GX2SurfaceFormat.T_BC1_UNorm,  TexFormat.BC1_UNORM},
            { GX2SurfaceFormat.T_BC1_SRGB, TexFormat.BC1_SRGB },
            { GX2SurfaceFormat.T_BC2_UNorm, TexFormat.BC2_UNORM },
            { GX2SurfaceFormat.T_BC2_SRGB, TexFormat.BC2_SRGB },
            { GX2SurfaceFormat.T_BC3_UNorm, TexFormat.BC3_UNORM },
            { GX2SurfaceFormat.T_BC3_SRGB,  TexFormat.BC3_SRGB },
            { GX2SurfaceFormat.T_BC4_UNorm, TexFormat.BC4_UNORM },
            { GX2SurfaceFormat.T_BC4_SNorm,TexFormat.BC4_SNORM },
            { GX2SurfaceFormat.T_BC5_UNorm, TexFormat.BC5_UNORM },
            { GX2SurfaceFormat.T_BC5_SNorm, TexFormat.BC5_SNORM },
        };
    }
}
