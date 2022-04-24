using System;
using System.Collections.Generic;
using OpenTK;
using GLFrameworkEngine;

namespace TurboLibrary.GameLogic
{
    public class CubePath<TPath, TPoint>
               where TPath : PathBase<TPath, TPoint>
               where TPoint : PathPointBase<TPath, TPoint>, new()
    {
        public List<CubePathPoint> Points = new List<CubePathPoint>();

        public class CubePathPoint
        {
            /// <summary>
            /// The original path point instance.
            /// </summary>
            public TPoint Point { get; set; }

            /// <summary>
            /// The 4 corner regions of a cube path.
            /// </summary>
            public Vector3[] Corners = new Vector3[4];

            /// <summary>
            /// bounding regions to each next point for calculating inside the cube point.
            /// </summary>
            public List<Vector3[]> NextPointRegions = new List<Vector3[]>();

            /// <summary>
            /// Determines if a point is inside the region connected to this.
            /// </summary>
            public bool IsInside(Vector3 point)
            {
                foreach (var region in NextPointRegions)
                {
                    int num = 0;

                    Vector3 b1 = Corners[0];
                    Vector3 b2 = Corners[1];
                    Vector3 b3 = region[0];
                    Vector3 b4 = region[1];

                    Vector3 t1 = Corners[2];
                    Vector3 t2 = Corners[3];
                    Vector3 t3 = region[2];
                    Vector3 t4 = region[3];

                    Vector3 dir1 = t1 - b1;
                    Vector3 dir2 = b2 - b1;
                    Vector3 dir3 = b4 - b2;

                    Vector3 cube3d_center = (b1 + t3) / 2.0f;
                    Vector3 dir_vec = point - cube3d_center;

                    if ((Vector3.Dot(dir_vec, dir1.Normalized()) * 2) > dir1.Length) num++;
                    if ((Vector3.Dot(dir_vec, dir2.Normalized()) * 2) > dir2.Length) num++;
                    if ((Vector3.Dot(dir_vec, dir3.Normalized()) * 2) > dir3.Length) num++;

                    if (num == 0)
                        return true;
                }
                return false;
            }

            /// <summary>
            /// Determines if a bounding box is overlapping the region connected to this.
            /// </summary>
            public bool IsOverlapping(BoundingBox box)
            {
                return false;
            }
        }

        public void Setup(List<TPath> paths)
        {
            if (paths == null)
                return;

            Points.Clear();
            foreach (var path in paths)
            {
                foreach (var point in path.Points)
                {
                    var pt = new CubePathPoint();
                    Points.Add(pt);
                    //Calculate the corner points
                    pt.Corners = GetCornerPoints(point);
                    //Calculate the boundings
                    foreach (var nextPt in point.NextPoints)
                    {
                        //Create a bounding region from the current 4 corners and the next 4 corners
                        pt.NextPointRegions.Add(GetCornerPoints(nextPt));
                    }
                }
            }
        }

        public void DrawDebug(GLContext context)
        {
            var mat = new StandardMaterial();
            mat.Render(context);
        }

        /// <summary>
        /// Checks if a selection of cube path points intersect this one.
        /// </summary>
        public bool IsIntersect(List<CubePathPoint> points)
        {
            return false;
        }

        /// <summary>
        /// Determines if the x y z point is inside the path region.
        /// </summary>
        public bool IsPointInPath(float x, float y, float z)
        {
            foreach (var point in Points)
                if (point.IsInside(new Vector3(x, y, z)))
                    return true;

            return false;
        }

        private Vector3[] GetCornerPoints(TPoint point)
        {
            Vector3[] cube = new Vector3[4]
            {
                    new Vector3(-1,-1, 1),
                    new Vector3(1,-1, 1),
                    new Vector3(-1, 1, 1),
                    new Vector3(1, 1, 1),
            };

            Vector3 translate = new Vector3(point.Translate.X, point.Translate.Y, point.Translate.Z);
            Vector3 scale = new Vector3(point.Scale.Value.X, point.Scale.Value.Y, 0) / 2;
            Vector3 rotation = new Vector3(point.Rotate.X, point.Rotate.Y, point.Rotate.Z);

            Matrix4 pointTransform = Matrix4Extension.CreateTransform(translate, rotation, scale);
            for (int i = 0; i < 4; i++)
                cube[i] = Vector3.TransformPosition(cube[i], pointTransform);

            return cube;
        }
    }
}
