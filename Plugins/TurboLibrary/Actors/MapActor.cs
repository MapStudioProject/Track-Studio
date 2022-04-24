using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using EffectLibrary;
using Toolbox.Core.ViewModels;
using OpenTK;
using AGraphicsLibrary;

namespace TurboLibrary.Actors
{
    public class MapActor : ActorModelBase
    {
        /// <summary>
        /// Represents a list of map camera effects emitted via effect area.
        /// Camera effects attach by camera position.
        /// </summary>
        public Handle[] CameraEffects = new Handle[0x4]; //Max of 4 camera effects in a map

        /// <summary>
        /// Represents a list of map direct effects emitted via effect area.
        /// These attach directly to the world position placed in elink.
        /// </summary>
        public Handle[] DirectEffects = new Handle[0x100]; //Max of 16 areas in a map

        List<EmitterMovableObject> EffectObjects = new List<EmitterMovableObject>();

        private bool isEmitted = false;

        List<ProbeDebugVoxelDrawer> ProbeDrawers = new List<ProbeDebugVoxelDrawer>();

        public override void Calc()
        {
            base.Calc();

            var camera = GLContext.ActiveContext.Camera;

            if (!isEmitted)
                CalcAreaEffect(camera);

            foreach(var effect in EffectObjects)
                effect.EmitterHandle.emitterSet.SetMatrix(effect.Transform.TransformMatrix);
        }

        public void CalcAreaEffect(Camera camera) {
           EmitEffectAreaDirect(DirectEffects, "Gu_DossunIseki", Matrix4.Identity);
            isEmitted = true;
        }

        public override void Draw(GLContext context)
        {
         //   DrawDebugProbeDrawer(context);
        }

        private void DrawDebugProbeDrawer(GLContext context)
        {
            if (ProbeMapManager.ProbeLighting == null)
                return;

            if (ProbeDrawers.Count == 0)
            {
                foreach (var volume in ProbeMapManager.ProbeLighting.Boxes)
                    ProbeDrawers.Add(new ProbeDebugVoxelDrawer(volume));
            }

            foreach (var drawer in ProbeDrawers)
                drawer.Draw(context);
        }

        /// <summary>
        /// </summary>
        public static void EmitEffectAreaCamera(string resName)
        {
        }

        public override void Dispose()
        {
            foreach (var vol in ProbeDrawers)
                vol.Dispose();
        }

        /// <summary>
        /// </summary>
        public static void EmitEffectAreaDirect(Handle[] handles, string resName, Matrix4 transform)
        {
            EffectManager.Instance.EmitDirect(handles, $"AreaEnv_{resName}", transform);
        }

        /// <summary>
        /// A transformable object to attach emitters to with an attached emitter handle.
        /// </summary>
        class EmitterMovableObject : TransformableObject
        {
            public Handle EmitterHandle;

            public EmitterMovableObject(NodeBase parent) : base(parent)
            {

            }
        }
    }
}
