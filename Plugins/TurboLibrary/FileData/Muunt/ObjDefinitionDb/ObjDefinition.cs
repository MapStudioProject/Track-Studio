using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents an entry of the objflow.byaml file, describing how an Obj is loaded and behaves in-game.
    /// </summary>
    [ByamlObject]
    public class ObjDefinition
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the way the AI interacts with this Obj.
        /// </summary>
        [ByamlMember]
        [BindGUI("AI React")]
        public AiReact AiReact
        {
            get => aiReact;
            set => SetField(ref aiReact, value);
        }
        private AiReact aiReact;

        /// <summary>
        /// Gets or sets a value possibly indicating if the AI should check this Obj when trying to take a cut.
        /// </summary>
        [ByamlMember]
        [BindGUI("AI Cut Obj")]
        public bool CalcCut
        {
            get => calcCut;
            set => SetField(ref calcCut, value);
        }
        private bool calcCut;

        /// <summary>
        /// Gets or sets a value possibly indicating whether this object can be clipped from view.
        /// </summary>
        [ByamlMember]
        [BindGUI("Clip")]
        public bool Clip
        {
            get => clip;
            set => SetField(ref clip, value);
        }
        private bool clip;

        /// <summary>
        /// Gets or sets a distance possibly indicating when the object should be clipped from view.
        /// </summary>
        [ByamlMember]
        [BindGUI("ClipRadius")]
        public float ClipRadius
        {
            get => clipRadius;
            set => SetField(ref clipRadius, value);
        }
        private float clipRadius;

        /// <summary>
        /// Gets or sets a height offset of a generic collision box.
        /// </summary>
        [ByamlMember]
        [BindGUI("Col Offset Y")]
        public float ColOffsetY
        {
            get => colOffsetY;
            set => SetField(ref colOffsetY, value);
        }
        private float colOffsetY;

        /// <summary>
        /// Gets or sets the shape of a generic collision box.
        /// </summary>
        [ByamlMember]
        [BindGUI("Col Shape")]
        public int ColShape
        {
            get => colShape;
            set => SetField(ref colShape, value);
        }
        private int colShape;

        /// <summary>
        /// Gets or sets the size of a generic collision box.
        /// </summary>
        [ByamlMember]
        [BindGUI("Col Size")]
        public ByamlVector3F ColSize
        {
            get => colSize;
            set => SetField(ref colSize, value);
        }
        private ByamlVector3F colSize;

        /// <summary>
        /// Gets or sets an unknown value.
        /// </summary>
        [ByamlMember]
        [BindGUI("DemoCameraCheck")]
        public bool DemoCameraCheck
        {
            get => demoCameraCheck;
            set => SetField(ref demoCameraCheck, value);
        }
        private bool demoCameraCheck;

        /// <summary>
        /// Gets or sets a list of unknown values.
        /// </summary>
        [ByamlMember("Item")]
        public List<int> Items { get; set; }

        /// <summary>
        /// Gets or sets a list of unknown values.
        /// </summary>
        [ByamlMember("ItemObj")]
        public List<int> ItemObjs { get; set; }

        /// <summary>
        /// Gets or sets a list of unknown values.
        /// </summary>
        [ByamlMember("Kart")]
        public List<int> Karts { get; set; }

        /// <summary>
        /// Gets or sets a list of unknown values.
        /// </summary>
        [ByamlMember("KartObj")]
        public List<int> KartObjs { get; set; }

        /// <summary>
        /// Gets or sets the name of the Obj.
        /// </summary>
        [ByamlMember]
        [BindGUI("Label")]
        public string Label
        {
            get => label;
            set => SetField(ref label, value);
        }
        private string label;

        /// <summary>
        /// Gets or sets an unknown light setting.
        /// </summary>
        [ByamlMember]
        [BindGUI("LightSetting")]
        public int LightSetting
        {
            get => lightSetting;
            set => SetField(ref lightSetting, value);
        }
        private int lightSetting;

        /// <summary>
        /// Gets or sets a distance possibly indicating when the object should be rendered with full detail.
        /// </summary>
        [ByamlMember]
        [BindGUI("Lod 1 Distance")]
        public float Lod1
        {
            get => lod1;
            set => SetField(ref lod1, value);
        }
        private float lod1;

        /// <summary>
        /// Gets or sets a distance possibly indicating when the object should be rendered with a lower detail second
        /// LoD model.
        /// </summary>
        [ByamlMember]
        [BindGUI("Lod 2 Distance")]
        public float Lod2
        {
            get => lod2;
            set => SetField(ref lod2, value);
        }
        private float lod2;

        /// <summary>
        /// Gets or sets a distance possibly indicating when the object should not be drawn at all anymore.
        /// </summary>
        [ByamlMember]
        [BindGUI("LOD No Draw Distance")]
        public float Lod_NoDisp
        {
            get => lod_NoDisp;
            set => SetField(ref lod_NoDisp, value);
        }
        private float lod_NoDisp;

        /// <summary>
        /// Gets or sets an object manager ID.
        /// </summary>
        [ByamlMember]
        [BindGUI("Object Manager ID")]
        public int MgrId
        {
            get => mgrId;
            set => SetField(ref mgrId, value);
        }
        private int mgrId;

        /// <summary>
        /// Gets or sets a possible type determining how the object is rendered.
        /// </summary>
        [ByamlMember]
        [BindGUI("Draw Type")]
        public int ModelDraw
        {
            get => modelDraw;
            set => SetField(ref modelDraw, value);
        }
        private int modelDraw;

        /// <summary>
        /// Gets or sets an unknown model effect number.
        /// </summary>
        [ByamlMember]
        [BindGUI("Model Effect ID")]
        public int ModelEffNo
        {
            get => modelEffNo;
            set => SetField(ref modelEffNo, value);
        }
        private int modelEffNo;

        /// <summary>
        /// Gets or sets an optional model name.
        /// </summary>
        [ByamlMember(Optional = true)]
        [BindGUI("Model Name")]
        public string ModelName
        {
            get => modelName;
            set => SetField(ref modelName, value);
        }
        private string modelName;

        /// <summary>
        /// Gets or sets a value indicating whether this object already moves before the online sync is done.
        /// </summary>
        [ByamlMember]
        [BindGUI("MoveBeforeSync")]
        public bool MoveBeforeSync
        {
            get => moveBeforeSync;
            set => SetField(ref moveBeforeSync, value);
        }
        private bool moveBeforeSync;

        /// <summary>
        /// Gets or sets an unknown value.
        /// </summary>
        [ByamlMember]
        [BindGUI("NotCreate")]
        public bool NotCreate
        {
            get => notCreate;
            set => SetField(ref notCreate, value);
        }
        private bool notCreate;

        /// <summary>
        /// Gets or sets the ID of the Obj with which it is reference in <see cref="CourseDefinition"/> files.
        /// </summary>
        [ByamlMember]
        [BindGUI("ObjId")]
        public int ObjId
        {
            get => objId;
            set => SetField(ref objId, value);
        }
        private int objId;

        /// <summary>
        /// Gets or sets an offset.
        /// </summary>
        [ByamlMember]
        [BindGUI("Offset")]
        public float Offset
        {
            get => offset;
            set => SetField(ref offset, value);
        }
        private float offset;

        /// <summary>
        /// Gets or sets the origin type of the model.
        /// </summary>
        [ByamlMember]
        [BindGUI("Origin")]
        public int Origin
        {
            get => origin;
            set => SetField(ref origin, value);
        }
        private int origin;

        /// <summary>
        /// Gets or sets a value indicating whether the piranha plant item targets and tries to eat this item.
        /// </summary>
        [ByamlMember]
        [BindGUI("PackunEat")]
        public bool PackunEat
        {
            get => packunEat;
            set => SetField(ref packunEat, value);
        }
        private bool packunEat;

        /// <summary>
        /// Gets or sets the path type this Obj possibly requires.
        /// </summary>
        [ByamlMember]
        [BindGUI("PathType")]
        public PathType PathType
        {
            get => pathType;
            set => SetField(ref pathType, value);
        }
        private PathType pathType;

        /// <summary>
        /// Gets or sets a value possibly indicating how pylons Objs react to this object (a pylon can destroy an item
        /// box upon touching for example).
        /// </summary>
        [ByamlMember]
        [BindGUI("PylonReact")]
        public int PylonReact
        {
            get => pylonReact;
            set => SetField(ref pylonReact, value);
        }
        private int pylonReact;

        /// <summary>
        /// Gets or sets the list of resource names.
        /// </summary>
        [ByamlMember("ResName")]
        public List<string> ResNames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this object should be handled as a skydome or not.
        /// </summary>
        [ByamlMember]
        [BindGUI("Is Skybox")]
        public bool VR
        {
            get => vr;
            set => SetField(ref vr, value);
        }
        private bool vr;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    // ---- ENUMERATIONS -------------------------------------------------------------------------------------------

    public enum PathType
    {
        None = 0,
    }

    /// <summary>
    /// Represents the possibly AI reactions to an <see cref="ObjDefinition"/>.
    /// </summary>
    public enum AiReact
    {
        /// <summary>
        /// The AI takes no action.
        /// </summary>
        None = 0,

        /// <summary>
        /// The AI tries to circumvent this Obj.
        /// </summary>
        Repel = 1,

        /// <summary>
        /// The AI tries to collide with this Obj.
        /// </summary>
        Attract = 2,

        /// <summary>
        /// An unknown AI reaction.
        /// </summary>
        Unknown3 = 3
    }
}
