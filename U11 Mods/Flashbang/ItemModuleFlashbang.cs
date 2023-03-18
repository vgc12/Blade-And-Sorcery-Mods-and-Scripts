using System;
using ThunderRoad;
using UnityEngine;
namespace Flashbang
{
    public class ItemModuleFlashbang : ItemModule
    {
        public string pinId, stopId, pinHolderRef, stopHolderRef, pinPullSound, explodeSound, ringSound;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<Flashbang>();
        }
       
    }
}
