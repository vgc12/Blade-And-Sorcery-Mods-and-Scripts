using System;
using System.Collections;
using ThunderRoad;
using UnityEngine;
using LaserPointerScript.Shared;


namespace LaserPointerScript
{

    class LaserPointerFull : MonoBehaviour
    {
        // this does the same as the explosive wihout exploding.
        protected Item item;
        protected ItemModuleLaserPointer module;
        private LineRenderer lR;
        private AudioSource clickSound;
        private LayerMask layerMask;
        private bool laserToggled;
        private float maxDistance;
        private float lrMaxDistance;
        private Animator animator;

       

  
        

        protected void Awake()
        {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleLaserPointer>();
            clickSound = item.GetCustomReference(module.clickSound).GetComponent<AudioSource>();
            clickSound.enabled = true;
           
            animator = item.GetCustomReference(module.animatorRef).GetComponent<Animator>();
            layerMask = (LayerMask)536870912 | (LayerMask)268435456 | (LayerMask)33554432 | (LayerMask)8388608 | (LayerMask)512 | (LayerMask)32 | (LayerMask)2;
            layerMask = ~layerMask;
            lR = item.GetCustomReference(module.lineRenderer).GetComponent<LineRenderer>();
            laserToggled = false;
            lR.gameObject.SetActive(false);
            maxDistance = 5000;
            lrMaxDistance = 2.437128f;
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
        }

        protected void Update()
        {
            if (laserToggled)
            {

                UpdateLaser();
            }
            else if (!laserToggled)
            {
                lR.gameObject.SetActive(false);
            }
        }

        private void SliceEnemy(Creature creature)
        {
            try
            {
                if (!creature.ragdoll.GetPart(RagdollPart.Type.Head).isSliced)
                    creature.ragdoll.GetPart(RagdollPart.Type.Head).Slice();
                if (!creature.ragdoll.GetPart(RagdollPart.Type.LeftHand).isSliced)
                    creature.ragdoll.GetPart(RagdollPart.Type.LeftHand).Slice();
                if (!creature.ragdoll.GetPart(RagdollPart.Type.RightHand).isSliced)
                    creature.ragdoll.GetPart(RagdollPart.Type.RightHand).Slice();
                if (!creature.ragdoll.GetPart(RagdollPart.Type.LeftArm).isSliced)
                    creature.ragdoll.GetPart(RagdollPart.Type.LeftArm).Slice();
                if (!creature.ragdoll.GetPart(RagdollPart.Type.RightArm).isSliced)
                    creature.ragdoll.GetPart(RagdollPart.Type.RightArm).Slice();
                if (!creature.ragdoll.GetPart(RagdollPart.Type.RightFoot).isSliced)
                    creature.ragdoll.GetPart(RagdollPart.Type.RightFoot).Slice();
                if (!creature.ragdoll.GetPart(RagdollPart.Type.LeftFoot).isSliced)
                    creature.ragdoll.GetPart(RagdollPart.Type.LeftFoot).Slice();
                if (!creature.ragdoll.GetPart(RagdollPart.Type.LeftLeg).isSliced)
                    creature.ragdoll.GetPart(RagdollPart.Type.LeftLeg).Slice();
                if (!creature.ragdoll.GetPart(RagdollPart.Type.RightLeg).isSliced)
                    creature.ragdoll.GetPart(RagdollPart.Type.RightLeg).Slice();
            }
            catch (Exception ex)
            {
                Debug.Log(" " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void UpdateLaser()
        {
            if (Physics.Raycast(lR.transform.position, lR.transform.forward, out RaycastHit hit, maxDistance, layerMask))
            {
                if (hit.collider.GetComponentInParent<Creature>())
                {
                    Creature creature = hit.collider.GetComponentInParent<Creature>();
                    if (creature.isActiveAndEnabled && creature.ragdoll.isActiveAndEnabled && !creature.isPlayer && creature.state != Creature.State.Dead)
                    {
                        if (creature.state == Creature.State.Alive)
                            creature.Kill();
                        SliceEnemy(creature);
                    }
                }
                else
                {
                    RaycastCheck(lR, hit);
                }

            }
        }

        private void RaycastCheck(LineRenderer LR, RaycastHit hit)
        {
            if (hit.collider)
            {
                if (hit.distance < lrMaxDistance)
                {
                    LR.SetPosition(1, new Vector3(0, 0, hit.distance));
                }
                else
                {
                    LR.SetPosition(1, new Vector3(0, 0, lrMaxDistance));
                }

            }
            else
            {
                LR.SetPosition(1, new Vector3(0, 0, lrMaxDistance));

            }
        }
        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                animator.Play("Press", 0, 0);
                laserToggled = !laserToggled;
                if (laserToggled)
                    lR.gameObject.SetActive(true);
                clickSound.Play();
            }
            
        }
    }
}
