using ThunderRoad;


namespace CameraZoom
{
    public class ItemModuleCameraZoom : ItemModule
    {
        
        public string cameraRef;
        public string handleRef;
        public string zoomSound;
        public float holdToActivateTime;
        public float zoomedInFOV;
        public float baseZoomFOV;
        
        

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<CameraZoom>();
        }
    }
}
