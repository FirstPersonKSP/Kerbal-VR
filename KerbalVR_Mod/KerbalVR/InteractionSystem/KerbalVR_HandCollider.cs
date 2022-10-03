using UnityEngine;

namespace KerbalVR
{
	public class HandCollider : MonoBehaviour
	{
		public InteractableBehaviour HoveredObject { get; private set; }

		public SphereCollider collider;
		protected Rigidbody handRigidbody;
		protected VRLadder ladder;

		internal void Initialize(Hand hand, VRLadder ladder)
		{
			// add interactable collider
			collider = this.gameObject.AddComponent<SphereCollider>();
			collider.isTrigger = true;
			collider.radius = 0.001f; // this will be modified by Hand

			handRigidbody = this.gameObject.AddComponent<Rigidbody>();
			handRigidbody.useGravity = false;
			handRigidbody.isKinematic = true;

			this.ladder = ladder;

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
			if (interactable != null && interactable.enabled)
			{
				HoveredObject = interactable;
			}
			else if (other.CompareTag(VRLadder.COLLIDER_TAG))
			{
				HoveredObject = ladder;
				ladder.LadderTransform = other.transform;
			}
		}

		protected void OnTriggerExit(Collider other)
		{
			if (HoveredObject != null)
			{
				InteractableBehaviour interactable = other.gameObject.GetComponent<InteractableBehaviour>();
				if (interactable != null && interactable == HoveredObject)
				{
					HoveredObject = null;
				}
				if (other.transform == ladder.LadderTransform)
				{
					HoveredObject = null;
					ladder.LadderTransform = null;
				}
			}
		}
	}
}