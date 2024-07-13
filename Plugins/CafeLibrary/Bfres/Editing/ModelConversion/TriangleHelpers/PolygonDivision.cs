using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BfresLibrary;
using IONET.Core.Model;

namespace CafeLibrary
{
    public class PolygonDivision
    {
        public static List<DivSubMesh> Divide(List<IOVertex> vertices, List<int> indices)
        {    //Sub meshes
            List<DivSubMesh> subMeshes = new List<DivSubMesh>();

            //Generate a triangle list to make checks easier.
            List<Triangle> triangles = new List<Triangle>();
            for (int i = 0; i < indices.Count / 3; i++)
            {
                int ind = i * 3;

                var tri = new Triangle();
                triangles.Add(tri);
                for (int j = 0; j < 3; j++)
                {
                    tri.Indices.Add(indices[ind + j]);
                    tri.Vertices.Add(vertices[indices[ind + j]].Position);
                }
            }
            triangles = triangles.OrderBy(tri => tri.GetMinZ()).ToList();

            //Generates a sub mesh division
            void GenerateDiv(List<Triangle> xstrip)
            {
                Vector3 min = new Vector3(float.MaxValue);
                Vector3 max = new Vector3(float.MinValue);

                List<uint> faces = new List<uint>();
                foreach (var tri in xstrip)
                {
                    foreach (var index in tri.Indices)
                    {
                        faces.Add((uint)index);

                        max.X = MathF.Max(vertices[index].Position.X, max.X);
                        max.Y = MathF.Max(vertices[index].Position.Y, max.Y);
                        max.Z = MathF.Max(vertices[index].Position.Z, max.Z);
                        min.X = MathF.Min(vertices[index].Position.X, min.X);
                        min.Y = MathF.Min(vertices[index].Position.Y, min.Y);
                        min.Z = MathF.Min(vertices[index].Position.Z, min.Z);
                    }
                }
                subMeshes.Add(new DivSubMesh()
                {
                    Faces = faces,
                    Min = min, Max = max,
                });
            }

            float distMax = 1000;

            List<Triangle> zstrip = new List<Triangle>();
            Triangle zseltri = triangles[0];
            int zindex = 0;

            while (true)
            {
                // Distance between current triangle set
                while (triangles[zindex].GetMinZ() - zseltri.GetMinZ() <= distMax) // Add triangles within 1000 units to be in the same group
                {
                    zstrip.Add(triangles[zindex]);
                    zindex++;
                    // Last triangle, break the loop
                    if (zindex == triangles.Count) break;
                }
                zstrip = zstrip.OrderBy(face => face.GetMinX()).ToList();

                // Check triangles in the X direction
                List<Triangle> xstrip = new List<Triangle>();
                var xseltri = zstrip[0];
                int xindex = 0;
                while (true)
                {
                    // Distance between current triangle set
                    while (zstrip[xindex].GetMinX() - xseltri.GetMinX() <= distMax) // Add triangles within 1000 units to be in the same group
                    {
                        xstrip.Add(zstrip[xindex]);
                        xindex++;
                        // Last triangle, break the loop
                        if (xindex == zstrip.Count) break;
                    }

                    // Check triangles in the Y direction
                    List<Triangle> ystrip = new List<Triangle>();
                    var yseltri = xstrip[0];
                    int yindex = 0;
                    while (true)
                    {
                        // Distance between current triangle set
                        while (xstrip[yindex].GetMinY() - yseltri.GetMinY() <= distMax) // Add triangles within 1000 units to be in the same group
                        {
                            ystrip.Add(xstrip[yindex]);
                            yindex++;
                            // Last triangle, break the loop
                            if (yindex == xstrip.Count) break;
                        }
                        GenerateDiv(ystrip);

                        if (yindex == xstrip.Count) break;
                        yseltri = xstrip[yindex];
                        ystrip.Clear();
                    }

                    if (xindex == zstrip.Count) break;
                    xseltri = zstrip[xindex];
                    xstrip.Clear();
                }

                if (zindex == triangles.Count) break;
                zseltri = triangles[zindex];
                zstrip.Clear();
            }

            return subMeshes;
        }
    }

    public class DivSubMesh
    {
        public List<uint> Faces = new List<uint>();

        public Vector3 Min;
        public Vector3 Max;
    }

    class Triangle
    {
        public List<int> Indices = new List<int>();
        public List<Vector3> Vertices = new List<Vector3>();

        public float GetMinX() => Vertices.Min(x => x.X);
        public float GetMinY() => Vertices.Min(x => x.Y);
        public float GetMinZ() => Vertices.Min(x => x.Z);

        public float GetMaxX() => Vertices.Max(x => x.X);
        public float GetMaxY() => Vertices.Max(x => x.Y);
        public float GetMaxZ() => Vertices.Max(x => x.Z);
    }
}
