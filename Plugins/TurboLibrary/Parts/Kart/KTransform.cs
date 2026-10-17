using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboLibrary.Parts.Kart
{
    public class KTransform
    {
        public Vector3 Position;
        public Vector3 Scale = Vector3.One;
        public Quaternion Rotation = Quaternion.Identity;
    }
}
