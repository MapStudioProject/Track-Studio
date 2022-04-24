using Toolbox.Core;
using GLFrameworkEngine;
using OpenTK;

namespace TurboLibrary
{
    /// <summary>
    /// Represents an clip area controlling model clipping.
    /// </summary>
    [ByamlObject]
    public class ClipArea : PrmObject, System.ICloneable
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or set the shape the outer form of this clip area spans. Only <see cref="AreaShape.Cube"/> is known for
        /// these to be valid.
        /// </summary>
        [ByamlMember]
        [BindGUI("Area Shape", Category = "Properties")]
        public AreaShape AreaShape { get; set; }

        /// <summary>
        /// Gets or sets the action taken for this clip area. Only <see cref="AreaType.Clip"/> is valid for clip areas.
        /// </summary>
        [ByamlMember]
        [BindGUI("Area Type", Category = "Properties")]
        public AreaType AreaType { get; set; }

        public ClipArea()
        {
            AreaShape = AreaShape.Cube;
            AreaType = AreaType.Clip;
            Scale = new ByamlVector3F(1, 1, 1);
        }

        public BoundingBox GetBoundingBox()
        {
            var points = GetCornerPoints(this);
            BoundingBox.CalculateMinMax(points, out Vector3 min, out Vector3 max);
            return BoundingBox.FromMinMax(min, max);
        }

        static Matrix4 InitalTransform => new Matrix4(
            50, 0, 0, 0,
            0, 50, 0, 0,
            0, 0, 50, 0,
            0, 50, 0, 1);

        private Vector3[] GetCornerPoints(ClipArea area)
        {
            Vector3[] cube = new Vector3[4]
               {
                        new Vector3(-1,-1, 1),
                        new Vector3(1,-1, 1),
                        new Vector3(-1, 1, -1),
                        new Vector3(1, 1, -1),
               };

            Vector3 scale = new Vector3(area.Scale.X, area.Scale.Y, area.Scale.Z);
            Vector3 pos = new Vector3(area.Translate.X, area.Translate.Y, area.Translate.Z);
            Vector3 rot = new Vector3(area.Rotate.X, area.Rotate.Y, area.Rotate.Z);

            Matrix4 pointTransform = InitalTransform * Matrix4Extension.CreateTransform(pos, rot, scale);
            for (int i = 0; i < 4; i++)
                cube[i] = Vector3.TransformPosition(cube[i], pointTransform);

            return cube;
        }

        public object Clone()
        {
            return new ClipArea()
            {
                AreaShape = this.AreaShape,
                AreaType = this.AreaType,
                Prm1 = this.Prm1,
                Prm2 = this.Prm2,
                Rotate = this.Rotate,
                Scale = this.Scale,
                Translate = this.Translate,
            };
        }
    }
}
