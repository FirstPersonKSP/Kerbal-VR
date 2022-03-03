using System;
using UnityEngine;
using Valve.VR;

namespace KerbalVR
{
    public class PinchCollider : MonoBehaviour
    {
        protected SteamVR_Action_Boolean_Source pinchIndex;
        protected SteamVR_Action_Boolean_Source pinchThumb;

        protected Hand hand;

        protected SphereCollider collider;
        protected Rigidbody rigidBody;

        IPinchInteractable hoveredInteractable;
        IPinchInteractable heldInteractable;

        internal void Initialize(Hand hand)
        {
            this.hand = hand;

            pinchIndex = SteamVR_Input.GetBooleanAction("default", "PinchIndex")[hand.handType];
            pinchIndex.onChange += OnChangePinch;

            pinchThumb = SteamVR_Input.GetBooleanAction("default", "PinchThumb")[hand.handType];
            pinchThumb.onChange += OnChangePinch;

            collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.02f;
            collider.center = new Vector3(0.01f, -0.02f, -0.01f);

            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;

#if PINCH_GIZMOS
            var handGizmo = Utils.CreateGizmo();
            handGizmo.transform.SetParent(transform, false);
            gameObject.AddComponent<ColliderVisualizer>();
#endif
        }

        private void Update()
        {
            if (heldInteractable != null)
            {
                heldInteractable.OnHold(hand);
            }
        }

        private void OnChangePinch(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        {
            bool isPinching = pinchIndex.state && pinchThumb.state;

            if (isPinching && hoveredInteractable != null)
            {
                heldInteractable = hoveredInteractable;
                heldInteractable.OnPinch(hand);
            }
            else if (!isPinching && heldInteractable != null)
            {
                heldInteractable.OnRelease(hand);
                heldInteractable = null;
            }
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (hoveredInteractable == null && other.gameObject.layer == 20)
            {
                hoveredInteractable = other.gameObject.GetComponent<IPinchInteractable>();
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (hoveredInteractable != null && hoveredInteractable.GameObject == other.gameObject)
            {
                hoveredInteractable = null;
            }
        }

    }
}