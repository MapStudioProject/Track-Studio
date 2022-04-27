using System.Collections.Generic;
using System.ComponentModel;
using Syroot.Maths;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a point of a path of type <typeparamref name="TPath"/>.
    /// </summary>
    /// <typeparam name="TPath">The type of the path this point belongs to.</typeparam>
    /// <typeparam name="TPoint">The type of the point itself.</typeparam>
    [ByamlObject]
    public abstract class PathPointBase<TPath, TPoint> : ICourseReferencable, INotifyPropertyChanged
        where TPath : PathBase<TPath, TPoint>
        where TPoint : PathPointBase<TPath, TPoint>, ICourseReferencable, new()
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the path without any following logic.
        /// </summary>
        internal TPath PathInternal;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the parent path holding this point.
        /// </summary>
        public TPath Path
        {
            get
            {
                return PathInternal;
            }
            set
            {
                // Remove from a possible current parent.
                if (PathInternal != null)
                {
                    PathInternal.Points.Remove((TPoint)this);
                }

                // Set the new parent.
                PathInternal = value;
                // Add to the new parent's points.
                if (PathInternal != null)
                {
                    if (!PathInternal.Points.Contains((TPoint)this))
                        PathInternal.Points.Add((TPoint)this);
                }
            }
        }

        /// <summary>
        /// Gets the index of this point in the parent path or -1 if there is no parent.
        /// </summary>
        public int Index
        {
            get
            {
                if (PathInternal == null)
                {
                    return -1;
                }
                return PathInternal.Points.IndexOf((TPoint)this);
            }
        }

        private ByamlVector3F? _scale;
        private ByamlVector3F _translate;
        private ByamlVector3F _rotate;

        /// <summary>
        /// Gets or sets the scale of the object. Might be optional for specific path types.
        /// </summary>
        [ByamlMember(Optional = true)]
        public ByamlVector3F? Scale
        {
            get
            {
                if (!_scale.HasValue)
                    return new ByamlVector3F(1,1,1);

                return _scale;
            }
            set
            {
                _scale = value;
                if (_scale.HasValue && _scale.Value.X != value.Value.X && _scale.Value.Y != value.Value.Y && _scale.Value.Z != value.Value.Z)
                {
                    NotifyPropertyChanged("Scale");
                }
            }
        }

        /// <summary>
        /// Gets or sets the rotation of the object in radian.
        /// </summary>
        [ByamlMember]
        public ByamlVector3F Rotate { get; set; }

        public ByamlVector3F RotateDegrees
        {
            get { return _rotate * Toolbox.Core.STMath.Rad2Deg; }
            set
            {
                var radians = value * Toolbox.Core.STMath.Deg2Rad;
                if (_rotate.X != radians.X || _rotate.Y != radians.Y || _rotate.Z != radians.Z)
                {
                    _rotate = radians;
                    NotifyPropertyChanged("RotateDegrees");
                }
            }
        }

        /// <summary>
        /// Gets or sets the position at which the object is placed.
        /// </summary>
        [ByamlMember]
        public ByamlVector3F Translate
        {
            get { return _translate; }
            set
            {
                if (_translate.X != value.X || _translate.Y != value.Y || _translate.Z != value.Z)
                {
                    _translate = value;
                    NotifyPropertyChanged("Translate");
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of point instances preceeding this one.
        /// </summary>
        public List<TPoint> PreviousPoints { get; set; }

        /// <summary>
        /// Gets or sets the list of point instances following this one.
        /// </summary>
        public List<TPoint> NextPoints { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="PathPointReference"/> instances for preceeding points.
        /// </summary>
        [ByamlMember("PrevPt", Optional = true)]
        protected List<PathPointReference> PreviousPointIndices;

        /// <summary>
        /// Gets or sets the list of <see cref="PathPointReference"/> instances for succeeding points.
        /// </summary>
        [ByamlMember("NextPt", Optional = true)]
        protected List<PathPointReference> NextPointIndices;

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Allows references of course data objects to be resolved to provide real instances instead of the raw values
        /// in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public virtual void DeserializeReferences(CourseDefinition courseDefinition)
        {
            // Solve the previous and next point references.
            IList<TPath> paths = GetPathReferenceList(courseDefinition);

            for (int i = 0; i < paths.Count; i++)
            {
                //if (paths[i].Points.Count == 0)
                  //  throw new System.Exception($"{this.GetType()} Group: {i} is empty!");
            }

            if (PreviousPointIndices != null)
            {
                PreviousPoints = new List<TPoint>();
                foreach (PathPointReference index in PreviousPointIndices)
                {
                    if (index.PathIndex == -1 || index.PointIndex == -1)
                        continue;

                    if (index.PathIndex >= paths.Count)
                        StudioLogger.WriteErrorException($"{this.GetType()}: Invalid previous path index ({index.PathIndex}) at point {this.Index}");

                    TPath referencedPath = paths[index.PathIndex];

                    if (index.PointIndex >= referencedPath.Points.Count)
                        StudioLogger.WriteErrorException($"{this.GetType()}: Invalid previous point index ({index.PointIndex}) at point {this.Index}");

                    PreviousPoints.Add(referencedPath.Points[index.PointIndex]);
                }
            }

            if (NextPointIndices != null)
            {
                NextPoints = new List<TPoint>();
                foreach (PathPointReference index in NextPointIndices)
                {
                    if (index.PathIndex == -1 || index.PointIndex == -1)
                        continue;

                    if (index.PathIndex >= paths.Count)
                        StudioLogger.WriteErrorException($"{this.GetType()}: Invalid next path index ({index.PathIndex}) at path {this.Index}");

                    TPath referencedPath = paths[index.PathIndex];

                    if (index.PointIndex >= referencedPath.Points.Count)
                        StudioLogger.WriteErrorException($"{this.GetType()}: Invalid next point index ({index.PointIndex}) at path {this.Index}");

                    NextPoints.Add(referencedPath.Points[index.PointIndex]);
                }
            }
        }

        /// <summary>
        /// Allows references between course objects to be serialized into raw values stored in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public virtual void SerializeReferences(CourseDefinition courseDefinition)
        {
            // Solve the previous and next point references.
            IList<TPath> paths = GetPathReferenceList(courseDefinition);

            if (PreviousPoints == null)
            {
                PreviousPointIndices = null;
            }
            else
            {
                PreviousPointIndices = new List<PathPointReference>();
                foreach (TPoint previousPoint in PreviousPoints)
                {
                    int pathIndex = paths.IndexOf(previousPoint.Path);
                    int pointIndex = previousPoint.Index;
                    PreviousPointIndices.Add(new PathPointReference(pathIndex, pointIndex));
                }
            }

            if (NextPoints == null)
            {
                NextPointIndices = null;
            }
            else
            {
                NextPointIndices = new List<PathPointReference>();
                foreach (TPoint nextPoint in NextPoints)
                {
                    int pathIndex = paths.IndexOf(nextPoint.Path);
                    int pointIndex = nextPoint.Index;
                    NextPointIndices.Add(new PathPointReference(pathIndex, pointIndex));
                }
            }
        }

        // ---- METHODS (PROTECTED) ------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the array of paths in the <see cref="CourseDefinition"/> which can be referenced by previous and
        /// next points.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> to get the paths from.</param>
        /// <returns>The array of paths which can be referenced.</returns>
        protected abstract IList<TPath> GetPathReferenceList(CourseDefinition courseDefinition);

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
