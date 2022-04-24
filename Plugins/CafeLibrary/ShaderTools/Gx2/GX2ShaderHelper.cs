using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafeLibrary.Rendering
{
    public class GX2ShaderHelper
    {
        public static uint GetStreamCount(GX2ShaderVarType type)
        {
            switch (type)
            {
                case GX2ShaderVarType.INT:
                case GX2ShaderVarType.INT2:
                case GX2ShaderVarType.INT3:
                case GX2ShaderVarType.INT4:
                case GX2ShaderVarType.FLOAT:
                case GX2ShaderVarType.FLOAT2:
                case GX2ShaderVarType.FLOAT3:
                case GX2ShaderVarType.FLOAT4:
                    return 1;
                case GX2ShaderVarType.MATRIX4X4:
                    return 4;
                default:
                    throw new Exception("Unsupported/Invalid GX2ShaderVarType");
            }
        }
    }
}
