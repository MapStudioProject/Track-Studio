using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using IONET.Core.Model;

namespace CafeLibrary
{
    class PolygonDivision
    {
        public class PolygonSettings
        {
            /// <summary>
            /// The max cube size of the root octrees.
            /// </summary>
            public int MaxRootSize = 2048;
            /// <summary>
            /// The min cube size of the root octrees.
            /// </summary>
            public int MinRootSize = 128;
            /// <summary>
            /// The max cube size of the all octrees.
            /// </summary>
            public int MaxCubeSize = 0x100000;
            /// <summary>
            /// The min cube size of the all octrees.
            /// </summary>
            public int MinCubeSize = 32;
            /// <summary>
            /// The max depth size of the all octrees.
            /// </summary>
            public int MaxOctreeDepth = 10;
            /// <summary>
            /// The max amount of triangles in an octree.
            /// When the limit is reached, octrees will divide until the min cube size is reached.
            /// </summary>
            public int MaxTrianglesInCube = 10;
            /// <summary>
            /// The min amount of padding to use for the collison boundings.
            /// </summary>
            public Vector3 PaddingMin = new Vector3(-50, -50, -50);

            /// <summary>
            /// The max amount of padding to use for the collison boundings.
            /// </summary>
            public Vector3 PaddingMax = new Vector3(50, 50, 50);
        }

        public static List<PolygonOctree> Divide(List<IOVertex> vertices, List<int> indices, PolygonSettings settings)
        {
            Dictionary<ushort, Triangle> triangles = new Dictionary<ushort, Triangle>();
            Vector3 minCoordinate = new Vector3(float.MaxValue);
            Vector3 maxCoordinate = new Vector3(float.MinValue);

            for (int i = 0; i < indices.Count / 3; i++)
            {
                int index = i * 3;
                triangles.Add((ushort)i, new Triangle()
                {
                    Vertices = new Vector3[3]
                    {
                        vertices[indices[index+0]].Position,
                        vertices[indices[index+1]].Position,
                        vertices[indices[index+2]].Position
                    },
                });

                // Get the position vectors and find the smallest and biggest coordinates.
                for (int j = 0; j < 3; j++)
                {
                    Vector3 position = vertices[indices[index + j]].Position;
                    minCoordinate.X = Math.Min(position.X, minCoordinate.X);
                    minCoordinate.Y = Math.Min(position.Y, minCoordinate.Y);
                    minCoordinate.Z = Math.Min(position.Z, minCoordinate.Z);
                    maxCoordinate.X = Math.Max(position.X, maxCoordinate.X);
                    maxCoordinate.Y = Math.Max(position.Y, maxCoordinate.Y);
                    maxCoordinate.Z = Math.Max(position.Z, maxCoordinate.Z);
                }
            }

            //Padd the coordinates
            minCoordinate += settings.PaddingMin;
            maxCoordinate += settings.PaddingMax;

            // Compute the octree.
            Vector3 size = maxCoordinate - minCoordinate;
            Vector3 exponents = new Vector3(
                (uint)GetNext2Exponent(size.X),
                (uint)GetNext2Exponent(size.Y),
                (uint)GetNext2Exponent(size.Z));
            int cubeSizePower = GetNext2Exponent(Math.Min(Math.Min(size.X, size.Y), size.Z));
            if (cubeSizePower > GetNext2Exponent(settings.MaxRootSize))
                cubeSizePower = GetNext2Exponent(settings.MaxRootSize);

            int cubeSize = 1 << cubeSizePower;
            Vector3 cubeCounts = new Vector3(
                (uint)Math.Max(1, (1 << (int)exponents.X) / cubeSize),
                (uint)Math.Max(1, (1 << (int)exponents.Y) / cubeSize),
                (uint)Math.Max(1, (1 << (int)exponents.Z) / cubeSize));
            // Generate the root nodes, which are square cubes required to cover all of the model.
            var roots = new List<PolygonOctree>();

            int cubeBlow = 50;
            for (int z = 0; z < cubeCounts.Z; z++)
            {
                for (int y = 0; y < cubeCounts.Y; y++)
                {
                    for (int x = 0; x < cubeCounts.X; x++)
                    {
                        Vector3 cubePosition = minCoordinate + ((float)cubeSize) * new Vector3(x, y, z);
                        var octree = new PolygonOctree(triangles, cubePosition, cubeSize,
                            settings.MaxTrianglesInCube, settings.MaxCubeSize,
                            settings.MinCubeSize, cubeBlow, settings.MaxOctreeDepth);
                        if (octree.TriangleIndices.Count > 0)
                            roots.Add(octree);
                    }
                }
            }
            return roots.ToList();
        }

        static void CalculateMinMax(Vector3[] vertices, out Vector3 min, out Vector3 max)
        {
            Vector3 minCoordinate = new Vector3(float.MaxValue);
            Vector3 maxCoordinate = new Vector3(float.MinValue);

            for (int j = 0; j < vertices.Length; j++)
            {
                Vector3 position = vertices[j];
                minCoordinate.X = Math.Min(position.X, minCoordinate.X);
                minCoordinate.Y = Math.Min(position.Y, minCoordinate.Y);
                minCoordinate.Z = Math.Min(position.Z, minCoordinate.Z);
                maxCoordinate.X = Math.Max(position.X, maxCoordinate.X);
                maxCoordinate.Y = Math.Max(position.Y, maxCoordinate.Y);
                maxCoordinate.Z = Math.Max(position.Z, maxCoordinate.Z);
            }
            min = minCoordinate;
            max = maxCoordinate;
        }

        internal static int GetNext2Exponent(float value)
        {
            if (value <= 1) return 0;
            return (int)Math.Ceiling(Math.Log(value, 2));
        }

        public class PolygonOctree
        {
            public List<PolygonOctree> Children = new List<PolygonOctree>();

            public PolygonOctree(Dictionary<ushort, Triangle> triangles, Vector3 cubePosition, float cubeSize,
                int maxTrianglesInCube, int maxCubeSize, int minCubeSize, int cubeBlow, int maxDepth, int depth = 0)
            {
                //Adjust the cube sizes 
                Vector3 cubeCenter = cubePosition + new Vector3(cubeSize / 2f, cubeSize / 2f, cubeSize / 2f);
                float newsize = cubeSize + cubeBlow;
                Vector3 newPosition = cubeCenter - new Vector3(newsize / 2f, newsize / 2f, newsize / 2f);

                // Go through all triangles and remember them if they overlap with the region of this cube.
                Dictionary<ushort, Triangle> containedTriangles = new Dictionary<ushort, Triangle>();
                foreach (KeyValuePair<ushort, Triangle> triangle in triangles)
                {
                    if (TriangleHelper.TriangleCubeOverlap(triangle.Value, newPosition, newsize))
                    {
                        containedTriangles.Add(triangle.Key, triangle.Value);
                    }
                }

                float halfWidth = cubeSize / 2f;

                bool isTriangleList = cubeSize <= maxCubeSize && containedTriangles.Count <= maxTrianglesInCube ||
                                      cubeSize <= minCubeSize || depth > maxDepth;

                if (containedTriangles.Count > maxTrianglesInCube && halfWidth >= minCubeSize)
                {
                    // Too many triangles are in this cube, and it can still be subdivided into smaller cubes.
                    float childCubeSize = cubeSize / 2f;
                    Children = new List<PolygonOctree>();
                    int i = 0;
                    for (int z = 0; z < 2; z++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            for (int x = 0; x < 2; x++)
                            {
                                Vector3 childCubePosition = cubePosition + childCubeSize * new Vector3(x, y, z);
                                var octree = new PolygonOctree(containedTriangles, childCubePosition, childCubeSize,
                                    maxTrianglesInCube, maxCubeSize, minCubeSize, cubeBlow, maxDepth, depth + 1);
                                if (octree.TriangleIndices.Count > 0)
                                    Children.Add(octree);
                            }
                        }
                    }
                }
                else
                {
                    // Either the amount of triangles in this cube is okay or it cannot be subdivided any further.
                    TriangleIndices = containedTriangles.Keys.ToList();
                    //Calculate the bounding indices
                    var positions = containedTriangles.SelectMany(x => x.Value.Vertices).ToArray();
                    CalculateMinMax(positions, out Vector3 min, out Vector3 max);
                    Min = min;
                    Max = max;
                }
            }

            public Vector3 Max { get; set; }
            public Vector3 Min { get; set; }

            public List<ushort> TriangleIndices = new List<ushort>();
        }
    }

    public class Triangle
    {
        public Vector3[] Vertices = new Vector3[3];

        public Vector3 Normal;
    }
}
