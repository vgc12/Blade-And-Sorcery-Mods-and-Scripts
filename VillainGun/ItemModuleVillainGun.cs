using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;

namespace VillainGun
{
    public class ItemModuleVillainGun : ItemModule
    {
        public string p, pZero, pOne, pTwo, lrRef, screenRef, chargeSound, fireSound;
        public bool soundEffectsEnabled;


        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<VillainGun>();
        }

    }
}
