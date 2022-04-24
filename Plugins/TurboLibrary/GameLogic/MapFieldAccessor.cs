using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurboLibrary.GameLogic;

namespace TurboLibrary
{
    public class MapFieldAccessor
    {
        public static MapFieldAccessor Instance;

        public int ClipIndex = -1;

        public GravityCamPathAccessor GravityPaths = new GravityCamPathAccessor();
        public LapPathAccessor LapPaths = new LapPathAccessor();
        public ClipAreaAccessor ClipAreaAccessor = new ClipAreaAccessor();
        public CourseDefinition CourseDefinition;

        public void Setup(CourseDefinition course)
        {
            Instance = this;

            CourseDefinition = course;
            LapPaths.Setup(course.LapPaths);
            GravityPaths.Setup(course.GravityPaths);
            ClipAreaAccessor.SetupClip(course.ClipPattern, course.ClipAreas);
        }
    }
}
