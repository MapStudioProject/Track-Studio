using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;
using GLFrameworkEngine;

namespace CafeLibrary.Rendering
{
    /// <summary>
    /// The texture view used to render the texture to shader
    /// This includes the rendered texture instance and additional properties.
    /// </summary>
    public class TextureView : TextureAsset
    {
        /// <summary>
        /// The component to determine the red channel output.
        /// </summary>
        public STChannelType RedChannel { get; set; }

        /// <summary>
        /// The component to determine the green channel output.
        /// </summary>
        public STChannelType GreenChannel { get; set; }

        /// <summary>
        /// The component to determine the blue channel output.
        /// </summary>
        public STChannelType BlueChannel { get; set; }

        /// <summary>
        /// The component to determine the alpha channel output.
        /// </summary>
        public STChannelType AlphaChannel { get; set; }

        public TextureView(string directory, STGenericTexture texture)
        {
            Name = texture.Name;
            RedChannel = texture.RedChannel;
            GreenChannel = texture.GreenChannel;
            BlueChannel = texture.BlueChannel;
            AlphaChannel = texture.AlphaChannel;

            if (TextureCache.CacheTexturesToDisk)
            {
                if (!TextureCache.HasTextueCached(directory, texture.Name))
                {
                    TextureCache.SaveTextureToDisk(directory, texture);
                }
                RenderableTex = TextureCache.LoadTextureFromDisk(directory, texture.Name);
            }
            else
            {
                if (!IsPow2(texture.Width) || !IsPow2(texture.Height))
                    RenderableTex = TextureCache.LoadTextureDecompressed(texture.GetBitmap(), texture.IsSRGB);
                else
                {
                    texture.LoadRenderableTexture();
                    RenderableTex = (GLTexture)texture.RenderableTex;
                }
            }
        }

        static bool IsPow2(uint Value)
        {
            return Value != 0 && (Value & (Value - 1)) == 0;
        }
    }
}
