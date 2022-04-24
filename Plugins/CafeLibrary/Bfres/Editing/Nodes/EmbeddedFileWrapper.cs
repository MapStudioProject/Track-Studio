using System;
using System.IO;
using System.Linq;
using Toolbox.Core.ViewModels;
using Toolbox.Core;
using BfresLibrary;
using MapStudio.UI;

namespace CafeLibrary
{
    public class EmbeddedFilesFolder : NodeBase
    {
        public override string Header => "Embedded Files";

        ResFile ResFile;

        public EmbeddedFilesFolder(ResFile resFile) {
            ResFile = resFile;

            this.ContextMenus.Add(new MenuItemModel("Export All", ExportAllDialog));
            this.ContextMenus.Add(new MenuItemModel("Add File", AddFileDialog));
            this.ContextMenus.Add(new MenuItemModel(""));
            this.ContextMenus.Add(new MenuItemModel("Clear", Clear));

            Reload();
        }

        public void Reload()
        {
            this.Children.Clear();
            foreach (var file in ResFile.ExternalFiles)
                AddChild(new EmbeddedFileWrapper(file.Key, file.Value));
        }

        public void ExportAllDialog()
        {
            var dlg = new ImguiFolderDialog();
            if (dlg.ShowDialog())
            {
                foreach (var file in ResFile.ExternalFiles)
                    File.WriteAllBytes($"{dlg.SelectedPath}\\{file.Key}", file.Value.Data);
            }
        }

        public void AddFileDialog()
        {
            var dlg = new ImguiFileDialog();
            dlg.MultiSelect = true;
            dlg.FilterAll = true;

            if (dlg.ShowDialog())
            {
                foreach (var file in dlg.FilePaths)
                {
                    var fileNode = EmbeddedFileWrapper.FromFile(file);
                    fileNode.Header = Utils.RenameDuplicateString(fileNode.Header, this.Children.Select(x => x.Header).ToList());

                    AddChild(fileNode);
                    ResFile.ExternalFiles.Add(fileNode.Header, fileNode.ExternalFile);
                }
            }
        }

        public void Clear()
        {
            int result = TinyFileDialog.MessageBoxInfoYesNo("Are you sure you want to remove these? Operation cannot be undone.");
            if (result != 1)
                return;

            var files = this.Children.ToList();
            foreach (EmbeddedFileWrapper file in files)
                Remove(file);
        }

        public void Remove(EmbeddedFileWrapper file)
        {
            ResFile.ExternalFiles.Remove(file.ExternalFile);
            this.Children.Remove(file);
        }
    }

    public class EmbeddedFileWrapper : NodeBase
    {
        public ExternalFile ExternalFile;

        public EmbeddedFileWrapper(string name, ExternalFile file) : base(name) {
            ExternalFile = file;
            Tag = file.Data;
            CanRename = true;
            Icon = IconManager.FILE_ICON.ToString();

            MemoryEditor mem = new MemoryEditor();
            this.TagUI.UIDrawer += delegate
            {
                mem.Draw(file.Data, file.Data.Length);
            };

            this.ContextMenus.Add(new MenuItemModel("Export", Export));
            this.ContextMenus.Add(new MenuItemModel("Replace", Replace));
            this.ContextMenus.Add(new MenuItemModel(""));
            this.ContextMenus.Add(new MenuItemModel("Remove", Remove));
        }

        public static EmbeddedFileWrapper FromFile(string filePath)
        {
            string name = Path.GetFileName(filePath);
            return new EmbeddedFileWrapper(name, new ExternalFile() {
                Data = File.ReadAllBytes(filePath) 
            });
        }

        private void Export()
        {
            ImguiFileDialog dlg = new ImguiFileDialog();
            dlg.FileName = this.Header;
            dlg.SaveDialog = true;
            if (dlg.ShowDialog()) {
                File.WriteAllBytes(dlg.FilePath, ExternalFile.Data);
            }
        }

        private void Replace()
        {
            ImguiFileDialog dlg = new ImguiFileDialog();
            dlg.FileName = this.Header;
            dlg.SaveDialog = false;
            if (dlg.ShowDialog()) {
                ExternalFile.Data = File.ReadAllBytes(dlg.FilePath);
            }
        }

        private void Remove() {
            int result = TinyFileDialog.MessageBoxInfoYesNo(string.Format("Are you sure you want to remove {0}? Operation cannot be undone.", this.Header));
            if (result != 1)
                return;

            ((EmbeddedFilesFolder)Parent).Remove(this);
        }
    }
}
