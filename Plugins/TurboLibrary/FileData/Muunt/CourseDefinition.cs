using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using ByamlExt.Byaml;
using Toolbox.Core;

namespace TurboLibrary
{
    public class CourseDefinition : ByamlData, IByamlSerializable
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        public bool IsSwitch
        {
            get
            {
                return BymlData.byteOrder == Syroot.BinaryData.ByteOrder.LittleEndian;
            }
            set
            {
                if (value)
                    BymlData.byteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
                else 
                    BymlData.byteOrder = Syroot.BinaryData.ByteOrder.BigEndian;
            }
        }

        //For saving back in the same place
        private string originalPath;

        private BymlFileData BymlData;

        public CourseDefinition()
        {
            EffectSW = 0;
            FirstCurve = "left";
            IsFirstLeft = true;
            HeadLight = CourseHeadLight.Off;
            IsJugemAbove = false;
            LapJugemPos = 0;
            LapCount = 3;
            ObjParams = new List<int>();
            for (int i = 0; i < 8; i++)
                ObjParams.Add(0);
            PatternCount = 0;

            BymlData = new BymlFileData()
            {
                byteOrder = Syroot.BinaryData.ByteOrder.BigEndian,
                SupportPaths = true,
                Version = 1,
            };
        }

        public void SetAsSwitchByaml() {
            BymlData.byteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
        }

        public void SetAsWiiUByaml() {
            BymlData.byteOrder = Syroot.BinaryData.ByteOrder.BigEndian;
        }

        public CourseDefinition(string fileName)
        {
            originalPath = fileName;
            Load(System.IO.File.OpenRead(fileName));
        }

        public CourseDefinition(System.IO.Stream stream) {
            Load(stream);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseDefinition"/> class from the given stream.
        /// </summary>
        /// <param name="stream">The stream from which the instance will be loaded.</param>
        private void Load(System.IO.Stream stream) {

            BymlData = ByamlFile.LoadN(stream, true);
         //   Console.WriteLine($"Loaded byaml! {fileName}");
            ByamlSerialize.Deserialize(this, BymlData.RootNode);
         //   Console.WriteLine("Deserialized byaml!");

            // After loading all the instances, allow references to be resolved.
            Areas?.ForEach(x => x.DeserializeReferences(this));
            Clips?.ForEach(x => x.DeserializeReferences(this));
            ClipPattern?.AreaFlag?.ForEach(x => x.DeserializeReferences(this));
            EnemyPaths?.ForEach(x => x.DeserializeReferences(this));
            GCameraPaths?.ForEach(x => x.DeserializeReferences(this));
            GlidePaths?.ForEach(x => x.DeserializeReferences(this));
            GravityPaths?.ForEach(x => x.DeserializeReferences(this));
            ItemPaths?.ForEach(x => x.DeserializeReferences(this));
            JugemPaths?.ForEach(x => x.DeserializeReferences(this));
            LapPaths?.ForEach(x => x.DeserializeReferences(this));
            ObjPaths?.ForEach(x => x.DeserializeReferences(this));
            Paths?.ForEach(x => x.DeserializeReferences(this));
            PullPaths?.ForEach(x => x.DeserializeReferences(this));
            Objs?.ForEach(x => x.DeserializeReferences(this));
            ReplayCameras?.ForEach(x => x.DeserializeReferences(this));
            IntroCameras?.ForEach(x => x.DeserializeReferences(this));
            SteerAssistPaths?.ForEach(x => x.DeserializeReferences(this));
            RouteChanges?.ForEach(x => x.DeserializeReferences(this));

            //Convert baked in tool obj paths to editable rail paths
            if (ObjPaths != null)
            {
                List<ObjPath> converted = ObjPaths.Where(x => x.BakedRailPath == true).ToList();
                if (converted.Count > 0 && Paths == null)
                    Paths = new List<Path>();

                foreach (var objPath in converted) {
                    var path = Path.ConvertFromObjPath(objPath);
                    Paths.Add(path);
                    ObjPaths.Remove(objPath);

                    //Link the obj path
                    foreach (var obj in Objs)
                    {
                        if (obj.ObjPath == objPath)
                        {
                            int ptIndex = -1;
                            if (obj.ObjPathPoint != null)
                                ptIndex = obj.ObjPath.Points.IndexOf(obj.ObjPathPoint);

                            obj.ObjPath = null;
                            obj.ObjPathPoint = null;
                            obj.Path = path;
                            if (ptIndex != -1 && ptIndex < path.Points.Count)
                                obj.PathPoint = path.Points[ptIndex];
                        }
                    }
                }
             }
        }

        public void Save() { this.Save(originalPath); }

        public void Save(string fileName)
        {
            using (var stream = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                Save(stream);
        }

        public void Save(System.IO.Stream stream) {
            SaveMapObjList();

            //Convert editable rail paths to obj paths.
            List<Path> converted = Paths?.Where(x => x.UseAsObjPath == true).ToList();
            if (converted != null) {
                foreach (var path in converted) {
                    var objPath = ObjPath.ConvertFromPath(path);
                    ObjPaths.Add(objPath);

                    //Link the obj path
                    foreach (var obj in Objs)
                    {
                        if (obj.Path == path)
                        {
                            int ptIndex = -1;
                            if (obj.PathPoint != null)
                                ptIndex = obj.Path.Points.IndexOf(obj.PathPoint);

                            obj.ObjPath = objPath;
                            if (ptIndex != -1 && ptIndex < objPath.Points.Count)
                                obj.ObjPathPoint = objPath.Points[ptIndex];
                        }
                    }
                    Paths.Remove(path);
                }
            }

            if (ClipPattern != null && ClipPattern.AreaFlag?.Count > 0)
            {
                Clips = new List<Clip>();
                foreach (var clip in ClipPattern.AreaFlag)
                    Clips.Add(clip);
            }

            // Before saving all the instances, allow references to be resolved.
            Areas?.ForEach(x => x.SerializeReferences(this));
            Clips?.ForEach(x => x.SerializeReferences(this));
            ClipPattern?.AreaFlag?.ForEach(x => x.SerializeReferences(this));
            EnemyPaths?.ForEach(x => x.SerializeReferences(this));
            GCameraPaths?.ForEach(x => x.SerializeReferences(this));
            GlidePaths?.ForEach(x => x.SerializeReferences(this));
            GravityPaths?.ForEach(x => x.SerializeReferences(this));
            ItemPaths?.ForEach(x => x.SerializeReferences(this));
            JugemPaths?.ForEach(x => x.SerializeReferences(this));
            LapPaths?.ForEach(x => x.SerializeReferences(this));
            PullPaths?.ForEach(x => x.SerializeReferences(this));
            Objs?.ForEach(x => x.SerializeReferences(this));
            ReplayCameras?.ForEach(x => x.SerializeReferences(this));
            IntroCameras?.ForEach(x => x.SerializeReferences(this));
            Paths?.ForEach(x => x.SerializeReferences(this));
            SteerAssistPaths?.ForEach(x => x.SerializeReferences(this));
            RouteChanges?.ForEach(x => x.SerializeReferences(this));

            BymlData.RootNode = ByamlSerialize.Serialize(this);
            ByamlFile.SaveN(stream, BymlData);

            //Re add converted obj paths back to rails for editors
            foreach (var path in converted)
            {
                if (!Paths.Contains(path))
                    Paths.Add(path);
            }
        }

        private void SaveMapObjList()
        {
            if (Objs == null) {
                this.MapObjResList = new List<string>();
                this.MapObjIdList = new List<int>();
                return;
            }

            //Order the obj list by ID from highest to smallest
            //This is very important for certain objs (like water boxes)
            Objs = Objs.OrderByDescending(x => x.ObjId).ToList();

            List<string> resNameList = new List<string>();
            List<int> resIDList = new List<int>();
            foreach (var ob in Objs)
            {
                if (GlobalSettings.ObjDatabase.ContainsKey(ob.ObjId)) {
                    List<string> names = GlobalSettings.ObjDatabase[ob.ObjId].ResNames;
                    for (int j = 0; j < names.Count; j++)
                    {
                        if (!resNameList.Contains(names[j]))
                            resNameList.Add(names[j]);
                    }
                }
                else if (GlobalSettings.ObjectList.ContainsKey(ob.ObjId))
                {
                    string name = GlobalSettings.ObjectList[ob.ObjId];
                    if (!resNameList.Contains(name))
                        resNameList.Add(name);
                }

                if (!resIDList.Contains(ob.ObjId))
                    resIDList.Add(ob.ObjId);
            }

            if (resNameList.Contains("Dokan1"))
            {
                resIDList.Add(1019); //Dokan1
            }

            if (resNameList.Contains("CmnGroupToad") ||
                resNameList.Contains("N64RTrain"))
            {
                resIDList.Add(1044); //KaraPillarBase
                resNameList.Add("CmnToad");
            }

            if (resNameList.Contains("KaraPillar"))
                resIDList.Add(9006); //KaraPillarBase
            if (resNameList.Contains("ItemBox"))
                resIDList.Add(9007); //ItemBoxFont
            
            this.MapObjResList = resNameList.Reverse<string>().Distinct().ToList();
            this.MapObjIdList = resIDList.OrderByDescending(x => x).Distinct().ToList();
        }


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a globally applied effect index.
        /// </summary>
        [ByamlMember(Optional = true)]
        [BindGUI("Global Effect ID", Category = "Track Settings")]
        public EffectArea.EffectSWFlags? EffectSW { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how headlights are turned on or off on this course.
        /// </summary>
        [ByamlMember(Optional = true)]
        [BindGUI("Head Light", Category = "Track Settings")]
        public CourseHeadLight? HeadLight { get; set; }

        /// <summary>
        /// Gets or sets a value which indicates whether the first curve is a left turn. Has to be in sync with
        /// <see cref="FirstCurve"/>.
        /// </summary>
        [ByamlMember(Optional = true)]
        [BindGUI("IsFirstLeft", Category = "Track Settings", 
            ToolTip = "Sets the start position as left changing the starting gates and latiku position.")]
        public bool? IsFirstLeft { get; set; }

        /// <summary>
        /// Gets or sets a value which indicates whether the first curve is a &quot;left&quot; or &quot;right&quot;
        /// turn. Has to be in sync with <see cref="IsFirstLeft"/>.
        /// </summary>
        [ByamlMember(Optional = true)]
        public string FirstCurve { get; set; }

        /// <summary>
        /// Gets or sets an unknown value.
        /// </summary>
        [ByamlMember(Optional = true)]
        [BindGUI("IsJugemAbove", Category = "Track Settings")]
        public bool? IsJugemAbove { get; set; }

        /// <summary>
        /// Gets or sets an unknown value.
        /// </summary>
        [ByamlMember(Optional = true)]
        [BindGUI("JugemAbove", Category = "Track Settings")]
        public int? JugemAbove { get; set; }

        /// <summary>
        /// Gets or sets an unknown value.
        /// </summary>
        [ByamlMember(Optional = true)]
        [BindGUI("LapJugemPos", Category = "Track Settings")]
        public int? LapJugemPos { get; set; }

        /// <summary>
        /// Gets or sets the number of laps which have to be driven to finish the track.
        /// </summary>
        [ByamlMember("LapNumber", Optional = true)]
        [BindGUI("Lap Count", Category = "Track Settings")]
        public int? LapCount { get; set; }

        /// <summary>
        /// Gets or sets a list of Obj parameters which are applied globally.
        /// </summary>
        public List<int> ObjParams { get; set; }

        /// <summary>
        /// Gets or sets the number of pattern sets out of which one is picked randomly at start.
        /// </summary>
        [ByamlMember("PatternNum", Optional = true)]
        [BindGUI("Pattern Count", Category = "Track Settings")]
        public int? PatternCount { get; set; }

        [BindGUI("Param 1", Category = "Params", ColumnIndex = 0)]
        public int Param1
        {
            get { return GetParam(0); }
            set { SetParam(0, value); }
        }

        [BindGUI("Param 2", Category = "Params", ColumnIndex = 1)]
        public int Param2
        {
            get { return GetParam(1); }
            set { SetParam(1, value); }
        }

        [BindGUI("Param 3", Category = "Params", ColumnIndex = 0)]
        public int Param3
        {
            get { return GetParam(2); }
            set { SetParam(2, value); }
        }

        [BindGUI("Param 4", Category = "Params", ColumnIndex = 1)]
        public int Param4
        {
            get { return GetParam(3); }
            set { SetParam(3, value); }
        }

        [BindGUI("Param 5", Category = "Params", ColumnIndex = 0)]
        public int Param5
        {
            get { return GetParam(4); }
            set { SetParam(4, value); }
        }

        [BindGUI("Param 6", Category = "Params", ColumnIndex = 1)]
        public int Param6
        {
            get { return GetParam(5); }
            set { SetParam(5, value); }
        }

        [BindGUI("Param 7", Category = "Params", ColumnIndex = 0)]
        public int Param7
        {
            get { return GetParam(6); }
            set { SetParam(6, value); }
        }

        [BindGUI("Param 8", Category = "Params", ColumnIndex = 1)]
        public int Param8
        {
            get { return GetParam(7); }
            set { SetParam(7, value); }
        }

        private int GetParam(int index) {
            return ObjParams.Count > index ? ObjParams[index] : 0;
        }

        private void SetParam(int index, int value) {
            if (ObjParams.Count <= index)
                ObjParams.Add(value);
            else
                ObjParams[index] = value;
        }

        // ---- Areas ----

        /// <summary>
        /// Gets or sets the list of <see cref="Area"/> instances.
        /// </summary>
        [ByamlMember("Area", Optional = true)]
        public List<Area> Areas { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="EffectArea"/> instances.
        /// </summary>
        [ByamlMember("EffectArea", Optional = true)]
        public List<EffectArea> EffectAreas { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Area"/> instances.
        /// </summary>
        [ByamlMember("CeilingArea", Optional = true)]
        public List<CeilingArea> CeilingAreas { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Area"/> instances.
        /// </summary>
        [ByamlMember("CurrentArea", Optional = true)]
        public List<CurrentArea> CurrentAreas { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Area"/> instances.
        /// </summary>
        [ByamlMember("PrisonArea", Optional = true)]
        public List<PrisonArea> PrisonAreas { get; set; }

        // ---- Clipping ----

        /// <summary>
        /// Gets or sets the list of <see cref="Clip"/> instances.
        /// </summary>
        [ByamlMember("Clip", Optional = true)]
        public List<Clip> Clips { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="ClipArea"/> instances.
        /// </summary>
        [ByamlMember("ClipArea", Optional = true)]
        public List<ClipArea> ClipAreas { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ClipPattern"/> instance.
        /// </summary>
        [ByamlMember("ClipPattern", Optional = true)]
        public ClipPattern ClipPattern { get; set; }

        // ---- Paths ----

        /// <summary>
        /// Gets or sets the list of <see cref="Path"/> instances.
        /// </summary>
        [ByamlMember("Path", Optional = true)]
        public List<Path> Paths { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="EnemyPath"/> instances.
        /// </summary>
        [ByamlMember("EnemyPath", Optional = true)]
        public List<EnemyPath> EnemyPaths { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="GCameraPath"/> instances.
        /// </summary>
        [ByamlMember("GCameraPath", Optional = true)]
        public List<GCameraPath> GCameraPaths { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="GlidePath"/> instances.
        /// </summary>
        [ByamlMember("GlidePath", Optional = true)]
        public List<GlidePath> GlidePaths { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="GravityPath"/> instances.
        /// </summary>
        [ByamlMember("GravityPath", Optional = true)]
        public List<GravityPath> GravityPaths { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="ItemPath"/> instances.
        /// </summary>
        [ByamlMember("ItemPath", Optional = true)]
        public List<ItemPath> ItemPaths { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="JugemPath"/> instances.
        /// </summary>
        [ByamlMember("JugemPath", Optional = true)]
        public List<JugemPath> JugemPaths { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="LapPath"/> instances.
        /// </summary>
        [ByamlMember("LapPath", Optional = true)]
        public List<LapPath> LapPaths { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="ObjPath"/> instances.
        /// </summary>
        [ByamlMember("ObjPath", Optional = true)]
        public List<ObjPath> ObjPaths { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="PullPath"/> instances.
        /// </summary>
        [ByamlMember("PullPath", Optional = true)]
        public List<PullPath> PullPaths { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="SteerAssistPath"/> instances.
        /// </summary>
        [ByamlMember("SteerAssistPath", Optional = true)]
        public List<SteerAssistPath> SteerAssistPaths { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="KillerPath"/> instances.
        /// </summary>
        [ByamlMember("KillerPath", Optional = true)]
        public List<KillerPath> KillerPaths { get; set; }

        // ---- Objects ----

        /// <summary>
        /// Gets or sets the list of ObjId's of objects to load for the track.
        /// </summary>
        [ByamlMember]
        public List<int> MapObjIdList { get; set; }

        /// <summary>
        /// Gets or sets the list of ResName's of objects to load for the track.
        /// </summary>
        [ByamlMember]
        public List<string> MapObjResList { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Obj"/> instances.
        /// </summary>
        [ByamlMember("Obj")]
        public List<Obj> Objs { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="SoundObj"/> instances.
        /// </summary>
        [ByamlMember("SoundObj", Optional = true)]
        public List<SoundObj> SoundObjs { get; set; }

        // ---- Cameras ----

        /// <summary>
        /// Gets or sets the list of <see cref="IntroCamera"/> instances.
        /// </summary>
        [ByamlMember("IntroCamera", Optional = true)]
        public List<IntroCamera> IntroCameras { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="ReplayCamera"/> instances.
        /// </summary>
        [ByamlMember("ReplayCamera", Optional = true)]
        public List<ReplayCamera> ReplayCameras { get; set; }

        [ByamlMember("RouteChange", Optional = true)]
        public List<RouteChange> RouteChanges { get; set; }

        public enum CourseHeadLight
        {
            Off = 0,
            On = 1,
            ByLapPath = 2,
        }

        public void DeserializeByaml(IDictionary<string, object> dictionary)
        {
            // ObjParams
            ObjParams = new List<int>();
            for (int i = 1; i <= 8; i++)
            {
                if (dictionary.TryGetValue("OBJPrm" + i.ToString(), out object objParam))
                {
                    ObjParams.Add((int)objParam);
                }
            }
        }

        public void SerializeByaml(IDictionary<string, object> dictionary)
        {
            if (HeadLight == CourseHeadLight.Off) HeadLight = null;

            // ObjParams
            for (int i = 1; i <= ObjParams.Count; i++)
            {
                dictionary["OBJPrm" + i.ToString()] = ObjParams[i - 1];
            }

            if (IsFirstLeft == true)
                FirstCurve = "left";
            else
                FirstCurve = "right";
        }
    }
}
