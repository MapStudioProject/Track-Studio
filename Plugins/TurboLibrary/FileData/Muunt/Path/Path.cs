using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a tangentially smoothed path used for different aspects in the game.
    /// </summary>
    [ByamlObject]
    public class Path : PathBase<Path, PathPoint>
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        public Path()
        {
            Points = new List<PathPoint>();

            UnitIdNum = 0;
            RailType = 0;
            IsClosed = false;
            Delete = false;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value possibly indicating whether Objs using this path are dispoed after reaching the end
        /// of the (non-closed) path.
        /// </summary>
        [ByamlMember]
        [BindGUI("Delete", Category = "Properties")]
        public bool Delete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this path is circular and that the last point connects to the first.
        /// </summary>
        [ByamlMember]
        [BindGUI("Loop", Category = "Properties")]
        public bool IsClosed
        {
            get { return isClosed; }
            set
            {
                if (isClosed != value)
                {
                    isClosed = value;
                    NotifyPropertyChanged("IsClosed");
                }
            }
        }

        [BindGUI("Obj Path", Category = "Properties")]
        public bool UseAsObjPath { get; set; } = false;

        [BindGUI("Obj Path Split Width", Category = "Properties")]
        public float SplitWidth { get; set; } = 3.0f;

        private bool isClosed = false;
        private RailInterpolation _railType;

        /// <summary>
        /// Gets or sets the rail interpolation type.
        /// </summary>
        [ByamlMember]
        [BindGUI("Rail Type", Category = "Properties")]
        public RailInterpolation RailType
        {
            get
            {
                return _railType;
            }
            set
            {
                if (_railType != value) {
                        _railType = value;
                    NotifyPropertyChanged("RailType");
                }
            }
        }

        /// <summary>
        /// Gets or sets a list of objects referencing this path.
        /// </summary>
        public List<object> References { get; set; } = new List<object>();

        public override void SerializeReferences(CourseDefinition courseDefinition) {
            base.SerializeReferences(courseDefinition);

            foreach (var path in Points)
            {
                //Add control handles if none are set
                if (path.ControlPoints?.Count == 0)
                {
                    path.ControlPoints = new List<ByamlVector3F>();
                    path.ControlPoints.Add(path.Translate);
                    path.ControlPoints.Add(path.Translate);
                }
            }
        }

        public static Path ConvertFromObjPath(ObjPath objPath)
        {
            Path path = new Path();
            path.SplitWidth = objPath.SplitWidth;
            path.UseAsObjPath = true;
            path.IsClosed = objPath.IsClosed;
            path.Delete = false;
            path.RailType = objPath.BakedRailType.Value;
            foreach (var objPoint in objPath.Points)
            {
                var pt = new PathPoint();
                pt.ControlPoints = objPoint.ControlPoints;
                if (pt.ControlPoints == null || pt.ControlPoints.Count == 0)
                {
                    pt.ControlPoints = new List<ByamlVector3F>();
                    pt.ControlPoints.Add(objPoint.Translate);
                    pt.ControlPoints.Add(objPoint.Translate);
                }

                pt.Translate = objPoint.Translate;
                pt.Rotate = objPoint.Rotate;
                pt.Scale = objPoint.Scale;
                pt.Prm1 = objPoint.Prm1;
                pt.Prm2 = objPoint.Prm2;
                path.Points.Add(pt);
            }
            return path;
        }

        public enum RailInterpolation
        {
            Linear,
            Bezier,
        }
    }
}
