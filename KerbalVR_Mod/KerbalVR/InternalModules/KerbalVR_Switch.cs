using KerbalVR.IVAAdaptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace KerbalVR.InternalModules
{
	public class VRSwitch : InternalModule
	{
		[KSPField]
		public string switchTransformName = string.Empty;

		[KSPField]
		public Vector3 rotationAxis = Vector3.right;

		[KSPField]
		public Vector3 zeroVector = Vector3.up;

		[KSPField]
		public float minAngle = -40;

		[KSPField]
		public float maxAngle = 40;

		/// <summary>
		/// If has a cover, whether the cover turns off the switch when close
		/// </summary>
		[KSPField]
		public bool coverTurnsOffSwitch = true;

		[KSPField]
		public bool triState = false;

		VRSwitchInteractionListener interactionListener = null;
		VRCover cover = null;
		internal float currentAngle = 0;
		internal float middleAngle = 0;

		internal IVASwitch m_ivaSwitch;

#if PROP_GIZMOS
		GameObject gizmo;
		GameObject arrow;
#endif
		private void Start()
		{
			middleAngle = (maxAngle + minAngle) / 2f;

			var switchTransform = internalProp.FindModelTransform(switchTransformName);
			m_ivaSwitch = IVASwitch.ConstructSwitch(this, switchTransform);

			if (interactionListener == null)
			{
				interactionListener = Util.FindOrAddComponent<VRSwitchInteractionListener>(switchTransform.gameObject);
				interactionListener.switchModule = this;
				interactionListener.enabled = true;
				interactionListener.SetAngleToSwitchState();

#if PROP_GIZMOS
				if (arrow == null)
				{
					arrow = Utils.CreateArrow(Color.cyan, 0.2f);
					arrow.transform.SetParent(switchTransform.parent, false);
					arrow.transform.localRotation = Quaternion.LookRotation(rotationAxis);
					Utils.SetLayer(arrow, 20);
				}

				if (gizmo == null)
				{
					gizmo = Utils.CreateGizmo();
					gizmo.transform.SetParent(switchTransform, false);
					Utils.SetLayer(gizmo, 20);
				}
#endif
			}

			cover = gameObject.GetComponent<VRCover>();
			if (cover && coverTurnsOffSwitch)
			{
				cover.OnCoverClose += Cover_OnCoverClose;

				// initialize cover state, since switch might be on when vehicle spawns (like gears)
				cover.SetState(m_ivaSwitch.CurrentState);
			}
		}

		void OnDestroy()
		{
			if (cover && coverTurnsOffSwitch)
			{
				cover.OnCoverClose -= Cover_OnCoverClose;
			}	
		}

		/// <summary>
		/// Event callback when cover is closed
		/// </summary>
		private void Cover_OnCoverClose()
		{
			m_ivaSwitch.SetState(false);
			interactionListener.SetAngle(maxAngle);
		}

		class VRSwitchInteractionListener : MonoBehaviour, IFingertipInteractable
		{
			public VRSwitch switchModule;

			Hand m_hand;
			float m_contactedAngle;
			bool m_interactable = false;
			/// <summary>
			/// Use to prevent sticky switch after toggle
			/// </summary>
			bool m_stateChanged = false;

			float GetFingertipAngle(Vector3 fingertipCenter)
			{
				Vector3 vec = fingertipCenter - transform.position;
				vec = transform.parent.InverseTransformDirection(vec);
				vec -= vec * Vector3.Dot(vec, switchModule.rotationAxis);
				vec = Vector3.Normalize(vec);

				Vector3 yaxis = Vector3.Cross(switchModule.rotationAxis, switchModule.zeroVector);

				float x = Vector3.Dot(vec, switchModule.zeroVector);
				float y = Vector3.Dot(vec, yaxis);

				return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
			}

			public void OnEnter(Hand hand, Collider buttonCollider)
			{
				m_interactable = !switchModule.cover || switchModule.cover.IsOpen;

				if (m_interactable)
				{
					m_hand = hand;
					enabled = true;
					m_contactedAngle = GetFingertipAngle(m_hand.FingertipPosition);
					m_stateChanged = false;

					HapticUtils.Light(hand.handType);
				}

				switchModule.m_ivaSwitch.SetAnimationsEnabled(false);
			}

			public void OnStay(Hand hand, Collider buttonCollider)
			{
				if (!m_stateChanged && m_interactable)
				{
					float newAngle = GetFingertipAngle(m_hand.FingertipPosition);
					float delta = newAngle - m_contactedAngle;

					if (delta > 180) delta -= 360;
					if (delta < -180) delta += 360;

					float angle = switchModule.currentAngle + delta;
					float clampedAngle = Mathf.Clamp(angle, switchModule.minAngle, switchModule.maxAngle);

					bool canSwitchOn, canSwitchOff;
					float switchOnThreshold, switchOffThreshold;
					if (switchModule.triState)
					{
						canSwitchOn = canSwitchOff = true;
						switchOnThreshold = (switchModule.minAngle + switchModule.middleAngle) / 2;
						switchOffThreshold = (switchModule.maxAngle + switchModule.middleAngle) / 2;
					}
					else
					{
						canSwitchOff = switchModule.m_ivaSwitch.CurrentState;
						canSwitchOn = !canSwitchOff;
						switchOnThreshold = switchOffThreshold = switchModule.middleAngle;
					}

					if (canSwitchOff && clampedAngle > switchOffThreshold)
					{
						// snap off
						switchModule.m_ivaSwitch.SetState(false);
						clampedAngle = switchModule.maxAngle;
						m_stateChanged = true;
						HapticUtils.Snap(hand.handType);
					}
					else if (canSwitchOn && clampedAngle < switchOnThreshold)
					{
						// snap on
						switchModule.m_ivaSwitch.SetState(true);
						clampedAngle = switchModule.minAngle;
						m_stateChanged = true;
						HapticUtils.Snap(hand.handType);
					}

					SetAngle(clampedAngle);
					m_contactedAngle = newAngle;
				}
			}

			public void OnExit(Hand hand, Collider buttonCollider)
			{
				SetAngleToSwitchState();
				switchModule.m_ivaSwitch.SetAnimationsEnabled(true);
			}

			internal void SetAngleToSwitchState()
			{
				if (switchModule.triState)
				{
					SetAngle(switchModule.middleAngle);
				}
				else
				{
					SetAngle(switchModule.m_ivaSwitch.CurrentState ? switchModule.minAngle : switchModule.maxAngle);
				}
			}

			internal void SetAngle(float angle)
			{
				switchModule.currentAngle = angle;
				transform.localRotation = Quaternion.AngleAxis(switchModule.currentAngle, switchModule.rotationAxis);
			}
		}
	}
}
