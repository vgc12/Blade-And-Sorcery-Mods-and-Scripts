using UnityEngine;
using ThunderRoad;




namespace CameraZoom
{
    class CameraZoom : MonoBehaviour
    {
        protected Item item;
        private Camera camera;
        private Handle mainHandle;
        private bool spellButtonDown;
        private bool isZoomed = false;
        private AudioSource zoomSound;
        private float buttonDownTime;
        protected ItemModuleCameraZoom module; 

        // i called this camera zoom because technically thats what this does, but really its meant as a variable 
        // zoom script for scopes

        protected void Start()
        {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleCameraZoom>();
            if (!string.IsNullOrEmpty(module.cameraRef))
                camera = item.GetCustomReference(module.cameraRef).GetComponent<Camera>();
            if (!string.IsNullOrEmpty(module.handleRef))
                mainHandle = item.GetCustomReference(module.handleRef).GetComponent<Handle>();
            if (!string.IsNullOrEmpty(module.zoomSound))
                zoomSound = item.GetCustomReference(module.zoomSound).GetComponent<AudioSource>();
            if (camera != null)
                camera.fieldOfView = module.baseZoomFOV;
            if (mainHandle != null)
                item.OnHeldActionEvent += HeldActionEvent;
        }

        public void LateUpdate()
        {
            if (spellButtonDown)
            {
                buttonDownTime += Time.deltaTime;
                if (buttonDownTime >= module.holdToActivateTime)
                {
                    isZoomed = !isZoomed;
                    if (isZoomed)
                    {
                        ZoomIn();
                    }
                    else if (!isZoomed)
                    {
                        ZoomOut();
                    }
                    
                }
            }
            else if (!spellButtonDown)
            {
                buttonDownTime = 0.0f;
            }
        }

        private void ZoomOut()
        {
            if (camera != null)
            {
                if (zoomSound != null)
                    zoomSound.Play();
                camera.fieldOfView = module.baseZoomFOV;
                buttonDownTime = 0.0f;
            }
            return;
        }

        private void ZoomIn()
        {
            if (camera !=  null)
            {
                if (zoomSound !=  null)
                    zoomSound.Play();
                camera.fieldOfView = module.zoomedInFOV;
                buttonDownTime = 0.0f;
            }
            return;
        }

        public void HeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (!handle.Equals(mainHandle))
            {
                return;
            }
            if (action == Interactable.Action.AlternateUseStart)
            {
                spellButtonDown = true;

            }
            if (action != Interactable.Action.AlternateUseStop)
                return;
            spellButtonDown = false;
            
        }
    }
}
