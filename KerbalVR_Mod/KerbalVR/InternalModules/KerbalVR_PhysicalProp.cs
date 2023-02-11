using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	public class VRPhysicalProp : InternalModule
	{
		[KSPField]
		public bool isSticky = false; // when released, if this is overlapping another collider then it will attach to it

		[KSPField]
		public string placeSound = string.Empty;

		[SerializeField]
		InteractableBehaviour m_interactableBehaviour;
		[SerializeField]
		CollisionTracker m_collisionTracker;

		[SerializeField]
		Interaction m_interaction;

		Rigidbody m_rigidBody;
		FreeIva.InternalModuleFreeIva m_freeIvaModule;

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

					m_collider = colliderParams.Create(internalProp.hasModel ? transform : internalModel.transform);
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

					m_collisionTracker = m_collider.gameObject.AddComponent<CollisionTracker>();
					m_collisionTracker.PhysicalProp = this;
				}
				else
				{
					Utils.LogError($"VRPhysicalProp: prop {internalProp.propName} does not have a collider");
				}

				// setup audio
				{
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
						m_audioSource.spatialize = true;
					}
				}

				// setup interactions
				var interactionNode = node.GetNode("INTERACTION");
				if (interactionNode != null)
				{
					CreateInteraction(interactionNode);
				}
			}
		}

#region static stuff
		static Dictionary<string, TypeInfo> x_interactionTypes;

		static VRPhysicalProp()
		{
			x_interactionTypes = new Dictionary<string, TypeInfo>();
			foreach (var assembly in AssemblyLoader.loadedAssemblies)
			{
				try
				{
					assembly.TypeOperation(type =>
					{
						if (type.IsSubclassOf(typeof(Interaction)))
						{
							x_interactionTypes.Add(type.Name, type.GetTypeInfo());
						}
					});
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		public void CreateInteraction(ConfigNode interactionNode)
		{
			var name = interactionNode.GetValue("name");

			if (x_interactionTypes.TryGetValue(name, out var typeInfo))
			{
				m_interaction = (Interaction)gameObject.AddComponent(typeInfo.AsType());
				m_interaction.PhysicalProp = this;
				m_interaction.OnLoad(interactionNode);
			}
			else
			{
				Utils.LogError($"PROP {internalProp.propName}: No VRPhysicalProp.Interaction named {name} exists");
			}
		}
#endregion

		internal AudioClip LoadAudioClip(ConfigNode node, string key)
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

		public void PlayAudioClip(AudioClip clip, float volume, float pitch)
		{
			if (clip == null) return;
			m_audioSource.PlayOneShot(clip);
		}

		public void PlayAudioClip(AudioClip clip)
		{
			PlayAudioClip(clip, GameSettings.SHIP_VOLUME, 1.0f);
		}

		public void StartAudioLoop(AudioClip clip)
		{
			if (clip == null) return;
			m_audioSource.clip = clip;
			m_audioSource.loop = true;
			m_audioSource.volume = GameSettings.SHIP_VOLUME;
			m_audioSource.pitch = 1.0f;
			m_audioSource.Play();
		}

		public void StopAudioLoop()
		{
			m_audioSource.loop = false;
			m_audioSource.Stop();
		}

		public void OnImpact(float magnitude)
		{
			// TOD: maybe randomize a bit?
			// m_audioSource.pitch = UnityEngine.Random.Range(-0.2f, 0.2f);
			float volume = Mathf.InverseLerp(1.0f, 5.0f, magnitude);
			if (volume == 0) return;

			PlayAudioClip(m_impactAudioClip, volume, UnityEngine.Random.Range(0.8f, 1.2f));
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

			m_freeIvaModule = FreeIva.FreeIva.CurrentInternalModuleFreeIva;

			transform.SetParent(m_freeIvaModule.Centrifuge?.IVARotationRoot ?? m_freeIvaModule.internalModel.transform , true);

			if (m_interaction)
			{
				m_interaction.OnRelease(hand);
			}

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

				Vector3 propVelocity = KerbalVR.InteractionSystem.Instance.transform.TransformVector(hand.handActionPose[hand.handType].lastVelocity);
				m_rigidBody.velocity = propVelocity;
				m_rigidBody.angularVelocity = KerbalVR.InteractionSystem.Instance.transform.rotation * hand.handActionPose[hand.handType].lastAngularVelocity;

				// total hack? - apply reaction velocity in zero-g
				if (!FreeIva.KerbalIvaAddon.Instance.buckled && !FreeIva.KerbalIvaAddon.Instance.KerbalIva.UseRelativeMovement())
				{
					// TODO: should probably have some idea of how much mass this thing is
					FreeIva.KerbalIvaAddon.Instance.KerbalIva.KerbalRigidbody.WakeUp();
					FreeIva.KerbalIvaAddon.Instance.KerbalIva.KerbalRigidbody.velocity += -propVelocity * 0.7f;
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

			if (m_interaction)
			{
				m_interaction.OnGrab(hand);
			}

			PlayAudioClip(m_grabAudioClip);
		}

		void FixedUpdate()
		{
			if (m_applyGravity)
			{
				var accel = FreeIva.KerbalIvaAddon.GetInternalSubjectiveAcceleration(m_freeIvaModule, transform.position);
				m_rigidBody.AddForce(accel, ForceMode.Acceleration);
			}
		}

		internal class CollisionTracker : MonoBehaviour
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

		// TODO: maybe this should be pushed up to the interaction system level, and PhysicalProp is a certain kind of Interaction?
		// then stuff like flightStick etc are also just interactions?
		public class Interaction : MonoBehaviour
		{
			public VRPhysicalProp PhysicalProp;

			public virtual void OnLoad(ConfigNode interactionNode) { }

			public virtual void OnGrab(Hand hand) { }
			public virtual void OnRelease(Hand hand) { }
		}


		public class VRInteractionExtinguisher : Interaction
		{
			[SerializeField] Vector3 thrustVector;
			public Transform thrustTransform;
			public AudioClip m_audioClip;
			[SerializeField] ParticleSystem m_particleSystem;

			SteamVR_Action_Boolean_Source m_pinchAction;
			bool m_playingSound;

			public override void OnLoad(ConfigNode interactionNode)
			{
				base.OnLoad(interactionNode);

				string transformName = interactionNode.GetValue("thrustTransformName");
				thrustTransform = PhysicalProp.FindTransform(transformName);
				enabled = false;

				if (!interactionNode.TryGetValue(nameof(thrustVector), ref thrustVector))
				{
					Utils.LogError($"PROP {PhysicalProp.internalProp.propName} VRInteractionExtinguisher could not parse key {nameof(thrustVector)}");
				}

				m_audioClip = PhysicalProp.LoadAudioClip(interactionNode, "sound");

				string particleSystemName = string.Empty;
				if (interactionNode.TryGetValue(nameof(particleSystemName), ref particleSystemName))
				{
					var particlePrefab = AssetLoader.Instance.GetGameObject(particleSystemName);
					if (particlePrefab != null)
					{
						var particleObject = GameObject.Instantiate(particlePrefab);
						
						particleObject.layer = 20;
						particleObject.transform.SetParent(thrustTransform, false);
						particleObject.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, -thrustVector);

						m_particleSystem = particleObject.GetComponent<ParticleSystem>();
					}
				}
			}

			public override void OnGrab(Hand hand)
			{
				base.OnGrab(hand);

				// TODO: pinchIndex is a boolean action - should it be a float?
				m_pinchAction = SteamVR_Input.GetBooleanAction("default", "PinchIndex")[hand.handType];
				enabled = true;
				m_particleSystem.gameObject.SetActive(true);
			}

			public override void OnRelease(Hand hand)
			{
				base.OnRelease(hand);

				m_pinchAction = null;
				enabled = false;
			}

			void FixedUpdate()
			{
				if (m_pinchAction.state)
				{
					Vector3 accelerationVector = thrustTransform.TransformVector(thrustVector);

					FreeIva.KerbalIvaAddon.Instance.KerbalIva.KerbalRigidbody.WakeUp();
					FreeIva.KerbalIvaAddon.Instance.KerbalIva.KerbalRigidbody.AddForceAtPosition(accelerationVector, thrustTransform.position, ForceMode.Acceleration);

					if (!m_playingSound)
					{
						PhysicalProp.StartAudioLoop(m_audioClip);
						m_playingSound = true;
						m_particleSystem.Play();
					}
				}
				else
				{
					if (m_playingSound)
					{
						PhysicalProp.StopAudioLoop();
						m_playingSound = false;
						m_particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
					}
				}
			}
		}

		public class VRInteractionCamera : Interaction
		{
			[SerializeReference] AudioClip m_shutterSound;

			SteamVR_Action_Boolean_Source m_pinchAction;

			public override void OnLoad(ConfigNode interactionNode)
			{
				base.OnLoad(interactionNode);

				m_shutterSound = PhysicalProp.LoadAudioClip(interactionNode, "shutterSound");
			}

			public override void OnGrab(Hand hand)
			{
				base.OnGrab(hand);

				m_pinchAction = SteamVR_Input.GetBooleanAction("default", "PinchIndex")[hand.handType];
				m_pinchAction.onStateDown += OnPinchStateDown;
				enabled = true;
			}

			public override void OnRelease(Hand hand)
			{
				base.OnRelease(hand);

				m_pinchAction.onStateDown -= OnPinchStateDown;
			}

			void OnDestroy()
			{
				if (m_pinchAction != null)
				{
					m_pinchAction.onStateDown -= OnPinchStateDown;
				}
			}

			private void OnPinchStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
			{
				PhysicalProp.PlayAudioClip(m_shutterSound);

				string screenshotDir = Application.platform == RuntimePlatform.OSXPlayer
					? Path.Combine(Application.dataPath, "../../Screenshots")
					: Path.Combine(Application.dataPath, "../Screenshots");
				string screenshotFileName = Path.GetRandomFileName();
				string screenshotPath = Path.ChangeExtension(Path.Combine(screenshotDir, screenshotFileName), ".png");

				ScreenCapture.CaptureScreenshot(screenshotPath, ScreenCapture.StereoScreenCaptureMode.BothEyes);
			}
		}
	}
}
