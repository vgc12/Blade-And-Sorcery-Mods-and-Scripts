using Marai;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;

public class MaraiOrb : MonoBehaviour
{
    //Items and setup
    protected Item item;
    public Item swordItem; //Assigned when this object is spawned in game, see Marai.cs
    protected ItemModuleMaraiOrb module;

    //Weapon behavior
    private Transform swordTarget;
    private Transform defaultLookPos;
    private LayerMask creatureMask;
    private bool spinSword = false, retractSword = false;

    //Sword VFX
    public Animator swordAnimator;
    public ParticleSystem[] swordParticles;
    public MeshRenderer swordPhantomMesh;

    private RagdollHand weaponHand;
    private float retractTimer = 0;

    private Collider bladeCollider;

    private void Awake()
    {
        //Item Setup
        item = GetComponent<Item>();
        module = item.data.GetModule<ItemModuleMaraiOrb>();

        //Sword target isthe position the item should stay floating, Default look pos is the direction the sword should face when no enemies are around
        //or the ash of war mode is enabled
        swordTarget = item.GetCustomReference(module.RuneTarget).GetComponent<Transform>();
        defaultLookPos = item.GetCustomReference(module.DefaultLookPos).GetComponent<Transform>();


        item.OnUngrabEvent += Item_OnUnGrabEvent;
        item.OnHeldActionEvent += Item_OnHeldActionEvent;

    }

    private void Start()
    {
        //Sword collision detection components
        bladeCollider = swordItem.GetCustomReference("BladeCollider").GetComponent<Collider>();
        creatureMask = (1 << GameManager.GetLayer(LayerName.NPC) |
            1 << GameManager.GetLayer(LayerName.Ragdoll) |
            1 << GameManager.GetLayer(LayerName.BodyLocomotion) |
            1 << GameManager.GetLayer(LayerName.Avatar));

        //set up variables assinged to VFX on the sword, enables the red outline seen in game.
        swordAnimator = swordItem.GetComponentInChildren<Animator>();
        swordParticles = swordItem.GetComponentsInChildren<ParticleSystem>();
        PlayParticles(false);
        swordPhantomMesh = swordItem.GetCustomReference("MaraiPhantom").GetComponent<MeshRenderer>();

        //Enable Red outline on sword
        swordPhantomMesh.enabled = true;

    }



    private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
    {
        weaponHand = ragdollHand;
        Debug.Log(swordAnimator.GetBool("Spin") + " : Spin Parameter State");

        if (action == Interactable.Action.UseStart && !retractSword)
            spinSword = true;
        else if (action == Interactable.Action.UseStop && !retractSword)
            spinSword = false;
        else if (action == Interactable.Action.AlternateUseStart)
        {
            retractSword = true;
            spinSword = false;
        }

        PlayParticles(spinSword);
        swordAnimator.SetBool("Spin", spinSword);

    }


    private void Update()
    {

        // This makes collision more accurate and stops the sword from clipping through enemies
        swordItem.isFlying = true;
        swordItem.isMoving = true;


        //Get nearest creature if around
        Creature nearestCreature = null;
        nearestCreature = GetClosestEnemy(DetectEnemies(swordTarget.position, 5f, creatureMask), swordTarget);

        //Keep sword floating at transform point 
        if (retractSword)
            RetractSword();
        else
            swordItem.rb.velocity = 10 * (swordTarget.position - swordItem.transform.position);

        //point sword at enemy if near, else keep the sword stright forward, bending in the direction of where the player swings.
        swordItem.transform.LookAt(nearestCreature != null && !nearestCreature.isKilled && !spinSword && !retractSword ? nearestCreature.ragdoll.GetPart(RagdollPart.Type.Torso).transform : defaultLookPos, Vector3.up);

        //If ash of war is enabled, slice enemy limbs. 
        if (spinSword)
        {
            Collider[] enemyCols = Physics.OverlapBox(bladeCollider.transform.position, bladeCollider.transform.localScale / 2, bladeCollider.transform.rotation, creatureMask);
            if (enemyCols != null)
                SliceRagdollPart(GetRagdollParts(GetColliderGroups(enemyCols)));

            if (weaponHand.playerHand == Player.local.handRight)
                PlayerControl.handRight.HapticShort(1f);
            else
                PlayerControl.handLeft.HapticShort(1f);
        }

    }

    private void PlayParticles(bool play)
    {
        //Plays and stops the ParticleSystems on the sword.
        if (play)
        {
            swordParticles.ToList().ForEach(x => x.Play());
            return;
        }
        swordParticles.ToList().ForEach(x => x.Stop());

    }

    private List<Creature> DetectEnemies(Vector3 overlapPos, float range, LayerMask layerMask)
    {
        //Detects Creatures near to the player. if the Creature is Player it ignores the Creature.

        Collider[] potentialCreatureTransforms = Physics.OverlapSphere(overlapPos, range, layerMask);
        List<Creature> creatures = new List<Creature>();
        foreach (Collider col in potentialCreatureTransforms)
        {

            Creature creature = col.GetComponentInParent<Creature>() ?? null;
            if (creature != null && !creature.isPlayer)
            {
                creatures.Add(creature);
            }
        }
        return creatures;
    }

    private Creature GetClosestEnemy(List<Creature> enemies, Transform fromThis)
    {
        //Calculate what enemy is closest based off the List of creatures deteced from DetectEnemies()
        Creature bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = fromThis.position;
        foreach (Creature potentialTarget in enemies)
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {

                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;

            }
        }
        return bestTarget;
    }

    private void SliceRagdollPart(List<RagdollPart> ragdollParts)
    {
        //slices all ragdollparts that the boxoverlap detected
        foreach (RagdollPart ragdollPart in ragdollParts)
        {
            if (ragdollPart.sliceAllowed && !ragdollPart.ragdoll.creature.player)
            {
                if (ragdollPart.type == RagdollPart.Type.Head)
                    ragdollPart.ragdoll.creature.Kill();

                ragdollPart.TrySlice();
                break;
            }
        }
    }

    private List<RagdollPart> GetRagdollParts(List<ColliderGroup> colliderGroups)
    {
        //gets ragdollparts from the collidergroups
        List<RagdollPart> ragdollParts = new List<RagdollPart>();
        foreach (ColliderGroup CG in colliderGroups)
        {
            if (CG.collisionHandler.ragdollPart != null)
            {
                ragdollParts.Add(CG.collisionHandler.ragdollPart);
            }
        }

        return ragdollParts;
    }

    private List<ColliderGroup> GetColliderGroups(Collider[] colliders)
    {
        //Gets the ColliderGroups from the colliders detected from the OverlapBox
        List<ColliderGroup> colliderGroups = new List<ColliderGroup>();
        foreach (Collider col in colliders)
        {

            if (col.GetComponentInParent<ColliderGroup>() != null)
            {
                colliderGroups.Add(col.GetComponentInParent<ColliderGroup>());
            }
        }
        return colliderGroups;
    }

    //Despawn orbitem when player lets go for any reason.
    private void Item_OnUnGrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing) => DespawnItem();


    private void RetractSword()
    {
        // Pulls sword from transform point swordTarget back to the players hand

        retractTimer += 3f * Time.deltaTime;
        swordItem.transform.position = Vector3.Lerp(swordTarget.position, weaponHand.transform.position, retractTimer);

        Collider[] potentialHandCols = Physics.OverlapSphere(swordItem.transform.position, .3f);
        bool grabSword = false;

        foreach (Collider col in potentialHandCols)
        {
            if (grabSword) break;
            grabSword = col.GetComponentInParent<Creature>() != null && col.GetComponentInParent<Creature>() == Player.currentCreature;
        }

        if (weaponHand.playerHand == Player.local.handRight && grabSword)
        {
            Player.local.creature.handRight.UnGrab(false);
            Player.local.creature.handRight.Grab(swordItem.GetMainHandle(Side.Right), true);
        }
        else if (weaponHand.playerHand == Player.local.handLeft && grabSword)
        {
            Player.local.creature.handLeft.UnGrab(false);
            Player.local.creature.handLeft.Grab(swordItem.GetMainHandle(Side.Left), true);
        }
    }

    private void DespawnItem()
    {
        swordAnimator.SetBool("Spin", false);
        spinSword = false;
        item.GetComponentsInChildren<MeshRenderer>().ToList().ForEach(x => x.enabled = false);
        swordItem.isFlying = false;
        swordPhantomMesh.enabled = false;
        PlayParticles(false);
        item.Despawn();
    }
}
