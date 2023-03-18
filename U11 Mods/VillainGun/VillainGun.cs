using System;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;

namespace VillainGun
{
    // todo
    // figure out why line renderer is not being drawn, flatten enemies.


    public class VillainGun : MonoBehaviour
    {
        protected Item item;
        protected ItemModuleVillainGun module;

        private bool soundEffectsEnabled = true;

        //p might be useless in all honesty but im too lazy to see 
        private Transform p, p0, p1, p2;

        //points that define where the lineRenderer should curve
        private Vector3[] positions = new Vector3[50];
        private readonly int numPoints = 50;


        // no enemy detected but player is pressing trigger variables
        private readonly float range = 10;


        // line renderer and screen animation variables
        private LineRenderer lr;
        private Material lrMat;
        private MeshRenderer screenMeshRenderer;
        private List<Material> screenMats = new List<Material>();
        private float offset = 0;
        private float lrTimer = 5;

        //size of enemies
        //private List<Vector3> ragdollPartTransforms = new List<Vector3>();
        private Vector3 headSize;

    

        private int playerHandSide = 0;

        private AudioSource chargeSound, fireSound;
        
        //lerp screen colors
        private float lerpMinimum;
        private float lerpMaximum;
        private float t = 0.0f;

        //variables for how the gun should fire and if/when it should be firing
        private bool ChargeGun = false;
        private float chargeTimer = 0;
        private int gunMode = 0;

        private enum HandSide
        {
            Left = 0,
            Right = 1
        }

        protected void Awake()
        {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleVillainGun>();

            //transforms to define how the cubic bezier should curve
            lr = item.GetCustomReference(module.lrRef).GetComponent<LineRenderer>();
            p = item.GetCustomReference(module.p).GetComponent<Transform>();
            p0 = item.GetCustomReference(module.pZero).GetComponent<Transform>();
            p1 = item.GetCustomReference(module.pOne).GetComponent<Transform>();
            p2 = item.GetCustomReference(module.pTwo).GetComponent<Transform>();

            chargeSound = item.GetCustomReference(module.chargeSound).GetComponent<AudioSource>();
            fireSound = item.GetCustomReference(module.fireSound).GetComponent<AudioSource>();

            screenMeshRenderer = item.GetCustomReference(module.screenRef).GetComponent<MeshRenderer>();
            screenMeshRenderer.GetMaterials(screenMats);
            screenMeshRenderer.material = screenMats[0];

            lr.enabled = true;
            lrMat = lr.material;

            lr.positionCount = numPoints;
            lr.useWorldSpace = false;

            lrMat.SetColor("_BaseColor", Color.HSVToRGB(0, 0, 1));

            soundEffectsEnabled = module.soundEffectsEnabled;

            ChangeLerpColors();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
        }




        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (ragdollHand.playerHand == Player.local.handRight)
            {
                playerHandSide = (int)HandSide.Right;
            }
            if (ragdollHand.playerHand == Player.local.handLeft)
            {
                playerHandSide = (int)HandSide.Left;
            }


            if (action == Interactable.Action.UseStart)
                ChargeGun = true;
            if (action == Interactable.Action.AlternateUseStart)
            {
                gunMode++;
                
                gunMode %= screenMats.Count();
                ChangeLerpColors();
                ChangeLineRendererColor();
                ChangeScreenMaterial();
            }
            if (action != Interactable.Action.UseStop)
                return;
            ChargeGun = false;
        }

        private void ChangeLerpColors()
        {
            if(gunMode == 0)
            {
                lerpMinimum = .37f;
                lerpMaximum = .65f;

            }
            if(gunMode == 1)
            {
                lerpMinimum = .14f;
                lerpMaximum = .33f;
            }
            if(gunMode == 2)
            {
                lerpMinimum = .69f;
                lerpMaximum = .98f;
            }
        }

        private void ChangeLineRendererColor()
        {

            if (gunMode == 0)
                lrMat.SetColor("_BaseColor", Color.HSVToRGB(0, 0, 1));
            else if (gunMode == 1)
                lrMat.SetColor("_BaseColor", Color.HSVToRGB(.22f, 1, 1));
            else if (gunMode == 2)
                lrMat.SetColor("_BaseColor", Color.HSVToRGB(.9f, 1, 1));
            else return;

        }

        private void ResetQueue()
        {

            for (int i = 0; i < screenMats.Count; i++)
            {
                screenMats[i].renderQueue = 2000 + (screenMats.Count - i);
                Debug.Log(screenMats[i].renderQueue);
            }
        }

        //reused code from my MaterialSwap script
        private void ChangeScreenMaterial()
        {
            if (gunMode >= screenMats.Count)
            {
                ResetQueue();
            }

            screenMeshRenderer.material.renderQueue = 1999;

            if (gunMode == screenMats.Count - 1)
                screenMats[gunMode].renderQueue = 2001;
            screenMeshRenderer.material = screenMats[gunMode];
            Debug.Log(screenMeshRenderer.material.renderQueue);
            Debug.Log(screenMeshRenderer.material.name);
        }


        void Update()
        {
            // to move the lr, you cant actually move the lr, you have to move the points that calculate the curve of the lr, so p actually moves; and it calculates the range of the laser based off p0;
            lr.transform.position = Vector3.zero;
            lr.transform.rotation = Quaternion.identity;
            
            AnimateScreen();

            if (ChargeGun)
            {
                if (chargeTimer >= 0f && !chargeSound.isPlaying && soundEffectsEnabled)
                    chargeSound.Play();

                if (playerHandSide == (int)HandSide.Left)
                    PlayerControl.handLeft.HapticShort(1f);
                if (playerHandSide == (int)HandSide.Right)
                    PlayerControl.handRight.HapticShort(1f);

                chargeTimer -= Time.deltaTime;

                if (chargeTimer <= 0f)
                {
                    if (chargeSound.isPlaying && soundEffectsEnabled) 
                        chargeSound.Stop();

                    if(!fireSound.isPlaying && soundEffectsEnabled)
                        fireSound.Play();

                    lr.enabled = true;

                    p0.position = p.position;

                    Transform closestEnemy;

                    // get closest enemy
                    if (GetClosestEnemy(DetectEnemies(), p) != null)
                    {
                        closestEnemy = GetClosestEnemy(DetectEnemies(), p);
                        p2.position = closestEnemy.GetComponentInChildren<Creature>()
                            .ragdoll.GetPart(RagdollPart.Type.Torso).transform.position;
                    }
                    else
                    {
                        closestEnemy = null;
                        p2.position = p.position;
                    }


                    AnimateLineRenderer();
                    DrawLineToEnemy(closestEnemy);
                }
            }
            else if (!ChargeGun)
            {
                fireSound.Stop();
                chargeSound.Stop();
                lr.enabled = false;
                chargeTimer = 3f;
                PlayerControl.handRight.HapticLoop(false);
                PlayerControl.handLeft.HapticLoop(false);
            }
        }



        private void AnimateScreen()
        {
            //this fundamentaly isnt how Color works FIX THIS
                screenMeshRenderer.material.SetColor("_EmissionColor", Color.HSVToRGB(Mathf.Lerp(lerpMinimum, lerpMaximum , t), 1, 1));
            
            t += 0.5f * Time.deltaTime;

            if (t >= 1.0f)
            {
                float temp = lerpMaximum;
                lerpMaximum = lerpMinimum;
                lerpMinimum = temp;
                t = 0.0f;
            }
        }

        private void AnimateLineRenderer()
        {
            lr.enabled = true;
            lrTimer -= Time.deltaTime * 1.5f;
            if (lrTimer < 0)
                lrTimer = 5;
            offset = lrTimer;
            if (offset == 0) offset = 5;
            lrMat.SetTextureOffset("_BaseMap", new Vector2(offset, 0));
        }

        private void DrawLineToEnemy(Transform closestEnemy)
        {
            if (closestEnemy != null)
            {
                DrawQuadraticCurve();
                if (gunMode == 0)
                    Flatten(closestEnemy);
                else if (gunMode == 1)
                    GrowHead(closestEnemy);
                else if (gunMode == 2)
                    ShrinkHead(closestEnemy);
            }
            else if (closestEnemy == null)
            {
                DrawLinearCurve();
            }
        }

        private void Flatten(Transform closestEnemy)
        {
            Creature creature = closestEnemy.GetComponentInChildren<Creature>();
            creature.ragdoll.SetState(Ragdoll.State.Inert);

            //creature.ragdoll.parts.ForEach((RagdollPart x)
            //  => x.transform.localScale -= Time.deltaTime * new Vector3(0, 0, .5f));
            foreach (RagdollPart ragdollPart in creature.ragdoll.parts)
            {
                if (creature.ragdoll.headPart.transform.localScale.z >= .01f)
                {
                    ragdollPart.transform.localScale -= Time.deltaTime * new Vector3(0, 0, .5f);

                }

                if (creature.ragdoll.headPart.transform.localScale.z <= .01f)
                {
                    if (!creature.isKilled)
                        creature.Kill();

                }   
            }
        }
       
        private void ShrinkHead(Transform closestEnemy)
        {
            Creature creature = closestEnemy.GetComponentInChildren<Creature>();
            creature.ragdoll.SetState(Ragdoll.State.Inert);


            if ((creature.ragdoll.headPart.transform.localScale.x >= .01f
                || creature.ragdoll.headPart.transform.localScale.y >= .01f
                || creature.ragdoll.headPart.transform.localScale.z >= 0.1f))
            {
                creature.ragdoll.headPart.transform.localScale -= Time.deltaTime * new Vector3(.5f, .5f, .5f);
            }
            headSize = creature.ragdoll.headPart.transform.localScale;

            if ((creature.ragdoll.headPart.transform.localScale.x <= .01f
                || creature.ragdoll.headPart.transform.localScale.y <= .01f
                || creature.ragdoll.headPart.transform.localScale.z <= 0.1f))
            {
                if (!creature.isKilled)
                    creature.Kill();
                if (!creature.ragdoll.headPart.isSliced)
                    creature.ragdoll.TrySlice(creature.ragdoll.headPart);
                creature.ragdoll.headPart.transform.localScale = headSize;
            }
            
        }

        private void GrowHead(Transform closestEnemy)
        {
            Creature creature = closestEnemy.GetComponentInChildren<Creature>();
            creature.ragdoll.SetState(Ragdoll.State.Inert);
            if (creature.ragdoll.headPart.transform.localScale.x <= 3f
                || creature.ragdoll.headPart.transform.localScale.y <= 3f
                || creature.ragdoll.headPart.transform.localScale.z <= 3f)
            {
                creature.ragdoll.headPart.transform.localScale += Time.deltaTime * new Vector3(.5f, .5f, .5f);
            }
            headSize = creature.ragdoll.headPart.transform.localScale;

            if (creature.ragdoll.headPart.transform.localScale.x >= 3f
                || creature.ragdoll.headPart.transform.localScale.y >= 3f
                || creature.ragdoll.headPart.transform.localScale.z >= 3f)
            {
                creature.Kill();

                if (!creature.ragdoll.headPart.isSliced)
                    creature.ragdoll.TrySlice(creature.ragdoll.headPart);
                creature.ragdoll.headPart.transform.localScale = headSize;
            }
          
        }

        private List<Transform> DetectEnemies()
        {
            //detects creatures near to the player. if the creature is dead or is the player it ignores the creature.

            Collider[] potentialCreatureTransforms = Physics.OverlapSphere(item.transform.position, range);
            List<Transform> creatureTransforms = new List<Transform>();
            foreach (Collider col in potentialCreatureTransforms)
            {

                Creature creature = col.GetComponentInParent<Creature>() ?? null;
                if (creature != null && !creature.isPlayer)
                {
                    creatureTransforms.Add(col.transform.root);
                }
            }
            return creatureTransforms;
        }

        private Transform GetClosestEnemy(List<Transform> enemies, Transform fromThis)
        {

            Transform bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPosition = fromThis.position;
            foreach (Transform potentialTarget in enemies)
            {
                Vector3 directionToTarget = potentialTarget.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {

                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget;

                }
            }
            return bestTarget;
        }

        private void DrawQuadraticCurve()
        {
            float t = 0;
            float increment = 1 / (float)numPoints;
            for (int i = 1; i < numPoints + 1; i++)
            {
                t += increment;
                positions[i - 1] = QuadraticCurve(t, p0.position, p1.position, p2.position);
            }
            lr.SetPositions(positions);
        }

        //Draws straight line when no enemies detected
        private void DrawLinearCurve()
        {
            float t = 0;
            float increment = 1 / (float)numPoints;
            for (int i = 1; i < numPoints + 1; i++)
            {
                t += increment;
                positions[i - 1] = LinearCurve(t, p0.position, p1.position);

            }
            lr.SetPositions(positions);
        }

        // calculates a Quadratic bezier curve, p2 will always be position of nearest enemy unless there is no enemies in range
        private Vector3 QuadraticCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {

            //(1-t)^2*P0 + 2(1-t)*t*P1 + t^2 * P2
            float u = 1 - t; //(1-t)
            float tt = t * t;// t^2
            float uu = u * u;//(1-t)^2
            Vector3 p = uu * p0;
            p += 2 * u * t * p1;
            p += tt * p2;
            return p;
        }

        // if no enemies in range draw straight line forward
        private Vector3 LinearCurve(float t, Vector3 p0, Vector3 p1)
        {
            return p0 + t * (p1 - p0);
        }
    }
}
