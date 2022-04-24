using System.Collections.Generic;
using System.Numerics;
using ByamlExt.Byaml;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a path for Objs.
    /// </summary>
    [ByamlObject]
    public class ObjPath : PathBase<ObjPath, ObjPathPoint>
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether this path is circular and that the last point connects to the first.
        /// </summary>
        [ByamlMember]
        [BindGUI("Loop", Category = "Properties")]
        public bool IsClosed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this is customizable.
        /// This determines to use the in tool method to rebake the points.
        /// </summary>
        [ByamlMember("TS_Bake", Optional = true)]
        public bool? BakedRailPath { get; set; }

        [ByamlMember("TS_BakeRail", Optional = true)]
        public Path.RailInterpolation? BakedRailType { get; set; }
        
        /// <summary>
        /// Gets or sets the total number of object points.
        /// </summary>
        [ByamlMember]
        public int PtNum { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the thickness between the object points.
        /// </summary>
        [ByamlMember]
        [BindGUI("Split Width", Category = "Properties")]
        public float SplitWidth { get; set; }

        /// <summary>
        /// Gets the baked path points used for animating an object.
        /// </summary>
        [ByamlMember]
        public List<ByamlPathPoint> ObjPt { get; set; }

        public static ObjPath ConvertFromPath(Path path)
        {
            ObjPath objPath = new ObjPath();
            objPath.BakeFromPath(path);
            return objPath;
        }

        public void BakeFromPath(Path path)
        {
            this.Points = new List<ObjPathPoint>();
            this.ObjPt = new List<ByamlPathPoint>();
            this.IsClosed = path.IsClosed;
            this.SplitWidth = path.SplitWidth; //Todo
            this.BakedRailType = path.RailType;

            if (path.RailType == Path.RailInterpolation.Bezier)
                BakeBezierPoints(path);
            else
                BakeLinearPoints(path);

            PtNum = this.ObjPt.Count;
            this.BakedRailPath = true;
        }

        void BakeLinearPoints(Path path)
        {
            for (int i = 0; i < path.Points.Count; i++)
            {
                var point = path.Points[i];
                var objPt = new ObjPathPoint()
                {
                    Translate = point.Translate,
                    Rotate = point.Rotate,
                    PathIndex = ObjPt.Count,
                    Prm1 = 0,
                    Prm2 = 0,
                };
                objPt.Path = this;

                var nextPoint = path.Points[0];
                if (i < path.Points.Count - 1)
                {
                    nextPoint = path.Points[i + 1];
                }
                else if (!IsClosed)
                    break;

                //Get the distance and create points between the rail thickness
                var dist = nextPoint.Translate - point.Translate;
                var length = new Vector3(dist.X, dist.Y, dist.Z).Length();
                for (int j = 0; j < length; j++)
                {

                }

                ObjPt.Add(new ByamlPathPoint()
                {
                    Position = new Syroot.Maths.Vector3F(
                        point.Translate.X, point.Translate.Y, point.Translate.Z),
                    Normal = new Syroot.Maths.Vector3F(0, 1, 0),
                    Unknown = 1,
                });
            }
        }

        void BakeBezierPoints(Path path)
        {
            int posIndex = 0;

            //Create a list of points to interpolate
            ByamlVector3F[] connectLinePositions = new ByamlVector3F[path.Points.Count * 3];
            //Add the last point to interpolate back to at the end if looped
            if (path.IsClosed)
                connectLinePositions = new ByamlVector3F[(path.Points.Count + 1) * 3];

            for (int i = 0; i < path.Points.Count; i++)
            {
                var point = path.Points[i];

                //Add position and control points 
                connectLinePositions[posIndex] = point.Translate;
                connectLinePositions[posIndex + 1] = point.ControlPoints[0];
                connectLinePositions[posIndex + 2] = point.ControlPoints[1];

                posIndex += 3;

                //Loop back to first point if needed
                if (i == path.Points.Count - 1 && i != 0 && path.IsClosed)
                {
                    //Add position and control points 
                    connectLinePositions[posIndex] = path.Points[0].Translate;
                    connectLinePositions[posIndex + 1] = path.Points[0].ControlPoints[0];
                    connectLinePositions[posIndex + 2] = path.Points[0].ControlPoints[1];
                }
            }

            posIndex = 0;
            for (int i = 0; i < path.Points.Count; i++)
            {
                var point = path.Points[i];

                var objPt = new ObjPathPoint()
                {
                    Translate = point.Translate,
                    Rotate = point.Rotate,
                    PathIndex = ObjPt.Count,
                    ControlPoints = point.ControlPoints,
                    Prm1 = 0,
                    Prm2 = 0,
                };
                objPt.Path = this;

                if (i == path.Points.Count - 1 && !path.IsClosed)
                    break;

                //Make sure the point has control points
                if (point.ControlPoints?.Count == 2)
                {
                    //Interpolate between the control points
                    ByamlVector3F p0 = connectLinePositions[posIndex];
                    ByamlVector3F p1 = connectLinePositions[posIndex + 2];
                    ByamlVector3F p2 = connectLinePositions[posIndex + 4];
                    ByamlVector3F p3 = connectLinePositions[posIndex + 3];

                    for (float t = 0f; t <= 1.0; t += 0.0625f)
                    {
                        float u = 1f - t;
                        float tt = t * t;
                        float uu = u * u;
                        float uuu = uu * u;
                        float ttt = tt * t;

                        var pt = (uuu * p0 +
                                        3 * uu * t * p1 +
                                        3 * u * tt * p2 +
                                            ttt * p3);

                        ObjPt.Add(new ByamlPathPoint()
                        {
                           Position = new Syroot.Maths.Vector3F(pt.X, pt.Y, pt.Z),
                           Normal = new Syroot.Maths.Vector3F(0, 1, 0),
                           Unknown = 1,
                        });
                    }
                }
                else
                {
                    var pt = connectLinePositions[posIndex];
                    var pt2 = connectLinePositions[posIndex + 3];
                    
                    //Else display the direct lines between the current and next non handle point
                    ObjPt.Add(new ByamlPathPoint()
                    {
                        Position = new Syroot.Maths.Vector3F(pt.X, pt.Y, pt.Z),
                        Normal = new Syroot.Maths.Vector3F(0, 1, 0),
                        Unknown = 1,
                    });
                    ObjPt.Add(new ByamlPathPoint()
                    {
                        Position = new Syroot.Maths.Vector3F(pt2.X, pt2.Y, pt2.Z),
                        Normal = new Syroot.Maths.Vector3F(0, 1, 0),
                        Unknown = 1,
                    });
                }
                posIndex += 3;
            }
        }


    }
}
