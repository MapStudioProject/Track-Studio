using MapStudio.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// General storage for MapObj descriptions, parameter usage, parameter descriptions, parameter defaults, etc.
    /// </summary>
    public sealed class ParamDataBaseSingleton
    {
        public string GetUserArchivesPath() => System.IO.Path.Combine(Runtime.ExecutableDir, "User", "MapObjArchives");
        private readonly string ArchiveExt = "*.json";

        private Dictionary<int, MapObjMeta> MetaDB = new Dictionary<int, MapObjMeta>();

        public static ParamDataBaseSingleton Instance { get; } = new ParamDataBaseSingleton();

        private ParamDataBaseSingleton()
        {
            // Load vanilla archives first
            string archiveU_base = System.IO.Path.Combine(Runtime.ExecutableDir, "Resources", "MapObj_U_Base.json");
            string archiveU_DLC = System.IO.Path.Combine(Runtime.ExecutableDir, "Resources", "MapObj_U_DLC.json");
            string archiveDX_base = System.IO.Path.Combine(Runtime.ExecutableDir, "Resources", "MapObj_DX_Base.json");
            string archiveDX_BCP = System.IO.Path.Combine(Runtime.ExecutableDir, "Resources", "MapObj_DX_BCP.json");

            if (File.Exists(archiveU_base))
                LoadArchiveFromFile(MetaDB, archiveU_base);
            if (File.Exists(archiveU_DLC))
                LoadArchiveFromFile(MetaDB, archiveU_DLC);
            if (File.Exists(archiveDX_base))
                LoadArchiveFromFile(MetaDB, archiveDX_base);
            if (File.Exists(archiveDX_BCP))
                LoadArchiveFromFile(MetaDB, archiveDX_BCP);

            // Load any use archives, if present
            LoadArchivesFromDirectory(MetaDB, GetUserArchivesPath(), ArchiveExt);

            //Console.WriteLine("Master MetaDB:\n=======================");
            //foreach (KeyValuePair<int, MapObjMeta> obj in MetaDB)
            //{
            //    Console.WriteLine($"MapObjMeta ({obj.Key}):");
            //    obj.Value.WriteDebugLog();
            //    Console.WriteLine("------");
            //}
        }

        public MapObjMeta GetMeta(int objId)
        {
            if (!MetaDB.ContainsKey(objId)) {
                // Attempted to retrieve meta info for an object not in the database. This can happen if
                //  Track studio's internal files run behind a game release with new mapObjs (unlikely).
                //  Furthermore, a <c>objflow.byaml</c> might have been modified with new mapObjs.
                //  We explicitly mention this was added later and not documented properly.
                Console.Error.WriteLine($"Encountered unknown mapObj id: {objId}. Added it to the paramDB.");
                MetaDB[objId] = new MapObjMeta(false);
            }
            return MetaDB[objId];
        }

        /// <summary>
        /// Adds new meta information to a meta database, merging pre-existing data if needed.
        /// </summary>
        /// <param name="objId">Object ID</param>
        /// <param name="newMeta">New meta info to store</param>
        /// <param name="metaArchive">Pre-existing archive</param>
        private void AddMetaToArchive(int objId, MapObjMeta newMeta, Dictionary<int, MapObjMeta> metaArchive)
        {
            if (!metaArchive.ContainsKey(objId))
            {
                // Attempted to retrieve meta info for an object not in the database, add it!
                metaArchive[objId] = new MapObjMeta(true);
            }
            MapObjMeta curMeta = metaArchive[objId];
            curMeta.Merge(newMeta);
        }

        /// <summary>
        /// Populates a metaDB based on all files in a directory.
        /// </summary>
        /// <param name="metaDB">Meta database to populate</param>
        /// <param name="path">Directory path</param>
        /// <param name="archiveExt">File extension</param>
        private void LoadArchivesFromDirectory(Dictionary<int, MapObjMeta> metaDB, string path, string archiveExt)
        {
            string[] files = [];
            Console.WriteLine($"Loading MapObj archives ({archiveExt}) from {path}");
            try
            {
                files = Directory.GetFiles(path, archiveExt);
                Array.Sort(files);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Could not open directory: {path}\n{e.GetType()}: {e.Message}");
                DialogHandler.ShowException(e);
            }
            Console.WriteLine($"Found {files.Length} MapObj archives!");
            foreach (var file in files)
            {
                LoadArchiveFromFile(metaDB, file, true);
            }
        }

        /// <summary>
        /// Populates a metaDB based on a single file
        /// </summary>
        /// <param name="metaDB">Meta database to populate</param>
        /// <param name="path">Filepath</param>
        private void LoadArchiveFromFile(Dictionary<int, MapObjMeta> metaDB, string path, bool writeToConsole = false)
        {
            Console.WriteLine($"Reading MapObj archive {path}...");
            var content = File.ReadAllText(path);
            try
            {
                Dictionary<int, MapObjMeta> objects = JsonConvert.DeserializeObject<Dictionary<int, MapObjMeta>>(content);
                foreach (KeyValuePair<int, MapObjMeta> obj in objects)
                {
                    AddMetaToArchive(obj.Key, obj.Value, metaDB);
                    //Console.WriteLine($"MapObjMeta ({obj.Key}):");
                    //obj.Value.WriteDebugLog();
                    //Console.WriteLine("------");
                }
                if (writeToConsole)
                    StudioLogger.WriteLine($"Succesfully loaded user meta <{System.IO.Path.GetFileName(path)}>");                
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                if (writeToConsole)
                    StudioLogger.WriteError($"Error loading user meta <{System.IO.Path.GetFileName(path)}>");
                DialogHandler.ShowException(new Exception($"Error loading MapObj meta in {path}.\n{e.Message}\n", e));
            }
        }
    }
}
