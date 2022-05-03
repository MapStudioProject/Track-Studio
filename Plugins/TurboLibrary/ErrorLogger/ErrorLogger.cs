using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KclLibrary;
using OpenTK;
using MapStudio.UI;
using Toolbox.Core;

namespace TurboLibrary
{
    public class ErrorLogger
    {
        public static void CheckCourseErrors(CourseDefinition courseDefinition)
        {
            var map = new MapFieldAccessor();
            map.Setup(courseDefinition);
            /*
                        for (int i = 0; i < courseDefinition.EnemyPaths?.Count; i++) {
                            for (int j = 0; j < courseDefinition.EnemyPaths[i].Points.Count; j++) {
                                var point = courseDefinition.EnemyPaths[i].Points[j];

                                if (!map.LapPaths.IsPointInPath(point.Translate.X, point.Translate.Y, point.Translate.Z))
                                    StudioLogger.WriteError(string.Format(TranslationSource.GetText("ENEMY_ERROR"), i, j));
                            }
                        }*/
            CheckEnemyPathErrors(courseDefinition);
            CheckLapPathErrors(courseDefinition);
            CheckGliderErrors(courseDefinition);
            CheckObjectErrors(courseDefinition.Objs);
        }

        static void CheckEnemyPathErrors(CourseDefinition courseDefinition)
        {
            for (int i = 0; i < courseDefinition.EnemyPaths?.Count; i++)
            {
                foreach (var point in courseDefinition.EnemyPaths[i].Points)
                {
                    foreach (var nextPt in point.NextPoints)
                    {
                        //A child is connected to the parent in a self loop
                        //if (nextPt.NextPoints.Contains(point))
                         //   StudioLogger.WriteError($"Enemy point self connection error at group {i} point {point.Index}!");
                    }
                }
            }
        }

        static void CheckLapPathErrors(CourseDefinition courseDefinition)
        {
            if (courseDefinition.EnemyPaths?.Count == 0 && courseDefinition.LapPaths?.Count > 0 ||
                courseDefinition.EnemyPaths?.Count > 0 && courseDefinition.LapPaths?.Count == 0)
                StudioLogger.WriteError(string.Format(TranslationSource.GetText("LAPPATH_AI_ERROR"), 8));
        }

        static void CheckGliderErrors(CourseDefinition courseDefinition)
        {
            //The game uses only 8 glider path which uses the last path instead when more are added.
            if (courseDefinition.GlidePaths?.Count > 8)
                StudioLogger.WriteError(string.Format(TranslationSource.GetText("GLIDE_MAX_ERROR"), 8));

            for (int i = 0; i < courseDefinition.GlidePaths?.Count; i++) {
                var path = courseDefinition.GlidePaths[i];
                var isCannon = path.GlideType == GlidePath.GliderType.Cannon;

                for (int j = 0; j < courseDefinition.GlidePaths[i].Points.Count; j++) {
                    var point = courseDefinition.GlidePaths[i].Points[j];

                    if (j == 0 && isCannon  && !point.Cannon)
                        StudioLogger.WriteError(string.Format(TranslationSource.GetText("GLIDE_CANNON_ERROR"), i, j));

                    if (j == path.Points.Count - 1 && isCannon && point.Cannon)
                        StudioLogger.WriteError(string.Format(TranslationSource.GetText("GLIDE_CANNON_END_ERROR"), i, j));
                }
            }
        }

        static void CheckObjectErrors(List<Obj> objects)
        {
            if (!objects.Any(x => x.ObjId == 6003))
                StudioLogger.WriteWarning(TranslationSource.GetText("START_WARNING"));
            if (!objects.Any(x => x.IsSkybox))
                StudioLogger.WriteWarning(TranslationSource.GetText("SKYBOX_WARNING"));

            int index = 0;
            foreach (var obj in objects)
            {
                if (!GlobalSettings.ObjectList.ContainsKey(obj.ObjId))
                    continue;

                string name = GlobalSettings.ObjectList[obj.ObjId];
                string objectInfo = $"Object({index}) {name}";
                bool isObjPath = obj.ObjPath != null || (obj.Path != null);

                if (isObjPath && obj.Speed == 0)
                    StudioLogger.WriteWarning(string.Format(TranslationSource.GetText("SPEED_WARNING"), objectInfo));

                if (GlobalSettings.ObjDatabase.ContainsKey(obj.ObjId))
                {
                    ObjDefinition objDef = GlobalSettings.ObjDatabase[obj.ObjId];
                    if ((int)objDef.PathType == 3 && !isObjPath)
                        StudioLogger.WriteError(string.Format(TranslationSource.GetText("LINK_ERROR"), objectInfo));
                }
                index++;
            }
        }

        static void CheckCollisionErrors(KCLFile kclFile)
        {
            int numGlderWalls = 0;
            foreach (var model in kclFile.Models) {
                foreach (var prisim in model.Prisms) {
                    var tri = model.GetTriangle(prisim);
                    bool isWall = isWallPolygonAngle(new Vector3(tri.Normal.X, tri.Normal.Y, tri.Normal.Z));

                    //Check if prism is a wall for glider types in the current renderer
                    if (tri.Attribute == 31 && isWall) {
                        numGlderWalls++;
                    }
                }
                if (numGlderWalls > 0)
                    StudioLogger.WriteWarning(string.Format(TranslationSource.GetText("COLLIDE_WARNING"), numGlderWalls));
            }
        }

        static bool isWallPolygonAngle(Vector3 normal) {
            return isWallPolygonAngle(Vector3.Dot(normal, new Vector3(0, 1, 0)));
        }

        static bool isWallPolygonAngle(float v) {
            // 70 degrees -- Math.cos(70*Math.PI/180)
            return Math.Abs(v) < 0.3420201433256688;
        }
    }
}
