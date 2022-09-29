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

			if (isPinching && !wasPinching && hoveredInteractable != null)
			{
				heldInteractable = hoveredInteractable;
				heldInteractable.OnPinch(hand);
				hand.FingertipEnabled = false;
			}
			else if (!isPinching && wasPinching && heldInteractable != null)
			{
				heldInteractable.OnRelease(hand);
				heldInteractable = null;
				hand.FingertipEnabled = true;
			}
			else if (isPinching && wasPinching && heldInteractable != null)
			{
				heldInteractable.OnHold(hand);
			}

			wasPinching = isPinching;
		}

		bool IsPinching()
		{
			// if we're running partial tracking, activate pinch whenever the fingertips are close together
			if (hand.handSkeleton.skeletalTrackingLevel >= EVRSkeletalTrackingLevel.VRSkeletalTracking_Partial)
			{
				var fingertipDistance = Vector3.Distance(hand.handSkeleton.indexTip.position, hand.handSkeleton.thumbTip.position);

				if (fingertipDistance <= collider.radius * 4.0f)
				{
					return true;
				}
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