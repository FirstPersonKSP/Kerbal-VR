using KerbalVR.IVAAdaptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	class VRSwitch : InternalModule
	{
		[KSPField]
		public string switchTransformName = null;

		[KSPField]
		public Vector3 rotationAxis = Vector3.right;

		[KSPField]
		public Vector3 zeroVector = Vector3.up;

		[KSPField]
		public Vector3 switchVector = Vector3.up;

		[KSPField]
		public float minAngle = -40;

		[KSPField]
		public float maxAngle = 40;

		VRSwitchInteractionListener interactionListener = null;
		internal float currentAngle = 0;

		internal IVASwitch m_ivaSwitch;

#if PROP_GIZMOS
		GameObject gizmo;
		GameObject arrow;
#endif
		public override void OnAwake()
		{
			base.OnAwake();

			var switchTransform = internalProp.FindModelTransform(switchTransformName);

			if (switchTransform != null && interactionListener == null)
			{
				interactionListener = switchTransform.gameObject.AddComponent<VRSwitchInteractionListener>();
				interactionListener.switchModule = this;

				currentAngle = minAngle;

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
		}

		public void Start()
        {
			m_ivaSwitch = IVASwitch.ConstructSwitch(gameObject);
        }
	}

	class VRSwitchInteractionListener : MonoBehaviour, IFingertipInteractable
	{
		public VRSwitch switchModule;

		Hand m_hand;
		float m_contactedAngle;
		bool m_isMoving = false;

		bool m_currentState = false;

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

		public void Awake()
        {
			m_currentState = switchModule.m_ivaSwitch.CurrentState;
			SetAngle(m_currentState ? switchModule.maxAngle : switchModule.minAngle);
        }

		public void Update()
		{
			if (m_isMoving)
			{
				float newAngle = GetFingertipAngle(m_hand.FingertipPosition);
				float delta = newAngle - m_contactedAngle;

				if (delta > 180) delta -= 360;
				if (delta < -180) delta += 360;

				float angle = switchModule.currentAngle + delta;
				float clampedAngle = Mathf.Clamp(angle, switchModule.minAngle, switchModule.maxAngle);

				if (angle != clampedAngle)
                {
					m_isMoving = false;

					bool newState = clampedAngle == switchModule.maxAngle;

					switchModule.m_ivaSwitch.SetState(newState);
                }

				SetAngle(clampedAngle);
			}
		}

		public void OnEnter(Hand hand, Collider buttonCollider, SteamVR_Input_Sources inputSource)
        {
			if (!m_isMoving)
			{
				m_hand = hand;
				m_contactedAngle = GetFingertipAngle(m_hand.FingertipPosition);
				m_isMoving = true;
			}
		}

		public void OnExit(Hand hand, Collider buttonCollider, SteamVR_Input_Sources inputSource)
        {
			
		}

		public void OnStay(Hand hand, Collider buttonCollider, SteamVR_Input_Sources inputSource)
        {

		}

		void SetAngle(float angle)
		{
			switchModule.currentAngle = angle;
			transform.localRotation = Quaternion.AngleAxis(switchModule.currentAngle, switchModule.rotationAxis);
		}
	}
}
