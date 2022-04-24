using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using GLFrameworkEngine;

namespace TurboLibrary
{
    public class ClipAreaAccessor
    {
        List<ClipArea> _clipAreas;

        /// <summary>
        /// The bitflags used for storing clip areas.
        /// </summary>
        public List<long> BitFlags = new List<long>();

        /// <summary>
        /// Gets the clip areas.
        /// </summary>
        public List<ClipArea> GetClipAreas() => _clipAreas;

        public void SetupClip(ClipPattern clipPattern, List<ClipArea> clipAreas)
        {
            if (clipPattern == null)
                return;

            _clipAreas = clipAreas;

            for (int i = 0; i < clipPattern.AreaFlag.Count; i++)
            {
                //Store 4 ushorts into a single 8 bit flag.
                //Each bit representing an area in that clip.
                List<byte> flags = new List<byte>();
                for (int j = 0; j < 4; j++)
                {
                    if (clipPattern.AreaFlag[i].Count == 0)
                    {
                        for (int f = 0; f < 4; f++)
                            clipPattern.AreaFlag[i].Add(0);
                    }

                    int areaFlag = clipPattern.AreaFlag[i][j];
                    flags.AddRange(BitConverter.GetBytes((ushort)areaFlag));
                }
                BitFlags.Add(BitConverter.ToInt64(flags.ToArray(), 0));
            }
        }
    }

    public class ClipSubMeshCulling
    {
        /// <summary>
        /// Determines if the given bounding box is inside the active clip region.
        /// If the box is inside the region then it will be culled/unloaded.
        /// </summary>
        public static bool IsInside(BoundingBox bounding)
        {
            var course = MapFieldAccessor.Instance.CourseDefinition;
            //No clip present so skip
            if (course.ClipPattern == null)
                return false;

            //Search for clips that the bounding is inside of and check if the clip is currently culled or not.
            for (int i = 0; i < course.ClipPattern.AreaFlag.Count; i++)
            {
                if (MapFieldAccessor.Instance.ClipIndex == i && isInside(course, bounding, course.ClipPattern.AreaFlag[i]))
                    return true;
            }
            return false;
        }

        static bool isInside(CourseDefinition course, BoundingBox bounding, Clip clip)
        {
            foreach (var area in clip.Areas)
            {
                //Area removed from editor. Skip
                if (area == null || !course.ClipAreas.Contains(area))
                    continue;

                //Check if mesh box is inside the clip box
                var clipBox = area.GetBoundingBox();
                if (clipBox.IsInside(bounding))
                    return true;
            }
            return false;
        }
    }
}
