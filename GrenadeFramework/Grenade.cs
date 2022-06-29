using System;
using System.Collections;
using ThunderRoad;
using UnityEngine;


namespace SimpleGrenadeFramework
{
    class Grenade : MonoBehaviour
    {

        protected Item item;
        protected ItemModuleSimpleGrenade module;
        private ParticleSystem explosionParticle;
        private AudioSource explodeSound;

        private readonly float radius = 10f;
        private float explosionForce;
        private float explosionMult;
        private readonly float delay = 3f;
        private float countDown;

        private bool spellButtonDown;
        private bool exploded;
        private readonly ForceMode forceMode = ForceMode.Impulse;

        protected void Awake()
        {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleSimpleGrenade>();
            if (!string.IsNullOrEmpty(module.explosionSound))
                explodeSound = item.GetCustomReference(module.explosionSound).GetComponent<AudioSource>();
        
       
                countDown = delay;

            if (!string.IsNullOrEmpty(module.particleRef))
                explosionParticle = item.GetCustomReference(module.particleRef).GetComponent<ParticleSystem>();
            explosionMult = module.explosionMult;
       
            explosionForce = module.explosionForce;
            exploded = false;
        
            explodeSound.enabled = true;

            item.OnHeldActionEvent += Item_OnHeldActionEvent;
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {

            if (action == Interactable.Action.AlternateUseStart)
            {
                spellButtonDown = true;
            }

        }

        //slices enemy limbs
        private void SliceEnemy(Creature creature)
        {
            try
            {
                if (!creature.ragdoll.GetPart(RagdollPart.Type.Head).isSliced)
                    creature.ragdoll.GetPart(RagdollPart.Type.Head).Slice();
                if (!creature.ragdoll.GetPart(RagdollPart.Type.LeftArm).isSliced)
                    creature.ragdoll.GetPart(RagdollPart.Type.LeftArm).Slice();
                if (!creature.ragdoll.GetPart(RagdollPart.Type.RightArm).isSliced)
                    creature.ragdoll.GetPart(RagdollPart.Type.RightArm).Slice();
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


        public void Update()
        {
            if (spellButtonDown)
            {
                countDown -= Time.deltaTime;
                if (countDown <= 0f && !exploded)
                {
                    FragExplode();
                    item.Despawn();
                    spellButtonDown = false;
                }
            }

        }

        //adds force to enemies when they die
        private void AddForce(Creature enemyCreature)
        {
            enemyCreature.locomotion.rb.AddExplosionForce(explosionForce * enemyCreature.locomotion.rb.mass, enemyCreature.transform.position, radius, explosionMult, forceMode);
            enemyCreature.locomotion.rb.AddForce(UnityEngine.Vector3.up * explosionMult * enemyCreature.locomotion.rb.mass, forceMode);
            foreach (RagdollPart ragdollPart in enemyCreature.ragdoll.parts)
            {
                ragdollPart.rb.AddExplosionForce(explosionForce * ragdollPart.rb.mass, ragdollPart.transform.position, radius, explosionMult, forceMode);
                ragdollPart.rb.AddForce(UnityEngine.Vector3.up * explosionMult * ragdollPart.rb.mass, forceMode);
            }

        }

        //responsible for in game explosion
        private void FragExplode()
        {
            try
            {
                explosionParticle.transform.parent = null;
                explosionParticle.Play();
                explodeSound.transform.parent = null;
                explodeSound.Play();
                Collider[] col = Physics.OverlapSphere(item.transform.position, radius);

                foreach (Collider enemyCol in col)
                {
                    Rigidbody enemyRB = enemyCol.GetComponent<Rigidbody>();

                    if (enemyRB != null)
                    {

                        if (enemyRB && enemyRB != Player.local.locomotion.rb && enemyRB != item.rb)
                        {
                            Creature enemyCreature = enemyRB.gameObject.GetComponentInParent<Creature>();

                            if (enemyCreature != null)
                            {
                                if (enemyCreature.isActiveAndEnabled && enemyCreature.ragdoll.isActiveAndEnabled && (!enemyCreature.isPlayer))
                                {
                                    if (enemyCreature.state == Creature.State.Alive)
                                        enemyCreature.TestKill();
                                    SliceEnemy(enemyCreature);

                                    AddForce(enemyCreature);

                                }

                            }
                            else
                            {
                                enemyRB.AddExplosionForce(explosionForce, transform.position, radius);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("" + ex.Message + " " + ex.StackTrace);
            }
        }
      





      

        
    

    }
}
