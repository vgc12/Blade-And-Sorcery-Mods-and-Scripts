using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Marai
{
    public class Marai : MonoBehaviour
    {
        private Item item;
        private ItemModuleMarai module;
        

        private void Awake()
        {
            //Item Setup
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleMarai>();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;

            //Visual effects for when sword is in ash of war mode, must be disabled on start
            item.GetCustomReference("MaraiPhantom").GetComponent<MeshRenderer>().enabled = false;
            item.GetComponentsInChildren<ParticleSystem>().ToList().ForEach(x => x.Stop());

        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                
                Player.local.GetHand(Side.Right).ragdollHand.TryAction(Interactable.Action.Ungrab);
                Player.local.GetHand(Side.Left).ragdollHand.TryAction(Interactable.Action.Ungrab);
                item.Throw();
                SpawnOrb(ragdollHand);

            }
        }

        private void SpawnOrb(RagdollHand ragdollHand)
        {
            ItemData data = Catalog.GetData<ItemData>(module.OrbItemId, true);
           

            data.SpawnAsync(delegate (Item i)
            {
              
                if (ragdollHand.playerHand == Player.local.handRight)
                {
                    Player.local.creature.handRight.Grab(i.GetMainHandle(Side.Right), true);
                }
                else
                {
                    Player.local.creature.handLeft.Grab(i.GetMainHandle(Side.Left), true);
                }

                
                i.GetComponentInChildren<MaraiOrb>().swordItem = item;
               


            }, null, null, null, true, null);

        }

        private void Update()
        {
            if (item.isFlying)
            {
                Debug.Log(item.isFlying);
            }
        }
    }
}