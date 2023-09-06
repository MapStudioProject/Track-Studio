using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;
using Toolbox.Core.IO;

namespace CafeLibrary
{
    internal class ZDic
    {
        public static void DumpExternalDictionaries()
        {
            string path = Path.Combine(PluginConfig.TotkGamePath, "Pack", "ZsDic.pack.zs");
            if (!File.Exists(path))
                return;

            string target = Path.Combine(Runtime.ExecutableDir, "User", "ZstdDictionaries");
            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);

            try
            {
                var archive = STFileLoader.OpenFileFormat(path) as IArchiveFile;
                if (archive != null)
                {
                    foreach (var dic in archive.Files)
                    {
                        string filePath = Path.Combine(target, dic.FileName);
                        dic.FileData.SaveToFile(filePath);
                    }
                }
            }
            catch
            {

            }
        }
    }
}
