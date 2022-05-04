using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections.ObjectModel;
using Toolbox.Core.ViewModels;
using Toolbox.Core;
using MapStudio.UI;
using BfresLibrary;
using CafeLibrary.Rendering;
using Syroot.NintenTools.NSW.Bntx;

namespace CafeLibrary
{
    /// <summary>
    /// Represents a tree node for texture editing.
    /// </summary>
    public class TextureNode : NodeBase
    {
        public string SourceFilePath = "";

        public TextureNode(STGenericTexture tex) : base(tex.Name)
        {
            this.Tag = tex;

            ContextMenus.Add(new MenuItemModel("Export", ExportTextureDialog));
            ContextMenus.Add(new MenuItemModel("Replace", ReplaceTextureDialog));
            ContextMenus.Add(new MenuItemModel(""));
            ContextMenus.Add(new MenuItemModel("Rename", () => this.ActivateRename = true));
            ContextMenus.Add(new MenuItemModel(""));
            ContextMenus.Add(new MenuItemModel("Update", () => UpdateSourceFile()));
            /*   ContextMenus.Add(new MenuItemModel("SetAlpha128", () =>
               {
                   foreach (TextureNode node in Parent.Children)
                   {
                       if (node.IsSelected)
                           node.SetAlpha(50);
                   }
               }));*/
            ContextMenus.Add(new MenuItemModel("Export Alpha Channel", () =>
            {
                ExportChannel(3);
            }));
            ContextMenus.Add(new MenuItemModel("Inject Alpha Channel", () =>
            {
                InjectChannel(3);
            }));
          /*  ContextMenus.Add(new MenuItemModel("Decode Bake Alpha", () =>
            {
                DecodeBakeAlpha();
            }));*/
            ContextMenus.Add(new MenuItemModel(""));
            ContextMenus.Add(new MenuItemModel("Delete", () =>
            {
                int result = TinyFileDialog.MessageBoxInfoYesNo("Are you sure you want to remove these textures? Operation cannot be undone.");
                if (result != 1)
                    return;

                RemoveTexture(true);
            }));

            tex.DisplayPropertiesChanged += (o, e) =>
            {
                //Reload the display texture
                if (tex is FtexTexture)
                    ((FtexTexture)tex).ReloadImage();
                if (tex is BntxTexture)
                    ((BntxTexture)tex).ReloadImage();

                if (tex.Name != this.Header)
                    this.Header = tex.Name;

                ((TextureFolder)this.Parent).OnTextureEdited?.Invoke(this, EventArgs.Empty);
            };
            this.OnHeaderRenamed += delegate
            {
                //Update the lookup name table from the parent bfres file
                var folder = this.Parent as TextureFolder;
                folder.OnTextureRenamed?.Invoke(this, EventArgs.Empty);

                string name = tex.Name;
                //Remove original entry
                var textureData = folder.ResFile.Textures[name];
                folder.ResFile.Textures.RemoveKey(name);

                //Add new one
                if (!folder.ResFile.Textures.ContainsKey(this.Header))
                    folder.ResFile.Textures.Add(this.Header, textureData);
                //Update the name
                tex.Name = this.Header;
            };
        }

        //File updated externally
        public void UpdateSourceFile()
        {
            if (string.IsNullOrEmpty(SourceFilePath) || !File.Exists(SourceFilePath))
                return;

            var textureData = this.Tag as STGenericTexture;

            var type = Tag is BntxTexture ? typeof(BntxTexture) : typeof(FtexTexture);
            var dlg = new TextureDialog(type);
            var tex = dlg.AddTexture(SourceFilePath);
            //Keep same settings
            tex.Format = textureData.Platform.OutputFormat;
            tex.ChannelRed = textureData.RedChannel;
            tex.ChannelBlue = textureData.BlueChannel;
            tex.ChannelGreen = textureData.GreenChannel;
            tex.ChannelAlpha = textureData.AlphaChannel;
            tex.MipCount = textureData.MipCount;
            //Apply and encode image
            dlg.Apply();
            //Import
            ReplaceTexture(dlg.Textures.FirstOrDefault());
        }

        public void DecodeBakeAlpha(int arrayLevel = 0)
        {
            var textureData = this.Tag as STGenericTexture;

            var decoded = textureData.GetDecodedSurface(arrayLevel);
            //Load into image sharp for manpulating image data
            var dest = ImageSharpTextureHelper.DecodeHdrAlpha(decoded, (int)textureData.Width, (int)textureData.Height);

            textureData.Platform.OutputFormat = TexFormat.RGBA8_UNORM;
            InjectEdit(dest, arrayLevel);
        }

        public void ExportChannel(int channel, int arrayLevel = 0)
        {
            string name = "";
            if (channel == 0) name = "_Red";
            if (channel == 1) name = "_Green";
            if (channel == 2) name = "_Blue";
            if (channel == 3) name = "_Alpha";

            ImguiFileDialog dlg = new ImguiFileDialog();
            dlg.SaveDialog = true;
            dlg.FileName = $"{this.Header}{name}.png";
            foreach (var ext in TextureDialog.SupportedExtensions)
                dlg.AddFilter(ext, ext);
            if (!dlg.ShowDialog())
                return;

            var textureData = this.Tag as STGenericTexture;

            var decoded = textureData.GetDecodedSurface(arrayLevel);
            byte[] output = new byte[textureData.Width * textureData.Height * 4];
            int index = 0;
            for (int w = 0; w < textureData.Width; w++)
            {
                for (int h = 0; h < textureData.Height; h++)
                {
                    output[index] = decoded[index + channel];
                    output[index+1] = decoded[index + channel];
                    output[index+2] = decoded[index + channel];
                    output[index+3] = 255;
                    index += 4;
                }
            }
            ImageSharpTextureHelper.ExportFile(dlg.FilePath, output, (int)textureData.Width, (int)textureData.Height);
        }

        public void InjectEdit(byte[] decoded, int arrayLevel = 0)
        {
            var textureData = this.Tag as STGenericTexture;

            var type = Tag is BntxTexture ? typeof(BntxTexture) : typeof(FtexTexture);
            var dlg = new TextureDialog(type);
            //Add existing decoded image with edits applied
            var tex = dlg.AddTexture(this.Header, decoded, textureData.Width, textureData.Height, textureData.MipCount, textureData.Platform.OutputFormat);
            //Keep same settings
            tex.Format = textureData.Platform.OutputFormat;
            tex.ChannelRed = textureData.RedChannel;
            tex.ChannelBlue = textureData.BlueChannel;
            tex.ChannelGreen = textureData.GreenChannel;
            tex.ChannelAlpha = textureData.AlphaChannel;
            tex.MipCount = textureData.MipCount;
            //Apply and encode image
            dlg.Apply();
            //Import
            ReplaceTexture(dlg.Textures.FirstOrDefault());
        }

        public void InjectChannel(int channel, int arrayLevel = 0)
        {
            ImguiFileDialog dlg = new ImguiFileDialog();
            dlg.MultiSelect = false;
            foreach (var ext in TextureDialog.SupportedExtensions)
                dlg.AddFilter(ext, ext);

            if (dlg.ShowDialog())
            {
                var textureData = this.Tag as STGenericTexture;

                var data = ImageSharpTextureHelper.GetRgba(dlg.FilePath, (int)textureData.Width, (int)textureData.Height);

                var decoded = textureData.GetDecodedSurface(arrayLevel);
                decoded = ImageUtility.ConvertBgraToRgba(decoded);

                int index = 0;
                for (int w = 0; w < textureData.Width; w++)
                {
                    for (int h = 0; h < textureData.Height; h++)
                    {
                        decoded[index + channel] = data[index];
                        index += 4;
                    }
                }
              //  textureData.Platform.OutputFormat = TexFormat.BC3_UNORM;
                InjectEdit(decoded, arrayLevel);
            }
        }

        public void ExportTextureDialog()
        {
            //Multi select export
            var selected = this.Parent.Children.Where(x => x.IsSelected).ToList();
            if (selected.Count > 1)
            {
                ImguiFolderDialog dlg = new ImguiFolderDialog();
                //Todo configurable formats for folder dialog
                if (dlg.ShowDialog())
                {
                    foreach (var sel in selected)
                    {
                        var tex = sel.Tag as STGenericTexture;
                        tex.Export($"{dlg.SelectedPath}\\{tex.Name}.png", new TextureExportSettings());
                    }
                }
            }
            else
            {
                ImguiFileDialog dlg = new ImguiFileDialog();
                dlg.SaveDialog = true;
                dlg.FileName = $"{this.Header}.png";
                dlg.AddFilter(".bftex", ".bftex");
                dlg.AddFilter(".dds", ".dds");
                foreach (var ext in TextureDialog.SupportedExtensions)
                    dlg.AddFilter(ext, ext);

                if (dlg.ShowDialog())
                {
                    if (dlg.FilePath.EndsWith(".bftex"))
                        ExportBntx(dlg.FilePath);
                    else
                    {
                        var tex = this.Tag as STGenericTexture;
                        tex.Export(dlg.FilePath, new TextureExportSettings());
                    }
                }
            }
        }

        public void ExportBntx(string filePath)
        {
            var folder = this.Parent as TextureFolder;

            if (this.Tag is BntxTexture)
            {
                var tex = this.Tag as BntxTexture;
                tex.Texture.Export(filePath, folder.BntxFile);
            }
            else
            {
                var tex = this.Tag as FtexTexture;
                folder.ResFile.Textures[tex.Name].Export(filePath, folder.ResFile);
            }
        }

        public void ReplaceTextureDialog()
        {
            //Multi select replace
            var selected = this.Parent.Children.Where(x => x.IsSelected).ToList();
            if (selected.Count > 1)
            {
                ImguiFileDialog dlg = new ImguiFileDialog();
                dlg.SaveDialog = false;
                dlg.AddFilter(".bftex", ".bftex");
                dlg.AddFilter(".dds", ".dds");
                foreach (var ext in TextureDialog.SupportedExtensions)
                    dlg.AddFilter(ext, ext);

                //Mutli replace each selected texture node with the given texture
                if (dlg.ShowDialog())
                {
                    foreach (TextureNode tex in selected)
                        tex.ReplaceTexture(dlg.FilePath);
                }
            }
            else
            {
                ImguiFileDialog dlg = new ImguiFileDialog();
                dlg.SaveDialog = false;
                dlg.AddFilter(".bftex", ".bftex");
                dlg.AddFilter(".dds", ".dds");
                foreach (var ext in TextureDialog.SupportedExtensions)
                    dlg.AddFilter(ext, ext);

                if (dlg.ShowDialog())
                    ReplaceTexture(dlg.FilePath);
            }
        }

        public void ReplaceTexture(string filePath)
        {
            var folder = this.Parent as TextureFolder;
            //Add to the replace dialog. This dialog is capable of replacing multiple files via dictionary
            folder.ReplaceTextures(new Dictionary<string, TextureNode>() {
                { filePath, this }
            });
        }


        public void ReplaceRawTexture(string filePath)
        {
            var folder = this.Parent as TextureFolder;
            //Keep user data
            var userData = folder.ResFile.Textures[Header].UserData;

            var tex = BfresTextureImporter.ImportTextureRaw(folder.ResFile, folder.BntxFile, filePath);
            folder.ResFile.Textures[Header] = tex;
            tex.UserData = userData;
            tex.Name = this.Header;

            //Dispose previous
            ((STGenericTexture)this.Tag).RenderableTex?.Dispose();
            //Setup display texture
            this.Tag = BfresLoader.GetTexture(folder.ResFile, tex);
            //Update icon
            IconManager.RemoveTextureIcon(tex.Name);

            folder.OnTextureReplaced?.Invoke(this, EventArgs.Empty);

            IconManager.AddTexture(tex.Name, ((STGenericTexture)this.Tag), 18, 18);
        }

        public void ReplaceTexture(ImportedTexture importedTex)
        {
            var folder = this.Parent as TextureFolder;
            var mipmaps = importedTex.Surfaces[0].Mipmaps;

            List<STGenericTexture.Surface> surfaces = new List<STGenericTexture.Surface>();
            foreach (var surface in importedTex.Surfaces)
            {
                surfaces.Add(new STGenericTexture.Surface()
                {
                    mipmaps = surface.Mipmaps,
                });
            }
            //Keep user data
            var userData = folder.ResFile.Textures[Header].UserData;

            //Import replaced texture and set it in bfres
            var tex = BfresTextureImporter.ImportTexture(folder.ResFile, folder.BntxFile, Header, surfaces, importedTex.Format,
                (uint)importedTex.Width, (uint)importedTex.Height, (uint)mipmaps.Count);
            folder.ResFile.Textures[Header] = tex;
            tex.UserData = userData;
            tex.Name = this.Header;

            //Dispose previous
            ((STGenericTexture)this.Tag).RenderableTex?.Dispose();
            //Setup display texture
            var genericTex = BfresLoader.GetTexture(folder.ResFile, tex);
            this.Tag = genericTex;
            //Setup channels
            genericTex.RedChannel = importedTex.ChannelRed;
            genericTex.BlueChannel = importedTex.ChannelBlue;
            genericTex.GreenChannel = importedTex.ChannelGreen;
            genericTex.AlphaChannel = importedTex.ChannelAlpha;
            if (genericTex is BntxTexture)
                ((BntxTexture)genericTex).UpdateChannelSelectors();
            if (genericTex is FtexTexture)
                ((FtexTexture)genericTex).UpdateChannelSelectors();
            //Update icon
            IconManager.RemoveTextureIcon(tex.Name);

            folder.OnTextureReplaced?.Invoke(this, EventArgs.Empty);

            IconManager.AddTexture(tex.Name, ((STGenericTexture)this.Tag), 18, 18);
        }

        private void RemoveTexture(bool undo = false) {
            var selected = this.Parent.Children.Where(x => x.IsSelected).ToList();
            if (selected.Count > 1)
            {
                foreach (TextureNode tex in selected)
                    ((TextureFolder)tex.Parent).RemoveTexture(tex, undo);
            }
            else
                ((TextureFolder)this.Parent).RemoveTexture(this, undo);
        }
    }

    /// <summary>
    /// Represents a tree node for texture folder editing.
    /// </summary>
    public class TextureFolder : NodeBase
    {
        public override string Header => "Textures";

        public ResFile ResFile;
        public BntxFile BntxFile;

        public EventHandler OnTextureAdded;
        public EventHandler OnTextureRemoved;
        public EventHandler OnTextureReplaced;
        public EventHandler OnTextureEdited;
        public EventHandler OnTextureRenamed;

        /// <summary>
        /// Gets the textures from the folder into a lookup dictionary.
        /// </summary>
        public Dictionary<string, STGenericTexture> GetTextures()
        {
            Dictionary<string, STGenericTexture> textures = new Dictionary<string, STGenericTexture>();
            foreach (var node in this.Children)
                textures.Add(node.Header, node.Tag as STGenericTexture);
            return textures;
        }

        //For direct editing on bntx
        public TextureFolder(BntxFile bntx)
        {
            Init(new ResFile() { IsPlatformSwitch = true }, bntx);
        }

        //For editing bfres + bntx (optional)
        public TextureFolder(ResFile resFile, BntxFile bntx)
        {
            Init(resFile, bntx);
        }

        public static BntxFile CreateBntx()
        {
            var bntx = new BntxFile();
            bntx.Target = new char[] { 'N', 'X', ' ', ' ' };
            bntx.Name = "textures.bntx";
            bntx.Alignment = 0xC;
            bntx.TargetAddressSize = 0x40;
            bntx.VersionMajor = 0;
            bntx.VersionMajor2 = 4;
            bntx.VersionMinor = 0;
            bntx.VersionMinor2 = 0;
            bntx.Textures = new List<Texture>();
            bntx.TextureDict = new Syroot.NintenTools.NSW.Bntx.ResDict();
            bntx.RelocationTable = new RelocationTable();
            bntx.Flag = 0;
            return bntx;
        }

        private void Init(ResFile resFile, BntxFile bntx)
        {
            ResFile = resFile;
            BntxFile = bntx;

            //switch embeds bntx file data
            if (ResFile.IsPlatformSwitch)
                Tag = bntx;

            this.TagUI.UIDrawer += delegate
            {
                if (ResFile.IsPlatformSwitch)
                    ImguiBinder.LoadPropertiesComponentModelBase(bntx);
            };

            if (ResFile.IsPlatformSwitch)
            {
                ContextMenus.Add(new MenuItemModel("Export Bntx", ExportBntx));
                ContextMenus.Add(new MenuItemModel("Replace Bntx", ReplaceBntx));
                ContextMenus.Add(new MenuItemModel(""));
            }
            ContextMenus.Add(new MenuItemModel("Import Texture", ImportTextures));
            ContextMenus.Add(new MenuItemModel("Replace Textures (From Folder)", ReplaceTexturesFromFolder));
            ContextMenus.Add(new MenuItemModel("Export All Textures", ExportAllTextures));
            ContextMenus.Add(new MenuItemModel(""));
            ContextMenus.Add(new MenuItemModel("Sort", SortTextures));
            ContextMenus.Add(new MenuItemModel(""));
            ContextMenus.Add(new MenuItemModel("Clear", () =>
            {
                if (this.Children.Count == 0)
                    return;

                int result = TinyFileDialog.MessageBoxInfoYesNo("Are you sure you want to remove these textures? Operation cannot be undone.");
                if (result != 1)
                    return;

                Clear();
            }));

            foreach (var texture in ResFile.Textures)
            {
                var tex = BfresLoader.GetTexture(ResFile, texture.Value);
                texture.Value.Name = texture.Key;
                tex.Name = texture.Key;
                this.AddChild(new TextureNode(tex));
            }
        }

        public void CheckErrors() { }

        public void OnSave()
        {
            if (!ResFile.IsPlatformSwitch)
                return;

            //Save bntx textures from bfres
            if (BntxFile.TextureDict == null)
                BntxFile.TextureDict = new Syroot.NintenTools.NSW.Bntx.ResDict();
            if (BntxFile.Textures == null)
                BntxFile.Textures = new List<Texture>();

            BntxFile.Textures.Clear();
            BntxFile.TextureDict.Clear();
            foreach (BfresLibrary.Switch.SwitchTexture texture in ResFile.Textures.Values)
            {
                BntxFile.TextureDict.Add(texture.Name);
                BntxFile.Textures.Add(texture.Texture);
            }

            //Save bntx external data 
            var mem = new MemoryStream();
            BntxFile.Save(mem);
            var data = mem.ToArray();

            //Apply as external data
            foreach (var externalFile in ResFile.ExternalFiles.Values)
            {
                if (externalFile.LoadedFileData is BntxFile) {
                    externalFile.Data = data;
                    return;
                }
            }

            //Add to external files if not in external files
            ResFile.ExternalFiles.Add("textures.bntx", new ExternalFile()
            {
                Data = data,
                LoadedFileData = BntxFile,
            });
        }

        private void ExportBntx()
        {
            //Dialog for exporting bntx. 
            ImguiFileDialog fileDialog = new ImguiFileDialog();
            fileDialog.FileName = BntxFile.Name;
            fileDialog.SaveDialog = true;
            fileDialog.AddFilter(".bntx", "Texture Archive");

            if (fileDialog.ShowDialog()) {
                BntxFile.Save(fileDialog.FilePath);
            }
        }

        private void ReplaceBntx()
        {
            //Dialog for importing bntx. 
            ImguiFileDialog fileDialog = new ImguiFileDialog();
            fileDialog.SaveDialog = false;
            fileDialog.AddFilter(".bntx", "Texture Archive");

            if (fileDialog.ShowDialog()) {
                BntxFile = new BntxFile(fileDialog.FilePath);
                var textures = this.Children.ToList();
                foreach (TextureNode tex in textures)
                    RemoveTexture(tex);

                ResFile.Textures.Clear();
                foreach (var texture in BntxFile.Textures) {
                    var tex = new BntxTexture(BntxFile, texture);
                    //add to tree
                    this.AddChild(new TextureNode(tex));
                    //add to bfres
                    ResFile.Textures.Add(texture.Name, new BfresLibrary.Switch.SwitchTexture(BntxFile, texture));
                }
            }
        }

        private void Clear()
        {
            var textures = this.Children.ToList();
            foreach (TextureNode tex in textures)
                RemoveTexture(tex);
        }

        private bool ascending = true;

        private void SortTextures()
        {
            //Sort between different orders
            var sorted = ascending ?
                 Children.OrderBy(x => x.Header).ToList() :
                 Children.OrderByDescending(x => x.Header).ToList();
            //Switch order on next attempt
            ascending = !ascending;
            //Sort the collection
            for (int i = 0; i < sorted.Count; i++)
                Children.Move(Children.IndexOf(sorted[i]), i);
        }

        public void ImportTextures()
        {
            //Dialog for importing textures. 
            ImguiFileDialog fileDialog = new ImguiFileDialog();
            fileDialog.MultiSelect = true;
            foreach (var ext in TextureDialog.SupportedExtensions)
                fileDialog.AddFilter(ext, ext);
            fileDialog.AddFilter(".dds", "dds");
            fileDialog.AddFilter(".bftex", "bftex");

            if (fileDialog.ShowDialog())
                ImportTexture(fileDialog.FilePaths);
        }

        public void ImportBFTEXTexture(string name, Stream data) {

            var tex = BfresTextureImporter.ImportTextureBFTEX(ResFile, BntxFile, data);
            ProcessNewTexture("", tex);
        }

        public void ImportDDSTexture(string name, byte[] data) {
            ImportDDSTexture(name, new MemoryStream(data));
        }

        public void ImportDDSTexture(string name, Stream data)
        {
            if (this.Children.Any(x => x.Header == name))
                return;

            var tex = BfresTextureImporter.ImportTextureDDS(ResFile, BntxFile, name, data);
            ProcessNewTexture("", tex);
        }

        public void ImportTexture(string filePath) => ImportTexture(new string[] { filePath });

        public void ImportTexture(string[] filePaths)
        {
            var type = ResFile.IsPlatformSwitch ? typeof(BntxTexture) : typeof(FtexTexture);
            var dlg = new TextureDialog(type, TexFormat.BC1_SRGB);

            foreach (var filePath in filePaths)
            {
                string ext = Path.GetExtension(filePath);
                //Compressed types. (.dds2 for paint dot net dds extension)
                if (ext == ".dds" || ext == ".dds2" || ext == ".bftex")
                    AddTexture(filePath);
                //Use file dialog for uncompressed types
                else
                    dlg.AddTexture(filePath);
            }

            if (dlg.Textures.Count == 0)
                return;

            DialogHandler.Show(dlg.Name, 850, 350, dlg.Render, (o) =>
            {
                if (o != true)
                    return;

                ProcessLoading.Instance.IsLoading = true;
                foreach (var tex in dlg.Textures)
                {
                    var surfaces = tex.Surfaces;
                    AddTexture(tex.FilePath, tex);
                }
                ProcessLoading.Instance.IsLoading = false;
            });
        }

        public void ReplaceTextures(Dictionary<string, TextureNode> texturesToReplace)
        {
            var dlg = new TextureDialog(ResFile.IsPlatformSwitch ? typeof(BntxTexture) : typeof(FtexTexture));
            foreach (var texturePairs in texturesToReplace)
            {
                //file path for file
                string filePath = texturePairs.Key;
                //target texture
                var texture = texturePairs.Value;
                var textureData = texture.Tag as STGenericTexture;

                string ext = Path.GetExtension(texturePairs.Key);
                //Compressed types. (.dds2 for paint dot net dds extension)
                if (ext == ".dds" || ext == ".dds2" || ext == ".bftex")
                    texture.ReplaceRawTexture(filePath);
                //Use file dialog for uncompressed types
                else
                {
                    //Use the original settings by default
                    var tex = dlg.AddTexture(filePath);
                    if (tex == null) //Texture failed to load, skip
                        return;

                    tex.PlatformSwizzle = textureData.Platform;
                    tex.ChannelRed = textureData.RedChannel;
                    tex.ChannelGreen = textureData.GreenChannel;
                    tex.ChannelBlue = textureData.BlueChannel;
                    tex.ChannelAlpha = textureData.AlphaChannel;
                    tex.Format = textureData.Platform.OutputFormat;
                    tex.MipCount = textureData.MipCount;
                }
            }
            if (dlg.Textures.Count == 0)
                return;

            DialogHandler.Show(dlg.Name, 850, 350, dlg.Render, (o) =>
            {
                if (o != true)
                    return;

                foreach (var tex in dlg.Textures)
                {
                    var surfaces = tex.Surfaces;
                    texturesToReplace[tex.FilePath].ReplaceTexture(tex);
                }
            });
        }

        public void ExportAllTextures()
        {
            if (this.Children.Count == 0)
                return;

            ImguiFolderDialog folderDialog = new ImguiFolderDialog();
            if (folderDialog.ShowDialog())
                ExportAllTextures(folderDialog.SelectedPath, ".png");
        }

        public void ExportAllTextures(string folder, string ext = ".png")
        {
            foreach (var tex in this.Children)
            {
                var texData = tex.Tag as STGenericTexture;
                texData.Export($"{folder}\\{tex.Header}{ext}", new TextureExportSettings());
            }
            FileUtility.OpenFolder(folder);
        }

        public void ReplaceTexturesFromFolder()
        {
            ImguiFolderDialog folderDialog = new ImguiFolderDialog();
            if (folderDialog.ShowDialog())
            {
                var textureList = new Dictionary<string, TextureNode>();
                foreach (var file in Directory.GetFiles(folderDialog.SelectedPath))
                {
                    foreach (TextureNode tex in this.Children) {
                        if (Path.GetFileNameWithoutExtension(file) == tex.Header)
                            textureList.Add(file, tex);
                    }
                }

                if (textureList.Count > 0)
                    this.ReplaceTextures(textureList);
            }
        }

        private void AddTexture(string filePath)
        {
            var tex = BfresTextureImporter.ImportTextureRaw(ResFile, BntxFile, filePath);
            ProcessNewTexture(filePath, tex);
        }

        private void AddTexture(string filePath, ImportedTexture texture)
        {
            List<STGenericTexture.Surface> surfaces = new List<STGenericTexture.Surface>();
            foreach (var surface in texture.Surfaces)
            {
                surfaces.Add(new STGenericTexture.Surface() {
                    mipmaps = surface.Mipmaps,
                });
            }
            var tex = BfresTextureImporter.ImportTexture(ResFile, BntxFile, texture.Name, surfaces, texture.Format, (uint)texture.Width, (uint)texture.Height, (uint)texture.MipCount);
            var texNode = ProcessNewTexture(filePath, tex);
            var genericTex = ((STGenericTexture)texNode.Tag);

            genericTex.RedChannel = texture.ChannelRed;
            genericTex.GreenChannel = texture.ChannelGreen;
            genericTex.BlueChannel = texture.ChannelBlue;
            genericTex.AlphaChannel = texture.ChannelAlpha;

            if (genericTex is BntxTexture)
                ((BntxTexture)genericTex).UpdateChannelSelectors();
            if (genericTex is FtexTexture)
                ((FtexTexture)genericTex).UpdateChannelSelectors();
        }

        private TextureNode ProcessNewTexture(string filePath, TextureShared texture)
        {
            //Remove existing textures if names match.
            if (this.ResFile.Textures.ContainsKey(texture.Name))
            {
                this.ResFile.Textures.RemoveKey(texture.Name);
                RemoveTexture(texture.Name);
            }

            this.ResFile.Textures.Add(texture.Name, texture);
            //Reload
            var tex = BfresLoader.GetTexture(ResFile, texture);
            var texNode = new TextureNode(tex);
            //Store the source path for potential updating
            texNode.SourceFilePath = filePath;
            this.AddChild(texNode);

            OnTextureAdded?.Invoke(texNode, EventArgs.Empty);

            return texNode;
        }

        //Remove texture by name
        public void RemoveTexture(string name, bool undo = false)
        {
            var texNode = this.Children.FirstOrDefault(x => x.Header == name);
            RemoveTexture((TextureNode)texNode, undo);
        }

        //Remove texture by tree node removing
        public void RemoveTexture(TextureNode texNode, bool undo = false)
        {
            //Remove from bfres
            if (ResFile.Textures.ContainsKey(texNode.Header))
                ResFile.Textures.Remove(ResFile.Textures[texNode.Header]);
            //Remove from tree
            this.Children.Remove(texNode);
            //Dispose render
            var tag = texNode.Tag as STGenericTexture;
            tag.RenderableTex?.Dispose();
            //Remove icon
            IconManager.RemoveTextureIcon(texNode.Header);
            OnTextureRemoved?.Invoke(texNode, EventArgs.Empty);
            if (Workspace.ActiveWorkspace.PropertyWindow.SelectedObject == texNode)
                Workspace.ActiveWorkspace.PropertyWindow.SelectedObject = null;
        }
    }
}
