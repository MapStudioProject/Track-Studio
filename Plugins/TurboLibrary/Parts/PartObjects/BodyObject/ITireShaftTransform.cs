using PartsEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsEditor
{
    public interface ITireShaftTransform
    { 
        float FrontScaleX { get; set; }
        float FrontScaleYZ { get; set; }
        float BackScaleX { get; set; }
        float BackScaleYZ { get; set; }
        Vector3F FrontOffset { get; set; }
        Vector3F FrontAntiGravityOffset { get; set; }
        Vector3F Unknown1 { get; set; }
        Vector3F BackOffset { get; set; }
        Vector3F BackAntiGravityOffset { get; set; }
        Vector3F Unknown2 { get; set; }
    }
}
