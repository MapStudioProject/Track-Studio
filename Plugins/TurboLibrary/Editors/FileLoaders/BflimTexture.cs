using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.Imaging;
using Toolbox.Core.WiiU;
using MapStudio.UI;
using System.Runtime.InteropServices;
using Toolbox.Core.ViewModels;

namespace TurboLibrary
{
    public class BflimEditor : FileEditor, IFileFormat
    {
        public string[] Description => new string[] { "bflim" };
        public string[] Extension => new string[] { "*.bflim" };

        /// <summary>
        /// Wether or not the file can be saved.
        /// </summary>
        public bool CanSave { get; set; } = true;

        /// <summary>
        /// Information of the loaded file.
        /// </summary>
        public File_Info FileInfo { get; set; } = new File_Info();

        public bool Identify(File_Info fileInfo, Stream stream) {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(4, "FLIM", (int)reader.BaseStream.Length - 0x28);
            }
        }

        BflimTexture BflimTexture;

        public void Load(Stream stream)
        {
            BflimTexture = new BflimTexture();
            using (var reader = new FileReader(stream)) {
                BflimTexture.Name = System.IO.Path.GetFileNameWithoutExtension(this.FileInfo.FileName);
                BflimTexture.Read(reader);
            }
            var node = new NodeBase("texture") {  Tag = BflimTexture };
            Root.AddChild(node);
        }

        public void Save(Stream stream)
        {
            using (var writer = new FileWriter(stream)) {
                BflimTexture.Write(writer);
            }
        }
    }

    public class BflimTexture : STGenericTexture, IDragDropNode, IContextMenu
    {
        public EventHandler TextureReplaced;

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

        FileHeader FileHeaderInfo;
        ImageHeader ImageInfo;

        public GX2.GX2TileMode TileMode
        {
            get { return (GX2.GX2TileMode)((int)ImageInfo.swizzleTileMode & 31); }
            set {
                ImageInfo.swizzleTileMode = (byte)((int)ImageInfo.swizzleTileMode & 224 | (int)(byte)value & 31);
            }
        }

        public uint Swizzle
        {
            get { return (uint)(((int)((uint)ImageInfo.swizzleTileMode >> 5) & 7) << 8); }
            set {
                ImageInfo.swizzleTileMode = (byte)((int)ImageInfo.swizzleTileMode & 31 | (int)(byte)(value >> 8) << 5);
            }
        }

        public byte[] ImageData;

        public BflimTexture() {
            FileHeaderInfo = new FileHeader();
            ImageInfo = new ImageHeader();
            ImageInfo.Alignment = 8192;

            Swizzle = 1024;
            TileMode = GX2.GX2TileMode.MODE_2D_TILED_THIN1;
            MipCount = 1;
            Depth = 1;
            ArrayCount = 1;
            DisplayProperties = this;

            uint swizzle = (Swizzle >> 8) & 7;

            Platform = new WiiUSwizzle(TexFormat.BC3_SRGB)
            {
                AAMode = GX2.GX2AAMode.GX2_AA_MODE_1X,
                TileMode = this.TileMode,
                SurfaceDimension = GX2.GX2SurfaceDimension.DIM_2D,
                SurfaceUse = GX2.GX2SurfaceUse.USE_TEXTURE,
                MipOffsets = new uint[0],
                Swizzle = swizzle,
                Alignment = (uint)ImageInfo.Alignment,
            };
        }

        public BflimTexture(string filePath) : base()
        {
            using (var reader = new FileReader(filePath))
            {
                this.Name = System.IO.Path.GetFileNameWithoutExtension(filePath);
                Read(reader);
            }
        }

        public BflimTexture(Stream stream) : base()
        {
            using (var reader = new FileReader(stream)) {
                Read(reader);
            }
        }


        public void Save(Stream stream)
        {
            if (ImageData == null)
                return;

            using (var writer = new FileWriter(stream)) {
                Write(writer);
            }
        }

        public void InitDefault() {
            RenderableTex = GLFrameworkEngine.GLTexture2D.CreateConstantColorTexture(4, 4, 0, 0, 0, 0);
        }

        public MenuItemModel[] GetContextMenuItems()
        {
              return new MenuItemModel[]
              {
                  new MenuItemModel("Export", ExportDialog),
                  new MenuItemModel("Replace", Replace),
              };
        }

        public void SaveDialog()
        {
            ImguiFileDialog fileDialog = new ImguiFileDialog();
            fileDialog.SaveDialog = true;
            fileDialog.FileName = this.Name + ".bflim";
            fileDialog.AddFilter(".bflim", "bflim");
            if (fileDialog.ShowDialog())
                this.Save(File.OpenWrite(fileDialog.FilePath));
        }

        public void ExportDialog()
        {
            ImguiFileDialog fileDialog = new ImguiFileDialog();
            fileDialog.SaveDialog = true;
            fileDialog.FileName = this.Name;
            foreach (var ext in TextureDialog.SupportedExtensions)
                fileDialog.AddFilter(ext, ext);
            fileDialog.AddFilter(".dds", "dds");
            if (fileDialog.ShowDialog())
                this.Export(fileDialog.FilePath, new TextureExportSettings());
        }
            
        private void Replace()
        {
            ImguiFileDialog fileDialog = new ImguiFileDialog();
            fileDialog.MultiSelect = true;
            foreach (var ext in TextureDialog.SupportedExtensions)
                fileDialog.AddFilter(ext, ext);
            fileDialog.AddFilter(".dds", "dds");

            if (fileDialog.ShowDialog())
            {
                var type = typeof(BflimTexture);
                var dlg = new TextureDialog(type, TexFormat.BC1_SRGB);

                foreach (var fileName in fileDialog.FilePaths)
                {
                    string ext = System.IO.Path.GetExtension(fileName);
                    //Compressed types. (.dds2 for paint dot net dds extension)
                    if (ext == ".dds" || ext == ".dds2")
                    {
                        DDS dds = new DDS(fileName);
                        ReplaceTexture((int)dds.Width, (int)dds.Height, dds.Platform.OutputFormat, dds.ImageData);
                    }
                    //Use file dialog for uncompressed types
                    else
                        dlg.AddTexture(fileName);
                }
                if (dlg.Textures.Count == 0)
                    return;

                DialogHandler.Show(dlg.Name, 850, 350, dlg.Render, (o) =>
                {
                    if (o != true)
                        return;

                    ProcessLoading.Instance.IsLoading = true;

                    int index = 0;
                    foreach (var tex in dlg.Textures)
                    {
                        ProcessLoading.Instance.Update(index++, dlg.Textures.Count, $"Swizzling {tex.Name}");

                        var surfaces = tex.Surfaces;
                        ReplaceTexture(tex.Width, tex.Height, tex.Format, surfaces[0].Mipmaps[0]);
                    }

                    ProcessLoading.Instance.IsLoading = false;
                });
            }
        }

        public void ReplaceDialogNoConfig()
        {
            ImguiFileDialog fileDialog = new ImguiFileDialog();
            fileDialog.MultiSelect = true;
            foreach (var ext in TextureDialog.SupportedExtensions)
                fileDialog.AddFilter(ext, ext);
            fileDialog.AddFilter(".dds", "dds");

            if (fileDialog.ShowDialog())
            {
                var type = typeof(BflimTexture);
                var dlg = new TextureDialog(type, TexFormat.BC1_SRGB);

                foreach (var fileName in fileDialog.FilePaths)
                {
                    string ext = System.IO.Path.GetExtension(fileName);
                    //Compressed types. (.dds2 for paint dot net dds extension)
                    if (ext == ".dds" || ext == ".dds2")
                    {
                        DDS dds = new DDS(fileName);
                        ReplaceTexture((int)dds.Width, (int)dds.Height, dds.Platform.OutputFormat, dds.ImageData);
                    }
                    //Use file dialog for uncompressed types
                    else
                        dlg.AddTexture(fileName);
                }

                dlg.Apply();
                foreach (var tex in dlg.Textures)
                {
                    var surfaces = tex.Surfaces;
                    ReplaceTexture(tex.Width, tex.Height, tex.Format, surfaces[0].Mipmaps[0]);
                }
            }
        }

        private void ReplaceTexture(int width, int height, TexFormat format, byte[] data)
        {
            ImageInfo.Width = (short)width;
            ImageInfo.Height = (short)height;
            ImageInfo.Format = FormatsWiiU.FirstOrDefault(x => x.Value == format).Key;

            uint swizzle = (this.Swizzle >> 8) & 7;

            ((WiiUSwizzle)this.Platform).Swizzle = swizzle;
            ((WiiUSwizzle)this.Platform).UpdateFormat(format);
            ImageData = ((WiiUSwizzle)this.Platform).SwizzleImageData(data, (uint)width, (uint)height, 1);
            ImageInfo.Alignment = (short)((WiiUSwizzle)this.Platform).Alignment;
            this.TileMode = ((WiiUSwizzle)this.Platform).TileMode;

            ReloadImage(ImageInfo);

            TextureReplaced?.Invoke(this, EventArgs.Empty);
        }

        public void Read(FileReader reader)
        {
            uint FileSize = (uint)reader.BaseStream.Length;

            reader.SeekBegin(FileSize - 0x28);
            reader.SetByteOrder(true);

            FileHeaderInfo = reader.ReadStruct<FileHeader>();
            ImageInfo =  reader.ReadStruct<ImageHeader>();

            reader.Position = 0;
            ImageData = reader.ReadBytes((int)ImageInfo.DataSize);

            ReloadImage(ImageInfo);
        }

        public void Write(FileWriter writer)
        {
            ImageInfo.DataSize = ImageData.Length;

            writer.SetByteOrder(true);
            writer.Write(ImageData);

            var headerPos = writer.Position;
            writer.WriteStruct(FileHeaderInfo);
            writer.WriteStruct(ImageInfo);

            //write file size
            using (writer.TemporarySeek(headerPos + 12, System.IO.SeekOrigin.Begin)) {
                writer.Write((uint)writer.BaseStream.Length);
            }
        }

        private void ReloadImage(ImageHeader image)
        {
            MipCount = 1;
            Depth = 1;
            ArrayCount = 1;
            DisplayProperties = this;
            Width = (uint)image.Width;
            Height = (uint)image.Height;

            Platform = new WiiUSwizzle(FormatsWiiU[image.Format])
            {
                AAMode = GX2.GX2AAMode.GX2_AA_MODE_1X,
                TileMode = this.TileMode,
                SurfaceDimension = GX2.GX2SurfaceDimension.DIM_2D,
                SurfaceUse = GX2.GX2SurfaceUse.USE_TEXTURE,
                MipOffsets = new uint[0],
                Swizzle = this.Swizzle,
                Alignment = (uint)image.Alignment,
            };
            LoadRenderableTexture();
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class FileHeader
        {
            public uint magic = 0x464C494D;
            public ushort bom = 0xFEFF;
            public ushort headerSize = 0x14;
            public uint version = 0x02020000;
            public uint fileSize;
            public ushort numBlocks = 1;
            public ushort padding;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class ImageHeader
        {
            public uint magic = 0x696D6167;
            public uint blockSize = 0x10;
            public short Width;
            public short Height;
            public short Alignment;
            public byte Format;
            public byte swizzleTileMode;
            public int DataSize;
        }

        public override byte[] GetImageData(int ArrayLevel = 0, int MipLevel = 0, int DepthLevel = 0) {
            return ImageData;
        }

        public override void SetImageData(List<byte[]> imageData, uint width, uint height, int arrayLevel = 0)
        {
            ImageInfo.Width = (short)width;
            ImageInfo.Height = (short)height;
            ImageInfo.Format = FormatsWiiU.FirstOrDefault(x => x.Value == Platform.OutputFormat).Key;

            uint swizzle = (this.Swizzle >> 8) & 7;

            ((WiiUSwizzle)this.Platform).Swizzle = swizzle;
            ((WiiUSwizzle)this.Platform).UpdateFormat(Platform.OutputFormat);
            ImageData = ((WiiUSwizzle)this.Platform).SwizzleImageData(imageData[0], (uint)width, (uint)height, 1);
            ImageInfo.Alignment = (short)((WiiUSwizzle)this.Platform).Alignment;
            this.TileMode = ((WiiUSwizzle)this.Platform).TileMode;
        }

        public static Dictionary<byte, TexFormat> FormatsWiiU = new Dictionary<byte, TexFormat>()
        {
            [0] = TexFormat.L8,
            [1] = TexFormat.A8_UNORM,
            [2] = TexFormat.LA4,
            [3] = TexFormat.LA8,
            [4] = TexFormat.RGB8_UNORM, //HILO8
            [5] = TexFormat.BGR565_UNORM,
            [6] = TexFormat.BGRA8_UNORM,
            [7] = TexFormat.RGB5A1_UNORM,
            [8] = TexFormat.BGRA4_UNORM,
            [9] = TexFormat.RGB8_UNORM,
            [10] = TexFormat.ETC1_UNORM,
            [11] = TexFormat.ETC1_A4,
            [12] = TexFormat.BC1_UNORM,
            [13] = TexFormat.BC2_UNORM,
            [14] = TexFormat.BC3_UNORM,
            [15] = TexFormat.BC4_UNORM, //BC4L_UNORM
            [16] = TexFormat.BC4_UNORM, //BC4A_UNORM
            [17] = TexFormat.BC5_UNORM,
            [18] = TexFormat.L4,
            [19] = TexFormat.A4,
            [20] = TexFormat.RGBA8_SRGB,
            [21] = TexFormat.BC1_SRGB,
            [22] = TexFormat.BC2_SRGB,
            [23] = TexFormat.BC3_SRGB,
            [24] = TexFormat.RGBB10A2_UNORM,
            [25] = TexFormat.RGB565_UNORM,
        };
    }
}
