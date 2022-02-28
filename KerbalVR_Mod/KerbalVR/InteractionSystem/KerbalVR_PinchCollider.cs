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

        private void OnChangePinch(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        {
            
        }
    }
}