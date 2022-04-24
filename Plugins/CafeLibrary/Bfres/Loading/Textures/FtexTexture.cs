using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core;
using Toolbox.Core.Imaging;
using Toolbox.Core.WiiU;
using BfresLibrary.WiiU;
using BfresLibrary;
using BfresLibrary.GX2;
using MapStudio.UI;

namespace CafeLibrary.Rendering
{
    public class FtexTexture : STGenericTexture, IDragDropNode, IRenamableNode
    {
        /// <summary>
        /// The texture section used in the bfres.
        /// </summary>
        public Texture Texture;

        /// <summary>
        /// The file in which the data in this section is parented to.
        /// </summary>
        public ResFile ParentFile { get; set; }

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
        };

        public void Renamed(string text) {
            Texture.Name = text;
        }

        public FtexTexture() { }

        public FtexTexture(ResFile resFile, Texture texture) : base() {
            ParentFile = resFile;
            Texture = texture;
            ReloadImage();
        }

        public void ReloadImage()
        {
            Name = Texture.Name;
            Width = Texture.Width;
            Height = Texture.Height;
            MipCount = Texture.MipCount;
            Depth = Texture.Depth;
            ArrayCount = Texture.ArrayLength;
            RedChannel = SetChannel(Texture.CompSelR);
            GreenChannel = SetChannel(Texture.CompSelG);
            BlueChannel = SetChannel(Texture.CompSelB);
            AlphaChannel = SetChannel(Texture.CompSelA);
            DisplayProperties = Texture;

            this.DisplayPropertiesChanged += delegate
            {
                RedChannel = SetChannel(Texture.CompSelR);
                GreenChannel = SetChannel(Texture.CompSelG);
                BlueChannel = SetChannel(Texture.CompSelB);
                AlphaChannel = SetChannel(Texture.CompSelA);
            };

            Platform = new WiiUSwizzle((GX2.GX2SurfaceFormat)Texture.Format)
            {
                AAMode = (GX2.GX2AAMode)Texture.AAMode,
                TileMode = (GX2.GX2TileMode)Texture.TileMode,
                SurfaceDimension = (GX2.GX2SurfaceDimension)Texture.Dim,
                SurfaceUse = (GX2.GX2SurfaceUse)Texture.Use,
                MipOffsets = Texture.MipOffsets,
                Swizzle = Texture.Swizzle,
                Alignment = Texture.Alignment,
                MipData = Texture.MipData,
                Pitch = Texture.Pitch,
            };

           // if (Texture.Dim == GX2SurfaceDim.Dim2DArray)
            //    SurfaceType = STSurfaceType.Texture2D_Array;
        }

        public void UpdateChannelSelectors()
        {
            Texture.CompSelR = GetChannel(RedChannel);
            Texture.CompSelG = GetChannel(GreenChannel);
            Texture.CompSelB = GetChannel(BlueChannel);
            Texture.CompSelA = GetChannel(AlphaChannel);
        }

        public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0, int DepthLevel = 0) {
            return Texture.Data;
        }

        public override void SetImageData(List<byte[]> imageData, uint width, uint height, int arrayLevel = 0)
        {
            throw new NotImplementedException();
        }

        private STChannelType SetChannel(GX2CompSel channelType)
        {
            if (channelType == GX2CompSel.ChannelR) return STChannelType.Red;
            else if (channelType == GX2CompSel.ChannelG) return STChannelType.Green;
            else if (channelType == GX2CompSel.ChannelB) return STChannelType.Blue;
            else if (channelType == GX2CompSel.ChannelA) return STChannelType.Alpha;
            else if (channelType == GX2CompSel.Always0) return STChannelType.Zero;
            else return STChannelType.One;
        }

        private GX2CompSel GetChannel(STChannelType channelType)
        {
            if (channelType == STChannelType.Red) return GX2CompSel.ChannelR;
            if (channelType == STChannelType.Green) return GX2CompSel.ChannelG;
            if (channelType == STChannelType.Blue) return GX2CompSel.ChannelB;
            if (channelType == STChannelType.Alpha) return GX2CompSel.ChannelA;
            if (channelType == STChannelType.Zero) return GX2CompSel.Always0;
            else return GX2CompSel.Always1;
        }
    }
}
