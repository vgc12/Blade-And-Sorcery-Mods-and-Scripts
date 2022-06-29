using LaserPointerScript.Shared;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace LaserPointerScript
{
    class LaserPointerSingle : MonoBehaviour
    {
        // this is a laser pointer that basically acts like a sword, slices off single limbs
        protected Item item;
        protected ItemModuleLaserPointer module;
        private LineRenderer lR;
        private AudioSource clickSound;
        private LayerMask layerMask;
        private bool laserToggled;
        private float maxDistance;
        private float lrMaxDistance;
        private Animator animator;
        private Collider[] colliders;
        






        protected void Awake()
        {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleLaserPointer>();
            clickSound = item.GetCustomReference(module.clickSound).GetComponent<AudioSource>();
            clickSound.enabled = true;
            animator = item.GetCustomReference(module.animatorRef).GetComponent<Animator>();
            layerMask = (LayerMask)((LayerMask)536870912 | (LayerMask)268435456 | (LayerMask)33554432 | (LayerMask)8388608 | (LayerMask)512 | (LayerMask)32 | (LayerMask)2);
            layerMask = (LayerMask)~layerMask;
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



        private void UpdateLaser()
        {
            if (Physics.Raycast(lR.transform.position, lR.transform.forward, out RaycastHit hit, maxDistance, layerMask))
            {
            avoidNullException:
                colliders = Physics.OverlapSphere(hit.point, .1f, layerMask);

                if (colliders == null)
                    goto avoidNullException;

                List<ColliderGroup> colliderGroups = new List<ColliderGroup>();
                List<RagdollPart> ragdollParts = new List<RagdollPart>();
                RaycastCheck(lR, hit);

                foreach (Collider col in colliders)
                {
                    
                    if (col.GetComponentInParent<ColliderGroup>() != (Object)null)
                    {
                        RaycastCheck(lR, hit);
                        colliderGroups.Add(col.GetComponentInParent<ColliderGroup>());
                    }
                }
                
                foreach (ColliderGroup CG in colliderGroups)
                {
                    if (CG.collisionHandler.ragdollPart != (Object)null)
                    {
                        RaycastCheck(lR, hit);
                        ragdollParts.Add(CG.collisionHandler.ragdollPart);
                    }
                    else
                        RaycastCheck(lR, hit);
                }
                
                foreach (RagdollPart ragdollPart in ragdollParts)
                {
                    if (ragdollPart.sliceAllowed && !ragdollPart.ragdoll.creature.player)
                    {
                        RaycastCheck(lR, hit);
                        ragdollPart.gameObject.SetActive(true);
                        ragdollPart.bone.animationJoint.gameObject.SetActive(true);
                        ragdollPart.ragdoll.Slice(ragdollPart);
                        ragdollPart.ragdoll.creature.Kill();

                    }
                    else
                        RaycastCheck(lR, hit);

                }
                RaycastCheck(lR, hit);
                ragdollParts.Clear();
                colliderGroups.Clear();
                

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
