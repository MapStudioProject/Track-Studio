using PartsEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsEditor
{
    public interface ITireTransform
    {
        Vector3F FrontOffset { get; set; }
        Vector3F BackOffset { get; set; }
        Vector3F FrontAntiGravityOffset { get; set; }
        Vector3F BackAntiGravityOffset { get; set; }
    }
}
