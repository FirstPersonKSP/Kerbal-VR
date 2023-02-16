using UnityEngine;
using Valve.VR;

namespace KerbalVR
{
	public class PinchCollider : MonoBehaviour
	{
		protected SteamVR_Action_Boolean_Source pinchIndex;
		protected SteamVR_Action_Boolean_Source pinchThumb;

		protected Hand hand;

		public SphereCollider collider;
		protected Rigidbody rigidBody;

		IPinchInteractable hoveredInteractable;
		IPinchInteractable heldInteractable;

		bool wasPinching;

		internal void Initialize(Hand hand)
		{
			this.hand = hand;

			pinchIndex = SteamVR_Input.GetBooleanAction("default", "PinchIndex")[hand.handType];
			pinchThumb = SteamVR_Input.GetBooleanAction("default", "PinchThumb")[hand.handType];

			collider = gameObject.AddComponent<SphereCollider>();
			collider.isTrigger = true;
			collider.radius = 0.001f; // this will be modified by Hand

			rigidBody = gameObject.AddComponent<Rigidbody>();
			rigidBody.useGravity = false;
			rigidBody.isKinematic = true;

#if PINCH_GIZMOS
			gameObject.AddComponent<ColliderVisualizer>();
			var pinchGizmo = Utils.CreateGizmo();
			pinchGizmo.transform.SetParent(transform, false);
#endif
		}

		private void Update()
		{
			bool isPinching = IsPinching();

			if (isPinching && !wasPinching)
			{
				if (hoveredInteractable != null)
				{
					heldInteractable = hoveredInteractable;
					heldInteractable.OnPinch(hand);
					hand.FingertipEnabled = false;
				}
				else if (Scene.IsInIVA())
				{
					hand.SummonDialingWand();
				}
			}
			else if (!isPinching && wasPinching)
			{
				if (heldInteractable != null)
				{
					heldInteractable.OnRelease(hand);
					heldInteractable = null;
					hand.FingertipEnabled = true;
				}
				else if (Scene.IsInIVA())
				{
					hand.DismissDialingWand();
				}
			}
			else if (isPinching && wasPinching && heldInteractable != null)
			{
				heldInteractable.OnHold(hand);
			}

			wasPinching = isPinching;
		}

		public bool IsPinching()
		{
			if (hand.IsFingerTrackingPinching())
			{
				return true;
			}

			if (pinchIndex.state && pinchThumb.state)
			{
				return true;
			}

			return false;
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