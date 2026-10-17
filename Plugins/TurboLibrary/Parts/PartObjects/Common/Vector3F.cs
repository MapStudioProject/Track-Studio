using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;

namespace PartsEditor
{
    public struct Vector3F
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3F(float v)
        {
            X = v;
            Y = v;
            Z = v;
        }

        public Vector3F(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3F Lerp(Vector3F a, Vector3F b, float weight)
        {
            return new Vector3F(
                STMath.Lerp(a.X, b.X, weight),
                STMath.Lerp(a.Y, b.Y, weight),
                STMath.Lerp(a.Z, b.Z, weight));
        }

        public override string ToString()
        {
            return $"({X} {Y} {Z})";
        }

        public static Vector3F operator +(Vector3F a, Vector3F b)
        {
            return new Vector3F(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3F operator *(Vector3F a, Vector3F b)
        {
            return new Vector3F(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3F operator -(Vector3F a, Vector3F b)
        {
            return new Vector3F(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
    }
}
