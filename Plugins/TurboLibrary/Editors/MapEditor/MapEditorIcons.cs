using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using Toolbox.Core.ViewModels;

namespace TurboLibrary.MuuntEditor
{
    public class MapEditorIcons
    {
        public const char OBJECT_ICON = '\uf1b3';
        public const char AREA_BOX = '\uf1b2';
        public const char EFFECT_AREA = '\uf1b2';
        public const char CLIP_AREA = '\uf1b2';

        public const char POINT_ICON = '\uf55b';
        public const char CUBE_POINT_ICON = '\uf6d1';
        public const char REPLAY_CAMERA_ICON = '\uf008';

        public static Vector4 AREA_BOX_COLOR = new Vector4(0, 1, 0, 1);
        public static Vector4 EFFECT_AREA_COLOR = new Vector4(0, 0.88f, 0.96f, 1);
        public static Vector4 CLIP_AREA_COLOR = new Vector4(0.7f, 0.7f, 0.7f, 1);
        public static Vector4 LAP_PATH_COLOR = new Vector4(0.3f, 0.57f, 1, 1);
        public static Vector4 GRAVITY_PATH_COLOR = new Vector4(0.59f, 0.34f, 0.56f, 1);
        public static Vector4 GCAMERA_PATH_COLOR = new Vector4(0.88f, 0.43f, 1, 1);
        public static Vector4 ENEMY_PATH_COLOR = new Vector4(0.97f, 0.21f, 0, 1);
        public static Vector4 ITEM_PATH_COLOR = new Vector4(0.1f, 0.76f, 0, 1);
        public static Vector4 STEER_ASSIT_PATH_COLOR = new Vector4(0.5f, 0.5f, 0.5f, 1);
        public static Vector4 GLIDER_PATH_COLOR = new Vector4(0.95f, 0.95f, 0, 1);

        public static Vector4 PULL_PATH_COLOR = new Vector4(0.594f, 0.39f, 0.51f, 1);
        public static Vector4 RAIL_PATH_COLOR = new Vector4(0f, 0.65f, 0.8f, 1);
        public static Vector4 OBJ_PATH_COLOR = new Vector4(0.1f, 0.1f, 0.1f, 1);
        public static Vector4 JUGEM_PATH_COLOR = new Vector4(0.9f, 1, 0.5f, 1);

        public static void ReloadIcons(NodeBase groupNode, Type type)
        {
            groupNode.Icon = MapEditorIcons.POINT_ICON.ToString();

            if (type == typeof(LapPath))
            {
                groupNode.Icon = MapEditorIcons.CUBE_POINT_ICON.ToString();
                groupNode.IconColor = MapEditorIcons.LAP_PATH_COLOR;
            }
            else if (type == typeof(GravityPath))
            {
                groupNode.Icon = MapEditorIcons.CUBE_POINT_ICON.ToString();
                groupNode.IconColor = MapEditorIcons.GRAVITY_PATH_COLOR;
            }
            else if (type == typeof(GCameraPath))
            {
                groupNode.Icon = MapEditorIcons.CUBE_POINT_ICON.ToString();
                groupNode.IconColor = MapEditorIcons.GCAMERA_PATH_COLOR;
            }
            else if (type == typeof(EnemyPath))
                groupNode.IconColor = MapEditorIcons.ENEMY_PATH_COLOR;
            else if (type == typeof(ItemPath))
                groupNode.IconColor = MapEditorIcons.ITEM_PATH_COLOR;
            else if (type == typeof(GlidePath))
                groupNode.IconColor = MapEditorIcons.GLIDER_PATH_COLOR;
            else if (type == typeof(SteerAssistPath))
                groupNode.IconColor = MapEditorIcons.STEER_ASSIT_PATH_COLOR;
            else if (type == typeof(PullPath))
                groupNode.IconColor = MapEditorIcons.PULL_PATH_COLOR;
            else if (type == typeof(Path))
                groupNode.IconColor = MapEditorIcons.RAIL_PATH_COLOR;
            else if (type == typeof(ObjPath))
                groupNode.IconColor = MapEditorIcons.OBJ_PATH_COLOR;
            else if (type == typeof(JugemPath))
                groupNode.IconColor = MapEditorIcons.JUGEM_PATH_COLOR;
        }
    }
}
