using Toolbox.Core;

namespace TurboLibrary
{
    /// <summary>
    /// Represents the camera movements and cuts triggered by drivers in the replay video.
    /// </summary>
    [ByamlObject]
    public class ReplayCamera : SpatialObject
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        [ByamlMember("Camera_Path", Optional = true)]
        private int? _pathIndex;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets an unknown camera type.
        /// </summary>
        [ByamlMember("CameraType")]
        [BindGUI("Camera Type", Category = "Properties")]
        public CameraMode Type { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Path"/> this camera moves along.
        /// </summary>
        public Path Path { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the blur effect for far-away geometry.
        /// </summary>
        [ByamlMember]
        [BindGUI("DepthOfField", Category = "Properties")]
        public int DepthOfField { get; set; }

        /// <summary>
        /// Gets or sets the distance of the camera to the driver.
        /// </summary>
        [ByamlMember]
        [BindGUI("Driver Distance", Category = "Properties")]
        public int Distance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to lock the view target onto the driver who triggered the camera.
        /// </summary>
        [ByamlMember]
        [BindGUI("Follow Driver", Category = "Properties")]
        public bool Follow { get; set; }

        /// <summary>
        /// Gets or sets the rotation around the X axis.
        /// </summary>
        [ByamlMember]
        [BindGUI("Pitch", Category = "Rotation")]
        public int Pitch { get; set; }

        /// <summary>
        /// Gets or sets the rotation around the Y axis.
        /// </summary>
        [ByamlMember]
        [BindGUI("Yaw", Category = "Properties")]
        public int Yaw { get; set; }

        /// <summary>
        /// Gets or sets the rotation around the Z axis.
        /// </summary>
        [ByamlMember]
        [BindGUI("Roll", Category = "Rotation")]
        public int Roll { get; set; }

        /// <summary>
        /// Gets or sets an unknown angle on the X axis.
        /// </summary>
        [ByamlMember]
        [BindGUI("Angle X", Category = "Rotation")]
        public int AngleX { get; set; }

        /// <summary>
        /// Gets or sets an unknown angle on the Y axis.
        /// </summary>
        [ByamlMember]
        [BindGUI("Angle Y", Category = "Rotation")]
        public int AngleY { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field of view angle is computed in accordance to the distance to
        /// the driver who triggered the camera.
        /// </summary>
        [ByamlMember]
        [BindGUI("Auto Fovy", Category = "Field Of View")]
        public bool AutoFovy { get; set; }

        /// <summary>
        /// Gets or sets the field of view angle possibly at the start of the move.
        /// </summary>
        [ByamlMember]
        [BindGUI("Start Fovy", Category = "Field Of View")]
        public int Fovy { get; set; }

        /// <summary>
        /// Gets or sets the field of view angle possibly at the end of the move.
        /// </summary>
        [ByamlMember]
        [BindGUI("End Fovy", Category = "Field Of View")]
        public int Fovy2 { get; set; }

        /// <summary>
        /// Gets or sets a speed possibly controlling how the FoV change is done.
        /// </summary>
        [ByamlMember]
        [BindGUI("Fovy Speed", Category = "Field Of View")]
        public int FovySpeed { get; set; }

        /// <summary>
        /// Gets or sets the group this camera belongs to.
        /// </summary>
        [ByamlMember]
        [BindGUI("Group", Category = "Properties")]
        public int Group { get; set; }

        /// <summary>
        /// Gets or sets the first parameter.
        /// </summary>
        [ByamlMember("prm1")]
        [BindGUI("Param 1", Category = "Params")]
        public int Prm1 { get; set; }

        /// <summary>
        /// Gets or sets the second parameter.
        /// </summary>
        [ByamlMember("prm2")]
        [BindGUI("Param 2", Category = "Params")]
        public int Prm2 { get; set; }

        public enum CameraMode
        {
            OnlineSpectator_FixedSearch = 0, //Online only?
            OnlineSpectator_PathSearch = 1, //Online only? These are often seen with normal lines to move around on.
            KartFixedSearch = 2, //Fixed point attached to kart 
            KartPathSearch = 3, //Generally these are at the center 0 0 0 as they follow the player around.
        }

        public ReplayCamera()
        {
            Translate = new ByamlVector3F(0, 0, 0);
            Scale = new ByamlVector3F(1, 1, 1);
            Fovy = 45;
            Fovy2 = 55;
            AngleX = 0;
            AngleY = 0;
            Type = CameraMode.KartFixedSearch;
            FovySpeed = 10;
            AutoFovy = false;
            Path = null;
            Group = 0;
            Distance = 0;
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Allows references between BYAML instances to be resolved to provide real instances
        /// instead of the raw values in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public void DeserializeReferences(CourseDefinition courseDefinition)
        {
            Path = (_pathIndex == null || _pathIndex == -1 || _pathIndex >= courseDefinition.Paths?.Count) ? null : courseDefinition.Paths[_pathIndex.Value];
            if (Path != null) Path.References.Add(this);
        }

        /// <summary>
        /// Allows references between BYAML instances to be serialized into raw values stored in the
        /// BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public void SerializeReferences(CourseDefinition courseDefinition)
        {
            _pathIndex = Path == null ? null : (int?)courseDefinition.Paths.IndexOf(Path);
        }

        public override string ToString() => "Replay Camera";
    }
}
