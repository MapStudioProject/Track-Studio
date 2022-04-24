using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Toolbox.Core.ViewModels;
using BfresLibrary;
using BfresLibrary.Core;
using MapStudio.UI;

namespace CafeLibrary
{
    public class SubSectionBase : NodeBase
    {
        public virtual string[] Extensions { get; } = new string[0];

        public SubSectionBase()
        {
            ContextMenus.Add(new MenuItemModel(TranslationSource.GetText("NEW"), CreateNew));
            ContextMenus.Add(new MenuItemModel(TranslationSource.GetText("IMPORT"), ImportDialog));
            ContextMenus.Add(new MenuItemModel(TranslationSource.GetText("EXPORT"), ExportDialog));
            ContextMenus.Add(new MenuItemModel(TranslationSource.GetText("REPLACE_FROM_FOLDER"), ReplaceFromFolderDialog));
            ContextMenus.Add(new MenuItemModel(""));
            ContextMenus.Add(new MenuItemModel(TranslationSource.GetText("SORT"), Sort));
            ContextMenus.Add(new MenuItemModel(TranslationSource.GetText("CLEAR"), Clear));
        }

        public virtual void CreateNew()
        {

        }

        public virtual void ImportDialog()
        {
            var dlg = new ImguiFileDialog();
            for (int i = 0; i < Extensions.Length; i++)
                dlg.AddFilter(Extensions[i], Extensions[i]);
            if (dlg.ShowDialog())
            {

            }
        }

        public virtual void ExportDialog()
        {
            var dlg = new ImguiFolderDialog();
            if (dlg.ShowDialog())
            {

            }
        }

        public virtual void ReplaceFromFolderDialog()
        {
            var dlg = new ImguiFolderDialog();
            if (dlg.ShowDialog())
            {
                foreach (var file in Directory.GetFiles(dlg.SelectedPath))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    if (this.Children.Any(x => x.Header == name))
                        Replace(this.Children.FirstOrDefault(x => x.Header == name), file);
                }
            }
        }

        public virtual void Replace(NodeBase node, string filePath)
        {

        }

        public virtual void Sort()
        {

        }

        public virtual void Clear()
        {

        }
    }
}
