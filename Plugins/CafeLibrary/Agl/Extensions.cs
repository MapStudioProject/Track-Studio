using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syroot.Maths;
using AampLibraryCSharp;
using Toolbox.Core;

namespace AGraphicsLibrary
{
    public static class Extensions
    {
        public static STColor ToSTColor(this Vector4F value)
        {
            return new STColor(value.X, value.Y, value.Z, value.W);
        }

        public static Vector4F ToColorF(this STColor value)
        {
            return new Vector4F()
            {
                X = value.R,
                Y = value.G,
                Z = value.B,
                W = value.A,
            };
        }
    }
}
