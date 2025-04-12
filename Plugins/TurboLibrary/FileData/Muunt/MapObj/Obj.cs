using System.Collections.Generic;
using System.Linq;
using System.IO;
using Toolbox.Core;
using System.Reflection;

namespace TurboLibrary
{
    /// <summary>
    /// Represents an Obj placed on the course.
    /// </summary>
    public class Obj : SpatialObject, IByamlSerializable, ICourseReferencable
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        // References to paths and their points.
        [ByamlMember("Obj_Path", Optional = true)]
        private int? _pathIndex;
        [ByamlMember("Obj_PathPoint", Optional = true)]
        private int? _pathPointIndex;
        [ByamlMember("Obj_LapPath", Optional = true)]
        private int? _lapPathIndex;
        [ByamlMember("Obj_LapPoint", Optional = true)]
        private int? _lapPathPointIndex;
        [ByamlMember("Obj_ObjPath", Optional = true)]
        private int? _objPathIndex;
        [ByamlMember("Obj_ObjPoint", Optional = true)]
        private int? _objPathPointIndex;
        [ByamlMember("Obj_EnemyPath1", Optional = true)]
        private int? _enemyPath1Index;
        [ByamlMember("Obj_EnemyPath2", Optional = true)]
        private int? _enemyPath2Index;
        [ByamlMember("Obj_ItemPath1", Optional = true)]
        private int? _itemPath1Index;
        [ByamlMember("Obj_ItemPath2", Optional = true)]
        private int? _itemPath2Index;
        [ByamlMember("Obj_TargetObj", Optional = true)]
        private List<int>? _targetObjIndex;
        

        // References to other unit objects.
        [ByamlMember("Area_Obj", Optional = true)]
        private int? _areaIndex;
        [ByamlMember("Obj_Obj", Optional = true)]
        private int? _objIndex;

        public Dictionary<string, string> GetPathProperties()
        {
            return new Dictionary<string, string?>()
            {
                { "_pathIndex" , "Path" }, {  "_pathPointIndex","Path Point" },
                { "_lapPathIndex", "Lap Path"}, {  "_lapPathPointIndex","Lap Path Point" },
                {  "_objPathIndex", "Obj Path" }, { "_objPathPointIndex", "Obj Path Point"},
                { "_enemyPath1Index", "Enemy Path 1" }, {  "_enemyPath2Index", "Enemy Path 2" },
                {  "_itemPath1Index", "Item Path 1" }, { "_itemPath2Index", "Item Path 2"},
            };
        }

        public Dictionary<string, string> GetRelationProperties()
        {
            return new Dictionary<string, string>()
            {
                { "_areaIndex", "Area" }, {  "_objIndex","Object" },
            };
        }

        public int? GetLinkIndex(string name, bool returnDefault = false)
        {
            var input = typeof(Obj).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            var value = (int?)input.GetValue(this);
            if (value.HasValue)
                return value.Value;
            else if (returnDefault)
                return 0;
            else
                return null;
        }

        public void SetLinkIndex(string name, int index)
        {
            var input = typeof(Obj).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            input.SetValue(this, index == -1 ? null : index);
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the ID determining the type of this object.
        /// </summary>
        [ByamlMember]
        public int ObjId { get; set; }

        [BindGUI("MULTI2P", Category = "OBJECT", ColumnIndex = 0, Control = BindControl.ToggleButton)]
        public bool HasMulti2P
        {
            get { return ModeInclusion.HasFlag(ModeInclusion.Multi2P); }
            set {
                if (value)
                    ModeInclusion |= ModeInclusion.Multi2P;
                else
                    ModeInclusion &= ~ModeInclusion.Multi2P;
            }
        }

        [BindGUI("MULTI4P", Category = "OBJECT", ColumnIndex = 1, Control = BindControl.ToggleButton)]
        public bool HasMulti4P
        {
            get { return ModeInclusion.HasFlag(ModeInclusion.Multi4P); }
            set
            {
                if (value)
                    ModeInclusion |= ModeInclusion.Multi4P;
                else
                    ModeInclusion &= ~ModeInclusion.Multi4P;
            }
        }

        [BindGUI("WIFI", Category = "OBJECT", ColumnIndex = 2, Control = BindControl.ToggleButton)]
        public bool HasWiFi
        {
            get { return ModeInclusion.HasFlag(ModeInclusion.WiFi); }
            set
            {
                if (value)
                    ModeInclusion |= ModeInclusion.WiFi;
                else
                    ModeInclusion &= ~ModeInclusion.WiFi;
            }
        }

        [BindGUI("WIFI2P", Category = "OBJECT", ColumnIndex = 3, Control = BindControl.ToggleButton)]
        public bool HasWiFi2P
        {
            get { return ModeInclusion.HasFlag(ModeInclusion.WiFi2P); }
            set
            {
                if (value)
                    ModeInclusion |= ModeInclusion.WiFi2P;
                else
                    ModeInclusion &= ~ModeInclusion.WiFi2P;
            }
        }

        [BindGUI("NAME", Category = "OBJECT", ColumnIndex = 0)]
        public string Label
        {
            get
            {
                if (GlobalSettings.ObjDatabase.ContainsKey(ObjId))
                    return GlobalSettings.ObjDatabase[ObjId].Label + $" ({ObjId})";
                if (GlobalSettings.ObjectList.ContainsKey(ObjId))
                    return GlobalSettings.ObjectList[ObjId] + $" ({ObjId})";

                return ObjId.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether collision detection with this Obj will be skipped.
        /// </summary>
        [ByamlMember(Optional = true)]
        [BindGUI("DISABLE_COLLISION", Category = "OBJECT", ColumnIndex = 0)]
        public bool? NoCol { get; set; }

        /// <summary>
        /// Gets or sets an unknown setting which has never been used in the original courses.
        /// </summary>
        [ByamlMember]
        [BindGUI("TOP_VIEW", Category = "OBJECT", ColumnIndex = 0)]
        public bool TopView { get; set; }

        /// <summary>
        /// Gets or sets an unknown setting.
        /// </summary>
        [ByamlMember(Optional = true)]
        [BindGUI("SINGLE", Category = "OBJECT", ColumnIndex = 0)]
        public bool? Single { get; set; }

        /// <summary>
        /// Gets or sets the speed in which a path is followed.
        /// </summary>
        [ByamlMember]
        [BindGUI("PATH_SPEED", Category = "OBJECT", ColumnIndex = 0)]
        public float Speed { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Courses.Path"/> this Obj is attached to.
        /// </summary>
        public Path Path { get; set; }

        /// <summary>
        /// Gets or sets the point in the <see cref="Path"/> this Obj is attached to.
        /// </summary>
        public PathPoint PathPoint { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Courses.LapPath"/> this Obj is attached to.
        /// </summary>
        public LapPath LapPath { get; set; }

        /// <summary>
        /// Gets or sets the point in the <see cref="LapPath"/> this Obj is attached to.
        /// </summary>
        public LapPathPoint LapPathPoint { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Courses.ObjPath"/> this Obj is attached to.
        /// </summary>
        public ObjPath ObjPath { get; set; }

        /// <summary>
        /// Gets or sets the point in the <see cref="Courses.ObjPath"/> this Obj is attached to.
        /// </summary>
        public ObjPathPoint ObjPathPoint { get; set; }

        /// <summary>
        /// Gets or sets the first <see cref="EnemyPath"/> this Obj is attached to.
        /// </summary>
        public EnemyPath EnemyPath1 { get; set; }

        /// <summary>
        /// Gets or sets the second <see cref="EnemyPath"/> this Obj is attached to.
        /// </summary>
        public EnemyPath EnemyPath2 { get; set; }

        /// <summary>
        /// Gets or sets the first <see cref="ItemPath"/> this Obj is attached to.
        /// </summary>
        public ItemPath ItemPath1 { get; set; }

        /// <summary>
        /// Gets or sets the second <see cref="ItemPath"/> this Obj is attached to.
        /// </summary>
        public ItemPath ItemPath2 { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Area"/> this Obj is attached to.
        /// </summary>
        public Area ParentArea { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Obj"/> this Obj is attached to.
        /// </summary>
        public Obj ParentObj { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Obj"/> this Obj is targeted to.
        /// </summary>
        public List<Obj> TargetObjs { get; set; }

        /// <summary>
        /// Gets or sets the game modes in which this Obj will appear.
        /// </summary>
        public ModeInclusion ModeInclusion { get; set; }

        /// <summary>
        /// Gets or sets an array of 8 float values further controlling object behavior.
        /// </summary>
        [ByamlMember]
        public List<float> Params { get; set; }

        public string[] GetParameterNames()
        {
            if (ParamDatabase.ParameterObjs.ContainsKey(ObjId))
                return ParamDatabase.ParameterObjs[ObjId];
            else
                return new string[8];
        }

        public bool IsSkybox
        {
            get
            {
                return ObjId > 7000 && ObjId < 8000;
            }
        }

        public Obj()
        {
            this.HasMulti2P = true;
            this.HasMulti4P = true;
            this.HasWiFi = true;
            this.HasWiFi2P = true;
            this.Speed = 0;
            this.TopView = false;
            this.Translate = new ByamlVector3F();
            this.Scale = new ByamlVector3F(1, 1, 1);
            this.Rotate = new ByamlVector3F();
            ObjId = 1018;

            Params = new List<float>();
            for (int i = 0; i < 8; i++)
                Params.Add(0);
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Reads data from the given <paramref name="dictionary"/> to satisfy members.
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{String, Object}"/> to read the data from.</param>
        public void DeserializeByaml(IDictionary<string, object> dictionary)
        {
            ModeInclusion = ModeInclusion.FromDictionary(dictionary);
        }

        /// <summary>
        /// Writes instance members into the given <paramref name="dictionary"/> to store them as BYAML data.
        /// </summary>
        /// <param name="dictionary">The <see cref="Dictionary{String, Object}"/> to store the data in.</param>
        public void SerializeByaml(IDictionary<string, object> dictionary)
        {
            ModeInclusion.ToDictionary(dictionary);
        }

        /// <summary>
        /// Allows references of course data objects to be resolved to provide real instances instead of the raw values
        /// in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public void DeserializeReferences(CourseDefinition courseDefinition)
        {
            // References to paths and their points.
            try
            {
                Path = _pathIndex == null ? null : courseDefinition.Paths[_pathIndex.Value];
                PathPoint = _pathPointIndex == null ? null : Path.Points[_pathPointIndex.Value];
                LapPath = _lapPathIndex == null ? null : courseDefinition.LapPaths[_lapPathIndex.Value];
                LapPathPoint = _lapPathPointIndex == null ? null : LapPath.Points[_lapPathPointIndex.Value];
                ObjPath = _objPathIndex == null ? null : courseDefinition.ObjPaths[_objPathIndex.Value];
                ObjPathPoint = _objPathPointIndex == null ? null : ObjPath.Points[_objPathPointIndex.Value];
                EnemyPath1 = _enemyPath1Index == null ? null : courseDefinition.EnemyPaths[_enemyPath1Index.Value];
                EnemyPath2 = _enemyPath2Index == null ? null : courseDefinition.EnemyPaths[_enemyPath2Index.Value];
                ItemPath1 = _itemPath1Index == null ? null : courseDefinition.ItemPaths[_itemPath1Index.Value];
                ItemPath2 = _itemPath2Index == null ? null : courseDefinition.ItemPaths[_itemPath2Index.Value];
            }
            catch
            {

            }

            // References to other unit objects.
            ParentArea = _areaIndex == null ? null : courseDefinition.Areas[_areaIndex.Value];
            ParentObj = _objIndex == null ? null : courseDefinition.Objs[_objIndex.Value];

            if (_targetObjIndex != null)
            {
                TargetObjs = new List<Obj>();
                foreach (var idx in _targetObjIndex)
                    if (idx != -1 && idx < courseDefinition.Objs.Count)
                        TargetObjs.Add(courseDefinition.Objs[idx]);
            }

            if (Path != null) Path.References.Add(this);
        }

        /// <summary>
        /// Allows references between course objects to be serialized into raw values stored in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public void SerializeReferences(CourseDefinition courseDefinition)
        {
            if (Single == false) Single = null;
            if (NoCol == false) NoCol = null;

            // References to paths and their points.
            _pathIndex = Path == null ? null : (int?)courseDefinition.Paths?.IndexOf(Path);
            _pathPointIndex = PathPoint?.Index;
            _lapPathIndex = LapPath == null ? null : (int?)courseDefinition.LapPaths?.IndexOf(LapPath);
            _lapPathPointIndex = LapPathPoint?.Index;
            _objPathIndex = ObjPath == null ? null : (int?)courseDefinition.ObjPaths?.IndexOf(ObjPath);
            _objPathPointIndex = (ObjPath != null && ObjPathPoint != null) ? ObjPathPoint.Index : null;
            _enemyPath1Index = EnemyPath1 == null ? null : (int?)courseDefinition.EnemyPaths?.IndexOf(EnemyPath1);
            _enemyPath2Index = EnemyPath2 == null ? null : (int?)courseDefinition.EnemyPaths?.IndexOf(EnemyPath2);
            _itemPath1Index = ItemPath1 == null ? null : (int?)courseDefinition.ItemPaths?.IndexOf(ItemPath1);
            _itemPath2Index = ItemPath2 == null ? null : (int?)courseDefinition.ItemPaths?.IndexOf(ItemPath2);

            // References to other unit objects.
            _areaIndex = ParentArea == null ? null : (int?)courseDefinition.Areas?.IndexOf(ParentArea);
            _objIndex = ParentObj == null ? null : (int?)courseDefinition.Objs?.IndexOf(ParentObj);

            _targetObjIndex = TargetObjs == null ? null : new List<int>(); ;

            if (TargetObjs != null)
            {
                foreach (var obj in TargetObjs)
                    _targetObjIndex.Add((int)courseDefinition.Objs?.IndexOf(obj));
            }



            if (_objIndex == -1) _objIndex = null;
            if (_areaIndex == -1) _areaIndex = null;
            if (_pathIndex == -1) _pathIndex = null;
            if (_pathPointIndex == -1) _pathPointIndex = null;
            if (_objPathIndex == -1) _objPathIndex = null;
            if (_objPathPointIndex == -1) _objPathPointIndex = null;
            if (_lapPathIndex == -1) _lapPathIndex = null;
            if (_lapPathPointIndex == -1) _lapPathPointIndex = null;
            if (_enemyPath1Index == -1) _enemyPath1Index = null;
            if (_enemyPath2Index == -1) _enemyPath2Index = null;
            if (_itemPath1Index == -1) _itemPath1Index = null;
            if (_itemPath2Index == -1) _itemPath2Index = null;

            if (_pathIndex != null)
                System.Console.WriteLine($"Obj {ObjId} _pathIndex {_pathIndex}");
            if (_objPathIndex != null)
                System.Console.WriteLine($"Obj {ObjId} _objPathIndex {_pathIndex}");
        }

        public Obj Clone()
        {
            return new Obj()
            {
                ModeInclusion = this.ModeInclusion,
                Translate = this.Translate,
                Scale = this.Scale,
                Rotate = this.Rotate,
                TopView = this.TopView,
                Params = this.Params.ToList(),
                Speed = this.Speed,
                Single = this.Single,
                ObjId = this.ObjId,
                NoCol = this.NoCol,
                UnitIdNum = this.UnitIdNum,
                ParentObj = this.ParentObj,
                ParentArea = this.ParentArea,
                LapPath = this.LapPath,
                LapPathPoint = this.LapPathPoint,
                EnemyPath1 = this.EnemyPath1,
                EnemyPath2 = this.EnemyPath2,
                ItemPath1 = this.ItemPath1,
                ItemPath2 = this.ItemPath2,
                Path = this.Path,
                PathPoint = this.PathPoint,
                ObjPath = this.ObjPath,
                ObjPathPoint = this.ObjPathPoint,
            };
        }

        private object CalculateReference<T>(List<T> paths, int index) 
        {
            if (paths == null || index == -1 || paths.Count <= index)
                return null;

            return paths[index];
        }

        public override string ToString() => $"Object_{ObjId}";


        public static string GetResourceName(int objectID)
        {
            if (GlobalSettings.ObjDatabase.ContainsKey(objectID))
                return GlobalSettings.ObjDatabase[objectID].ResNames.FirstOrDefault();
            else if (GlobalSettings.ObjectList.ContainsKey(objectID))
                return GlobalSettings.ObjectList[objectID];
            else
                return "";
        }

        public static string FindFilePath(string resName)
        {
            //Common path for common race objects like coins
            string raceObjectsDX = GlobalSettings.GetContentPath(System.IO.Path.Combine("RaceCommon",resName,$"{resName}.bfres"));
            if (File.Exists(raceObjectsDX)) return raceObjectsDX;
            
            //The typical path for the base game map objects
            string mapObjectsDX = GlobalSettings.GetContentPath(System.IO.Path.Combine("MapObj",resName,$"{resName}.bfres"));
            if (File.Exists(mapObjectsDX)) return mapObjectsDX;
            
            //Same as above, but for MK8U
            string raceObjects = GlobalSettings.GetContentPath(System.IO.Path.Combine("race_common",resName,$"{resName}.bfres"));
            if (File.Exists(raceObjects)) return raceObjects;
            
            string mapObjects = GlobalSettings.GetContentPath(System.IO.Path.Combine("mapobj",resName,$"{resName}.bfres"));
            if (File.Exists(mapObjects)) return mapObjects;

            return string.Empty;
        }
    }
}
