using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a path used for different aspects in the game.
    /// </summary>
    [ByamlObject]
    public abstract class PathBase<TPath, TPoint> : UnitObject, ICourseReferencable, INotifyPropertyChanged
        where TPath : PathBase<TPath, TPoint>
        where TPoint : PathPointBase<TPath, TPoint>, ICourseReferencable, new()
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the list of point instances making up this path.
        /// </summary>
        [ByamlMember("PathPt")]
        public List<TPoint> Points { get; set; }

        public PathBase() {
            Points = new List<TPoint>();
        }

        public PathBase(TPoint point) {
            TPath thisTPath = (TPath)this;

            Points = new List<TPoint>();
            point.PathInternal = thisTPath;
            Points.Add(point);
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Allows references of course data objects to be resolved to provide real instances instead of the raw values
        /// in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public virtual void DeserializeReferences(CourseDefinition courseDefinition)
        {
            // Resolve the linked point list and set the path as the parent.
            Debug.WriteLine(this);
            TPath thisTPath = (TPath)this;
            foreach (TPoint point in Points)
            {
                point.PathInternal = thisTPath;
                point.DeserializeReferences(courseDefinition);
            }
        }

        /// <summary>
        /// Allows references between course objects to be serialized into raw values stored in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public virtual void SerializeReferences(CourseDefinition courseDefinition)
        {
            // Resolve the linked point list.
            foreach (TPoint point in Points)
            {
                point.SerializeReferences(courseDefinition);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
