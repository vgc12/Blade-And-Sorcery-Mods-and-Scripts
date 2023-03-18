using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ThunderRoad;


namespace Flashbang
{
    public class FlashbangPouch : MonoBehaviour
    {
        private Item item;
        private Holder holder;
        private ItemModuleFlashbangPouch module;
        private Flashbang flashbangScript;
        private bool waitingForSpawn = false;
       

        private void Awake()
        {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleFlashbangPouch>();
            
            holder = item.GetComponentInChildren<Holder>();
            holder.UnSnapped += Holder_UnsnappedEvent;
        }

        private void Start()
        {
            SnapToPouch();
        }


        private void SnapToPouch()
        {
            if (waitingForSpawn)
            {
                return;
            }
            ItemData data = Catalog.GetData<ItemData>(module.flashbangID, true);

            waitingForSpawn = true;
            data.SpawnAsync(delegate (Item i)
            {

                if (holder.HasSlotFree())
                {
                    holder.Snap(i, false, true);
                    flashbangScript = i.GetComponentInChildren<Flashbang>();
                }
               
                waitingForSpawn = false;
            },
            null, null, null, true, null);

            
            
        }

       

        private void Holder_UnsnappedEvent(Item item)
        {
            if (waitingForSpawn || !holder.HasSlotFree()) return;
            flashbangScript.GetData();
            SnapToPouch();
        }
    }
}
