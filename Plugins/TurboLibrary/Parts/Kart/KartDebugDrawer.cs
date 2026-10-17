using GLFrameworkEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboLibrary.Parts.Kart
{
    public class KartDebugDrawer : EditableObject
    {
        private KartVehicle KartVehicle;

        public KartDebugDrawer(KartVehicle kart) : base(null)
        {
            KartVehicle = kart;
            CanSelect = false;
        }

        public override void DrawModel(GLContext context, Pass pass)
        {
            if (pass != Pass.OPAQUE)
                return;

            KartVehicle.DrawDebug();
        }
    }
}
