using System;
using ThunderRoad;
using UnityEngine;


namespace Flashbang
{


    public class Flashbang : MonoBehaviour
    {
        public Holder pinHolder;
        public Holder stopHolder;

        private ItemModuleFlashbang module;
        private Item item;
        private Item stopItem;
        private bool pinUnsnapped = false, stopUnsnapped = false;
        private float timer = 4f, t = 0;
        private Light light;
        private MeshRenderer meshRenderer;
        private AudioSource pinSound, explodeSoundClose, ringSound;
        private bool exploded = false;
        private bool stopItemDespawned = false;
    




        private void Awake()
        {

            item = GetComponent<Item>();

            Holder[] temp = item.GetComponentsInChildren<Holder>();

            if (temp[0].interactableId == "StopHolder")
            {
                stopHolder = temp[0];
                pinHolder = temp[1];
            }
            else if(temp[0].interactableId == "PinHolder")
            {
                pinHolder = temp[0];
                stopHolder = temp[1];
            }
            
            module = item.data.GetModule<ItemModuleFlashbang>();
           
            light = item.GetComponentInChildren<Light>();
            light.enabled = false;
            light.range = 2.5f;
           


            meshRenderer = item.GetComponentInChildren<MeshRenderer>();
            meshRenderer.enabled = true;

         
            pinSound = item.GetCustomReference(module.pinPullSound).GetComponent<AudioSource>();
            explodeSoundClose = item.GetCustomReference(module.explodeSound).GetComponent<AudioSource>();
            ringSound = item.GetCustomReference(module.ringSound).GetComponent<AudioSource>();
            

           
            pinHolder.UnSnapped += PinHolder_UnsnapEvent;
           
           
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            item.OnUngrabEvent += Item_OnUngrabEvent;
           
        }

        private void Item_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            if(pinHolder.HasSlotFree() && !stopHolder.HasSlotFree())
            {
                stopHolder.UnSnapAll();
                stopUnsnapped = true;
            }
        }

        private void StopItem_OnDespawnEvent(EventTime eventTime)
        {
            stopItemDespawned = true;
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (!stopHolder.HasSlotFree() && action == Interactable.Action.UseStart)
            {
                if (!pinHolder.HasSlotFree())
                {
                    pinHolder.UnSnapAll();
                    pinUnsnapped = true;
                }

                pinSound.Play();
                stopHolder.UnSnapAll();
                stopUnsnapped = true;
            }
        }

        private void Update()
        {

            if (pinUnsnapped && stopUnsnapped)
            {
                timer -= Time.deltaTime;

                if (timer <= 0f)
                {
                    if (!stopItemDespawned)
                        stopItem.Despawn();

                    light.enabled = true;

                    if (!exploded)
                        Explode();
                    else if (exploded)
                        DecreaseLightIntensity();
                }

            }
        }

        private void DecreaseLightIntensity()
        {
            meshRenderer.enabled = false;

            light.intensity = Mathf.Lerp(8000000f, 0f, t);

            t += Time.deltaTime * 3f;

            if (light.intensity <= 0 && !ringSound.isPlaying && !explodeSoundClose.isPlaying)
                item.Despawn();
        }


        private void Explode()
        {
            Collider[] enemyCols = Physics.OverlapSphere(item.transform.position, 5f);

            foreach (Collider col in enemyCols)
            {
                if (!exploded)
                    explodeSoundClose.Play();


                Creature creature = col.GetComponentInParent<Creature>() ?? null;

                if (creature != null && !creature.isPlayer)
                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);

                else if (creature != null && creature.isPlayer)
                    ringSound.Play();


                exploded = true;

            }

        }


        public void GetData()
        {
         
            ItemData pinData = Catalog.GetData<ItemData>(module.pinId, true);
            SpawnWithItem(pinData, pinHolder, 0);
                
            ItemData stopData = Catalog.GetData<ItemData>(module.stopId, true);
            SpawnWithItem(stopData, stopHolder, 1);

            
        }

        private void SpawnWithItem(ItemData Data, Holder holder, int pinOrStop)
        {
            Data.SpawnAsync(delegate (Item i)
            {
                holder.Snap(i, false, true);
                if (pinOrStop == 1)
                {
                    stopItem = i;
                    stopItem.OnDespawnEvent += StopItem_OnDespawnEvent;
                }

            },
            new Vector3?(item.transform.position),
            new Quaternion?(Quaternion.Euler(item.transform.rotation.eulerAngles)),
            null, false, null
            );
        }

        protected void PinHolder_UnsnapEvent(Item pin)
        {

            pinUnsnapped = true;
            pinSound.Play();
        }
    }
}
