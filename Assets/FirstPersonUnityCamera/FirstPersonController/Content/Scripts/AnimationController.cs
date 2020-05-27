using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PanMig.FirstPersonUnityCamera
{
    public class AnimationController : MonoBehaviour
    {
        public Animator animator;
        public FPMovementController _character;

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            // Animate only when the character is running.
            if (_character.IsRunning)
            {
                animator.SetBool("running", true);
            }
            else
            {
                animator.SetBool("running", false);
            }
        }
    }
}