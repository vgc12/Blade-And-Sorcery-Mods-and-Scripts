using System;
using System.Collections.Generic;
using UnityEngine;
using ThunderRoad;

namespace MaterialSwitch
{

    internal class MaterialSwitch : MonoBehaviour
    {
        private Item item;
        private List<Material> matList = new List<Material>();
        private MeshRenderer renderer;
        protected ItemModuleMaterialSwitch module;
        private bool useTrigger;
        private int Index;


        protected void Awake()
        {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleMaterialSwitch>();
            renderer = item.GetCustomReference(module.meshRef).GetComponent<MeshRenderer>();
            renderer.GetMaterials(matList);
            renderer.material = matList[0];
            useTrigger = module.useTrigger;
            ResetQueue();
            Debug.Log("" + matList);
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
        }

        

        public void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {

            if (useTrigger)
            {
                if (action == Interactable.Action.UseStart)
                {
                    ChangeMaterial();
                }
            }
            // uses the A button on Oculus quest
            else if (!useTrigger)
            {
                if (action == Interactable.Action.AlternateUseStart)
                {
                    ChangeMaterial();
                }
            }
        }



        private void ChangeMaterial()
        {
            
            Index++;
            if (Index >= matList.Count)
            {
                Index = 0;
                
                ResetQueue();
            }


            Index %= matList.Count;
            renderer.material.renderQueue = 1999;
            // there is a bug somewhere here that makes the last material in the list share a render queue with the second to last, 
            //so this line fixes it but i still do not know where the bug is, this is fine though as the last material should always have the render queue 2001
            if (Index == matList.Count - 1)
                matList[Index].renderQueue = 2001;
            renderer.material = matList[Index];
        }


        private void ResetQueue()
        {
            // this changes the material by changing its render queue, specifically this method sets the first material in the list to have the highest render queue
            // therefore giving that material more priority than the rest of the materials.
            for (int i = 0; i < matList.Count; i++)
            {
                matList[i].renderQueue = 2000 + (matList.Count - i);
            }
        }
        

    }
}
