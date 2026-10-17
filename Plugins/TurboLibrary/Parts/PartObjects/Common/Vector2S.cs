using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsEditor
{
    public struct Vector2S
    {
        public short X { get; set; }
        public short Y { get; set; }

        public Vector2S(short x, short y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X} {Y})";
        }
    }
}
