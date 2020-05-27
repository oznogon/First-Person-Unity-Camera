using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PanMig.FirstPersonUnityCamera
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public float XMinRotation;
        public float XMaxRotation;
        [Range(1.0f, 10.0f)]
        public float Xsensitivity;
        [Range(1.0f, 10.0f)]
        public float Ysensitivity;
        private Camera cam;
        private float rotAroundX, rotAroundY;
        private bool camMoved;

        // Use this for initialization
        void Start()
        {
            cam = this.GetComponent<Camera>();
            rotAroundX = transform.eulerAngles.x;
            rotAroundY = transform.eulerAngles.y;
        }

        private void Update()
        {
            rotAroundX += Input.GetAxis("Mouse Y") * Xsensitivity;
            rotAroundY += Input.GetAxis("Mouse X") * Ysensitivity;

            // Clamp rotation values
            rotAroundX = Mathf.Clamp(rotAroundX, XMinRotation, XMaxRotation);

            CameraRotation();
        }

        private void CameraRotation()
        {
            // Rotation of the parent transform (player body)
            transform.parent.rotation = Quaternion.Euler(0, rotAroundY, 0);
            // Rotation of the camera's transform
            cam.transform.rotation = Quaternion.Euler(-rotAroundX, rotAroundY, 0);
        }
    }
}