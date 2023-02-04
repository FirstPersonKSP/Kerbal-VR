using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.InternalModules
{
	internal class VRPhysicalProp : InternalModule
	{
		[SerializeField]
		InteractableBehaviour m_interactableBehaviour;

		Rigidbody m_rigidBody;

		[SerializeField]
		Collider m_collider;
		bool m_otherHandGrabbed = false;

		bool m_applyGravity = false;

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);

			if (HighLogic.LoadedScene == GameScenes.LOADING)
			{
				var colliderNode = node.GetNode("COLLIDER");
				if (colliderNode != null)
				{
					object obj = new ColliderParams();
					ConfigNode.LoadObjectFromConfig(obj, colliderNode);
					var colliderParams = (ColliderParams)obj;

					m_collider = colliderParams.Create(transform);
				}
				else
				{
					m_collider = GetComponentInChildren<Collider>();
				}

				if (m_collider != null)
				{
					m_collider.isTrigger = false;

					m_interactableBehaviour = m_collider.gameObject.AddComponent<InteractableBehaviour>();
					m_interactableBehaviour.AttachHandOnGrab = false;
					
					m_collider.gameObject.layer = 16; // needs to be 16 to bounce off shell colliders, at least while moving.  Not sure if we want it interacting with the player.
				}
				else
				{
					Utils.LogError($"VRPhysicalProp: prop {internalProp.propName} does not have a collider");
				}
			}
		}

		void Start()
		{
			if (m_interactableBehaviour != null)
			{
				m_interactableBehaviour.OnGrab += OnGrab;
				m_interactableBehaviour.OnRelease += OnRelease;
				m_interactableBehaviour.OnOtherHandGrab += OnOtherHandGrab;
			}
		}

		private void OnOtherHandGrab(Hand hand)
		{
			m_otherHandGrabbed = true;
		}

		private void OnRelease(Hand hand)
		{
			if (m_otherHandGrabbed) return;

			transform.SetParent(internalModel.transform, true); // TODO: freeiva centrifuge?

			if (m_rigidBody == null)
			{
				m_rigidBody = gameObject.AddComponent<Rigidbody>();
			}

			m_rigidBody.isKinematic = true;
			m_rigidBody.useGravity = false;


			m_rigidBody.isKinematic = false;
			m_rigidBody.WakeUp();


			m_collider.enabled = true;



			m_rigidBody.velocity = KerbalVR.InteractionSystem.Instance.transform.TransformVector(hand.handActionPose[hand.handType].lastVelocity);

			// TODO: switch back to kinematic when it comes to rest (or not? it's fun to kick around)

			m_applyGravity = true;
		}

		private void OnGrab(Hand hand)
		{
			m_otherHandGrabbed = false;
			transform.SetParent(hand.handObject.transform, true);

			// disable the collider so it doesn't push us around - or possibly we can just use Physics.IgnoreCollision
			m_collider.enabled = false;

			if (m_rigidBody != null)
			{
				m_rigidBody.isKinematic = true;
				m_applyGravity = false;
			}
		}

		void FixedUpdate()
		{
			if (m_applyGravity && FreeIva.KerbalIvaAddon.Instance.KerbalIva.UseRelativeMovement())
			{
				Vector3 accel = FreeIva.KerbalIvaAddon.Instance.GetFlightAccelerationInternalSpace();
				m_rigidBody.AddForce(accel, ForceMode.Acceleration);
			}
		}
	}
}
