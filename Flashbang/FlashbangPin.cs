
using UnityEngine;
using ThunderRoad;

namespace Flashbang
{
    public class FlashbangPin : MonoBehaviour
    {
        public Item item;
        private void Awake()
        {
            item = GetComponent<Item>();
            item.OnUngrabEvent += Item_OnUngrabEvent;
        }

        private void Item_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing) => item.Despawn();
            
       


    }
}
