using System;
using System.Collections;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	
	// this will probably eventually be moved to Common so that we can have rails on the exterior too
	public class VRHandRail : InternalModule
	{
		[KSPField]
		public string railTransformName = string.Empty;

		InteractableBehaviour m_interactable;

		Vector3 m_grabbedPositon;
		Quaternion m_grabbedRotation;
		Vector3 velocity;

		void Start()
		{
			var transform = this.FindTransform(railTransformName);
			if (transform != null)
			{
				var colliderTransform = transform.GetComponentInChildren<Collider>();
				if (colliderTransform != null)
				{
					m_interactable = Utils.GetOrAddComponent<InteractableBehaviour>(colliderTransform.gameObject);
					m_interactable.OnGrab += Grabbed;
					m_interactable.OnRelease += Released;
				}
				else
				{
					Utils.LogError($"VRHandRail: No collider under transform ${railTransformName} found for prop {internalProp.propName}");
				}
			}

			enabled = false;
		}

		private void Released(Hand hand)
		{
			FreeIva.KerbalIvaAddon.KerbalIva.FreezeUpdates = false;
			FreeIva.KerbalIvaAddon.KerbalIva.KerbalRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			FreeIva.KerbalIvaAddon.KerbalIva.KerbalRigidbody.velocity = velocity;
			FreeIva.KerbalIvaAddon.KerbalIva.KerbalRigidbody.WakeUp();
			velocity = Vector3.zero;
			enabled = false;
		}

		private void Grabbed(Hand hand)
		{
			FreeIva.KerbalIvaAddon.Instance.Unbuckle();

			m_grabbedPositon = hand.GripPosition;
			m_grabbedRotation = hand.transform.rotation;

			if (!FreeIva.KerbalIvaAddon.KerbalIva.FreezeUpdates)
			{
				enabled = true;
				FreeIva.KerbalIvaAddon.KerbalIva.FreezeUpdates = true;
				FreeIva.KerbalIvaAddon.KerbalIva.KerbalRigidbody.interpolation = RigidbodyInterpolation.None;
			}
		}

		static float x_gain = 10f;

		void FixedUpdate()
		{
			if (m_interactable.IsGrabbed)
			{
				Vector3 offset = m_interactable.GrabbedHand.GripPosition - m_grabbedPositon;
				var rigidBody = FreeIva.KerbalIvaAddon.KerbalIva.KerbalRigidbody;
				rigidBody.MovePosition(rigidBody.position - offset);

				velocity = (x_gain * -offset / Time.fixedDeltaTime + velocity) / (1 + x_gain);
			}
		}
	}
}
