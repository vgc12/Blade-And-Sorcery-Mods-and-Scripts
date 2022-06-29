using ThunderRoad;


namespace SimpleGrenadeFramework
{
    public class ItemModuleSimpleGrenade : ItemModule
    {

        public float explosionForce;
        public float explosionMult;
        
        public string particleRef;
        public string explosionSound;

        public string lineRendererOne;
        public string lineRendererTwo;
        public string meshRef;
        //public string LineRendererTwo;



        public override void OnItemLoaded(Item item)
        {
            
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<Claymore>();
        }
    }
}
