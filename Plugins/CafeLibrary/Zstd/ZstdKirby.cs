using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;
using ZstdSharp;
using Toolbox.Core.IO;

namespace CafeLibrary
{
    public class ZstdKirby : ICompressionFormat
    {
        public string[] Description => new string[] { "ZSTD" };

        public string[] Extension => new string[] { ".zstd" };

        public bool CanCompress => true;

        static string fileNameTemp = "";

        public bool Identify(Stream stream, string fileName)
        {
            //Small hack to check current file name
            fileNameTemp = fileName;

            using (var reader = new FileReader(stream, true))
            {
                uint size = reader.ReadUInt32();
                uint magic = reader.ReadUInt32();

                reader.Position = 0;
                return magic == 0x28B52FFD || magic == 0xFD2FB528;
            }
        }

        public Stream Decompress(Stream stream)
        {
            stream.Position = 4;

            var mem = new MemoryStream();
            using (var decompressionStream = new DecompressionStream(stream))
            {
                decompressionStream.LoadDictionary(GetExternalDictionaries());
                decompressionStream.CopyTo(mem);
            }
            return mem;
        }

        public Stream Compress(Stream stream)
        {
            var mem = new MemoryStream();
            using var compressionStream = new CompressionStream(stream);
            {
                compressionStream.CopyTo(mem);
            }
            return mem;
        }

        public static byte[] Decompress(byte[] src)
        {
            using var decompressor = new Decompressor();
            {
                decompressor.LoadDictionary(GetExternalDictionaries());
                return decompressor.Unwrap(src).ToArray();
            }
        }

        static byte[] GetExternalDictionaries()
        {
            byte[] dictionary = new byte[0];

            string folder = Path.Combine(Runtime.ExecutableDir, "User", "ZstdDictionaries");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            void TransferZDic(string path)
            {
                //Check if old directory contains the file and move it
                string fileOld = Path.Combine(Runtime.ExecutableDir, "Lib", "ZstdDictionaries", path);
                string fileNew = Path.Combine(folder, path);
                if (!File.Exists(fileNew) && File.Exists(fileOld))
                {
                    File.Move(fileOld, fileNew);
                }
            }
            TransferZDic("bcett.byml.zsdic");
            TransferZDic("pack.zsdic");
            TransferZDic("zs.zsdic");

            if (Directory.Exists(folder))
            {
                void CheckZDic(string fileName, string expectedExtension)
                {
                    //Dictionary already set
                    if (dictionary.Length != 0) return;

                    string zDictPath = Path.Combine(folder, fileName);
                    //Then check if the input file uses the expected extension
                    if (File.Exists(zDictPath) && fileNameTemp.EndsWith(expectedExtension))
                        dictionary = File.ReadAllBytes(zDictPath);
                }

                //Order matters, zs must go last
                CheckZDic("bcett.byml.zsdic", "bcett.byml.zs");
                CheckZDic("pack.zsdic", "pack.zs");
                CheckZDic("zs.zsdic", ".zs");
            }
            return dictionary;
        }
    }
}
