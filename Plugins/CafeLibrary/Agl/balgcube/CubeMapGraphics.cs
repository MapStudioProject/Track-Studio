using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AampLibraryCSharp;

namespace AGraphicsLibrary
{
    public class CubeMapGraphics
    {
        public List<CubeMapObject> CubeMapObjects = new List<CubeMapObject>();

        private ParamList Root;

        public CubeMapGraphics()
        {

        }

        public CubeMapObject GetCubeMapObject(string name)
        {
            for (int i = 0; i < CubeMapObjects.Count; i++)
            {
                if (CubeMapObjects[i].CubeMapUint.Name == name)
                    return CubeMapObjects[i];
            }
            return CubeMapObjects.FirstOrDefault();
        }

        public CubeMapGraphics(AampFile aamp)
        {
            Root = aamp.RootNode;
            foreach (var obj in aamp.RootNode.childParams)
                CubeMapObjects.Add(new CubeMapObject(obj));
        }

        public byte[] Save(bool isVersion2)
        {
            AampFile aamp = new AampFile();
            aamp.RootNode = this.Root;
            aamp.ParameterIOType = "aglcube";

            if (isVersion2)
                aamp = aamp.ConvertToVersion2();
            else
                aamp = aamp.ConvertToVersion1();

            var mem = new System.IO.MemoryStream();
            aamp.unknownValue = 1;
            aamp.Save(mem);
            return mem.ToArray();
        }
    }
}
