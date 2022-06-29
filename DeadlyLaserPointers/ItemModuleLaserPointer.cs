using ThunderRoad;

namespace LaserPointerScript.Shared
{

    // initilization class for Blade and sorcery
    public class ItemModuleLaserPointer : ItemModule
    {
        public string lineRenderer;
        public string clickSound;
        public string laserType;
        public string animatorRef;
   
        


        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            if (laserType == "Full")
                item.gameObject.AddComponent<LaserPointerFull>();
            if (laserType == "Explode")
                item.gameObject.AddComponent<LaserPointerExplode>();
            if (laserType == "Single")
                item.gameObject.AddComponent<LaserPointerSingle>();
  
        }
    }
}
