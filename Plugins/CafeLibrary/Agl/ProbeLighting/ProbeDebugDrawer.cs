using System;
using System.Collections.Generic;
using System.IO;
using GLFrameworkEngine;
using Toolbox.Core.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace AGraphicsLibrary
{
    public class ProbeDebugDrawer
    {
        static UniformBlock shBlock;
        static UniformBlock probeInfoBlock;

        static List<ProbeInstance> ProbeInstances = new List<ProbeInstance>();

        static int MAX_DRAW = 100;

        static int numDraw = 0;

        static SphereRender SphereRender;

        static void Init(int volumeIndex)
        {
            var probeLighting = ProbeMapManager.ProbeLighting;
            var volume = probeLighting.Boxes[volumeIndex];
            ProbeInstances = InitProbes(volume);

            shBlock = new UniformBlock();
            probeInfoBlock = new UniformBlock();
            SphereRender = new SphereRender();

            UpdateVisibleProbes(Vector3.Zero);

            UpdateUniforms();
        }

        public static void Draw(GLContext context)
        {
            if (ProbeMapManager.ProbeLighting == null)
                return;

            //ProbeDebugVoxelDrawer.Draw(context);

            if (ProbeInstances.Count == 0)
                Init(0);

            var shader = GlobalShaders.GetShader("PROBE_DRAWER");
            context.CurrentShader = shader;

            int programID = shader.program;

            shBlock.RenderBuffer(programID, "ProbeSHBuffer");
            probeInfoBlock.RenderBuffer(programID, "ProbeInfo");

            SphereRender.DrawInstance(shader, numDraw);
        }

        static void UpdateUniforms()
        {
            //Init a buffer for the box data to draw
            SetupBlocks(out byte[] block, out byte[] infoBlock);

            shBlock.Buffer.Clear();
            shBlock.Add(block);

            probeInfoBlock.Buffer.Clear();
            probeInfoBlock.Add(infoBlock);
        }

        public static void UpdateVisibleProbes(Vector3 position)
        {
            if (ProbeInstances.Count == 0)
                return;

            float displaySize = 200;

            var displayRegion = new BoundingBox()
            {
                Max = new Vector3(position.X + displaySize, position.Y + displaySize, position.Z + displaySize),
                Min = new Vector3(position.X - displaySize, position.Y - displaySize, position.Z - displaySize),
            };

            for (int i = 0; i < ProbeInstances.Count; i++) {
                ProbeInstances[i].isVisible = displayRegion.IsInside(ProbeInstances[i].Position);
            }
            UpdateUniforms();
        }

        static void SetupBlocks(out byte[] shBlock, out byte[] infoBlock)
        {
            var mem = new MemoryStream();
            var mem2 = new MemoryStream();

            numDraw = 0;

            using (var writer2 = new FileWriter(mem2))
            using (var writer = new FileWriter(mem))
            {
                for (int i = 0; i < ProbeInstances.Count; i++)
                {
                    if (!ProbeInstances[i].isVisible)
                        continue;

                    if (numDraw > MAX_DRAW)
                        break;

                    writer2.Write(new Vector4(ProbeInstances[i].Position, ProbeInstances[i].Size));
                    writer.Write(ProbeInstances[i].SHData);
                    writer.Write(0);

                    numDraw++;
                }
            }
            shBlock = mem.ToArray();
            infoBlock = mem2.ToArray();
        }

        static List<ProbeInstance> InitProbes(ProbeVolume volume)
        {
            List<ProbeInstance> probes = new List<ProbeInstance>();

            var probeCount = volume.Grid.GetProbeCount();
            for (uint x = 0; x < probeCount.X; x++) {
                for (uint y = 0; y < probeCount.Y; y++) {
                    for (uint z = 0; z < probeCount.Z; z++) {
                        int voxelIndex = volume.Grid.GetVoxelIndex(x, y, z);
                        Vector3 positon = volume.Grid.GetVoxelPosition(x, y, z);

                        for (int j = 0; j < 1; j++) {
                            uint dataIndex = volume.IndexBuffer.GetSHDataIndex(voxelIndex, j);
                            if (!volume.IndexBuffer.IsIndexValueValid(dataIndex))
                                continue;

                            float[] data = volume.DataBuffer.GetSHData((int)dataIndex);

                            var probe = new ProbeInstance();
                            probe.SHData = data;
                            probe.Position = positon;
                            probe.Size = 30;
                            probe.BB = new BoundingBox()
                            {
                                Max = new Vector3(positon.X + 15, positon.Y + 15, positon.Z + 15),
                                Min = new Vector3(positon.X - 15, positon.Y - 15, positon.Z - 15),
                            };
                            probes.Add(probe);
                        }
                    }
                }
            }
            return probes;
        }

        class ProbeInstance
        {
            public float[] SHData;
            public Vector3 Position;
            public float Size;
            public bool isVisible = false;
            public BoundingBox BB;
        }
    }
}
