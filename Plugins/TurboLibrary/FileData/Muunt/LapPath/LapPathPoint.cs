using System.Collections.Generic;
using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a point of a <see cref="LapPath"/>.
    /// </summary>
    [ByamlObject]
    public class LapPathPoint : PathPointBase<LapPath, LapPathPoint>
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value possibly indicating whether this is a required point to complete a lap.
        /// </summary>
        [ByamlMember]
        [BindGUI("Check Point", Category = "Properties")]
        public int CheckPoint { get; set; }

        /// <summary>
        /// Gets or sets a value possibly indicating whether this point increases the lap count.
        /// </summary>
        [ByamlMember]
        [BindGUI("Lap Check", Category = "Properties")]
        public int LapCheck { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether headlights are turned on on this part of the path when
        /// <see cref="CourseDefinition.HeadLight"/> is set to <see cref="CourseHeadLight.ByLapPath"/>.
        /// </summary>
        [ByamlMember]
        [BindGUI("Activate Headlights", Category = "Properties")]
        public bool HeadLightSW { get; set; }

        /// <summary>
        /// Gets or sets the field of view angle of the camera at this part of the path.
        /// </summary>
        [ByamlMember]
        [BindGUI("Map Camera Fovy", Category = "Properties")]
        public int MapCameraFovy { get; set; }

        /// <summary>
        /// Gets or sets the height distance of the camera at this part of the path.
        /// </summary>
        [ByamlMember]
        [BindGUI("Map Camera Y", Category = "Properties")]
        public int MapCameraY { get; set; }

        /// <summary>
        /// Gets or sets a value handling a <see cref="ReturnPoint"/> or -1 if there is no return point.
        /// </summary>
        [ByamlMember]
        [BindGUI("Return Position Index", Category = "Properties")]
        public int ReturnPosition { get; set; }

        /// <summary>
        /// Gets or sets an index to a sound effect played at this part of the path or -1 if there is no additional
        /// effect.
        /// </summary>
        [ByamlMember]
        [BindGUI("Sound SW Index", Category = "Properties")]
        public int SoundSW { get; set; }

        /// <summary>
        /// Gets or sets a possible index into a <see cref="Clip"/> instance.
        /// </summary>
        [ByamlMember("ClipIdx", Optional = true)]
        [BindGUI("Clip Index", Category = "Culling")]
        public int? ClipIndex { get; set; }

        /// <summary>
        /// Gets or sets a possible index into a <see cref="Clip"/> instance. DLC courses use this index, stored in
        /// &quot;ClipNum&quot; instead of &quot;ClipIdx&quot;.
        /// </summary>
        [ByamlMember("ClipNum", Optional = true)]
        [BindGUI("Clip Index Extra", Category = "Culling")]
        public int? ClipIndexDlc { get; set; }

        [BindGUI("Return Point", Category = "ReturnPoint")]
        public ReturnPoint ReturnPoint { get; set; }

        public LapPathPoint() : base()
        {
            this.HeadLightSW = false;
            this.ClipIndex = -1;
            this.ClipIndexDlc = -1;
            this.CheckPoint = -1;
            this.ReturnPosition = -1;
            this.MapCameraFovy = 65;
            this.MapCameraY = 320;
            this.SoundSW = -1;
            this.LapCheck = -1;
            this.Scale = new ByamlVector3F(1000, 1000, 0);
            this.Translate = new ByamlVector3F(0, 0, 0);
            this.Rotate = new ByamlVector3F(0, 0, 0);
            ReturnPoint = new ReturnPoint();
        }

        public LapPathPoint Clone()
        {
            return new LapPathPoint()
            {
                HeadLightSW = this.HeadLightSW,
                ClipIndex = this.ClipIndex,
                ClipIndexDlc = this.ClipIndexDlc,
                CheckPoint = this.CheckPoint,
                LapCheck = this.LapCheck,
                SoundSW = this.SoundSW,
                MapCameraFovy = this.MapCameraFovy,
                MapCameraY = this.MapCameraY,
                Scale = this.Scale,
                Translate  = this.Translate,
                Rotate = this.Rotate,
            };
        }

        // ---- METHODS (PROTECTED) ------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the array of paths in the <see cref="CourseDefinition"/> which can be referenced by previous and
        /// next points.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> to get the paths from.</param>
        /// <returns>The array of paths which can be referenced.</returns>
        protected override IList<LapPath> GetPathReferenceList(CourseDefinition courseDefinition)
        {
            return courseDefinition.LapPaths;
        }

        /// <summary>
        /// Allows references of course data objects to be resolved to provide real instances instead of the raw values
        /// in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public override void DeserializeReferences(CourseDefinition courseDefinition)
        {
            base.DeserializeReferences(courseDefinition);
            // TODO: Find out what ClipIndex references exactly and resolve the reference.
        }

        /// <summary>
        /// Allows references between course objects to be serialized into raw values stored in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public override void SerializeReferences(CourseDefinition courseDefinition)
        {
            base.SerializeReferences(courseDefinition);
            // TODO: Find out what ClipIndex references exactly and resolve the reference.
        }

        public void CheckErrors(CourseDefinition course)
        {
        }

        public override string ToString()
        {
            string output = "";
            if (CheckPoint != -1)
                output += $"Required Pass #{ CheckPoint} ";
            if (LapCheck != -1)
                output += $"Lap Gate #{LapCheck} ";
            return output;
        }
    }
}
