using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Toolbox.Core.ViewModels;
using Toolbox.Core;
using Toolbox.Core.IO;
using BfshaLibrary;

namespace CafeLibrary
{
    public class BFSHA : IFileFormat
    {
        public bool CanSave { get; set; } = false;

        public string[] Description { get; set; } = new string[] { "BFSHA" };
        public string[] Extension { get; set; } = new string[] { "*.bfsha" };

        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                return reader.CheckSignature(4, "FSHA");
            }
        }

        public BfshaFile BfshaFile;

        public void Load(Stream stream)
        {
            BfshaFile = new BfshaFile(stream);
        }

        public void Save(Stream stream)
        {

        }
    }
}