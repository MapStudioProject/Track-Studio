using System.ComponentModel;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents an object on a course which is translated, rotated and scaled in space.
    /// </summary>
    [ByamlObject]
    public abstract class SpatialObject : UnitObject, INotifyPropertyChanged
    {
        private ByamlVector3F _translate;
        private ByamlVector3F _scale;
        private ByamlVector3F _rotate;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the scale of the object.
        /// </summary>
        [ByamlMember]
        public ByamlVector3F Scale
        {
            get { return _scale; }
            set
            {
                if (_scale.X != value.X || _scale.Y != value.Y || _scale.Z != value.Z)
                {
                    _scale = value;
                    NotifyPropertyChanged("Scale");
                }
            }
        }

        /// <summary>
        /// Gets or sets the rotation of the object in radians.
        /// </summary>
        [ByamlMember]
        public ByamlVector3F Rotate
        {
            get { return _rotate; }
            set { _rotate = value; }
        }

        /// <summary>
        /// Gets or sets the rotation of the object in degrees.
        /// </summary>
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
            set {
                if (_translate.X != value.X || _translate.Y != value.Y || _translate.Z != value.Z)
                {
                    _translate = value;
                    NotifyPropertyChanged("Translate");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
