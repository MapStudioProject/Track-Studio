using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;
using Toolbox.Core.IO;
using BfresLibrary;
using BfresLibrary.GX2;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace CafeLibrary.Rendering
{
    /// <summary>
    /// Represents a wrapper for a mesh type to draw indices.
    /// </summary>
    public class BfresPolygonGroupRender 
    {
        public List<GLFrameworkEngine.BoundingNode> Boundings = new List<GLFrameworkEngine.BoundingNode>();
        public List<SubMesh> SubMeshes = new List<SubMesh>();

        public List<DrawCall> DrawCalls = new List<DrawCall>();

        public BfresMeshRender ParentMesh;

        public int FaceCount;
        public int Offset;

        public bool[] InFustrum;

        public STPrimitiveType PrimitiveType = STPrimitiveType.Triangles;
        //GL variables. Todo add type handling to toolbox core, gl converter to helper class in gl framework
        public DrawElementsType DrawElementsType = DrawElementsType.UnsignedInt;

        Random rng = new Random();

        public BfresPolygonGroupRender(BfresMeshRender parentMesh, Shape shape, Mesh mesh, int boundingStartIndex, int offset) {
            ParentMesh = parentMesh;

            Reload(shape, mesh);

            FaceCount = (int)mesh.IndexCount;
            Offset = offset;

            InFustrum = new bool[mesh.SubMeshes.Count];
            for (int i = 0; i < mesh.SubMeshes.Count; i++) {
                SubMeshes.Add(mesh.SubMeshes[i]);
                InFustrum[i] = true;

                DrawCalls.Add(new DrawCall()
                {
                    Offset = (int)SubMeshes[i].Offset + offset,
                    Count = (int)SubMeshes[i].Count,
                    Color = new OpenTK.Vector4(rng.Next(0, 255) / 255.0f,
                        rng.Next(0, 255) / 255.0f,
                        rng.Next(0, 255) / 255.0f, 1.0f),
                });

                var node = shape.SubMeshBoundingNodes.FirstOrDefault(x => x.SubMeshIndex == i && x.SubMeshCount == 1);
                var boundingIndex = boundingStartIndex + i;
                if (node != null)
                    boundingIndex = shape.SubMeshBoundingNodes.IndexOf(node);

                var bounding = shape.SubMeshBoundings[boundingIndex < shape.SubMeshBoundings.Count ? boundingIndex : 0];
                var center = new Vector3(
                    bounding.Center.X,
                    bounding.Center.Y,
                    bounding.Center.Z);
                var extent = new Vector3(
                     bounding.Extent.X,
                     bounding.Extent.Y,
                     bounding.Extent.Z);

                Vector3 min = center - extent;
                Vector3 max = center + extent;

                var boundingNode = new GLFrameworkEngine.BoundingNode()
                {
                    Radius = shape.RadiusArray.FirstOrDefault(),
                    Center = center,
                };
                boundingNode.Box = GLFrameworkEngine.BoundingBox.FromMinMax(min, max);
                Boundings.Add(boundingNode);
            }
        }

        public void Reload(Shape shape, Mesh mesh)
        {
            if (!PrimitiveTypes.ContainsKey(mesh.PrimitiveType))
                throw new Exception($"Unsupported primitive type! {mesh.PrimitiveType}");

            //Set the primitive type
            PrimitiveType = PrimitiveTypes[mesh.PrimitiveType];

            DrawElementsType = DrawElementsType.UnsignedInt;
            if (mesh.IndexFormat == GX2IndexFormat.UInt16 || mesh.IndexFormat == GX2IndexFormat.UInt16LittleEndian)
                DrawElementsType = DrawElementsType.UnsignedShort;
        }


        //Converts bfres primitive types to generic types used for rendering.
        Dictionary<GX2PrimitiveType, STPrimitiveType> PrimitiveTypes = new Dictionary<GX2PrimitiveType, STPrimitiveType>()
        {
            { GX2PrimitiveType.Triangles, STPrimitiveType.Triangles },
            { GX2PrimitiveType.LineLoop, STPrimitiveType.LineLoop },
            { GX2PrimitiveType.Lines, STPrimitiveType.Lines },
            { GX2PrimitiveType.TriangleFan, STPrimitiveType.TriangleFans },
            { GX2PrimitiveType.Quads, STPrimitiveType.Quad },
            { GX2PrimitiveType.QuadStrip, STPrimitiveType.QuadStrips },
            { GX2PrimitiveType.TriangleStrip, STPrimitiveType.TriangleStrips },
        };

        public class DrawCall
        {
            public Vector4 Color;
            public int Offset;
            public int Count;
        }
    }
}
