using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a camera move played in the introductionary course video played at the start of offline races.
    /// </summary>
    [ByamlObject]
    public class IntroCamera : SpatialObject, ICourseReferencable
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        [ByamlMember("Camera_Path")]
        private int _pathIndex;

        [ByamlMember("Camera_AtPath")]
        private int _lookAtPathIndex { get; set; }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the index of the camera in the intro camera array.
        /// </summary>
        [ByamlMember("CameraNum")]
        [BindGUI("Camera Number", Category = "Intro Camera")]
        public int Num { get; set; }

        /// <summary>
        /// Gets or sets the number of frames the camera is active.
        /// </summary>
        [ByamlMember("CameraTime")]
        [BindGUI("Time", Category = "Intro Camera")]
        public int Time { get; set; }

        /// <summary>
        /// Gets or sets an unknown camera type.
        /// </summary>
        [ByamlMember("CameraType")]
        [BindGUI("CameraType", Category = "Intro Camera")]
        public CameraType Type { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Path"/> on which this camera looks at.
        /// </summary>
        public Path LookAtPath { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Path"/> on which this camera moves along.
        /// </summary>
        public Path Path { get; set; }

        /// <summary>
        /// Gets or sets the field of view angle possibly at the start of the move.
        /// </summary>
        [ByamlMember]
        [BindGUI("Start Fovy", Category = "Intro Camera")]
        public int Fovy { get; set; }

        /// <summary>
        /// Gets or sets the field of view angle possibly at the end of the move.
        /// </summary>
        [ByamlMember]
        [BindGUI("End Fovy", Category = "Intro Camera")]
        public int Fovy2 { get; set; }

        /// <summary>
        /// Gets or sets a speed possibly controlling how the FoV change is done.
        /// </summary>
        [ByamlMember]
        [BindGUI("Fovy Speed", Category = "Intro Camera")]
        public int FovySpeed { get; set; }

        public static IntroCamera CreateFirst()  { return new IntroCamera(1, 244, 40, 55, 4); }
        public static IntroCamera CreateSecond() { return new IntroCamera(2, 254, 55, 70, 6); }
        public static IntroCamera CreateThird()  { return new IntroCamera(3, 178, 55, 47, 5); }

        public IntroCamera()
        {
            this.Type = CameraType.PathMoveLookAt;
            this.Scale = new ByamlVector3F(1, 1, 1);
        }

        public IntroCamera(int num, int time, int fovStart, int fovEnd, int fovSpeed)
        {
            this.Num = num;
            this.Time = time;
            this.Fovy = fovStart;
            this.Fovy2 = fovEnd;
            this.FovySpeed = fovSpeed;
            this.Type = CameraType.PathMoveLookAt;
            this.Scale = new ByamlVector3F(1, 1, 1);
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Allows references of course file objects to be resolved to provide real instances instead of the raw values
        /// in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public void DeserializeReferences(CourseDefinition courseDefinition)
        {
            if (_pathIndex != -1)
                Path = courseDefinition.Paths[_pathIndex];
            if (_lookAtPathIndex != -1)
                LookAtPath = courseDefinition.Paths[_lookAtPathIndex];

            if (Path != null) Path.References.Add(this);
            if (LookAtPath != null) LookAtPath.References.Add(this);
        }

        /// <summary>
        /// Allows references between course objects to be serialized into raw values stored in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public void SerializeReferences(CourseDefinition courseDefinition)
        {
            _pathIndex = courseDefinition.Paths.IndexOf(Path);
            _lookAtPathIndex = courseDefinition.Paths.IndexOf(LookAtPath);
        }

        public override string ToString() => $"Intro Camera #{Num}";

        public enum CameraType
        {
            PointFixedPathLookAt = 5,
            PathMoveLookAt = 6,
        }
    }
}
