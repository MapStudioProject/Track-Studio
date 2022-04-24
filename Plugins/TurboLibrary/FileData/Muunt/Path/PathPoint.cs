using System.Collections.Generic;
using Syroot.Maths;
using System.Linq;
using Toolbox.Core;
using System;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a point of a <see cref="Path"/>.
    /// </summary>
    [ByamlObject]
    public class PathPoint : PathPointBase<Path, PathPoint>, ICloneable
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        public PathPoint()
        {
            ControlPoints = new List<ByamlVector3F>(2);
            Translate = new ByamlVector3F();
            Rotate = new ByamlVector3F();
            Prm1 = 0;
            Prm2 = 0;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the first parameter.
        /// </summary>
        [ByamlMember("prm1")]
        [BindGUI("Param 1", Category = "Params", ColumnIndex = 0)]
        public float Prm1 { get; set; }

        /// <summary>
        /// Gets or sets the second parameter.
        /// </summary>
        [ByamlMember("prm2")]
        [BindGUI("Param 2", Category = "Params", ColumnIndex = 1)]
        public float Prm2 { get; set; }

        public ByamlVector3F ControlPoint1
        {
            get {
                if (ControlPoints?.Count != 2)
                    return this.Translate;

                return ControlPoints[0];
            }
            set {
                if (ControlPoints?.Count != 2)
                    return;

                ControlPoints[0] = value;
            }
        }

        public ByamlVector3F ControlPoint2
        {
            get
            {
                if (ControlPoints?.Count != 2)
                    return this.Translate;

                return ControlPoints[1];
            }
            set
            {
                if (ControlPoints?.Count != 2)
                    return;

                ControlPoints[1] = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of tangential smoothing points (must be exactly 2).
        /// </summary>
        [ByamlMember]
        public List<ByamlVector3F> ControlPoints { get; set; }

        public bool UsesControlPoints()
        {
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                if (!ControlPoints[i].Equals(Translate))
                    return true;
            }
            return false;
        }

        // ---- METHODS (PROTECTED) ------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the array of paths in the <see cref="CourseDefinition"/> which can be referenced by previous and
        /// next points.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> to get the paths from.</param>
        /// <returns>The array of paths which can be referenced.</returns>
        protected override IList<Path> GetPathReferenceList(CourseDefinition courseDefinition)
        {
            return courseDefinition.Paths;
        }

        // ---- METHODS (PUBLIC) ------------------------------------------------------------------------------------

        /// <summary>
        /// Clones the point instance.
        /// </summary>
        public object Clone()
        {
            return new PathPoint()
            {
                Prm1 = this.Prm1,
                Prm2 = this.Prm2,
                Translate = this.Translate,
                Scale = this.Scale,
                Rotate = this.Rotate,
                ControlPoints = this.ControlPoints.ToList(),
            };
        }
    }
}
