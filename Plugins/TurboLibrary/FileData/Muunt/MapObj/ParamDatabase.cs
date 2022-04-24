using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboLibrary
{
    public class ParamDatabase
    {
        public static Dictionary<int, float[]> ParameterDefaults = new Dictionary<int, float[]>()
        {
            { 1040, new float[8] { 1, 0, 0, 0, 0, 0, 0, 0 }},  // TowerKuribo
            { 1036, new float[8] { 1, 0, 0, 0, 0, 0, 0, 0 }},  // PcBalloon
        };

        public static Dictionary<int, string[]> ParameterObjs = new Dictionary<int, string[]>()
        {
        {    1003, new string[8] {null, null, null, null, null, null, null, null} },  // Choropoo
        {    1004, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // Frogoon
        {    1005, new string[8] {null, null, null, null, null, null, null, null} },  // PylonB
        {    1006, new string[8] {"Initial Delay", "Life Time", "Delay", "Unknown 4", null, null, null, null} },  // PackunMusic
        {    1007, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // KuriboBoard
        {    1008, new string[8] {null, null, null, null, null, null, null, null} },  // DKBarrel
        {    1009, new string[8] {null, null, null, null, null, null, null, null} },  // PylonY
        {    1010, new string[8] {null, null, null, null, null, null, null, null} },  // DdQuicksand
        {    1011, new string[8] { null, "Lap Point Search Range", null, null, null, null, null, null} },  // Kuribo
        {    1012, new string[8] {"Inital Delay", "Wait Cycle", null, null, null, null, null, null} },  // Sanbo
        {    1013, new string[8] { "Is Double (MK8D)", null, null, null, null, null, null, null} },  // ItemBox
        {    1014, new string[8] {"Initial Delay", "Slam Delay", "Unknown 3", null, null, null, null, null} },  // Dossun
        {    1015, new string[8] {null, null, null, null, null, null, null, null} },  // Crab
        {    1016, new string[8] {null, null, null, null, null, null, null, null} },  // SnowRock
        {    1017, new string[8] {null, null, null, null, null, null, null, null} },  // BushBoard
        {    1018, new string[8] {"Unknown 1", null, null, "Unknown 4", null, null, null, null} },  // Coin
        {    1019, new string[8] {null, null, null, null, null, null, null, null} },  // Dokan1
        {    1021, new string[8] { "Initial Delay", "Spawn Interval", "Spawn Count", "Start Distance", "Space Between", null, null, null} },  // Basabasa
        {    1022, new string[8] {null, null, null, null, null, null, null, null} },  // Barrel
        {    1023, new string[8] {null, null, null, null, null, null, null, null} },  // CrashBox
        {    1024, new string[8] {null, null, null, null, null, null, null, null} },  // PylonR
        {    1025, new string[8] {"Inital Delay", "Unknown 2", "Spawn Count", "Start Distance", "Space Between", null, null, null} },  // Pukupuku
        {    1026, new string[8] {null, null, null, null, null, null, null, null} },  // TikiTak
        {    1027, new string[8] {"First Wait Frame", "Angle (Degrees)", null, null, null, null, null, null} },  // PackunFlower
        {    1028, new string[8] {null, null, null, null, null, null, null, null} },  // PuchiPackun
        {    1029, new string[8] {null, null, null, null, null, null, null, null} },  // SkateHeyhoR
        {    1030, new string[8] {null, null, null, null, null, null, null, null} },  // Note
        {    1031, new string[8] {null, null, "Spawn Count", "Unknown 4", "Unknown 5", null, null, null} },  // SkateHeyhoB
        {    1032, new string[8] {null, null, null, null, null, null, null, null} },  // SnowMan
        {    1033, new string[8] {null, "Unknown 2", null, null, null, null, null, null} },  // MovingCoin
        {    1034, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // MovingItemBox
        {    1035, new string[8] {null, null, null, null, null, null, null, null} },  // Oil
        {    1036, new string[8] {"Spawn Count", "Recolor Start Index", null, null, null, null, null, null} },  // PcBalloon
        {    1037, new string[8] {null, null, null, null, null, null, null, null} },  // N64YoshiEgg
        {    1039, new string[8] { "Min Return Lap Point", "Max Return Lap Point", null, null, null, null, null, null} },  // Bird
        {    1040, new string[8] {"Spawn Count", "Unknown 2", null, null, null, null, null, null} },  // TowerKuribo
        {    1041, new string[8] { "Unknown {3/4 seen} },", null, null, null, null, null, null, null} },  // Cow
        {    1042, new string[8] {null, null, "Unknown 3", "Unknown 4", "Unknown 5", null, null, null} },  // FishBone
        {    1043, new string[8] {"Unknown 1", "Spawn Count", "Unknown 3", null, null, null, null, null} },  // Teresa
        {    1044, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // CmnToad
        {    1055, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // RelayCar
        {    1056, new string[8] {null, null, null, null, null, null, null, null} },  // Seagull
        {    1057, new string[8] {null, null, null, null, null, null, null, null} },  // ExTram
        {    1058, new string[8] {null, null, null, null, null, null, null, null} },  // GingerBread
        {    1059, new string[8] {null, null, null, null, null, null, null, null} },  // CakePylonB
        {    1060, new string[8] {null, null, null, null, null, null, null, null} },  // CakePylonA
        {    1063, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // ShyguyWatchman
        {    1064, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // ShyguyPickax
        {    1066, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // PackunFlower2
        {    1067, new string[8] {"Initial Delay", null, null, null, null, null, null, null} },  // HhStatue
        {    1068, new string[8] {null, null, null, null, null, null, null, null} },  // CakeBalloon
        {    1070, new string[8] {null, null, null, null, null, null, null, null} },  // Karon
        {    1071, new string[8] {null, null, null, null, null, null, null, null} },  // PylonTechno
        {    1072, new string[8] {null, null, null, null, null, null, null, null} },  // DokanTechno
        {    1073, new string[8] {null, null, null, null, null, null, null, null} },  // PackunCake
        {    1074, new string[8] {null, "Unknown 2", null, null, null, null, null, null} },  // APMoveCBox
        {    1075, new string[8] {null, null, null, null, null, null, null, null} },  // Moray
        {    1076, new string[8] {null, null, null, null, null, null, null, null} },  // DokanCake
        {    1077, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // MareM
        {    1078, new string[8] {null, null, null, null, null, null, null, null} },  // BarrelFlower
        {    1079, new string[8] {null, null, null, null, null, null, null, null} },  // Helicopter
        {    1081, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, "Unknown 5", null, null, null} },  // SnowBoardHeyho
        {    1083, new string[8] {null, null, null, null, null, null, null, null} },  // CmnHeyho
        {    1084, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // FireSnake
        {    1085, new string[8] {null, null, null, null, null, null, null, null} },  // CmnBirdNest
        {    1086, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // MonteM
        {    1087, new string[8] {null, null, null, null, null, null, null, null} },  // CmnYoshi
        {    1088, new string[8] {null, null, null, null, null, null, null, null} },  // OcManta
        {    1090, new string[8] {null, null, null, null, null, null, null, null} },  // OcLifton
        {    1091, new string[8] {null, null, null, null, null, null, null, null} },  // SnorkelToad
        {    1092, new string[8] {null, null, null, null, null, null, null, null} },  // OcLightJellyfish
        {    1093, new string[8] {null, null, null, null, null, null, null, null} },  // CmnHawk
        {    1094, new string[8] {null, null, null, null, null, null, null, null} },  // FcClown
        {    1095, new string[8] {null, null, "Unknown 3", null, "Unknown 5", null, null, null} },  // PukupukuMecha
        {    1097, new string[8] {null, null, null, null, null, null, null, null} },  // CmnPatapataUD
        {    1098, new string[8] {null, null, null, null, null, null, null, null} },  // CmnPatapataLR
        {    1099, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // CmnGroupToad
        {    1100, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // NoteChild
        {    1101, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // PackunHone
        {    1103, new string[8] {null, null, null, null, null, null, null, null} },  // KoopaClaw
        {    1104, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // PackunTechno
        {    1105, new string[8] {null, null, null, null, null, null, null, null} },  // DokanHone
        {    1106, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // PitToadB
        {    1107, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // PitToadY
        {    1108, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // PitToadG
        {    1109, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // PitToadP
        {    1110, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // PitToadR
        {    1111, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // PitToadW
        {    1112, new string[8] {null, null, null, null, null, null, null, null} },  // DKBanana
        {    1114, new string[8] {null, null, null, null, null, null, null, null} },  // JungleBird
        {    1115, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // PitToadWro
        {    1117, new string[8] {null, null, null, null, null, null, null, null} },  // ShyguyRope
        {    1118, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // CmnNokonoko
        {    1119, new string[8] {"Road Index {0/1} },", "Initial Delay", "Slam Delay", null, null, null, null, null} },  // CrWanwanB
        {    1120, new string[8] {null, null, null, null, null, null, null, null} },  // CmnBros
        {    1123, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // TcHeyho
        {    1124, new string[8] {null, null, null, null, null, null, null, null} },  // SmToad
        {    1125, new string[8] {null, null, null, null, null, null, null, null} },  // SmHeyho
        {    1126, new string[8] {null, null, null, null, null, null, null, null} },  // SmYoshi
        {    1127, new string[8] {"Unknown 1", null, "Unknown 3", null, "Unknown 5", null, null, null} },  // JumpPukupuku
        {    1130, new string[8] {null, null, null, null, null, null, null, null} },  // CmnCow
        {    1131, new string[8] {null, "Unknown 2", null, null, null, null, null, null} },  // SpaceToad
        {    1132, new string[8] {null, null, null, "Unknown 4", null, null, null, null} },  // PackunTechno_NoAt
        {    1134, new string[8] {null, null, null, null, null, null, null, null} },  // CmnKaron
        {    1135, new string[8] {null, "Unknown 2", "Spawn Count", null, "Spawn Radius", null, null, null} },  // JumpPukuClip
        {    1136, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_StarDossun
        {    1137, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_CmnAnimalA
        {    1138, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_CmnAnimalB
        {    1139, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_CmnAnimalC
        {    1140, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_CmnAnimalD
        {    1144, new string[8] {null, null, null, null, null, null, null, null} },  // DL_CmnSoldier
        {    1145, new string[8] {null, null, null, null, null, null, null, null} },  // DL_IceToad
        {    1147, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", "Unknown 5", null, null, null} },  // DL_Keith
        {    1148, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // DL_Dekubaba
        {    1149, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MCAirship
        {    1150, new string[8] {null, null, null, null, null, null, null, null} },  // DL_ShyguyPickaxR
        {    1151, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // DL_YcHelicopter
        {    1152, new string[8] {null, null, null, null, null, null, null, null} },  // DL_IceHelicopter
        {    1153, new string[8] {null, null, null, null, null, null, null, null} },  // DL_WoPulleyShyguy
        {    1154, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // DL_Reset
        {    1156, new string[8] {null, null, null, null, null, null, null, null} },  // DL_NkClown
        {    1157, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // DL_NkClownW
        {    1158, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalTotakeke
        {    1159, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalRisa
        {    1160, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalFuta
        {    1161, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalAsami
        {    1162, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalKinuyo
        {    1163, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalTanukichi
        {    1164, new string[8] {null, "Unknown 2", null, null, null, null, null, null} },  // DL_MovingItemBoxDLC
        {    1165, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MechaKoopa
        {    1166, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_Wanwan
        {    1167, new string[8] {null, "Unknown 2", null, null, null, null, null, null} },  // DL_WdMovingCoin
        {    1168, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalKaizo
        {    1169, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalSnowman
        {    1170, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // DL_WdBird
        {    1171, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_SurpriseBox
        {    1172, new string[8] {null, null, null, null, null, null, null, null} },  // DL_RibbonToad
        {    1173, new string[8] {null, null, null, null, null, null, null, null} },  // DL_ItemBoxMetro
        {    1174, new string[8] {null, null, null, null, null, null, null, null} },  // DL_WdBarrel
        {    1175, new string[8] {null, null, null, null, null, null, null, null} },  // DL_WdBarrelR
        {    2004, new string[8] {null, null, null, null, null, null, null, null} },  // ChimneySmoke
        {    2006, new string[8] {null, null, null, null, null, null, null, null} },  // Fountain
        {    2007, new string[8] {"Initial Delay", "Life Time", "Unknown 3", null, null, null, null, null} },  // VolFlame
        {    2009, new string[8] {"Initial Delay", "Life Time", null, null, null, null, null, null} },  // VolBomb
        {    2010, new string[8] {null, null, null, null, null, null, null, null} },  // Flyingbug
        {    2011, new string[8] {null, null, null, null, null, null, null, null} },  // ButterflyB
        {    2012, new string[8] {null, null, null, null, null, null, null, null} },  // ButterflyA
        {    2013, new string[8] {null, null, null, null, null, null, null, null} },  // ButterflySp
        {    2014, new string[8] {null, null, null, null, null, null, null, null} },  // WPFountain
        {    2015, new string[8] {"Counter Clockwise {0/1}", "Initial Rotation", null, null, null, null, null, null} },  // WsFirebar
        {    2016, new string[8] { "Counter Clockwise {0/1}", "Initial Rotation", null, null, null, null, null, null} },  // WsFirering
        {    2017, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", "Unknown 5", null, null, null} },  // CmnWaterCurrent
        {    2018, new string[8] {null, null, null, null, null, null, null, null} },  // Balloon
        {    2021, new string[8] {null, null, null, null, null, null, null, null} },  // CmnCloud
        {    2022, new string[8] {null, null, null, null, null, null, null, null} },  // Candle
        {    2023, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, "Model Index {Boost plate} },"} },  // ClThunder
        {    2024, new string[8] {null, null, null, null, null, null, null, null} },  // DiTorchInside
        {    2025, new string[8] {null, null, null, null, null, null, null, null} },  // BDTorch
        {    2026, new string[8] {null, null, null, null, null, null, null, null} },  // VolTorch
        {    2027, new string[8] {null, null, null, null, null, null, null, null} },  // DiTorchOutside
        {    2028, new string[8] {null, null, null, "Unknown 4", null, null, null, null} },  // SandFallSmoke
        {    2030, new string[8] {null, null, null, null, null, null, null, null} },  // TCSearchLight
        {    2031, new string[8] {null, null, null, null, null, null, null, null} },  // SeaLight
        {    2032, new string[8] {null, null, null, null, null, null, null, null} },  // PsSplashA
        {    2033, new string[8] {null, null, null, null, null, null, null, null} },  // CakeSplash
        {    2034, new string[8] {null, null, null, null, null, null, null, null} },  // CakeFountain
        {    2035, new string[8] {null, null, null, null, null, null, null, null} },  // SunLight
        {    2036, new string[8] {null, null, null, null, null, null, null, null} },  // PowderSugar
        {    2037, new string[8] {null, null, null, null, null, null, null, null} },  // CakeBubble
        {    2039, new string[8] {null, null, null, null, null, null, null, null} },  // FcSearchLight
        {    2040, new string[8] {null, null, null, null, null, null, null, null} },  // OcManyFish
        {    2041, new string[8] {null, null, null, null, null, null, null, null} },  // CakeSplashB
        {    2042, new string[8] {null, null, null, null, null, null, null, null} },  // CakeFountainB
        {    2043, new string[8] {null, null, null, null, null, null, null, null} },  // CakeBottleBubble
        {    2045, new string[8] {null, null, null, null, null, null, null, null} },  // TCFountain
        {    2046, new string[8] {null, null, null, null, null, null, null, null} },  // DiFluff
        {    2047, new string[8] {null, null, null, null, null, null, null, null} },  // DiWaterfall
        {    2048, new string[8] {null, null, null, null, null, null, null, null} },  // DdWaterCurent
        {    2050, new string[8] {null, null, null, null, null, null, null, null} },  // BDSunLight
        {    2051, new string[8] {null, null, null, null, null, null, null, null} },  // VolTorchL
        {    2052, new string[8] {null, null, null, null, null, null, null, null} },  // TCSplashSet1
        {    2053, new string[8] {null, null, null, null, null, null, null, null} },  // TCSplashSet2
        {    2054, new string[8] {null, null, null, null, null, null, null, null} },  // TCFireworks
        {    2055, new string[8] {null, null, null, null, null, null, null, null} },  // TTCSunLight
        {    2056, new string[8] {null, null, null, null, null, null, null, null} },  // DkTorch
        {    2057, new string[8] {"Initial Delay", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // LaserBeam
        {    2059, new string[8] {null, null, null, null, null, null, null, null} },  // BCTorch1
        {    2060, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // BCExplosion
        {    2061, new string[8] {null, null, null, null, null, null, null, null} },  // BCSearchLight
        {    2062, new string[8] {null, null, null, null, null, null, null, null} },  // FireworksFc
        {    2063, new string[8] {null, null, null, null, null, null, null, null} },  // FireworksSl
        {    2064, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // FireworksN64R
        {    2065, new string[8] {null, null, null, null, null, null, null, null} },  // TTCSunLightS
        {    2066, new string[8] {null, null, null, null, null, null, null, null} },  // McSplash
        {    2067, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // OcWaterCurrent
        {    2068, new string[8] {null, null, null, null, null, null, null, null} },  // VolSmoke
        {    2069, new string[8] {null, null, null, null, null, null, null, null} },  // FireworksN64Rs
        {    2070, new string[8] {null, null, null, null, null, null, null, null} },  // DdQuickSandSplash
        {    2071, new string[8] {null, null, null, null, null, null, null, null} },  // EntranceSunLight
        {    2072, new string[8] {null, null, null, null, null, null, null, null} },  // LibrarySunLight
        {    2073, new string[8] {null, null, null, null, null, null, null, null} },  // CorridorSunLight
        {    2074, new string[8] {null, null, null, null, null, null, null, null} },  // AnnexeSunLight
        {    2077, new string[8] {null, null, null, null, null, null, null, null} },  // DpManyFish
        {    2078, new string[8] {null, null, null, null, null, null, null, null} },  // SlAurora
        {    2079, new string[8] {null, null, null, null, null, null, null, null} },  // FcSearchLightClip
        {    2081, new string[8] {null, null, null, null, null, null, null, null} },  // FcSearchLightOutside
        {    2082, new string[8] {null, null, null, null, null, null, null, null} },  // BCTorch2
        {    2083, new string[8] {null, null, null, null, null, null, null, null} },  // DkSunLight
        {    2084, new string[8] {null, null, null, null, null, null, null, null} },  // DL_Triforce
        {    2085, new string[8] {null, null, null, null, null, null, null, null} },  // DL_HySmoke
        {    2086, new string[8] {null, null, null, null, null, null, null, null} },  // DL_RainbowMountainA
        {    2087, new string[8] {null, null, null, null, null, null, null, null} },  // DL_RainbowMountainB
        {    2088, new string[8] {null, null, null, null, null, null, null, null} },  // DL_HyTorch
        {    2089, new string[8] {null, null, null, null, null, null, null, null} },  // DL_McStartLogo
        {    2090, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // DL_YoshiWaterCurrent
        {    2091, new string[8] {null, null, null, null, null, null, null, null} },  // DL_DrTorch
        {    2092, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_HySalute
        {    2093, new string[8] {null, null, null, null, null, null, null, null} },  // DL_HyHouseSmoke
        {    2094, new string[8] {null, null, null, null, null, null, null, null} },  // DL_WdChimneySmoke
        {    2096, new string[8] {null, null, null, null, null, null, null, null} },  // DL_NkThunder
        {    2097, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BbStartLogoA
        {    2098, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BbStartLogoB
        {    2099, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalFountain
        {    2100, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalSmoke
        {    3002, new string[8] { "Initial Delay", "Unknown 2", "Launch Velocity", "Launch Angle (Degrees)", null, null, null, null} },  // Rock1
        {    3004, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // CarA
        {    3006, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // TruckA
        {    3007, new string[8] {null, null, null, null, null, null, null, null} },  // DkAirship
        {    3008, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // Bus
        {    3009, new string[8] {null, null, null, null, null, null, null, null} },  // Trolley
        {    3011, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // CarrierCar
        {    3012, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // BDSandShip
        {    3013, new string[8] {null, null, null, null, null, null, null, null} },  // Submarine
        {    3014, new string[8] {null, null, null, null, null, null, null, null} },  // TrolleyNoMove
        {    3015, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // APJetFly
        {    3016, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // DiWheel
        {    3018, new string[8] {"Unknown 1", null, "Unknown 3", null, null, null, null, null} },  // Chairlift
        {    3020, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // CarSurf
        {    3021, new string[8] {null, null, null, null, null, null, null, null} },  // N64RTrain
        {    3022, new string[8] {null, null, null, null, null, null, null, null} },  // BDSandShipNoMove
        {    3023, new string[8] {null, null, null, null, null, null, null, null} },  // GessoShuttle
        {    3024, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_WarioTram
        {    3025, new string[8] {null, null, null, null, null, null, null, null} },  // DL_WarioTramB
        {    3026, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", "Unknown 5", "Unknown 6", "Unknown 7", "Unknown 8"} },  // DL_Metro
        {    3027, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalTrain
        {    4004, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // ClockGearY
        {    4005, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // ClockHandL
        {    4006, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // ClockGearZ
        {    4007, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // Furiko
        {    4036, new string[8] {null, null, null, null, null, null, null, null} },  // CityBoat
        {    4038, new string[8] {null, null, null, null, null, null, null, null} },  // HorrorRoad
        {    4039, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // ClockHandS
        {    4040, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // ClockGearPole
        {    4042, new string[8] {"Fall Delay", null, null, null, null, null, null, null} },  // KaraPillar
        {    4043, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // MpBoard
        {    4044, new string[8] {null, null, null, null, null, null, null, null} },  // ExDash
        {    4048, new string[8] {"Unknown 1", "Unknown 2", "Delay between rise", "Unknown 4", null, null, null, null} },  // BDSandGeyser
        {    4050, new string[8] {"Unknown 1", "Unknown 2", "Shake speed", "Unknown 4", "Unknown 5", "Unknown 6", null, null} },  // VolMovRoadPlus
        {    4051, new string[8] {null, null, "Shake speed", "Unknown 4", "Unknown 5", "Unknown 6", null, null} },  // VolMovRoad
        {    4052, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, "Model Index"} },  // VolcanoPiece
        {    4055, new string[8] {null, null, null, null, null, null, null, null} },  // DiDomino
        {    4060, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", "Unknown 5", null, null, null} },  // DkScreamPillar
        {    4061, new string[8] {null, null, null, null, null, null, null, null} },  // SmDash
        {    4065, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // ClockHandL2
        {    4066, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // ClockHandS2
        {    4068, new string[8] {null, null, null, null, null, null, null, null} },  // KoopaRoadR
        {    4069, new string[8] {null, null, null, null, null, null, null, null} },  // KoopaRoadL
        {    4070, new string[8] {null, null, null, null, null, null, null, null} },  // RRroadout
        {    4071, new string[8] {null, null, null, null, null, null, null, null} },  // RRroadin
        {    4072, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // TtcBoard
        {    4074, new string[8] {null, null, null, null, null, null, null, null} },  // SpikeBall
        {    4075, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // ClockGearArrow
        {    4077, new string[8] {null, null, null, null, null, null, null, null} },  // N64RRoad1
        {    4078, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // N64RRoad2
        {    4081, new string[8] {null, null, null, null, null, null, null, null} },  // DonutsRoadA
        {    4082, new string[8] {null, null, null, null, null, null, null, null} },  // DonutsRoadB
        {    4084, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // BCCannon
        {    4085, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // RRdash
        {    4086, new string[8] {null, null, null, null, null, null, null, "ID"} },  // DL_EbField
        {    4087, new string[8] {"Pattern //", null, null, null, null, null, null, null} },  // DL_EbDirt
        {    4088, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // DL_LotusLeaf
        {    4089, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_AnimalBalloon
        {    4090, new string[8] {null, null, null, null, null, null, null, null} },  // DL_EbDirtBig
        {    4091, new string[8] {null, null, null, null, null, null, null, null} },  // DL_HySwitch
        {    4092, new string[8] {null, null, null, null, null, null, null, null} },  // DL_HyJump
        {    4095, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", null, null, null, null, null} },  // DL_Iceberg
        {    4096, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_RainbowArrow
        {    4097, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MasterSword
        {    4098, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MCGoalLine
        {    4099, new string[8] {null, null, null, null, null, null, null, null} },  // DL_SfcRRoad1
        {    4100, new string[8] {null, null, null, null, null, null, null, null} },  // DL_SfcRRoad2
        {    4101, new string[8] {null, null, null, null, null, null, null, null} },  // DL_SfcRRoad3
        {    4102, new string[8] {null, null, null, null, null, null, null, null} },  // DL_SfcRRoad4
        {    4103, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MCDashSet
        {    4104, new string[8] {null, null, null, null, null, null, null, null} },  // DL_RibbonRoad1
        {    4106, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_TicketGate
        {    4107, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MetroTollBar
        {    4108, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MetroDash
        {    4109, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BbDash
        {    4110, new string[8] {null, null, null, null, null, null, null, null} },  // DL_FallenLeaf
        {    4111, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalShell
        {    4112, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_RibbonBox
        {    4113, new string[8] {null, null, null, null, null, null, null, null} },  // DL_RibbonRoad2
        {    4114, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalApple
        {    4115, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalOrange
        {    4116, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalLemon
        {    5002, new string[8] {null, null, null, null, null, null, null, null} },  // BumpingFlower
        {    5005, new string[8] {null, null, null, null, null, null, null, null} },  // WindMill
        {    5014, new string[8] {null, null, null, null, null, null, null, null} },  // TreeAgb
        {    5015, new string[8] {null, null, null, null, null, null, null, null} },  // TreeSnow
        {    5016, new string[8] {null, null, null, null, null, null, null, null} },  // TreeSph
        {    5017, new string[8] {null, null, null, null, null, null, null, null} },  // Tree64A
        {    5019, new string[8] {null, null, null, null, null, null, null, null} },  // TreeTri
        {    5020, new string[8] {null, null, null, null, null, null, null, null} },  // Tree64Deep
        {    5021, new string[8] {null, null, null, null, null, null, null, null} },  // WaterSurface
        {    5022, new string[8] {null, null, null, null, null, null, null, null} },  // WaterBox
        {    5023, new string[8] {null, null, null, null, null, null, null, null} },  // FlagStartDossun
        {    5024, new string[8] {null, null, null, null, null, null, null, null} },  // FlagStartMario
        {    5025, new string[8] {null, null, null, null, null, null, null, null} },  // FlagStartMooMoo
        {    5026, new string[8] {null, null, null, null, null, null, null, null} },  // FlagStartAGB
        {    5027, new string[8] {null, null, null, null, null, null, null, null} },  // YachtG
        {    5028, new string[8] {null, null, null, null, null, null, null, null} },  // YachtP
        {    5029, new string[8] {null, null, null, null, null, null, null, null} },  // YachtR
        {    5030, new string[8] {null, null, null, null, null, null, null, null} },  // YachtY
        {    5031, new string[8] {null, null, null, null, null, null, null, null} },  // HhDoor
        {    5033, new string[8] {null, null, null, null, null, null, null, null} },  // HhChandelier
        {    5034, new string[8] {null, null, null, null, null, null, null, null} },  // HhMovingWall
        {    5041, new string[8] {null, null, null, null, null, null, null, null} },  // RotaryBoard
        {    5042, new string[8] {null, null, null, null, null, null, null, null} },  // FlagRope1
        {    5044, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTriangle1
        {    5047, new string[8] {null, null, null, null, null, null, null, null} },  // TreeCakeB
        {    5049, new string[8] {null, null, null, null, null, null, null, null} },  // WindMillCake
        {    5051, new string[8] {null, null, null, null, null, null, null, null} },  // MpTrumpet
        {    5052, new string[8] {null, null, null, null, null, null, null, null} },  // MpTambourin
        {    5053, new string[8] {null, null, null, null, null, null, null, null} },  // MpSax
        {    5054, new string[8] {null, null, null, null, null, null, null, null} },  // MpSpeaker
        {    5056, new string[8] {null, null, null, null, null, null, null, null} },  // MpCymbal
        {    5057, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // MpPiston
        {    5058, new string[8] {null, null, null, null, null, null, null, null} },  // SearchLight
        {    5059, new string[8] {null, null, null, null, null, null, null, null} },  // TollBar
        {    5060, new string[8] {null, null, null, null, null, null, null, null} },  // TreeCakeD
        {    5062, new string[8] {null, null, null, null, null, null, null, null} },  // HhFan
        {    5063, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTriangle2
        {    5064, new string[8] {null, null, null, null, null, null, null, null} },  // ExExcavator
        {    5070, new string[8] {null, null, null, null, null, null, null, null} },  // ClGun
        {    5071, new string[8] {null, null, null, null, null, null, null, null} },  // ClBattleShip
        {    5072, new string[8] {null, null, null, null, null, null, null, null} },  // Tree64Yoshi
        {    5075, new string[8] {null, null, null, null, null, null, null, null} },  // BDSearchLight
        {    5076, new string[8] {null, null, null, null, null, null, null, null} },  // BDFlagSquare1
        {    5077, new string[8] {null, null, null, null, null, null, null, null} },  // BDWindmill
        {    5078, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTapestryDossun
        {    5079, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTapestryPeach
        {    5080, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTapestryMusic
        {    5081, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTriangle3
        {    5082, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // TcMirrorBall
        {    5083, new string[8] {null, null, null, null, null, null, null, null} },  // Kanransya
        {    5084, new string[8] {"// Cups in Circle", null, null, null, null, null, null, null} },  // CoffeeCup
        {    5085, new string[8] {null, null, null, null, null, null, null, null} },  // TreeSph_AddCol
        {    5088, new string[8] {null, null, null, null, null, null, null, null} },  // TreeTriB
        {    5089, new string[8] {null, null, null, null, null, null, null, null} },  // TreeBush
        {    5090, new string[8] {null, null, null, null, null, null, null, null} },  // TreeCityA
        {    5091, new string[8] {null, null, null, null, null, null, null, null} },  // APGuide
        {    5093, new string[8] {null, null, null, null, null, null, null, null} },  // TreeCakeBB
        {    5094, new string[8] {null, null, null, null, null, null, null, null} },  // TreeCakeBY
        {    5095, new string[8] {null, null, null, null, null, null, null, null} },  // APCBelt
        {    5096, new string[8] {null, null, null, null, null, null, null, null} },  // APSBelt
        {    5097, new string[8] {null, null, null, null, null, null, null, null} },  // APTollBar
        {    5098, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTriangle4
        {    5103, new string[8] {null, null, null, null, null, null, null, null} },  // WPTollBar
        {    5104, new string[8] {null, null, null, null, null, null, null, null} },  // McGate
        {    5106, new string[8] {null, null, null, null, null, null, null, null} },  // DrumInside
        {    5107, new string[8] {null, null, null, null, null, null, null, null} },  // FlagStadiumA
        {    5108, new string[8] {null, null, null, null, null, null, null, null} },  // FlagStadiumB
        {    5109, new string[8] {null, null, null, null, null, null, null, null} },  // FlagStadiumC
        {    5110, new string[8] {null, null, null, null, null, null, null, null} },  // ClTrampoline
        {    5111, new string[8] {null, null, null, null, null, null, null, null} },  // SmHelicopter
        {    5112, new string[8] {null, null, null, null, null, null, null, "Unknown 8"} },  // FcGallery
        {    5114, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // AccelRing
        {    5115, new string[8] {null, null, null, null, null, null, null, null} },  // RacingPole
        {    5116, new string[8] {null, null, null, null, null, null, null, "Unknown 8"} },  // TechnoStepGreen
        {    5117, new string[8] {null, null, null, null, null, null, null, "Unknown 8"} },  // TechnoStepRed
        {    5118, new string[8] {null, null, null, null, null, null, null, null} },  // BarrelCannon
        {    5119, new string[8] {null, null, null, null, null, null, null, null} },  // OcRing
        {    5120, new string[8] {null, null, null, null, null, null, null, null} },  // APPropeller
        {    5121, new string[8] {null, null, null, null, null, null, null, null} },  // FlagSquareAP
        {    5122, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTriangle5
        {    5124, new string[8] {null, null, null, null, null, null, null, null} },  // ClBattleShipS
        {    5125, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTapestryFc
        {    5130, new string[8] {null, null, null, null, null, null, null, null} },  // CakeCannon
        {    5131, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTapestryWp
        {    5132, new string[8] {null, null, null, null, null, null, null, null} },  // GuardrailRSpot
        {    5133, new string[8] {null, null, null, null, null, null, null, null} },  // ExExcavatorBig
        {    5134, new string[8] {null, null, null, null, null, null, null, null} },  // ClPropeller
        {    5136, new string[8] {null, null, null, null, null, null, null, null} },  // BDRope
        {    5137, new string[8] {null, null, null, null, null, null, null, null} },  // BDCloth
        {    5138, new string[8] {null, null, null, null, null, null, null, null} },  // GearDecoA
        {    5139, new string[8] {null, null, null, null, null, null, null, null} },  // GearDecoB
        {    5140, new string[8] {null, null, null, null, null, null, null, null} },  // GearDecoC
        {    5141, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // TcStarSpeaker
        {    5142, new string[8] {null, null, null, null, null, null, null, null} },  // TcSpeaker
        {    5145, new string[8] {null, null, null, null, null, null, null, null} },  // GearDecoD
        {    5146, new string[8] {null, null, null, null, null, null, null, null} },  // GearDecoE
        {    5147, new string[8] {null, null, null, null, null, null, null, null} },  // TcDisplay
        {    5155, new string[8] {null, null, null, null, null, null, null, null} },  // GearDecoF
        {    5156, new string[8] {null, null, null, null, null, null, null, null} },  // GearDecoG
        {    5157, new string[8] {null, null, null, null, null, null, null, null} },  // ClockSpring
        {    5161, new string[8] {null, null, null, null, null, null, null, null} },  // TcDisplayR
        {    5162, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // TcSoundRoadG
        {    5163, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // TcSoundRoadR
        {    5165, new string[8] {null, null, null, null, null, null, null, null} },  // TreeSnow_AddCol
        {    5166, new string[8] {null, null, null, null, null, null, null, null} },  // TreeCityACol
        {    5167, new string[8] {null, null, null, null, null, null, null, null} },  // FlagStadiumWarioC
        {    5168, new string[8] {null, null, null, null, null, null, null, null} },  // FlagStadiumWarioA
        {    5169, new string[8] {null, null, null, null, null, null, null, null} },  // FlagStadiumWarioB
        {    5170, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTapestryWs
        {    5171, new string[8] {null, null, null, null, null, null, null, null} },  // SlRopeL
        {    5172, new string[8] {null, null, null, null, null, null, null, null} },  // SlRopeM
        {    5173, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTapestrySl
        {    5174, new string[8] {null, null, null, null, null, null, null, null} },  // GravityBox
        {    5175, new string[8] {null, null, null, null, null, null, null, null} },  // FlagStartSnow
        {    5176, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // N64RAccelRing
        {    5177, new string[8] {null, null, null, null, null, null, null, null} },  // N64RCheck
        {    5178, new string[8] {null, null, null, null, null, null, null, null} },  // N64RStart
        {    5179, new string[8] {null, null, null, null, null, null, null, null} },  // SwanBoatR
        {    5180, new string[8] {null, null, null, null, null, null, null, "Tire Stack Count" } },  // Tire
        {    5181, new string[8] {null, null, null, null, null, null, null, null} },  // SwanBoatB
        {    5182, new string[8] {null, null, null, null, null, null, null, null} },  // WaterPlantA
        {    5183, new string[8] {null, null, null, null, null, null, null, null} },  // WaterPlantB
        {    5184, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTapestryPs
        {    5185, new string[8] {null, null, null, null, null, null, null, null} },  // WPBoatA
        {    5186, new string[8] {null, null, null, null, null, null, null, null} },  // WPBoatB
        {    5187, new string[8] {null, null, null, null, null, null, null, null} },  // WPBoatC
        {    5188, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // N64RAccelRingAir
        {    5189, new string[8] {null, null, null, null, null, null, null, null} },  // N64RStageSet
        {    5190, new string[8] {null, null, null, null, null, null, null, null} },  // UltraArm
        {    5192, new string[8] {null, null, null, null, null, null, null, null} },  // ClockBell
        {    5193, new string[8] {null, null, null, null, null, null, null, null} },  // Tree64Middle
        {    5194, new string[8] {null, null, null, null, null, null, null, null} },  // Tree64Light
        {    5195, new string[8] {null, null, null, null, null, null, null, null} },  // FlagPeachCircuit
        {    5196, new string[8] {null, null, null, null, null, null, null, null} },  // MilkTank
        {    5198, new string[8] {null, null, null, null, null, null, null, null} },  // StrawRoll
        {    5200, new string[8] {null, null, null, null, null, null, null, null} },  // ClockArm
        {    5201, new string[8] {null, null, null, null, null, null, null, null} },  // ClockCylinder
        {    5202, new string[8] {null, null, null, null, null, null, null, null} },  // BCGate
        {    5203, new string[8] {null, null, null, null, null, null, null, null} },  // BCElevator
        {    5204, new string[8] {null, null, null, null, null, null, null, null} },  // BCEngine
        {    5205, new string[8] {null, null, null, null, null, null, null, null} },  // BCFlag
        {    5207, new string[8] {null, null, null, null, null, null, null, null} },  // BCChain
        {    5210, new string[8] {null, null, null, null, null, null, null, null} },  // TreeDD
        {    5217, new string[8] {null, null, null, null, null, null, null, null} },  // TreeDonuts
        {    5218, new string[8] {null, null, null, null, null, null, null, null} },  // Tree64DeepCol
        {    5219, new string[8] {null, null, null, null, null, null, null, null} },  // Tree64MiddleCol
        {    5220, new string[8] {null, null, null, null, null, null, null, null} },  // Tree64LightCol
        {    5221, new string[8] {null, null, null, null, null, null, null, null} },  // TreePukupuku
        {    5222, new string[8] {null, null, null, null, null, null, null, null} },  // ConnectBoard
        {    5223, new string[8] {null, null, null, null, null, null, null, null} },  // SlRedSpot
        {    5224, new string[8] {null, null, null, null, null, null, null, null} },  // BDFlagSquare2
        {    5225, new string[8] {null, null, null, null, null, null, null, null} },  // FlagStartPukupuku
        {    5227, new string[8] {null, null, null, null, null, null, null, null} },  // SpinTurboBar_BD
        {    5228, new string[8] {null, null, null, null, null, null, null, null} },  // FlagSquare1
        {    5229, new string[8] {null, null, null, null, null, null, null, null} },  // WsGate
        {    5230, new string[8] {null, null, null, null, null, null, null, null} },  // PeachBell
        {    5231, new string[8] {null, null, null, null, null, null, null, null} },  // SpinTurboBar_TC
        {    5232, new string[8] {null, null, null, null, null, null, null, null} },  // SpinTurboBar_AP
        {    5234, new string[8] {null, null, null, null, null, null, null, null} },  // OceanWaterPlantA
        {    5235, new string[8] {null, null, null, null, null, null, null, null} },  // OceanWaterPlantB
        {    5236, new string[8] {null, null, null, null, null, null, null, null} },  // RRstationA
        {    5237, new string[8] {null, null, null, null, null, null, null, null} },  // RRstationB
        {    5238, new string[8] {null, null, null, null, null, null, null, null} },  // RRstationC
        {    5239, new string[8] {null, null, null, null, null, null, null, null} },  // RRguide
        {    5240, new string[8] {null, null, null, null, null, null, null, null} },  // RRring
        {    5241, new string[8] {null, null, null, null, null, null, null, null} },  // RRseat
        {    5242, new string[8] {null, null, null, null, null, null, null, null} },  // WindMillSmall
        {    5244, new string[8] {null, null, null, null, null, null, null, "Tire Stack Count"} },  // TireR
        {    5245, new string[8] {null, null, null, null, null, null, null, "Tire Stack Count" } },  // TireW
        {    5246, new string[8] {null, null, null, null, null, null, null, null} },  // PsFan
        {    5247, new string[8] {null, null, null, null, null, null, null, null} },  // TcMoveLight
        {    5248, new string[8] {null, null, null, null, null, null, null, null} },  // Weathercock
        {    5249, new string[8] {null, null, null, null, null, null, null, null} },  // RRstartring
        {    5250, new string[8] {null, null, null, null, null, null, null, null} },  // TcChandelier
        {    5251, new string[8] {null, null, null, null, null, null, null, null} },  // FlagMusicSwing
        {    5252, new string[8] {null, null, null, null, null, null, null, null} },  // SherbetPlantA
        {    5253, new string[8] {null, null, null, null, null, null, null, null} },  // SherbetPlantB
        {    5254, new string[8] {null, null, null, null, null, null, null, null} },  // FlagTriangle3Y
        {    5255, new string[8] {null, null, null, null, null, null, null, null} },  // N64RStageSet2
        {    5256, new string[8] {null, null, null, null, null, null, null, null} },  // N64RStar
        {    5257, new string[8] {null, null, null, null, null, null, null, null} },  // TcChandelierS
        {    5258, new string[8] {null, null, null, null, null, null, null, null} },  // TcChandelierM
        {    5259, new string[8] {null, null, null, null, null, null, null, null} },  // WindMillB
        {    5261, new string[8] {null, null, null, null, null, null, null, null} },  // SpinTurboBar_RR
        {    5262, new string[8] {null, null, null, null, null, null, null, null} },  // CmnDashBoard
        {    5263, new string[8] {null, null, null, null, null, null, null, null} },  // McJump
        {    5265, new string[8] {null, null, null, null, null, null, null, null} },  // MmJump
        {    5266, new string[8] {null, null, null, null, null, null, null, null} },  // KhJump
        {    5267, new string[8] {null, null, null, null, null, null, null, null} },  // SmFan
        {    5268, new string[8] {null, null, null, null, null, null, null, null} },  // BCParts
        {    5269, new string[8] {null, null, null, null, null, null, null, null} },  // ClGuide
        {    5270, new string[8] {null, null, null, null, null, null, null, null} },  // BCPiston
        {    5271, new string[8] {null, null, null, null, null, null, null, null} },  // BCBattleShipS
        {    5272, new string[8] {null, null, null, null, null, null, null, null} },  // SearchLightNoClip
        {    5273, new string[8] {null, null, null, null, null, null, null, null} },  // RRbox
        {    5274, new string[8] {null, null, null, null, null, null, null, null} },  // McKart
        {    5275, new string[8] {null, null, null, null, null, null, null, null} },  // TreeSphB
        {    5276, new string[8] {null, null, null, null, null, null, null, null} },  // CiJump
        {    5277, new string[8] {null, null, null, null, null, null, null, null} },  // TreeSphB_AddCol
        {    5278, new string[8] {null, null, null, null, null, null, null, null} },  // Holography
        {    5279, new string[8] {"Unknown 1", null, "Unknown 3", null, null, null, null, null} },  // ChairliftNoMove
        {    5280, new string[8] {null, null, null, null, null, null, null, null} },  // DL_TreeTri
        {    5281, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_TreeApple
        {    5282, new string[8] {null, null, null, null, null, null, null, null} },  // DL_SpinTurboBar_DR
        {    5283, new string[8] {null, null, null, null, null, null, null, null} },  // DL_SpinTurboBar_MC
        {    5284, new string[8] {null, null, null, null, null, null, null, null} },  // DL_WgExcavator
        {    5285, new string[8] {null, null, null, null, null, null, null, null} },  // DL_WgGear
        {    5286, new string[8] {null, null, null, null, null, null, null, null} },  // DL_WgGearBig
        {    5287, new string[8] {null, null, null, null, null, null, null, null} },  // DL_FlagStartIce
        {    5289, new string[8] {null, null, null, null, null, null, null, null} },  // DL_EbGridGate
        {    5290, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_TreeOrange
        {    5291, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_TreeLemon
        {    5292, new string[8] {null, null, null, null, null, null, null, null} },  // DL_TreeSphYoshi
        {    5293, new string[8] {null, null, null, null, null, null, null, null} },  // DL_FlagTriangle6
        {    5294, new string[8] {null, null, null, null, null, null, null, null} },  // DL_WgFlagSquare
        {    5295, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_TreeAnimal
        {    5296, new string[8] {null, null, null, null, null, null, null, null} },  // DL_FlagStadiumEB
        {    5297, new string[8] {null, null, null, null, null, null, null, null} },  // DL_TreePineA
        {    5298, new string[8] {null, null, null, null, null, null, null, null} },  // DL_TreePineB
        {    5299, new string[8] {null, null, null, null, null, null, null, null} },  // DL_FlagDragon
        {    5300, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MCRotaryBoardA
        {    5301, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MCBldgParts
        {    5302, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MCCars
        {    5303, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MCTriangleBoard
        {    5304, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MCPipeRing
        {    5305, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MCJets
        {    5306, new string[8] {null, null, null, null, null, null, null, null} },  // DL_HyFlagSquare
        {    5307, new string[8] {null, null, null, null, null, null, null, null} },  // DL_IceFlagSquare
        {    5308, new string[8] {null, null, null, null, null, null, null, null} },  // DL_YcYacht
        {    5309, new string[8] {null, null, null, null, null, null, null, null} },  // DL_SpinTurboBar_IP
        {    5310, new string[8] {null, null, null, null, null, null, null, null} },  // DL_WeathercockYoshi
        {    5311, new string[8] {null, null, null, null, null, null, null, null} },  // DL_FlagStartYoshi
        {    5312, new string[8] {null, null, null, null, null, null, null, null} },  // DL_FlagRopeYoshi
        {    5313, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MasterSwordBase
        {    5314, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MCRotaryBoardB
        {    5315, new string[8] {null, null, null, null, null, null, null, null} },  // DL_MCRotaryBoardC
        {    5316, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", "Unknown 5", "Unknown 6", null, null} },  // DL_WoElevator
        {    5317, new string[8] {null, null, null, null, null, null, null, null} },  // DL_WoWaterWheel
        {    5318, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BbAirship
        {    5319, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BbCars
        {    5320, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BbJets
        {    5321, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BpSwingride
        {    5322, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BpPirate
        {    5323, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BpParatroopa
        {    5324, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BpWingA
        {    5325, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BpWingB
        {    5326, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BpGear
        {    5327, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_CheeseBox
        {    5328, new string[8] {null, null, null, null, null, null, null, null} },  // DL_TreePalm
        {    5329, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_CoinStone
        {    5330, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BpKanransya
        {    5331, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BpCoaster
        {    5332, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_TreeAutumnApple
        {    5333, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_TreeAutumnLemon
        {    5334, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_TreeAutumnOrange
        {    5335, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_TreeWinter
        {    5336, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_TreeWinterApple
        {    5337, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_TreeWinterLemon
        {    5338, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_TreeWinterOrange
        {    5339, new string[8] {null, null, null, null, null, null, null, null} },  // DL_TreeSakura
        {    5340, new string[8] {null, null, null, null, null, null, null, null} },  // DL_TreeBushAutumn
        {    5341, new string[8] {null, null, null, null, null, null, null, null} },  // DL_TreeBushWinter
        {    5342, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_TreeAutumn
        {    5343, new string[8] {null, null, null, null, null, null, null, null} },  // DL_NkSearchLight
        {    5344, new string[8] {null, null, null, null, null, null, null, null} },  // DL_StationAlarm
        {    5345, new string[8] {null, null, null, null, null, null, null, null} },  // DL_FlagStartWoods
        {    5346, new string[8] {null, null, null, null, null, null, null, null} },  // DL_TreeSakura2
        {    5347, new string[8] {null, null, null, null, null, null, null, null} },  // DL_TreeSakura3
        {    5348, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalJump
        {    5349, new string[8] {null, null, null, null, null, null, null, null} },  // DL_NkGate
        {    5350, new string[8] {null, null, null, null, null, null, null, null} },  // DL_NkArrowArea_A
        {    5351, new string[8] {null, null, null, null, null, null, null, null} },  // DL_NkArrowArea_B
        {    5352, new string[8] {null, null, null, null, null, null, null, null} },  // DL_NkArrowArea_C
        {    5353, new string[8] {null, null, null, null, null, null, null, null} },  // DL_SpinTurboBar_Bb
        {    5354, new string[8] {null, null, null, null, null, null, null, null} },  // DL_Bbguide
        {    5355, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BbAccelRing
        {    5356, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BbCannon
        {    5357, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BbBuildA
        {    5358, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BbCarsB
        {    5359, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalFlag
        {    5360, new string[8] {null, null, null, null, null, null, null, null} },  // DL_FlagTriangle7
        {    5361, new string[8] {null, null, null, null, null, null, null, null} },  // DL_NkElevator
        {    5362, new string[8] {null, null, null, null, null, null, null, null} },  // DL_NkRotaryBoard_A
        {    5363, new string[8] {null, null, null, null, null, null, null, null} },  // DL_NkCars_A
        {    5372, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BpStartGate
        {    5373, new string[8] {null, null, null, null, null, null, null, null} },  // DL_SpinTurboBar_Nk
        {    5376, new string[8] {null, null, null, null, null, null, null, null} },  // DL_SpinTurboBar_Cl
        {    5377, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BbPipeRing
        {    5378, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BbRotaryBoardA
        {    5379, new string[8] {null, null, null, null, null, null, null, null} },  // DL_BbBoardPart
        {    5381, new string[8] {null, null, null, null, null, null, null, null} },  // DL_NkArrowArea_D
        {    5382, new string[8] {null, null, null, null, null, null, null, null} },  // DL_NkArrowArea_E
        {    5383, new string[8] {"Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", null, null, null, null} },  // DL_CoinStoneSnow
        {    6002, new string[8] {null, null, null, null, null, null, null, "Spawn Index"} },  // TestStart
        {    6003, new string[8] {null, null, null, null, null, null, null, "Spawn Index" } },  // Start
        {    6004, new string[8] {null, null, null, null, null, null, null, null} },  // Sun
        {    6005, new string[8] {null, null, null, null, null, null, null, null} },  // Moon
        {    6006, new string[8] {null, null, null, null, null, null, null, null} },  // Sunset
        {    6008, new string[8] {null, null, null, null, null, null, null, null} },  // SunInf
        {    6009, new string[8] {null, null, null, null, null, null, null, null} },  // SunInfY
        {    6010, new string[8] {null, null, null, null, null, null, null, null} },  // MoonInf
        {    6011, new string[8] {null, null, null, null, null, null, null, null} },  // MoonInfY
        {    6012, new string[8] {null, null, null, null, null, null, null, null} },  // SunsetInf
        {    6013, new string[8] {null, null, null, null, null, null, null, null} },  // SunsetInfY
        {    6014, new string[8] {null, null, null, null, null, null, null, null} },  // Moon2Inf
        {    6015, new string[8] {null, null, null, null, null, null, null, null} },  // Moon2InfY
        {    6016, new string[8] {null, null, null, null, null, null, null, null} },  // DL_RainbowLightA
        {    6017, new string[8] {null, null, null, null, null, null, null, null} },  // DL_RainbowLightB
        {    7005, new string[8] {null, null, null, null, null, null, null, null} },  // VR64Highway
        {    7006, new string[8] {null, null, null, null, null, null, null, null} },  // VRfair
        {    7008, new string[8] {null, null, null, null, null, null, null, null} },  // VRagbMario
        {    7009, new string[8] {null, null, null, null, null, null, null, null} },  // VR64Peach
        {    7010, new string[8] {null, null, null, null, null, null, null, null} },  // VRHorror
        {    7011, new string[8] {null, null, null, null, null, null, null, null} },  // VRCake
        {    7012, new string[8] {null, null, null, null, null, null, null, null} },  // VRcloudSea
        {    7013, new string[8] {null, null, null, null, null, null, null, null} },  // VRWaterPark
        {    7014, new string[8] {null, null, null, null, null, null, null, null} },  // VRFirst
        {    7015, new string[8] {null, null, null, null, null, null, null, null} },  // VRAirport
        {    7016, new string[8] {null, null, null, null, null, null, null, null} },  // VRTechno
        {    7017, new string[8] {null, null, null, null, null, null, null, null} },  // VRSnowMt
        {    7018, new string[8] {null, null, null, null, null, null, null, null} },  // VRCloud
        {    7019, new string[8] {null, null, null, null, null, null, null, null} },  // VRDesert
        {    7020, new string[8] {null, null, null, null, null, null, null, null} },  // VRExpert
        {    7021, new string[8] {null, null, null, null, null, null, null, null} },  // VRCity
        {    7022, new string[8] {null, null, null, null, null, null, null, null} },  // VRDossun
        {    7023, new string[8] {null, null, null, null, null, null, null, null} },  // VRMario
        {    7024, new string[8] {null, null, null, null, null, null, null, null} },  // VR64Yoshi
        {    7025, new string[8] {null, null, null, null, null, null, null, null} },  // VRMenu
        {    7026, new string[8] {null, null, null, null, null, null, null, null} },  // VRStorm
        {    7027, new string[8] {null, null, null, null, null, null, null, null} },  // VRGcDesert
        {    7028, new string[8] {null, null, null, null, null, null, null, null} },  // VRWiiMoo
        {    7029, new string[8] {null, null, null, null, null, null, null, null} },  // VRCosmos
        {    7030, new string[8] {null, null, null, null, null, null, null, null} },  // VRCustomizer
        {    7031, new string[8] {null, null, null, null, null, null, null, null} },  // VRWarioStadium
        {    7032, new string[8] {null, null, null, null, null, null, null, null} },  // VRG64Rainbow
        {    7033, new string[8] {null, null, null, null, null, null, null, null} },  // VRRainbowRoad
        {    7034, new string[8] {null, null, null, null, null, null, null, null} },  // VRClock
        {    7035, new string[8] {null, null, null, null, null, null, null, null} },  // VROcean
        {    7036, new string[8] {null, null, null, null, null, null, null, null} },  // VRSherbet
        {    7037, new string[8] {null, null, null, null, null, null, null, null} },  // VRDonuts
        {    7038, new string[8] {null, null, null, null, null, null, null, null} },  // VRPackunS
        {    7039, new string[8] {null, null, null, null, null, null, null, null} },  // VRPukuB
        {    7040, new string[8] {null, null, null, null, null, null, null, null} },  // VRBowser
        {    7041, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRSfcRainbow
        {    7042, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRMuteCity
        {    7043, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRDragon
        {    7044, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRExciteBike
        {    7045, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRHyrule
        {    7046, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRIcePark
        {    7047, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRBabyPark
        {    7048, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRYoshiCircuit
        {    7049, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRWoods
        {    7050, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRNeoBowserCity
        {    7051, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRRibbon
        {    7052, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRAnimalSpring
        {    7053, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRAnimalSummer
        {    7054, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRAnimalAutumn
        {    7055, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRAnimalWinter
        {    7056, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRCheeseLand
        {    7057, new string[8] {null, null, null, null, null, null, null, null} },  // DL_VRBigBlue
        {    8001, new string[8] {null, null, null, null, null, null, null, null} },  // ColCylinder
        {    8008, new string[8] {"Spawn Index", null, null, null, null, null, null, null} },  // StartEx
        {    8009, new string[8] {null, null, null, null, null, null, null, null} },  // ColCylinderStone
        {    8010, new string[8] {null, null, null, null, null, null, null, null} },  // ColCylinderWood
        {    8011, new string[8] {null, null, null, null, null, null, null, null} },  // ColCylinderMetal
        {    8014, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // CmnWaterCurrent_NoMdl
        {    8015, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // CmnWaterCurrent_NoEff
        {    8016, new string[8] {null, null, null, null, null, null, null, null} },  // ColSpinDash
        {    8017, new string[8] {null, null, null, null, null, null, null, null} },  // ColCylinderGum
        {    8021, new string[8] {null, null, null, null, null, null, null, null} },  // RRsat
        {    8022, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // N64RCoin
        {    8024, new string[8] {null, null, null, null, null, null, null, null} },  // ColLeafBox
        {    8025, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // NoteArea
        {    8026, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // ShortCutBox
        {    8027, new string[8] {null, null, null, null, null, null, null, null} },  // DL_EbCheerCol
        {    8028, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_EbApproachCol
        {    8029, new string[8] {"Unknown 1", "Unknown 2", null, null, null, null, null, null} },  // Adjuster200cc
        {    8030, new string[8] {null, null, null, null, null, null, null, null} },  // DL_WanwanSearchArea
        {    8031, new string[8] {null, null, null, null, null, null, null, null} },  // DL_WanwanAttackPoint
        {    8032, new string[8] {"Unknown 1", null, null, null, null, null, null, null} },  // DL_AnimalVoiceAutumn
        {    8033, new string[8] {null, null, null, null, null, null, null, null} },  // DL_AnimalVoiceSummer
        {    9006, new string[8] {null, null, null, null, null, null, null, null} },  // KaraPillarBase
        {    9007, new string[8] {null, null, null, null, null, null, null, null} },  // ItemBoxFont
        };
    }
}
