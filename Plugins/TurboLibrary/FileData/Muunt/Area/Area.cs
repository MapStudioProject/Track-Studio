using System;
using System.Collections.Generic;
using System.ComponentModel;
using Toolbox.Core;

namespace TurboLibrary
{
    [ByamlObject]
    public class Area : PrmObject, ICourseReferencable, INotifyPropertyChanged, ICloneable
    {
        [ByamlMember("Area_Path", Optional = true)]
        public int? _pathIndex;
        [ByamlMember("Area_PullPath", Optional = true)]
        public int? _pullPathIndex;

        //MK8D current and prison areas
        [ByamlMember("Area_Obj", Optional = true)]
        public int? _areaObjIndex;
        [ByamlMember("Area_ClipObj", Optional = true)]
        public int? _areaClipObjIndex;
        [ByamlMember("Camera_Area", Optional = true)]
        public List<int> _cameraAreas { get; set; }

        [ByamlMember]
        [BindGUI("Area Shape", Category = "Area")]
        public AreaShape AreaShape { get; set; }

        private AreaType _areaType;

        [ByamlMember]
        [BindGUI("Area Type", Category = "Area")]
        public AreaType AreaType
        {
            get
            {
                return _areaType;
            }
            set
            {
                _areaType = value;
                NotifyPropertyChanged("AreaType");
            }
        }

        [BindGUI("Replay Cameras", Category = "Area")]
        public List<ReplayCamera> ReplayCameras { get; set; } = new List<ReplayCamera>();

        public Path Path { get; set; }
        public PullPath PullPath { get; set; }
        public Obj Obj { get; set; }

        public Area() { Init(AreaType.Camera); }

        public Area(AreaType type) { Init(type); }

        void Init(AreaType type)
        {
            AreaType = type;
            AreaShape = AreaShape.Cube;
            Path = null;
            PullPath = null;
            Obj = null;
            Scale = new ByamlVector3F(1, 1, 1);
        }

        public void DeserializeReferences(CourseDefinition courseDefinition)
        {
            Path = _pathIndex == null ? null : courseDefinition.Paths[_pathIndex.Value];
            PullPath = _pullPathIndex == null ? null : courseDefinition.PullPaths[_pullPathIndex.Value];
            Obj = _areaObjIndex == null ? null : courseDefinition.Objs[_areaObjIndex.Value];
            if (_cameraAreas != null)
            {
                ReplayCameras = new List<ReplayCamera>();
                foreach (var index in _cameraAreas)
                    ReplayCameras.Add(courseDefinition.ReplayCameras[index]);
            }

            if (Path != null) Path.References.Add(this);
        }

        public void SerializeReferences(CourseDefinition courseDefinition)
        {
            _pathIndex = Path == null ? null : (int?)courseDefinition.Paths.IndexOf(Path);
            _pullPathIndex = PullPath == null ? null : (int?)courseDefinition.PullPaths.IndexOf(PullPath);
            _areaObjIndex = Obj == null ? null : (int?)courseDefinition.Objs.IndexOf(Obj);
           // _cameraAreas = null;
            if (ReplayCameras?.Count > 0)
            {
                _cameraAreas = new List<int>();
                foreach (var cam in ReplayCameras)
                {
                    var index = courseDefinition.ReplayCameras.IndexOf(cam);
                    if (index != -1)
                        _cameraAreas.Add(index);
                }
            }
        }

        public override string ToString() => "Area";

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            var cameras = new List<ReplayCamera>();
            cameras.AddRange(this.ReplayCameras);

            return new Area()
            {
                AreaShape = this.AreaShape,
                AreaType = this.AreaType,
                ReplayCameras = cameras,
                Path = this.Path,
                Obj = this.Obj,
                Prm1 = this.Prm1,
                Prm2 = this.Prm2,
                PullPath = this.PullPath,
                Rotate = this.Rotate,
                Scale = this.Scale,
                Translate = this.Translate,
            };
        }
    }
}
