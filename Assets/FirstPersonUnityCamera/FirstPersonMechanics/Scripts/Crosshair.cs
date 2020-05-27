﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PanMig.FirstPersonUnityCamera
{
    [System.Serializable]
    public struct CrosshairSize
    {
        public Vector2 small;
        public Vector2 medium;
        public Vector2 big;
    }

    public class Crosshair : MonoBehaviour
    {
        // Sprites
        [Header("Icons")]
        [SerializeField] private Sprite pickUp = null;
        [SerializeField] private Sprite note = null;
        [SerializeField] private Sprite crosshair = null;
        public CrosshairSize crosshairSize = new CrosshairSize();
        [SerializeField] private InteractionRayCaster _raycaster;

        // Crosshair image
        private Image img;

        // Use this for initialization
        void Start()
        {
            _raycaster = Camera.main.GetComponent<InteractionRayCaster>();

            _raycaster.onTargetChange += ChangeCrosshair;
            _raycaster.onNoTarget += ChangeCrosshair;

            img = gameObject.GetComponent<Image>();
        }

        private void OnDisable()
        {
            _raycaster.onTargetChange -= ChangeCrosshair;
            _raycaster.onNoTarget -= ChangeCrosshair;
        }

        void ChangeCrosshair()
        {
            if (_raycaster.Hit.collider != null)
            {
                switch (_raycaster.Hit.collider.tag)
                {
                    case "pickUp":
                        SetIcon(pickUp);
                        SetSize(crosshairSize.medium);
                        break;
                    case "note":
                        SetIcon(note);
                        SetSize(crosshairSize.medium);
                        break;
                    default:
                        SetIcon(crosshair);
                        SetSize(crosshairSize.small);
                        break;
                }
            }
            else
            {
                SetIcon(crosshair);
                SetSize(crosshairSize.small);
                return;
            }
        }

        void SetIcon(Sprite icon)
        {
            img.sprite = icon;
        }

        void SetSize(Vector2 size)
        {
            img.GetComponent<RectTransform>().sizeDelta = size;
        }
    }
}