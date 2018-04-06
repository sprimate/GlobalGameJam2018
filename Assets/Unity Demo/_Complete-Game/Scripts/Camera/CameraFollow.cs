using UnityEngine;
using System.Collections;

namespace CompleteProject
{
    public class CameraFollow : MonoSingleton<CameraFollow>
    {
        public Camera cam;
        public Transform target;            // The position that that camera will be following.
        public float smoothing = 5f;        // The speed with which the camera will be following.


        Vector3? offset;                     // The initial offset from the target.


        void Start ()
        {
            // Calculate the initial offset.
            if (target != null)
            {
            }
        }

        void FixedUpdate ()
        {
            if (target == null)
            {
                return;
            }

            if (offset == null)
            {
                offset = cam.transform.position - target.position;
            }
            // Create a postion the camera is aiming for based on the offset from the target.
            Vector3 targetCamPos = target.position + offset.Value;

            // Smoothly interpolate between the camera's current position and it's target position.
            cam.transform.position = Vector3.Lerp (cam.transform.position, targetCamPos, smoothing * Time.deltaTime);
        }
    }
}