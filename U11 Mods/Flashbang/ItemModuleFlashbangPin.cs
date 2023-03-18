using System;
using ThunderRoad;
using UnityEngine;
namespace Flashbang
{
    public class ItemModuleFlashbangPin : ItemModule
    {
        public string pinId;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<FlashbangPin>();
        }
    }
}
