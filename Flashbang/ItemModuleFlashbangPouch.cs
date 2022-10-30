using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Flashbang
{
    public class ItemModuleFlashbangPouch : ItemModule
    {
        public string flashbangID;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<FlashbangPouch>();
        }
    }
}
