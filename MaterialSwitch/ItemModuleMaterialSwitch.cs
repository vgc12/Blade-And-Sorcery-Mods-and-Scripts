using ThunderRoad;

namespace MaterialSwitch
{
    public class ItemModuleMaterialSwitch : ItemModule
    {
        public string meshRef;
        public bool useTrigger;
        


        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<MaterialSwitch>();
        }
    }
}
