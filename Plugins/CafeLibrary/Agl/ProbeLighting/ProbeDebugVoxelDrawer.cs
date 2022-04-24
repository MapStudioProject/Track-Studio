using System;
using System.Collections.Generic;
using System.IO;
using GLFrameworkEngine;
using Toolbox.Core.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace AGraphicsLibrary
{
    public class ProbeDebugVoxelDrawer
    {
        int vertexBuffer;

        VertexArrayObject vao;

        Vertex[] Vertices = new Vertex[0];

        ProbeVolume ProbeVolume;

        void Init(ProbeVolume volume)
        {
            ProbeVolume = volume;

            GL.GenBuffers(1, out vertexBuffer);

            vao = new VertexArrayObject(vertexBuffer);
            vao.AddAttribute(0, 3, VertexAttribPointerType.Float, false, Vertex.SIZE, 0);
            //7 coef
            vao.AddAttribute(1, 4, VertexAttribPointerType.Float, false, Vertex.SIZE, 12);
            vao.AddAttribute(2, 4, VertexAttribPointerType.Float, false, Vertex.SIZE, 28);
            vao.AddAttribute(3, 4, VertexAttribPointerType.Float, false, Vertex.SIZE, 44);
            vao.AddAttribute(4, 4, VertexAttribPointerType.Float, false, Vertex.SIZE, 60);
            vao.AddAttribute(5, 4, VertexAttribPointerType.Float, false, Vertex.SIZE, 76);
            vao.AddAttribute(6, 4, VertexAttribPointerType.Float, false, Vertex.SIZE, 92);
            vao.AddAttribute(7, 4, VertexAttribPointerType.Float, false, Vertex.SIZE, 108);

            vao.Initialize();

            Vertices = InitProbes(volume).ToArray();

            vao.Bind();
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, Vertex.SIZE * Vertices.Length, Vertices, BufferUsageHint.StaticDraw);
        }

        public ProbeDebugVoxelDrawer(ProbeVolume volume)
        {
            Init(volume);
        }

        public void Draw(GLContext context)
        {
            if (ProbeMapManager.ProbeLighting == null)
                return;

            //Draw box
            var mat = new StandardMaterial();
            mat.Render(context);

            BoundingBoxRender.Draw(context,
                new Vector3(ProbeVolume.Grid.Min.X, ProbeVolume.Grid.Min.Y, ProbeVolume.Grid.Min.Z),
                new Vector3(ProbeVolume.Grid.Max.X, ProbeVolume.Grid.Max.Y, ProbeVolume.Grid.Max.Z));

            var shader = GlobalShaders.GetShader("PROBE_VOXEL");
            context.CurrentShader = shader;

            vao.Use();

            GL.PointSize(20);
            GL.DrawArrays(PrimitiveType.Points, 0, Vertices.Length);
            GL.PointSize(1);
        }

        public void Dispose()
        {
            vao.Delete();
        }

        static List<Vertex> InitProbes(ProbeVolume volume)
        {
            List<Vertex> probes = new List<Vertex>();

            var probeCount = volume.Grid.GetProbeCount();
            for (uint x = 0; x < probeCount.X; x++) {
                for (uint y = 0; y < probeCount.Y; y++) {
                    for (uint z = 0; z < probeCount.Z; z++) {
                        int voxelIndex = volume.Grid.GetVoxelIndex(x, y, z);
                        Vector3 positon = volume.Grid.GetVoxelPosition(x, y, z);

                        for (int j = 0; j < 1; j++) {
                            uint dataIndex = volume.IndexBuffer.GetSHDataIndex(voxelIndex, j);
                            if (volume.IndexBuffer.IsIndexValueEmpty(dataIndex))
                                continue;

                            if (volume.IndexBuffer.IsIndexValueInvisible(dataIndex))
                            {
                                float[] data = new float[27];

                                var probe = new Vertex();
                                probe.Position = positon;
                                probe.Coef0 = new Vector4(data[0], data[1], data[2], data[3]);
                                probe.Coef1 = new Vector4(data[4], data[5], data[6], data[7]);
                                probe.Coef2 = new Vector4(data[8], data[9], data[10], data[11]);
                                probe.Coef3 = new Vector4(data[12], data[13], data[14], data[15]);
                                probe.Coef4 = new Vector4(data[16], data[17], data[18], data[19]);
                                probe.Coef5 = new Vector4(data[20], data[21], data[22], data[23]);
                                probe.Coef6 = new Vector4(data[24], data[25], data[26], 0);
                                probes.Add(probe);
                            }
                            else
                            {
                                float[] data = volume.DataBuffer.GetSHData((int)dataIndex);
                                var coef = LightProbeMgr.ConvertSH2RGB(data);

                                var probe = new Vertex();
                                probe.Position = positon;
                                probe.Coef0 = coef[0];
                                probe.Coef1 = coef[1];
                                probe.Coef2 = coef[2];
                                probe.Coef3 = coef[3];
                                probe.Coef4 = coef[4];
                                probe.Coef5 = coef[5];
                                probe.Coef6 = coef[6];
                                probes.Add(probe);
                            }
                        }
                    }
                }
            }
            return probes;
        }

        struct Vertex
        {
            public Vector3 Position;
            public Vector4 Coef0;
            public Vector4 Coef1;
            public Vector4 Coef2;
            public Vector4 Coef3;
            public Vector4 Coef4;
            public Vector4 Coef5;
            public Vector4 Coef6;

            public static int SIZE = 4 * (3 + (4 * 7));
        }
    }
}
