using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Marai
{

    public class ItemModuleMaraiOrb : ItemModule
    {
        public string OrbRef, FirstRuneRef, SecondRuneRef, RuneTarget, DefaultLookPos;
        public bool consumeMana;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<MaraiOrb>();
            item.gameObject.AddComponent<MaraiOrbFX>();
        }
    }
}
