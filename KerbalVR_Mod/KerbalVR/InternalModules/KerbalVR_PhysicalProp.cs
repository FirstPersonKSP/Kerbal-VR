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
		[KSPField]
		public bool isSticky = false; // when released, if this is overlapping another collider then it will attach to it

		[KSPField]
		public string placeSound = string.Empty;

		[SerializeField]
		InteractableBehaviour m_interactableBehaviour;
		[SerializeField]
		VRPhysicalPropCollisionTracker m_collisionTracker;

		Rigidbody m_rigidBody;

		[SerializeField]
		AudioSource m_audioSource;
		[SerializeReference]
		AudioClip m_grabAudioClip;
		[SerializeReference]
		AudioClip m_stickAudioClip;
		[SerializeReference]
		AudioClip m_impactAudioClip;

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

					m_collisionTracker = m_collider.gameObject.AddComponent<VRPhysicalPropCollisionTracker>();
					m_collisionTracker.PhysicalProp = this;
				}
				else
				{
					Utils.LogError($"VRPhysicalProp: prop {internalProp.propName} does not have a collider");
				}

				m_grabAudioClip = LoadAudioClip(node, "grabSound");
				m_stickAudioClip = LoadAudioClip(node, "stickSound");
				m_impactAudioClip = LoadAudioClip(node, "impactSound");

				if (m_grabAudioClip != null || m_stickAudioClip != null || m_impactAudioClip != null)
				{
					m_audioSource = m_collider.gameObject.AddComponent<AudioSource>();
					m_audioSource.volume = GameSettings.SHIP_VOLUME;
					m_audioSource.minDistance = 2;
					m_audioSource.maxDistance = 10;
					m_audioSource.playOnAwake = false;
				}
			}
		}

		AudioClip LoadAudioClip(ConfigNode node, string key)
		{
			string clipUrl = node.GetValue(key);
			if (clipUrl == null) return null;

			AudioClip result = GameDatabase.Instance.GetAudioClip(clipUrl);

			if (result == null)
			{
				Utils.LogError($"Failed to find audio clip {clipUrl} for prop {internalProp.propName}");
			}

			return result;
		}

		void PlayAudioClip(AudioClip clip)
		{
			if (clip == null) return;
			m_audioSource.PlayOneShot(clip);
		}

		public void OnImpact(float magnitude)
		{
			PlayAudioClip(m_impactAudioClip);
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

			if (isSticky && m_collisionTracker.ContactCollider != null)
			{
				if (m_rigidBody != null)
				{
					Component.Destroy(m_rigidBody);
					m_rigidBody = null;
				}

				PlayAudioClip(m_stickAudioClip);
				m_applyGravity = false;
			}
			else
			{
				if (m_rigidBody == null)
				{
					m_rigidBody = gameObject.AddComponent<Rigidbody>();
				}

				m_collider.isTrigger = false;
				m_collider.enabled = true;

				m_rigidBody.isKinematic = true;
				m_rigidBody.useGravity = false;


				m_rigidBody.isKinematic = false;
				m_rigidBody.WakeUp();

				m_rigidBody.velocity = KerbalVR.InteractionSystem.Instance.transform.TransformVector(hand.handActionPose[hand.handType].lastVelocity);

				// total hack?
				if (!FreeIva.KerbalIvaAddon.Instance.buckled)
				{
					FreeIva.KerbalIvaAddon.Instance.KerbalIva.KerbalRigidbody.AddForce(-m_rigidBody.velocity, ForceMode.VelocityChange);
				}

				m_applyGravity = true;
			}

			m_collider.enabled = true;

			// TODO: switch back to kinematic when it comes to rest (or not? it's fun to kick around)
		}

		private void OnGrab(Hand hand)
		{
			m_otherHandGrabbed = false;
			transform.SetParent(hand.handObject.transform, true);

			// disable the collider so it doesn't push us around - or possibly we can just use Physics.IgnoreCollision
			if (isSticky)
			{
				m_collider.isTrigger = true;
			}
			else
			{
				m_collider.enabled = false;
			}

			if (m_rigidBody != null)
			{
				m_rigidBody.isKinematic = true;
				m_applyGravity = false;
			}

			PlayAudioClip(m_grabAudioClip);
		}

		void FixedUpdate()
		{
			if (m_applyGravity && FreeIva.KerbalIvaAddon.Instance.KerbalIva.UseRelativeMovement())
			{
				Vector3 accel = FreeIva.KerbalIvaAddon.Instance.KerbalIva.GetInternalAcceleration();
				m_rigidBody.AddForce(accel, ForceMode.Acceleration);
			}
		}
	}

	internal class VRPhysicalPropCollisionTracker : MonoBehaviour
	{
		public VRPhysicalProp PhysicalProp;

		public Collider ContactCollider;

		void FixedUpdate()
		{
			ContactCollider = null;
		}

		void OnCollisionEnter(Collision other)
		{
			PhysicalProp.OnImpact(other.relativeVelocity.magnitude);
		}

		void OnTriggerStay(Collider other)
		{
			// how do we determine if this is a part of the iva shell?
			if (other.gameObject.layer == 16)
			{
				ContactCollider = other;
			}
		}
	}
}
