using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PanMig.FirstPersonUnityCamera
{
    [RequireComponent(typeof(Rigidbody))]
    public class FPMovementController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4.0f;
        [SerializeField] private float runSpeed = 7.0f;
        [SerializeField] private bool jumpingEnabled = true;
        [SerializeField] private float jumpSpeed = 7.5f;
        [Range(0.0f, 0.5f)]
        [SerializeField] private float fallRate = 0.1f;
        [SerializeField] private bool slopeLimitEnabled = false;
        [SerializeField] private float slopeLimit = 90.0f;

        [Header("Sounds")]
        [SerializeField] private AudioClip footstepClip = null;
        [SerializeField] private AudioClip landingClip = null;
        [SerializeField] private AudioClip jumpingClip = null;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float footstepsVolume = 0.0f;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float footstepsPitch = 0.0f;

        private bool isRunning = false;
        private bool isJumping = false;
        private bool isLanding = false;
        private bool canJump = true;
        private bool prevGrounded = false;
        private bool grounded;

        private Rigidbody rb;
        private CapsuleCollider _capsule;
        private float horizontalMovement;
        private float verticalMovement;
        private Vector3 moveDirection;
        private float distanceToPoints;
        private CameraHeadBob headBob;
        private Vector3 YAxisGravity;
        private AudioSource audioSource;

        #region Properties
        public bool IsRunning
        {
            get
            {
                return isRunning;
            }

            set
            {
                isRunning = value;
            }
        }

        public bool IsLanding
        {
            get
            {
                return isLanding;
            }

            set
            {
                isLanding = value;
            }
        }
        #endregion

        // Use this for initialization
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            _capsule = GetComponent<CapsuleCollider>();
            headBob = transform.GetComponentInChildren<CameraHeadBob>();
            audioSource = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {
            // Jumping
            if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && jumpingEnabled && canJump)
            {
                isJumping = true;
                Jump();
                PlayJumpingSound();
            }

            horizontalMovement = 0; verticalMovement = 0;

            // Calculate FPcontroller movement direction through WASD and arrow key Inputs
            if (AllowMovement(transform.right * Input.GetAxis("Horizontal")))
            {
                horizontalMovement = Input.GetAxis("Horizontal");
            }
            if (AllowMovement(transform.forward * Input.GetAxis("Vertical")))
            {
                verticalMovement = Input.GetAxis("Vertical");
            }
            // Normalize vector so movement in two axes simultaneously is balanced.
            moveDirection = (horizontalMovement * transform.right + verticalMovement * transform.forward).normalized;

            // Toggle run & jump
            IsRunning = Input.GetKey(KeyCode.LeftShift);

            if (!prevGrounded && IsGrounded())
            {
                StartCoroutine(headBob.LandingBob());
                PlayLandingSound();
            }

            prevGrounded = IsGrounded();
        }

        private void FixedUpdate()
        {
            // When calculating the moveDirection, the Y velocity always stays 0. 
            // As a result, the player falls very slowly. 
            // To solve this, we add the Y axis velocity to the Rigidbody velocity.

            YAxisGravity = new Vector3(0, rb.velocity.y - fallRate, 0);
            if (!isJumping) { Move(); }
            rb.velocity += YAxisGravity;
        }

        #region Player Movement

        public void Move()
        {
            if (!IsRunning)
            {
                rb.velocity = moveDirection * walkSpeed * Time.fixedDeltaTime * 100;
            }
            else
            {
                rb.velocity = moveDirection * runSpeed * Time.fixedDeltaTime * 100;
            }

            PlayFootStepsSound();
        }

        public void Jump()
        {
            if (canJump)
            {
                rb.AddForce(new Vector3(0, jumpSpeed, 0), ForceMode.Impulse);
            }
        }
        #endregion

        #region Sounds
        public void PlayFootStepsSound()
        {
            if (IsGrounded() && rb.velocity.magnitude > 5.0f && !audioSource.isPlaying)
            {
                if (IsRunning)
                {
                    audioSource.volume = footstepsVolume;
                    //audioSource.pitch = Random.Range(footstepspitch, footstepspitch + 0.15f);
                    audioSource.pitch = footstepsPitch;
                }
                else
                {
                    audioSource.volume = footstepsVolume / 2;
                    audioSource.pitch = .8f;
                }

                audioSource.PlayOneShot(footstepClip);
            }
        }

        public void PlayLandingSound()
        {
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(landingClip);
            }
        }

        public void PlayJumpingSound()
        {
            audioSource.PlayOneShot(jumpingClip);
        }
        #endregion

        #region RayCasting

        // Make a capsule cast to check whether there is an obstacle in front
        // of the player, but ONLY when jumping.
        public bool AllowMovement(Vector3 castDirection)
        {
            Vector3 point1;
            Vector3 point2;

            if (!IsGrounded())
            {
                // The distance from the bottom of the capsule to the top
                distanceToPoints = _capsule.height / 2 - _capsule.radius;
                // Top and bottom capsule points, respectively.
                // transform.position is used to get points relative to 
                // the capsule's local space.
                point1 = transform.position + _capsule.center + Vector3.up * distanceToPoints;
                point2 = transform.position + _capsule.center + Vector3.down * distanceToPoints;
                float radius = _capsule.radius * .95f;
                float capsuleCastDist = 0.1f;

                if (Physics.CapsuleCast(point1, point2, radius, castDirection, capsuleCastDist))
                {
                    return false;
                }
            }

            if (slopeLimitEnabled && IsGrounded())
            {
                float castDist = _capsule.height;
                RaycastHit hit;
                if (Physics.Raycast(transform.position + _capsule.center, Vector3.down, out hit, castDist)
                    && IsGrounded())
                {
                    float currentSlope = Vector3.Angle(hit.normal, transform.forward) - 90.0f;
                    if (currentSlope > slopeLimit)
                    {
                        canJump = false;
                        return false;
                    }
                }
            }

            canJump = true;
            return true;
        }

        // Make a sphere cast with down direction to determine whether the player is touching the ground.
        public bool IsGrounded()
        {
            Vector3 capsule_bottom = transform.position + _capsule.center + Vector3.down * distanceToPoints;
            float radius = 0.1f;
            float maxDist = 1.0f;
            RaycastHit hitInfo;

            if (Physics.SphereCast(capsule_bottom, radius, Vector3.down, out hitInfo, maxDist))
            {
                isJumping = false;
                return true;
            }

            return false;
        }
        #endregion
    }
}