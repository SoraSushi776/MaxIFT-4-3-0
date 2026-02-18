using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    [ExecuteInEditMode]
    public class PreviewCamera : MonoBehaviour
    {
        public Transform rotator;
        public Transform scale;
        public Camera cam;

        public void UpdateCam(CameraTrigger target)
        {
            if (target == null) return;
            if (rotator == null || scale == null || cam == null) return;
            rotator.localPosition = target.offset;
            rotator.localEulerAngles = target.rotation;
            scale.localScale = target.scale;
            cam.fieldOfView = target.fieldOfView;
        }
    }
}