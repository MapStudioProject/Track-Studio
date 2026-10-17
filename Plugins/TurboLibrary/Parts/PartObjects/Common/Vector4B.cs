using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsEditor
{
    public class Vector4B
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Z { get; set; }
        public byte W { get; set; }

        public Vector4B(byte x, byte y)
        {
            X = x;
            Y = y;
            Z = 0;
            W = 0;
        }

        public Vector4B(byte x, byte y, byte z)
        {
            X = x;
            Y = y;
            Z = z;
            W = 0;
        }

        public Vector4B(byte x, byte y, byte z, byte w) {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public override string ToString()
        {
            return $"({X} {Y} {Z} {W})";
        }
    }
}
