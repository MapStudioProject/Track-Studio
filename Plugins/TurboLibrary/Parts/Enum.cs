using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboLibrary.Parts
{
    public enum SusType
    {
        Kart = 0,
        BikeA = 1,
        BikeA2 = 2,
        BikeB = 3,
        BikeB2 = 4,
    }

    public enum EDriverID
    {
        Invalid = -1,
        Mario ,Luigi ,Peach ,Daisy ,Yoshi ,Kinopio ,Kinopico ,
        Nokonoko ,Koopa ,DK ,Wario  ,Waluigi ,Rosetta ,MetalMario ,MetalPeach ,
        Jugem ,Heyho ,BbMario ,BbLuigi ,BbPeach ,BbDaisy ,BbRosetta ,Larry ,Lemmy ,
        Wendy ,Ludwig ,Iggy ,Roy ,Morton ,Mii ,TanukiMario ,Link ,AnimalBoyA ,Shizue ,
        CatPeach ,HoneKoopa ,AnimalGirlA ,GoldMario ,Karon ,KoopaJr ,KingTeresa , 
        SplatoonGirl,SplatoonBoy,
    }

    public enum EBodyID
    {
        Invalid = -1, 
        K_Std ,K_Skl ,K_Ufo ,K_Sbm ,K_Cat,
        K_Fml ,K_Tri ,K_Wld ,K_Pch ,K_Ten ,K_Shp ,K_Snk,
        K_Spo ,K_Gld ,B_Std ,B_Fro ,B_Mgp ,B_Big ,B_Amb,
        B_Mix ,B_Kid ,B_Jet ,B_Ysi ,V_Atv ,V_Hnc ,V_Bea,
        K_Gla ,K_Slv ,K_Rst ,K_Bfl ,K_Tnk ,K_Bds ,B_Zlb,
        K_Btl ,K_Pwc ,B_Sct ,V_Drb ,K_Kpc ,V_Spl ,V_Tnd,
        B_Bow,
    }

    public enum ETireID
    {
        Invalid = -1,
        Std, Big, Sml, Rng, Slk,
        Mtl, Btn, Ofr, Spg, Wod,
        Fun, Zst, Zbi, Zsm, Zrn,
        Zsl,  Zof, Gld, Gla,
        Tri, Anm, Act, //todo confirm
    }

    public enum EWingID
    {
        Invalid = -1, Std, Jgm, Wlo, Zng, Umb, Prc, Prf, Flw, Kpa, Spl, Ptv, Gld, Hyr, Pap,
    }
}
