using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace TurboLibrary
{
    class CollisionCalculator
    {
        public static Dictionary<string, Vector4> AttributeColors = new Dictionary<string, Vector4>()
        {
            { "Item Wall", new Vector4(1, 1, 1, 0.5f) },
            { "Item Road", new Vector4(1, 1, 1, 0.5f) },
            { "Effect Trigger", new Vector4(1, 0, 1, 1) },
            { "Sound Trigger", new Vector4(1, 0.2f, 1, 1) },
            { "Fall Out", new Vector4(1, 0, 0, 1) },
            { "Lakitu Rescue", new Vector4(0.7f, 0, 0, 1) },
            { "Glider Activator", new Vector4(1, 1, 0, 1) },
        };

        public static Dictionary<string, Vector4> AttributeMatColors = new Dictionary<string, Vector4>()
        {
            { "Asphalt", new Vector4(0.4f, 0.4f, 0.4f, 1.0f) },
            { "Concrete", new Vector4(0.7f, 0.7f, 0.7f, 1.0f) },
            { "Stone", new Vector4(0.8f, 0.8f, 0.75f, 1.0f) },
            { "Wood", new Vector4(0.6f, 0.48f, 0.16f, 1.0f) },
            { "Metal Plate", new Vector4(0.35f, 0.35f, 0.35f, 1.0f) },
            { "Metal Mesh", new Vector4(0.3f, 0.3f, 0.3f, 1.0f) },
            { "Carpet", new Vector4(0.9f, 0.16f, 0.7f, 1.0f) },
            { "Grass", new Vector4(0, 1, 0, 1.0f) },
            { "Grass Small", new Vector4(0, 0.8f, 0, 1.0f) },
            { "Dirt", new Vector4(0.403f, 0.264f, 0.080f, 1.0f) },
            { "Water", new Vector4(0.104f, 0.274f, 1.000f, 1.0f) },
            { "Wet", new Vector4(0.001f, 0.274f, 1.000f, 1.0f) },
            { "Glass", new Vector4(1, 1, 1, 0.8f) },
            { "Piano", new Vector4(0.1f, 0.1f, 0.1f, 1) },
            { "Metallophone", new Vector4(0.2f, 0.1f, 0.1f, 1) },
            { "Bumpy", new Vector4(0.886f, 0.383f, 0.235f, 1) },
            { "Cloth", new Vector4(0.872f, 0.802f, 0.924f, 1) },
            { "Sponge", new Vector4(0.791f, 0.858f, 0.244f, 1) },
            { "Candy", new Vector4(0.920f, 0.417f, 1.000f, 1) },
            { "Dirt Small", new Vector4(0.616f, 0.323f, 0.152f, 1) },
            { "Sand Small", new Vector4(0.858f, 0.602f, 0.451f, 1) },
            { "Sand", new Vector4(0.896f, 0.754f, 0.543f, 1) },
            { "Snow", new Vector4(0.815f, 0.860f, 0.891f, 1) },
            { "Glider Panel", new Vector4(1, 1, 0, 1) },
            { "Dash", new Vector4(1, 0.5F, 0, 1) },
            { "Gravity Panel", new Vector4(0.834f, 0, 1, 1) },
            { "Chocolate", new Vector4(0.360f, 0.144f, 0.017f,1) },
            { "Lava", new Vector4(1.000f, 0.227f, 0.000f,1) },
            { "Ice", new Vector4(0.25f, 0.61f, 1f, 1.0f) },
        };

        public static List<string[]> AttributeMaterials = new List<string[]>()
        {
            //Road 1
            new string[] { "Asphalt", "Concrete", "Stone", "Wood", "Metal Plate", "Metal Mesh", "Carpet", "None",  },
            //Road 2
            new string[] { "Grass Small", "Dirt Small", "Glass", "Grass", "Piano", "Metallophone", "Bone", "None",  },
            //Road 3
            new string[] { "Bumpy", "Cloth", "Carpet", "Candy", "Sponge", "Carpet", "Cloud", "None",  },
            //Road 4
            new string[] { "Tec Road", "None", "None", "Wood Board", "Ocean Floor", "None", "Rainbow Road", "Rainbow Road (Glass Sound)",  },
            //Sand
            new string[] { "Sand Small", "Dirt Small", "Snow Small", "Muddy Small", "Carpet", "None", "None", "None",  },
            //Light Offroad
            new string[] { "Sand", "Dirt", "Carpet", "Wood", "Grass Small", "Water", "Rock", "None",  },
            //Offroad
            new string[] { "Sand", "Dirt", "Snow", "H Board", "Grass", "Water", "Rock", "Muddy", },
            //Offroad 2
            new string[] { "Chocolate", "Cream", "Cream", "SWAROVSKI", "Stone", "Rubber", "None", "None",  },
            //Heavy Offroad
            new string[] { "Sand", "Dirt", "Snow", "None", "Grass", "None", "None", "None",  },
            //Slippery
            new string[] { "Ice", "Glass", "Water", "Wet Asphalt", "None", "None", "None", "None",  },
            //Dash
            new string[] { "Dash", "Wet", "None", "None", "None", "None", "None", "None",  },
            //Gravity
            new string[] { "Gravity Pad", "None", "None", "None", "None", "None", "None", "None",  },
            //Glider
            new string[] { "Glider Pad", "None", "None", "None", "None", "None", "None", "None",  },
            //Pull
            new string[] { "Water", "River Moving", "None", "None", "None", "None", "None", "None",  },
            //Moving terrain
            new string[] { "Stairs", "Stairs 2", "None", "None", "None", "None", "None", "None",  },
            //Invisible road
            new string[] { "Concrete", "None", "None", "None", "None", "None", "None", "None",  },
            //RESQ
            new string[] { "Road", "None", "None", "None", "None", "None", "None", "None",  },
            //Wall
            new string[]{ "Concrete", "Cliff", "Metal", "Wood", "Ice", "Snow", "Ivy", "Rainbow" },
            //Wall 2
            new string[]{ "Chocolate", "Musical", "Techno fence", "Glass", "Candy", "Sponge", "Paper", "Cookie" },
            //Wall 3
            new string[]{ "Leaf", "Car", "None", "None", "None", "None", "None", "None" },
            //LWALL
            new string[]{ "Concrete", "Metal", "Bone", "None", "Cloth", "Cloth 2", "Rainbow Wall", "None" },
            //Invisible Wall
            new string[]{ "Concrete", "None", "None", "None", "None", "None", "None", "None" },
            //Boundary wall
            new string[] { "Cream", "Bush", "Plastic", "Leaf", "Ocean", "Tire", "Cloth", "Cloud",  },
            //Fall Out
            new string[]{ "Normal", "None", "None", "None", "None", "None", "None", "None" },
            //Dummy1
            new string[]{ "Unsolid", "None", "None", "None", "None", "None", "None", "None" },
            //Cannon
            new string[]{ "Normal", "None", "None", "None", "None", "None", "None", "None" },
            //Trigger
            new string[]{ "Normal", "None", "None", "None", "None", "None", "None", "None" },
            //Sound
            new string[]{ "Normal", "None", "None", "None", "None", "None", "None", "None" },
            //Valley
            new string[]{ "Void", "Water", "Lava", "Lava", "Water (desert)", "None", "None", "None" },
            new string[]{ "Normal", "None", "None", "None", "None", "None", "None", "None" },
            new string[]{ "Normal", "None", "None", "None", "None", "None", "None", "None" },
            //Zone (unsolid planes over collision)
            new string[]{ "Glider Activator", "Mud Particles", "Orange Flowers", "White Flowers (slow movement)", "White Flowers", "Green Flowers", "None", "Flowers", "Flowers", "Flowers" },
        };

        public static List<string[]> AttributeMaterialSounds = new List<string[]>()
        {
            //Road 1
            new string[] { "SNDG_GND_ASPHALT", "SNDG_GND_CONCRETE", "SNDG_GND_STONE", "SNDG_GND_WOOD", "SNDG_GND_TEPPAN", "SNDG_GND_KANAAMI", "SNDG_GND_CARPET","None",  },
            //Road 2
             new string[] { "SNDG_GND_GRASS_SMALL", "SNDG_GND_DIRT_SMALL", "SNDG_GND_GLASS", "SNDG_GND_GRASS", "SNDG_GND_PIANO", "SNDG_GND_METALLOPHONE", "SNDG_GND_BONE","None",  },
            //Road 3
            new string[] { "SNDG_GND_ASPHALT", "SNDG_GND_CLOTH", "SNDG_GND_CARPET", "SNDG_GND_CANDY", "SNDG_GND_SPONGE", "SNDG_GND_CARPET", "SNDG_GND_CLOUD", "None",  },
            //Road 4
            new string[]{  "SNDG_GND_TCROAD", "None", "None", "SNDG_GND_WBOARD", "SNDG_GND_MORAY", "None", "SNDG_GND_RAINBOW2","None", },
            //Sand
            new string[]{ "GND_SAND_SMALL", "GND_DIRT_SMALL", "GND_SNOW_SMALL", "SNDG_GND_NUKARUMI_SMALL", "SNDG_GND_CARPET", "None", "None", "None" },
            //Light Offroad
            new string[]{ "SNDG_GND_SAND", "SNDG_GND_DIRT", "SNDG_GND_CARPET", "SNDG_GND_WOOD", "SNDG_GND_GRASS_SMALL", "SNDG_GND_WATER", "SNDG_GND_GANSEKI","None",  },
            //Offroad
            new string[]{ "SNDG_GND_SAND", "SNDG_GND_DIRT", "SNDG_GND_SNOW", "SNDG_GND_HBOARD", "SNDG_GND_GRASS", "SNDG_GND_WATER", "SNDG_GND_GANSEKI", "SNDG_GND_NUKARUMI" },
            //Offroad 2
            new string[]{ "SNDG_GND_CHOCOLATE", "SNDG_GND_CREAM", "SNDG_GND_CREAM", "SNDG_GND_SWAROVSKI", "SNDG_GND_STONE", "SNDG_GND_RUBBER", "None", "None" },
            //Heavy Offroad
            new string[]{ "SNDG_GND_SAND", "SNDG_GND_DIRT", "SNDG_GND_SNOW", "None", "SNDG_GND_GRASS", "None", "None", "None",  },
            //Slippery
            new string[]{ "None", "SNDG_GND_GLASS", "SNDG_GND_WATER", "SNDG_GND_WETASPHALT", "None", "None", "None", "None" },
            //Dash
            new string[]{ "None", "SNDG_GND_ASPHALT", "None", "None", "None", "None", "None", "None" },
            //Gravity
            new string[]{ "s_GRAVITY", "SNDG_GND_ASPHALT", "None", "None", "None", "None", "None", "None" },
            //Glider
            new string[]{ "s_GLIDE", "None", "None", "None", "None", "None", "None", "None" },
            //Pull
            new string[]{ "SNDG_GND_WATER", "SNDG_GND_RIVERWILD", "None", "None", "None", "None", "None", "None" },
            //Moving terrain
            new string[]{ "s_SNDG_GND_STAIRS", "s_SNDG_GND_STAIRS", "None", "None", "None", "None", "None", "None" },
            //Invisible Road
            new string[]{ "SNDG_WALL_CONCRETE", "None", "None", "None", "None", "None", "None", "None" },
            //RESQ
            new string[]{ "None", "None", "None", "None", "None", "None", "None", "None" },
            //Wall
            new string[]{ "SNDG_WALL_CONCRETE", "SNDG_WALL_GAKE", "SNDG_WALL_METAL", "SNDG_WALL_WOOD", "SNDG_WALL_ICE", "SNDG_WALL_SNOW", "SNDG_WALL_IVY", "SNDG_WALL_RAINBOW" },
            //Wall 2
            new string[]{ "SNDG_WALL_CHOCOLATE", "SNDG_WALL_MUSICAL", "SNDG_WALL_TECHNO_FENCE", "SNDG_WALL_GLASS", "SNDG_WALL_CANDY", "SNDG_WALL_SPONGE", "SNDG_WALL_PAPER", "SNDG_WALL_COOKIE" },
            //Wall 3
            new string[]{ "SNDG_WALL_LEAF", "SNDG_WALL_CAR", "None", "None", "None", "None", "None", "None" },
            //Lava
            new string[]{ "SNDG_WALL_CONCRETE", "SNDG_WALL_METAL", "SNDG_WALL_BONE", "None", "SNDG_WALL_CLOTH", "SNDG_WALL_CLOTH", "SNDG_WALL_RAINBOW", "None" },
            //Invisible Wall
            new string[]{ "SNDG_WALL_CONCRETE", "None", "None", "None", "None", "None", "None", "None" },
            //Boundary Wall
            new string[]{ "SNDG_WALL_CREAM", "SNDG_WALL_BUSH", "SNDG_WALL_PLASTIC", "SNDG_WALL_LEAF", "SNDG_WALL_MORAY", "SNDG_WALL_TIRE", "SNDG_GND_CLOTH", "SNDG_GND_CLOUD" },
            new string[]{ "None", "None", "None", "None", "None", "None", "None", "None" },
            //Dummy1
            new string[]{ "None", "None", "None", "None", "None", "None", "None", "None" },
            //Cannon
            new string[]{ "None", "None", "None", "None", "None", "None", "None", "None" },
            //Trigger
            new string[]{ "None", "None", "None", "None", "None", "None", "None", "None" },
            //Sound
            new string[]{ "None", "None", "None", "None", "None", "None", "None", "None" },
            //Fall Out
            new string[]{ "Void", "Water", "Lava", "Lava", "Water (desert)", "None", "None", "None" },

            new string[]{ "None", "None", "None", "None", "None", "None", "None", "None" },
            new string[]{ "None", "None", "None", "None", "None", "None", "None", "None" },
            //Zone
            new string[]{ "None", "None", "None", "None", "None", "None", "None", "None" , "None", "None", "None"},
        };

        public static string[] AttributeList = new string[]
        {
            "Road 1", "Road 2", "Road 3", "Road 4",
            "Sand", "Light Offroad", "Offroad","Offroad 2", "Heavy Offroad",
            "Slippery", "Dash", "Gravity Pad", "Glider Pad", "Pull",
            "Moving Terrain", "Item Road", "Lakitu Rescue",
            "Wall 1", "Wall 2","Wall 3",
            "LWALL", "Item Wall", "BWALL", "Invisible Wall", "Dummy1",
            "Cannon", "Effect Trigger", "Sound Effect", "Fall Out", "Dummy2", "Dummy3", "Zone"
         };

        public static string[] SpecialType = new string[]
       {
            "None",
            "Trickable", //0x1000
            "Trickable (Speed Required)", //0x2000
            //Sticky collision typically used for stronger gravity
            "High Gravity", //0x4000
            "High Gravity + Bouncy?", //0x8000
        };
    }
}
