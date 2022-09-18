﻿using UnityEngine;

namespace KerbalVR
{
    public class HandCollider : MonoBehaviour
    {
        public InteractableBehaviour HoveredObject { get; private set; }

        protected SphereCollider handCollider;
        protected Rigidbody handRigidbody;

        internal void Initialize(Hand hand)
        {
            // add interactable collider
            handCollider = this.gameObject.AddComponent<SphereCollider>();
            handCollider.isTrigger = true;
            handCollider.radius = hand.profile.gripColliderSize;

            handRigidbody = this.gameObject.AddComponent<Rigidbody>();
            handRigidbody.useGravity = false;
            handRigidbody.isKinematic = true;

            // debugging stuff
#if HAND_GIZMOS
            gameObject.AddComponent<ColliderVisualizer>();
            var handGizmo = Utils.CreateGizmo();
            handGizmo.transform.SetParent(transform, false);
#endif
        }

        protected void OnTriggerEnter(Collider other)
        {
            InteractableBehaviour interactable = other.gameObject.GetComponent<InteractableBehaviour>();
            if (interactable != null)
            {
                HoveredObject = interactable;
            }
            else if (other.CompareTag(VRLadder.COLLIDER_TAG))
            {
                var vrLadder = gameObject.GetComponentUpwards<VRLadder>();
                HoveredObject = vrLadder;
                vrLadder.LadderTransform = other.transform;
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (HoveredObject != null)
            {
                InteractableBehaviour interactable = other.gameObject.GetComponent<InteractableBehaviour>();
                if (interactable == HoveredObject)
                {
                    HoveredObject = null;
                }
            }
        }
    }
}