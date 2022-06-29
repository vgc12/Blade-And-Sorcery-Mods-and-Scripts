using System;
using System.Collections;
using ThunderRoad;
using UnityEngine;



namespace SimpleGrenadeFramework
{

    class Claymore : MonoBehaviour
    {
        protected Item item;
        protected ItemModuleSimpleGrenade module;
       
        private ParticleSystem explosionParticle;
        private AudioSource explodeSound;

        


        private bool spellButtonDown;
        private bool exploded;

     
        private LineRenderer lROne, lRTwo;
        private LayerMask layerMask;
        private float lrMaxDistance;
        private MeshRenderer mesh;







    

        protected void Awake()
        {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleSimpleGrenade>();
            if (!string.IsNullOrEmpty(module.explosionSound))
                explodeSound = item.GetCustomReference(module.explosionSound).GetComponent<AudioSource>();
            

            if (!string.IsNullOrEmpty(module.particleRef))
                explosionParticle = item.GetCustomReference(module.particleRef).GetComponent<ParticleSystem>();
          
            exploded = false;
            explodeSound.enabled = true;
            mesh = item.GetCustomReference(module.meshRef).GetComponent<MeshRenderer>();
            mesh.gameObject.SetActive(true);

            // in this game for some reason raycasts will not detect enemies if they are further than about this distance away.
            lrMaxDistance = 2.437128f;
                
            layerMask = (LayerMask)536870912 | (LayerMask)268435456 | (LayerMask)33554432 | (LayerMask)8388608 | (LayerMask)512 | (LayerMask)32 | (LayerMask)2;
            layerMask = (LayerMask)~layerMask;
            lROne = item.GetCustomReference(module.lineRendererOne).GetComponent<LineRenderer>();
            lRTwo = item.GetCustomReference(module.lineRendererTwo).GetComponent<LineRenderer>();
            lROne.gameObject.SetActive(false);
            lRTwo.gameObject.SetActive(false);

            

            item.OnHeldActionEvent += Item_OnHeldActionEvent;
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {

            if (action == Interactable.Action.AlternateUseStart)
            {
                spellButtonDown = true;
            }

        }

       


        public void Update()
        {
            if (spellButtonDown)
            {
                lROne.gameObject.SetActive(true);
                lRTwo.gameObject.SetActive(true);
                if (!exploded)
                    ArmClaymore();
                if (exploded && !explodeSound.isPlaying)
                {

                    spellButtonDown = false;
                    item.Despawn();
                }

            }

        }

        // this is used to check how far the laser should go
        private void RaycastCheck(LineRenderer LR, RaycastHit hit)
        {
            if (hit.collider)
            {
                if (hit.distance < lrMaxDistance)
                    LR.SetPosition(1, new UnityEngine.Vector3(0, 0, hit.distance));
                else
                    LR.SetPosition(1, new Vector3(0, 0, lrMaxDistance));

            }
            else if (!hit.collider)
            {
                LR.SetPosition(1, new Vector3(0, 0, lrMaxDistance));
            }
            else
            {
                LR.SetPosition(1, new Vector3(0, 0, lrMaxDistance));
            }
        }


        private void ArmClaymore()
        {
            // two raycasts for each laser from the claymore to detect enemies, explodes when enemy walks in front of laser
            if (Physics.Raycast(lROne.transform.position, lROne.transform.forward, out RaycastHit hitOne, lrMaxDistance, layerMask))
            {

                if (hitOne.collider.GetComponentInParent<Creature>())
                {
                    Creature cCreatureOne = hitOne.collider.GetComponentInParent<Creature>();
                    if (!exploded && cCreatureOne.isActiveAndEnabled && cCreatureOne.ragdoll.isActiveAndEnabled && !cCreatureOne.isPlayer)
                    {
                            
                        explosionParticle.Play();
                        explodeSound.Play();
                        cCreatureOne.ragdoll.GetPart(RagdollPart.Type.LeftLeg).Slice();
                        cCreatureOne.ragdoll.GetPart(RagdollPart.Type.RightLeg).Slice();
                        lROne.enabled = false;
                        lRTwo.enabled = false;
                        mesh.gameObject.SetActive(false);
                        exploded = true;
                    }
                }
                else
                {
                    RaycastCheck(lROne, hitOne);
                }  
                
            }

            
            if (Physics.Raycast(lRTwo.transform.position, lRTwo.transform.forward, out RaycastHit hitTwo, lrMaxDistance, layerMask))
            {
                if (hitTwo.collider.GetComponentInParent<Creature>())
                {
                    Creature cCreatureTwo = hitTwo.collider.GetComponentInParent<Creature>();
                    if (!exploded && cCreatureTwo.isActiveAndEnabled && cCreatureTwo.ragdoll.isActiveAndEnabled && !cCreatureTwo.isPlayer)
                    {
                        explosionParticle.Play();
                           
                        explodeSound.Play();
                        if (!cCreatureTwo.ragdoll.GetPart(RagdollPart.Type.LeftLeg).isSliced)
                            cCreatureTwo.ragdoll.GetPart(RagdollPart.Type.LeftLeg).Slice();
                        if (!cCreatureTwo.ragdoll.GetPart(RagdollPart.Type.RightLeg).isSliced)
                            cCreatureTwo.ragdoll.GetPart(RagdollPart.Type.RightLeg).Slice();
                        lROne.enabled = false;
                        lRTwo.enabled = false;
                        mesh.gameObject.SetActive(false);
                        exploded = true;
                    }
                }
                else
                {
                    RaycastCheck(lRTwo, hitTwo);
                }

            }
            

        }
    }
}
