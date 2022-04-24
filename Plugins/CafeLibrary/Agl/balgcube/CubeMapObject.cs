using System.Linq;
using System.Collections.Generic;
using AampLibraryCSharp;
using System.ComponentModel;
using Syroot.Maths;

namespace AGraphicsLibrary
{
    public class CubeMapObject
    {
        public CubeMapUint CubeMapUint { get; set; }

        public List<IlluminantInfo> IlluminantInfos = new List<IlluminantInfo>();

        public string Name => CubeMapUint.Name;

        public CubeMapObject()
        {
            CubeMapUint = new CubeMapUint("area0");
        }

        public CubeMapObject(ParamList paramList)
        {
            foreach (var obj in paramList.paramObjects)
            {
                if (obj.HashString == "cubemap_unit")
                    CubeMapUint = new CubeMapUint(obj);
                else
                    IlluminantInfos.Add(new IlluminantInfo(obj));
            }
        }
    }
}
