using UnityEngine;

namespace KerbalVR
{
	public class HandCollider : MonoBehaviour
	{
		public InteractableBehaviour HoveredObject { get; private set; }
		public VRSeatBehaviour HoveredSeat => hoveredSeat;

		public SphereCollider collider;
		protected Rigidbody handRigidbody;
		protected VRLadder ladder;
		protected VRSeatBehaviour hoveredSeat;

		bool hoveredObjectIsHighPriority;

		Hand hand;

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
			this.hand = hand;

			// debugging stuff
#if HAND_GIZMOS
			gameObject.AddComponent<ColliderVisualizer>();
			var handGizmo = Utils.CreateGizmo();
			handGizmo.transform.SetParent(transform, false);
#endif
		}

		public void ClearHoveredObject()
		{
			HoveredObject = null;
			ladder.LadderTransform = null;
			hoveredSeat = null;
		}

		protected void OnTriggerEnter(Collider other)
		{
			int otherLayer = other.gameObject.layer;
			if (Scene.IsInIVA())
			{
				if (otherLayer != 16 && otherLayer != 20) return;
			}
			else
			{
				if (otherLayer != 21) return; // TODO: what about just grabbing anywhere on the outside of a part?
			}

			if (hand.heldObject != null) return;

			InteractableBehaviour interactable = other.gameObject.GetComponent<InteractableBehaviour>();
			if (interactable != null && interactable.enabled)
			{
				HoveredObject = interactable;
				hoveredObjectIsHighPriority = true;
			}
			else if (other.CompareTag(VRLadder.COLLIDER_TAG))
			{
				HoveredObject = ladder;
				ladder.LadderTransform = other.transform;
				hoveredObjectIsHighPriority = true;
			}
			else
			{
				var seat = other.gameObject.GetComponent<VRSeatBehaviour>();
				if (seat != null)
				{
					hoveredSeat = seat;
					hoveredObjectIsHighPriority = true;
				}
				else if (other.gameObject.layer == 16 && !hoveredObjectIsHighPriority && !FreeIva.KerbalIvaAddon.Instance.buckled && other.gameObject != FreeIva.KerbalIvaAddon.Instance.KerbalIva.gameObject)
				{
					// all other layer-16 things are grabbable as ladders when unbuckled, as long as you're not already hovering over something more important
					HoveredObject = ladder;
					ladder.LadderTransform = other.transform;
					hoveredObjectIsHighPriority = false;
				}
			}
		}

		void OnTriggerStay(Collider other)
		{
			if (HoveredObject == null)
			{
				OnTriggerEnter(other);
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
					hoveredObjectIsHighPriority = false;
				}
				if (other.transform == ladder.LadderTransform && HoveredObject == ladder)
				{
					HoveredObject = null;

					if (hand.heldObject != ladder)
					{
						ladder.LadderTransform = null;
					}

					hoveredObjectIsHighPriority = false;
				}
			}

			if (hoveredSeat != null && hoveredSeat.transform == other.transform)
			{
				hoveredSeat = null;
				hoveredObjectIsHighPriority = false;
			}
		}
	}
}