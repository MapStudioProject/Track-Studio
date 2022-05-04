using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MapStudio.UI;
using UIFramework;
using CLMS;
using ImGuiNET;

namespace TurboLibrary
{
    public class CourseUIEditor : Window
    {
        CourseIconTool8U CourseIconToolU;
        CourseNameToolBase CourseNameTool;

        public override string Name => "Track UI";
        public override ImGuiWindowFlags Flags => ImGuiWindowFlags.NoDocking;

        internal static string currentTrack = "";

        List<string> TrackListFull = new List<string>();

        public CourseUIEditor(string trackName)
        {
            if (GlobalSettings.IsMK8D)
                CourseNameTool = new CourseNameTool8D();
            else
                CourseNameTool = new CourseNameTool8U();

            Size = new System.Numerics.Vector2(400, 700);

            TrackListFull.AddRange(TrackList);
            TrackListFull.AddRange(TrackListRetro);
            TrackListFull.AddRange(TrackListDLC);
            if (GlobalSettings.IsMK8D)
            {
                TrackListFull.AddRange(TrackListBattleDLC);
            }

            if (TrackListFull.Contains(trackName))
            {
                currentTrack = trackName;
                UpdateTrack();
            }
        }

        public override void Render()
        {
            if (!loaded) {
                loaded = true;
                if (!GlobalSettings.IsMK8D)
                {
                    CourseIconToolU = new CourseIconTool8U();
                    CourseIconToolU.Load();
                    UpdateTrack();
                }
            }

            if (ImguiCustomWidgets.PathSelector("Output Save Path", ref PluginConfig.MK8ModPath)) {
                //Save settings to disc
                PluginConfig.Instance.Save();
            }

            var size = ImGui.GetWindowSize();
            if (ImGui.Button("Save", new System.Numerics.Vector2(size.X, 30)))
            {
                Save();
            }

            ImguiCustomWidgets.ComboScrollable("Track", currentTrack, ref currentTrack, TrackListFull, () =>
            {
                UpdateTrack();
            });

            if (CourseIconToolU != null)
                CourseIconToolU.Render();

            if (CourseNameTool != null)
                CourseNameTool.Render();
        }

        private void Save()
        {
            if (CourseNameTool != null)
                CourseNameTool.SaveMessage();
            if (CourseIconToolU != null)
                CourseIconToolU.Save();
        }

        private void UpdateTrack()
        {
            if (CourseNameTool != null)
                CourseNameTool.UpdateTrack();
            if (CourseIconToolU != null)
                CourseIconToolU.UpdateCourseIcon(currentTrack);
        }

        internal static List<string> TrackList = new List<string>()
        {
            "Gu_FirstCircuit",
            "Gu_WaterPark",
            "Gu_Cake",
            "Gu_DossunIseki",

            "Gu_City",
            "Gu_MarioCircuit",
            "Gu_Techno",
            "Gu_HorrorHouse",

            "Gu_Airport",
            "Gu_Ocean",
            "Gu_Expert",
            "Gu_SnowMountain",

            "Gu_Desert",
            "Gu_Cloud",
            "Gu_BowserCastle",
            "Gu_RainbowRoad",
        };

        internal static List<string> TrackListRetro = new List<string>()
        {
            "Gwii_MooMooMeadows",
            "Gagb_MarioCircuit",
            "Gds_PukupukuBeach",
            "G3ds_PackunSlider",

            "G64_KinopioHighway",
            "Gsfc_DonutsPlain3",
            "Ggc_DryDryDesert",
            "G3ds_DKJungle",

            "Ggc_SherbetLand",
            "Gds_TickTockClock",
            "G64_PeachCircuit",
            "G3ds_MusicPark",

            "G64_YoshiValley",
            "Gds_WarioStadium",
            "Gwii_GrumbleVolcano",
            "G64_RainbowRoad",
        };

        internal static List<string> TrackListDLC = new List<string>()
        {
            "Dwii_WariosMine",
            "Du_ExciteBike",
            "Du_DragonRoad",
            "Du_MuteCity",

            "Dgc_YoshiCircuit",
            "Dsfc_RainbowRoad",
            "Du_IcePark",
            "Du_Hyrule",

            "Dagb_CheeseLand",
            "Dgc_BabyPark",
            "Du_Woods",
            "Du_Animal",

            "D3ds_NeoBowserCity",
            "Dagb_RibbonRoad",
            "Du_Metro",
            "Du_BigBlue",
        };

        internal static List<string> TrackListBattleDLC = new List<string>()
        {
             "B3ds_WuhuTown",
             "Bgc_LuigiMansion",
             "Bsfc_Battle1",
             "Bu_DekaLine",
             "Bu_Moon",
             "Bu_BattleStadium",
             "Bu_Dojo",
             "Bu_Sweets",
        };

    }
}
