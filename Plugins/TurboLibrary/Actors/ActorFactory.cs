using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;

namespace TurboLibrary.Actors
{
    public class ActorFactory
    {
        static Dictionary<string, Type> ActorList = new Dictionary<string, Type>()
        {
            { "ItemBox", typeof(MapObjItemBox) },
            { "Kuribo",  typeof(MapObjKuribo) },
            { "TowerKuribo",  typeof(MapObjTowerKuribo) },
            { "HhStatue", typeof(MapObjHhStatue) },
            { "DemoCamera", typeof(DemoCameraDirector) },
           // { "EffectDrawer", typeof(EffectRenderer) },
            { "WsFirering", typeof(MapObjWsFirering) },
            { "BCTorch1", typeof(MapObjBCTorch1) },
            { "BCTorch2", typeof(MapObjBCTorch2) },
        };

        public static ActorBase GetActorEntry(string name)
        {
            ActorBase actor = new ActorModelBase();
            if (ActorList.ContainsKey(name))
                actor = (ActorBase)Activator.CreateInstance(ActorList[name]);
            actor.Name = name;
            return actor;
        }
    }
}
