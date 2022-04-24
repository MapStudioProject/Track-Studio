using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using BfresLibrary;
using Toolbox.Core.IO;
using MapStudio.UI;
using Syroot.NintenTools.NSW.Bntx;

namespace CafeLibrary
{
    public class MaterialPresetDumper
    {
        public static void ExportMaterials(string gamePath)
        {
            if (!Directory.Exists(gamePath))
                return;

            if (!Directory.Exists($"{Toolbox.Core.Runtime.ExecutableDir}\\Presets\\Materials\\"))
                Directory.CreateDirectory($"{Toolbox.Core.Runtime.ExecutableDir}\\Presets\\Materials\\");


            //Just check for a mk8d specific asset
            bool isMK8D = File.Exists($"{gamePath}\\RaceCommon\\TS_PolicePackun\\TS_PolicePackun.bfres");

            string dir = $"{Toolbox.Core.Runtime.ExecutableDir}\\Presets\\Materials";
            string game_folder = gamePath;
            dir = isMK8D ? $"{dir}\\MK8D" : $"{dir}\\MK8U";

            if (Directory.Exists(dir))
                return;

            Directory.CreateDirectory(dir);

            ProcessLoading.Instance.IsLoading = true;
            ProcessLoading.Instance.Update(10, 100, "Dumping game material presets!");

            Dictionary<string, List<MaterialEntry>> presets = new Dictionary<string, List<MaterialEntry>>();

            //Parse a csv containing material dump info
            using (var textReader = new StreamReader($"{Toolbox.Core.Runtime.ExecutableDir}\\Lib\\Presets\\TurboMaterialDumper.csv"))
            {
                //Header
                textReader.ReadLine();
                while (!textReader.EndOfStream)
                {
                    string line = textReader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    var args = line.Split(",");
                    if (args.Length < 4)
                        continue;

                    //File to get materials
                    string fileName = args[0].Trim();
                    //The material to dump
                    string materialName = args[1].Trim();
                    //Path to save the material
                    string presetSavePath = args[2].Trim();
                    //Save with textures or not
                    bool keepTextures = args[3].Trim() == "true";

                    var entry = new MaterialEntry(materialName, $"{dir}\\{presetSavePath}", keepTextures);
                    if (!presets.ContainsKey(fileName))
                        presets.Add(fileName, new List<MaterialEntry>());

                    presets[fileName].Add(entry);
                }
            }

            //Dump each preset into the preset folder
            foreach (var preset in presets)
                DumpBfresMaterials($"{game_folder}\\{preset.Key}", preset.Value.ToArray());

            ProcessLoading.Instance.Update(100, 100, "Dumping game material presets!");
            ProcessLoading.Instance.IsLoading = false;
        }

        private static void DumpAllMaterials(string dir, string filePath)
        {
            if (!File.Exists(filePath))
                return;

            ResFile resFile = new ResFile(new MemoryStream(YAZ0.Decompress(filePath)));
            foreach (var model in resFile.Models.Values)
            {
                foreach (var mat in model.Materials.Values)
                {
                    var preset = new MaterialEntry(mat.Name, $"{dir}\\{mat.Name}.zip");
                    SaveAsPreset(preset.FilePath, mat, resFile, preset.ExportTextures, preset.ExportAnimations);
                }
            }
        }

        /// <summary>
        /// Method for dumping preset materials from a material entry.
        /// </summary>
        public static void DumpBfresMaterials(string filePath, MaterialEntry[] materialsToExport)
        {
            if (!File.Exists(filePath))
                return;

            Console.WriteLine($"Dumping {filePath} materials.");

            ResFile resFile = new ResFile(new MemoryStream(YAZ0.Decompress(filePath)));
            foreach (var model in resFile.Models.Values) {
                foreach (var mat in materialsToExport)
                {
                    if (model.Materials.ContainsKey(mat.Name) && !File.Exists(mat.FilePath))
                        SaveAsPreset(mat.FilePath, model.Materials[mat.Name], resFile, mat.ExportTextures, mat.ExportAnimations);
                }
            }
        }

        public class MaterialEntry
        {
            public bool ExportTextures = false;
            public bool ExportAnimations = true;
            public string Name;
            public string FilePath;

            public MaterialEntry(string name, string filePath, bool exportTextures = false, bool exportAnims = true) {
                Name = name;
                FilePath = filePath;
                ExportTextures = exportTextures;
                ExportAnimations = exportAnims;
            }
        }

        /// <summary>
        /// Saves a material preset to the preset folder.
        /// </summary>
        public static void SaveAsPreset(string filePath, Material material, ResFile resFile, bool exportTextures = true, bool exportAnims = true)
        {
            //Export the material as a preset for reusing with other materials
            Console.WriteLine($"Dumping preset {filePath}.");

            //Don't want the shader archive name to change unless the preset is loaded
            string archiveName = material.ShaderAssign.ShaderArchiveName;
            string presetName = Path.GetFileNameWithoutExtension(filePath);
            string dir = Path.GetDirectoryName(filePath);
            string presetFolder = $"{dir}\\{presetName}";
            string ShaderArchive = material.ShaderAssign.ShaderArchiveName;

            if (!Directory.Exists(presetFolder))
                Directory.CreateDirectory(presetFolder);

            //Check for embedded shaders
            foreach (var file in resFile.ExternalFiles)
            {
                if (file.Key == $"{ShaderArchive}.bfsha")
                {
                    //Export the shader with a unique name
                    //This will allow usage with multiple bfsha
                    string name = GetShaderName(file.Value.Data, ShaderArchive);
                    //Assign the new shader archive name
                    material.ShaderAssign.ShaderArchiveName = name;
                    //Export the shader with a new internal name
                    SaveShaderPreset(resFile.IsPlatformSwitch, $"{presetFolder}\\{name}.bfsha", file.Value.Data, name);
                }
            }
            if (exportTextures)
            {
                var textures = resFile.Textures;
                var externalFile = resFile.ExternalFiles.Values.FirstOrDefault(x => x.LoadedFileData is BntxFile);
                for (int i = 0; i < material.TextureRefs.Count; i++)
                {
                    string name = material.TextureRefs[i].Name;
                    //Skip bake maps as they generally aren't needed due to requiring mesh specific UV layers
                    if (material.Samplers[i].Name == "_b0" || material.Samplers[i].Name == "_b1")
                        continue;

                    if (externalFile != null)
                    {
                        var bntx = externalFile.LoadedFileData as BntxFile;
                        var tex = bntx.Textures.FirstOrDefault(x => x.Name == name);
                        if (tex != null)
                            tex.Export($"{presetFolder}\\{name}.bftex", bntx);
                    }
                    else if (textures.ContainsKey(name))
                        textures[name].Export($"{presetFolder}\\{name}.bftex", resFile);
                }
            }
            if (exportAnims)
            {
                void ExportMaterialAnim(MaterialAnim anim, string ext)
                {
                    if (anim.MaterialAnimDataList.Any(x => x.Name == material.Name))
                        anim.Export($"{presetFolder}\\{anim.Name}{ext}", resFile);
                }
                foreach (var anim in resFile.ShaderParamAnims.Values)
                    ExportMaterialAnim(anim, ".bfsp");
                foreach (var anim in resFile.TexPatternAnims.Values)
                    ExportMaterialAnim(anim, ".bftxp");
                foreach (var anim in resFile.TexSrtAnims.Values)
                    ExportMaterialAnim(anim, ".bfsrt");
                foreach (var anim in resFile.ColorAnims.Values)
                    ExportMaterialAnim(anim, ".bfclr");
            }
            material.Export($"{presetFolder}\\{presetName}.json", resFile);

            //Remove previous preset
            if (File.Exists($"{dir}\\{presetName}.zip"))
                File.Delete($"{dir}\\{presetName}.zip");

            //Package preset
            ZipFile.CreateFromDirectory(presetFolder, $"{dir}\\{presetName}.zip");
            //Remove directory
            foreach (var file in Directory.GetFiles(presetFolder))
                File.Delete(file);

            Directory.Delete(presetFolder);

            //reset shader archive name back
            material.ShaderAssign.ShaderArchiveName = archiveName;
        }

        //Gets a unique shader name based on the file data contents
        private static string GetShaderName(byte[] data, string internalName)
        {
            string hash = Toolbox.Core.Hashes.Cryptography.Crc32.Compute(data).ToString("X");
            //Limit to the same size of the string as these are hex edited internally
            int max_length = internalName.Length;
            return $"{hash}".PadLeft(max_length, '_');
        }

        //Saves shader to disc with a custom internal name
        private static void SaveShaderPreset(bool isSwitch, string filePath, byte[] data, string internalName)
        {
            //preset already exists
            if (File.Exists(filePath))
                return;

            var mem = new MemoryStream();
            using (var reader = new Toolbox.Core.IO.FileReader(data))
            using (var writer = new Toolbox.Core.IO.FileWriter(mem))
            {
                writer.Write(data);

                //Name offset
                if (isSwitch)
                {
                    reader.SetByteOrder(false);
                    reader.SeekBegin(16);
                    uint nameOffset = reader.ReadUInt32();

                    //Write the name from the name offset
                    writer.SeekBegin(nameOffset);
                    writer.WriteString(internalName);
                }
                else
                {
                    reader.SetByteOrder(true);
                    reader.SeekBegin(20);
                    uint nameOffset = reader.ReadUInt32();

                    //Write the name from the name offset
                    writer.SeekBegin(nameOffset + 20);
                    writer.WriteString(internalName);
                }
            }
            File.WriteAllBytes(filePath, mem.ToArray());
        }
    }
}
