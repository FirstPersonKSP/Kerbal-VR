using UnityEngine;
using Valve.VR;

namespace KerbalVR
{
    /// <summary>
    /// A helper class to collect objects that collide with the index fingertip.
    /// </summary>
    public class FingertipCollider : MonoBehaviour
    {

        #region Properties
        public Hand hand;

        public Vector3 FingertipCenter
        {
            get { return fingertipCollider.transform.TransformPoint(fingertipCollider.center); }
        }

		public float FingertipRadius => fingertipCollider.radius;

        public bool InteractionsEnabled
        {
            get { return enabled; }
            set
            {
                fingertipCollider.enabled = value;
                enabled = value;
            }
        }

        #endregion

        #region Private Members
        protected SphereCollider fingertipCollider;
        protected Rigidbody fingertipRigidbody;
        #endregion

        internal void Initialize(Hand hand)
        {
            this.hand = hand;

            fingertipRigidbody = this.gameObject.AddComponent<Rigidbody>();
            fingertipRigidbody.isKinematic = true;
            fingertipCollider = this.gameObject.AddComponent<SphereCollider>();
            fingertipCollider.isTrigger = true;
            fingertipCollider.radius = 0.005f;

#if FINGER_GIZMOS
            gameObject.AddComponent<ColliderVisualizer>();
            var fingertipGizmo = Utils.CreateGizmo();
            fingertipGizmo.transform.SetParent(transform, false);
#endif
        }

        protected void OnTriggerEnter(Collider other)
        {
            // only interact with layer 20 (Internal Space) objects
            if (other.gameObject.layer == 20)
            {
                var interactable = other.gameObject.GetComponent<IFingertipInteractable>();
                if (interactable != null)
                {
                    interactable.OnEnter(hand, other);
                }
            }
        }

        protected void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == 20)
            {
                var interactable = other.gameObject.GetComponent<IFingertipInteractable>();
                if (interactable != null)
                {
                    interactable.OnStay(hand, other);
                }
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == 20)
            {
                var interactable = other.gameObject.GetComponent<IFingertipInteractable>();
                if (interactable != null)
                {
                    interactable.OnExit(hand, other);
                }
            }
        }
    }
}