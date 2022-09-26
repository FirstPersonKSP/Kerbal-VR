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

			pinchIndex.onUpdate += OnUpdatePinch;
			pinchThumb.onUpdate += OnUpdatePinch;

			collider = gameObject.AddComponent<SphereCollider>();
			collider.isTrigger = true;
			collider.radius = hand.profile.pinchColliderSize;

			rigidBody = gameObject.AddComponent<Rigidbody>();
			rigidBody.useGravity = false;
			rigidBody.isKinematic = true;

#if PINCH_GIZMOS
			gameObject.AddComponent<ColliderVisualizer>();
			var pinchGizmo = Utils.CreateGizmo();
			pinchGizmo.transform.SetParent(transform, false);
#endif
		}

		private void OnUpdatePinch(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
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
				hand.FingertipEnabled = false;
			}
			else if (!isPinching && heldInteractable != null)
			{
				heldInteractable.OnRelease(hand);
				heldInteractable = null;
				hand.FingertipEnabled = true;
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