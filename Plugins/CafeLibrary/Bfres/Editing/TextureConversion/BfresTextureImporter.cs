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
    public class BfresTextureImporter
    {
        public static TextureShared ImportTextureRaw(ResFile resFile, BntxFile bntxFile, string fileName)
        {
            TextureShared texture = null;
            if (resFile.IsPlatformSwitch)
                texture = BfresSwitchTextureImporter.ImportSwitch(bntxFile, fileName);
            else
                texture = BfresWiiUTextureImporter.ImportWiiU(resFile, fileName);
            return texture;
        }

        public static TextureShared ImportTextureBFTEX(ResFile resFile, BntxFile bntxFile, Stream stream)
        {
            if (resFile.IsPlatformSwitch)
            {
                var tex = new Texture();
                tex.Import(stream);
                return new BfresLibrary.Switch.SwitchTexture(bntxFile, tex);
            }
            else
            {
                var tex = new BfresLibrary.WiiU.Texture();
                tex.Import(stream, resFile);
                return tex;
            }
        }

        public static TextureShared ImportTexture(ResFile resFile, BntxFile bntxFile, string fileName,
            List<STGenericTexture.Surface> surfaces, TexFormat format, uint width, uint height, uint mipCount)
        {
            TextureShared texture = null;
            if (resFile.IsPlatformSwitch)
                texture = BfresSwitchTextureImporter.ImportSwitch(bntxFile, fileName, surfaces, format, width, height, mipCount);
            else
                texture = BfresWiiUTextureImporter.ImportWiiU(fileName, surfaces, format, width, height, mipCount);
            return texture;
        }

        public static TextureShared ImportTextureDDS(ResFile resFile, BntxFile bntxFile, string name, Stream data)
        {
            TextureShared texture = null;
            if (resFile.IsPlatformSwitch)
                texture = BfresSwitchTextureImporter.ImportSwitchDDS(bntxFile, name, data);
            else
                texture = BfresWiiUTextureImporter.ImportWiiUDDS(resFile, name, data);
            return texture;
        }
    }
}
