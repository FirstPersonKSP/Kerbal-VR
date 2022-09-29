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

		public bool InteractionsEnabled
		{
			get { return enabled; }
			set
			{
				collider.enabled = value;
				enabled = value;
			}
		}

		#endregion

		#region Private Members
		public SphereCollider collider;
		protected Rigidbody fingertipRigidbody;
		#endregion

		internal void Initialize(Hand hand)
		{
			this.hand = hand;

			fingertipRigidbody = this.gameObject.AddComponent<Rigidbody>();
			fingertipRigidbody.isKinematic = true;
			collider = this.gameObject.AddComponent<SphereCollider>();
			collider.isTrigger = true;
			collider.radius = 0.001f; // this will be modified by Hand

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