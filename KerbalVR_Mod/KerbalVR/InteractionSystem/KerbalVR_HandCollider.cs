using UnityEngine;

namespace KerbalVR
{
    public class HandCollider : MonoBehaviour
    {
        public InteractableBehaviour HoveredObject { get; private set; }

        protected SphereCollider handCollider;
        protected Rigidbody handRigidbody;

        protected void Awake()
        {
            // add interactable collider
            handCollider = this.gameObject.AddComponent<SphereCollider>();
            handCollider.isTrigger = true;
            handCollider.center = Hand.GripOffset;
            handCollider.radius = 0.08f;

            handRigidbody = this.gameObject.AddComponent<Rigidbody>();
            handRigidbody.useGravity = false;
            handRigidbody.isKinematic = true;

            // debugging stuff
#if HAND_GIZMOS
            var handGizmo = Utils.CreateGizmo();
            handGizmo.transform.SetParent(handObject.transform, false);
            handObject.AddComponent<ColliderVisualizer>();
#endif
        }

        protected void OnTriggerEnter(Collider other)
        {
            InteractableBehaviour interactable = other.gameObject.GetComponent<InteractableBehaviour>();
            if (interactable != null)
            {
                HoveredObject = interactable;
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