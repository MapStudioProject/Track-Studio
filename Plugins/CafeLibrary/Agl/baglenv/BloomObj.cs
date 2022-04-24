using System.Linq;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;

namespace AGraphicsLibrary
{
    /// <summary>
    /// Represents an object that controls bloom.
    /// </summary>
    public class BloomObj : EnvObject
    {
        internal override ParamObject Parent { get; set; }

        public BloomObj(string name)
        {
            Parent = new ParamObject();
            Name = name;
        }

        public BloomObj(ParamObject paramObject) {
            Parent = paramObject;
        }
    }
}
