using System;
using ThunderRoad;
using UnityEngine;
using LaserPointerScript.Shared;

namespace LaserPointerScript
{
   


    class LaserPointerExplode : MonoBehaviour
    {
        // causes enemies to explode when laser is pointecd at them

        private readonly float explosionForce = 40f;
        private readonly float explosionMult = 1.2f;
        private readonly float radius = 1f;
        private float lrMaxDistance;
        private readonly ForceMode forceMode = ForceMode.Impulse;
   
        protected Item item;
        protected ItemModuleLaserPointer module;
        private LineRenderer lR;
        private AudioSource clickSound;
        private LayerMask layerMask;
        private bool laserToggled;
        private float maxDistance;
        private Animator animator;


        protected void Awake()
        {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleLaserPointer>();
            clickSound = item.GetCustomReference(module.clickSound).GetComponent<AudioSource>();
            clickSound.enabled = true;
            maxDistance = 5000;
            lrMaxDistance = 2.437128f;
            animator = item.GetCustomReference(module.animatorRef).GetComponent<Animator>();
            layerMask = (LayerMask)536870912 | (LayerMask)268435456 | (LayerMask)33554432 | (LayerMask)8388608 | (LayerMask)512 | (LayerMask)32 | (LayerMask)2;
            layerMask = ~layerMask;
            lR = item.GetCustomReference(module.lineRenderer).GetComponent<LineRenderer>();
            laserToggled = false;
            lR.gameObject.SetActive(false);
           
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

                AddForce(creature);
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
        
        private void AddForce(Creature enemyCreature)
        {
            enemyCreature.locomotion.rb.AddExplosionForce(explosionForce * enemyCreature.locomotion.rb.mass, enemyCreature.transform.position, radius, explosionMult, forceMode);
            enemyCreature.locomotion.rb.AddForce(Vector3.up * explosionMult * enemyCreature.locomotion.rb.mass, forceMode);
            foreach (RagdollPart ragdollPart in enemyCreature.ragdoll.parts)
            {
                ragdollPart.rb.AddExplosionForce(explosionForce * ragdollPart.rb.mass, ragdollPart.transform.position, radius, explosionMult, forceMode);
                ragdollPart.rb.AddForce(Vector3.up * explosionMult * ragdollPart.rb.mass, forceMode);
            }

        }
    }
}
    
