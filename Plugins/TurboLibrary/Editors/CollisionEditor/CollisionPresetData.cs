using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TurboLibrary.CollisionEditor
{
    public class CollisionPresetData
    {
        public static List<CollisionPresetData> CollisionPresets = new List<CollisionPresetData>();

        public string GameTitle;

        public string Platform;

        public string Comments;

        public float PrismThickness = 30f;
        public float SphereRadius = 25f;

        public float PaddingMin = -50f;
        public float PaddingMax = 50f;
        public int MaxRootSize = 2048;
        public int MinRootSize = 128;
        public int MinCubeSize = 32;
        public int MaxTrianglesInCube = 10;

        public Dictionary<ushort, string> MaterialPresets = new Dictionary<ushort, string>();

        public ushort GetMaterialID(string type)
        {
            return MaterialPresets.FirstOrDefault(x => x.Value == type).Key;
        }

        public static void SavePresets(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            for (int i = 0; i < CollisionPresets.Count; i++)
            {
                string json = JsonConvert.SerializeObject(CollisionPresets[i], Formatting.Indented);
                File.WriteAllText($"{folderPath}/{CollisionPresets[i].GameTitle}.json", json);
            }
        }

        public static void LoadPresets(string[] filePaths)
        {
            CollisionPresets.Clear();
            for (int i = 0; i < filePaths.Length; i++)
            {
                CollisionPresets.Add(JsonConvert.DeserializeObject<CollisionPresetData>(
                    File.ReadAllText(filePaths[i])));
            }
        }
    }

    public class CollisionEntry
    {
        public string Name { get; set; }
        public string Type
        {
            get { return TypeID.ToString(); }
        }

        public ushort TypeID { get; set; }

        public CollisionEntry(string name)
        {
            Name = name;

            if (name.StartsWith("COL_"))
            {
                string attribute = name.Replace("COL_", string.Empty);

                ushort value = 0;
                ushort.TryParse(attribute, System.Globalization.NumberStyles.HexNumber, null, out value);
                TypeID = value;
            }
        }
    }
}
