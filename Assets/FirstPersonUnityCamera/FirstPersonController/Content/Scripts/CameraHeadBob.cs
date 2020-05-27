using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PanMig.FirstPersonUnityCamera
{
    [RequireComponent(typeof(Camera))]

    public class CameraHeadBob : MonoBehaviour
    {
        [Header("Movement Bobbing")]
        [SerializeField] private float bobbingSpeed = 0.1f;
        [SerializeField] private float bobbingAmount = 0.1f;
        [SerializeField] private float bobbingRunSpeed = 0.2f;
        [SerializeField] private float bobbingRunAmount = 0.15f;
        // The camera position when the head isn't bobbing.
        [SerializeField] private float restPosition = 1.0f;

        [Header("Jump Bobbing")]
        [SerializeField] private float jumpBobDuration = 0.2f;
        [SerializeField] private float jumpBobAmount = 0.3f;

        private float timer = 0.0f;
        private float offset;
        float horizontal;
        float vertical;
        float jump;
        Vector3 localPos;
        private FPMovementController _character;

        private void Awake()
        {
            _character = transform.parent.GetComponent<FPMovementController>();
        }

        void Update()
        {
            float waveslice = 0.0f;
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            jump = Input.GetAxis("Jump");

            localPos = transform.localPosition;

            if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
            {
                timer = 0.0f;
            }
            else if (!_character.IsLanding)
            {
                // If the character isn't landing from a jump or fall,
                // advance through the head bob cycle.
                waveslice = Mathf.Sin(timer);

                if (!_character.IsRunning)
                {
                    timer = timer + bobbingSpeed;
                }
                else
                {
                    timer = timer + bobbingRunSpeed;
                }

                if (timer > Mathf.PI * 2)
                {
                    timer = timer - (Mathf.PI * 2);
                }
            }

            if (waveslice != 0)
            {
                float translateChange;

                if (!_character.IsRunning)
                {
                    translateChange = waveslice * bobbingAmount;
                }
                else
                {
                    translateChange = waveslice * bobbingRunAmount;
                }

                float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
                totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
                translateChange = totalAxes * translateChange;
                localPos.y = restPosition + translateChange;
            }
            else
            {
                localPos.y = restPosition;
                localPos.y = restPosition + offset;
            }

            transform.localPosition = localPos;
        }

        public IEnumerator LandingBob()
        {
            // Make the camera move down slightly when landing from a jump or
            // fall.
            _character.IsLanding = true;
            float t = 0f;

            while (t < jumpBobDuration)
            {
                offset = Mathf.Lerp(0f, -jumpBobAmount, t / jumpBobDuration);
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            // Then move the camera back to a neutral position.
            t = 0f;
            while (t < jumpBobDuration)
            {
                offset = Mathf.Lerp(-jumpBobAmount, 0f, t / jumpBobDuration);
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            offset = 0f;
            _character.IsLanding = false;
        }
    }
}