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
	public class VRPhysicalProp : FreeIva.PhysicalProp
	{
		[SerializeField]
		InteractableBehaviour m_interactableBehaviour;

		bool m_otherHandGrabbed = false;

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);

			if (m_collider != null)
			{
				m_interactableBehaviour = m_collider.gameObject.AddComponent<InteractableBehaviour>();
				m_interactableBehaviour.AttachHandOnGrab = false;
			}
		}

		void Start()
		{
			base.Start();

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


			Vector3 linearVelocity = KerbalVR.InteractionSystem.Instance.transform.TransformVector(hand.handActionPose[hand.handType].lastVelocity);
			Vector3 angularVelocity = KerbalVR.InteractionSystem.Instance.transform.rotation * hand.handActionPose[hand.handType].lastAngularVelocity;

			base.Release(linearVelocity, angularVelocity);
		}

		private void OnGrab(Hand hand)
		{
			m_otherHandGrabbed = false;
			rigidBodyObject.transform.SetParent(hand.handObject.transform, true);

			base.Grab();
		}

		protected override void PlayStickyFeedback()
		{
			base.PlayStickyFeedback();

			if (isSticky && m_interactableBehaviour.GrabbedHand)
			{
				HapticUtils.Light(m_interactableBehaviour.GrabbedHand.handType);
			}
		}

		public interface IVRInteraction
		{
			void OnGrab(Hand hand);
			void OnRelease(Hand hand);
		}

		// TODO: maybe this should be pushed up to the interaction system level, and PhysicalProp is a certain kind of Interaction?
		// then stuff like flightStick etc are also just interactions?
		public class VRInteraction : MonoBehaviour, IVRInteraction
		{
			public VRPhysicalProp PhysicalProp;

			public virtual void OnLoad(ConfigNode interactionNode) { }

			public virtual void OnGrab(Hand hand) { }
			public virtual void OnRelease(Hand hand) { }
			public virtual void OnImpact(float magnitude) { }
		}

		public class VRInteractionExtinguisher : VRInteraction
		{
			[SerializeField] Vector3 thrustVector;
			[SerializeField] Vector3 thrustPosition;
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

				interactionNode.TryGetValue(nameof(thrustPosition), ref thrustPosition);

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
						particleObject.transform.localPosition = thrustPosition;
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

		public class VRInteractionCamera : VRInteraction
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

		public class VRInteractionFlashlight : VRInteraction
		{
			[SerializeReference] AudioClip m_buttonSound;
			[SerializeField] Material m_lensMaterial;
			[SerializeField] Light m_light;

			SteamVR_Action_Boolean_Source m_pinchThumbAction;

			static int EMISSIVE_COLOR_PROPERTY_ID = Shader.PropertyToID("_EmissiveColor");

			public override void OnLoad(ConfigNode interactionNode)
			{
				base.OnLoad(interactionNode);

				m_buttonSound = PhysicalProp.LoadAudioClip(interactionNode, "buttonSound");
				m_lensMaterial = PhysicalProp.FindTransform(interactionNode.GetValue("lensObjectName"))?.GetComponent<MeshRenderer>()?.material;
				m_light = PhysicalProp.FindTransform(interactionNode.GetValue("lightObjectName"))?.GetComponent<Light>();

				if (m_light != null)
				{
					m_light.enabled = false;
				}

				if (m_lensMaterial != null)
				{
					m_lensMaterial.SetColor(EMISSIVE_COLOR_PROPERTY_ID, Color.black);
				}
			}

			public override void OnGrab(Hand hand)
			{
				base.OnGrab(hand);

				m_pinchThumbAction = SteamVR_Input.GetBooleanAction("default", "PinchThumb")[hand.handType];
				m_pinchThumbAction.onStateDown += OnButtonStateDown;
			}

			public override void OnRelease(Hand hand)
			{
				base.OnRelease(hand);
				m_pinchThumbAction.onStateDown -= OnButtonStateDown;
			}

			void OnDestroy()
			{
				if (m_pinchThumbAction != null)
				{
					m_pinchThumbAction.onStateDown -= OnButtonStateDown;
				}
			}

			private void OnButtonStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
			{
				PhysicalProp.PlayAudioClip(m_buttonSound);

				if (m_light != null)
				{
					m_light.enabled = !m_light.enabled;

					if (m_lensMaterial != null)
					{
						m_lensMaterial.SetColor(EMISSIVE_COLOR_PROPERTY_ID, m_light.enabled ? Color.white : Color.black);
					}
				}
			}
		}

		public class VRInteractionSqueak : VRInteraction
		{
			[SerializeReference] AudioClip m_squeakSound;

			SteamVR_Action_Boolean_Source m_pinchAction;

			public override void OnLoad(ConfigNode interactionNode)
			{
				base.OnLoad(interactionNode);
				m_squeakSound = PhysicalProp.LoadAudioClip(interactionNode, "squeakSound");
			}

			public override void OnGrab(Hand hand)
			{
				base.OnGrab(hand);
				m_pinchAction = SteamVR_Input.GetBooleanAction("default", "PinchIndex")[hand.handType];
				m_pinchAction.onStateDown += OnPinchStateDown;
				enabled = true;

				PhysicalProp.PlayAudioClip(m_squeakSound);
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
				PhysicalProp.PlayAudioClip(m_squeakSound);
			}
		}

		public class VRInteractionBreakable : VRInteraction
		{
			[SerializeReference] AudioClip breakSound;
			[SerializeField] float breakSpeed = 4;
			[SerializeField] ParticleSystem m_particleSystem;

			public override void OnLoad(ConfigNode interactionNode)
			{
				breakSound = PhysicalProp.LoadAudioClip(interactionNode, nameof(breakSound));

				interactionNode.TryGetValue(nameof(breakSpeed), ref breakSpeed);

				string particleSystemName = interactionNode.GetValue(nameof(particleSystemName));
				if (particleSystemName != null)
				{
					var particlePrefab = AssetLoader.Instance.GetGameObject(particleSystemName);
					if (particlePrefab != null)
					{
						var particleObject = GameObject.Instantiate(particlePrefab);

						particleObject.layer = 20;
						particleObject.transform.SetParent(PhysicalProp.m_collider.transform, false);
						particleObject.transform.localPosition = PhysicalProp.m_collider.bounds.center;

						m_particleSystem = particleObject.GetComponent<ParticleSystem>();
					}
				}
			}

			public override void OnImpact(float magnitude)
			{
				if (magnitude > breakSpeed)
				{
					var freeIvaModule = FreeIva.FreeIva.CurrentInternalModuleFreeIva;

					m_particleSystem.transform.SetParent(freeIvaModule.Centrifuge?.IVARotationRoot ?? freeIvaModule.internalModel.transform, true);
					m_particleSystem.Play();

					if (breakSound != null)
					{
						var audioSource = CameraUtils.CloneComponent(PhysicalProp.m_audioSource, m_particleSystem.gameObject);

						audioSource.PlayOneShot(breakSound);
					}

					GameObject.Destroy(PhysicalProp.rigidBodyObject);
				}
			}
		}
	}
}
