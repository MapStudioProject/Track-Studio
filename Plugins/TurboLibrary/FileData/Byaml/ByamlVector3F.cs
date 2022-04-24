using Syroot.Maths;
using System.ComponentModel;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a <see cref="Vector3"/> serializable as BYAML data.
    /// </summary>
    [ByamlObject]
    public struct ByamlVector3F : INotifyPropertyChanged
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private float _x;
        private float _y;
        private float _z;

        /// <summary>
        /// The X float component.
        /// </summary>
        [ByamlMember]
        public float X
        {
            get { return _x; }
            set
            {
                _x = value;
                NotifyPropertyChanged("X");
            }
        }

        /// <summary>
        /// The Y float component.
        /// </summary>
        [ByamlMember]
        public float Y
        {
            get { return _y; }
            set
            { 
                _y = value;
                NotifyPropertyChanged("Y");
            }
        }

        /// <summary>
        /// The Z float component.
        /// </summary>
        [ByamlMember]
        public float Z
        {
            get { return _z; }
            set
            {
                _z = value; 
                NotifyPropertyChanged("Z");
            }
        }

        // ---- CONSTRUCTORS -------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ByamlVector3F"/> struct with the given values for the X, Y and
        /// Z components.
        /// </summary>
        /// <param name="x">The value of the X component.</param>
        /// <param name="y">The value of the Y component.</param>
        /// <param name="z">The value of the Z component.</param>
        public ByamlVector3F(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
            PropertyChanged = null;
        }

        public override bool Equals(object obj)
        {
           return (this.X == ((ByamlVector3F)obj).X &&
                   this.Y == ((ByamlVector3F)obj).Y &&
                   this.Z == ((ByamlVector3F)obj).Z);
        }

        // ---- OPERATORS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// Implicit conversion from <see cref="Vector3F"/>.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3F"/> to convert from.</param>
        /// <returns>The retrieved <see cref="ByamlVector3F"/>.</returns>
        public static implicit operator ByamlVector3F(Vector3F vector)
        {
            return new ByamlVector3F(vector.X, vector.Y, vector.Z);
        }

        public static ByamlVector3F operator *(ByamlVector3F left, ByamlVector3F right)
        {
            return new ByamlVector3F(
                left.X * right.X,
                left.Y * right.Y,
                left.Z * right.Z);
        }

        public static ByamlVector3F operator +(ByamlVector3F left, ByamlVector3F right)
        {
            return new ByamlVector3F(
                left.X + right.X,
                left.Y + right.Y,
                left.Z + right.Z);
        }

        public static ByamlVector3F operator -(ByamlVector3F left, ByamlVector3F right)
        {
            return new ByamlVector3F(
                left.X + right.X,
                left.Y + right.Y,
                left.Z + right.Z);
        }

        public static ByamlVector3F operator *(float left, ByamlVector3F right)
        {
            return new ByamlVector3F(
                left * right.X,
                left * right.Y,
                left * right.Z);
        }

        public static ByamlVector3F operator *(ByamlVector3F left, float right)
        {
            return new ByamlVector3F(
                left.X * right,
                left.Y * right,
                left.Z * right);
        }

        public static ByamlVector3F operator +(ByamlVector3F left, float right)
        {
            return new ByamlVector3F(
                left.X * right,
                left.Y * right,
                left.Z * right);
        }

        public static ByamlVector3F operator -(ByamlVector3F left, float right)
        {
            return new ByamlVector3F(
                left.X * right,
                left.Y * right,
                left.Z * right);
        }



        public override string ToString()
        {
            return $"({X} {Y} {Z})";
        }

        public event PropertyChangedEventHandler PropertyChanged;
  
        private void NotifyPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
