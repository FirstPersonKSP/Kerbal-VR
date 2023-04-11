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
		[KSPField]
		public string vrInteraction = string.Empty;

		[SerializeField]
		InteractableBehaviour m_interactableBehaviour;

		SteamVR_Action_Boolean_Source m_action;

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

		new void Start()
		{
			base.Start();

			if (m_interactableBehaviour != null)
			{
				m_interactableBehaviour.OnGrab += OnGrab;
				m_interactableBehaviour.OnRelease += OnRelease;
				m_interactableBehaviour.OnOtherHandGrab += OnOtherHandGrab;
			}
		}

		void BindPinchAction(Hand hand)
		{
			if (vrInteraction != string.Empty)
			{
				m_action = SteamVR_Input.GetBooleanAction("default", vrInteraction)[hand.handType];
				m_action.onStateDown += OnPinchStateDown;
				m_action.onStateUp += OnPinchStateUp;
			}
		}

		void UnbindPinchAction()
		{
			if (m_action != null)
			{
				m_action.onStateDown -= OnPinchStateDown;
				m_action.onStateUp -= OnPinchStateUp;
			}
		}

		private void OnOtherHandGrab(Hand hand)
		{
			UnbindPinchAction();
			BindPinchAction(hand);
			m_otherHandGrabbed = true;
			rigidBodyObject.transform.SetParent(hand.handObject.transform, true);
		}

		private void OnRelease(Hand hand)
		{
			UnbindPinchAction();

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

			BindPinchAction(hand);
		}

		private void OnPinchStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			StartInteraction();
		}

		private void OnPinchStateUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			StopInteraction();
		}

		protected override void PlayStickyFeedback()
		{
			base.PlayStickyFeedback();

			if (isSticky && m_interactableBehaviour.GrabbedHand)
			{
				HapticUtils.Light(m_interactableBehaviour.GrabbedHand.handType);
			}
		}
	}
}
