using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InteractionCommon
{
	internal class VRYoke : MonoBehaviour
	{
		[Persistent]
		public string steerTransformName = string.Empty;

		[Persistent]
		public float steerAngle = 30.0f;

		[Persistent]
		public Vector3 steerAxis = Vector3.up;

		[Persistent]
		public string pushTransformName = string.Empty;

		[Persistent]
		public float pushAngle = 9.0f;

		[Persistent]
		public Vector3 pushAxis = Vector3.right;

		InteractableBehaviour m_interactable;
		RotationUtil m_steerRotationUtil;
		RotationUtil m_pushRotationUtil;
		Vessel m_vessel;

		public static void Create(GameObject gameObject, ref VRYoke yoke, ConfigNode node)
		{
			if (HighLogic.LoadedScene == GameScenes.LOADING)
			{
				yoke = gameObject.AddComponent<InteractionCommon.VRYoke>();
				ConfigNode.LoadObjectFromConfig(yoke, node);
			}
		}

		public void OnStart(Transform steerTransform, Transform pushTransform, Vessel vessel)
		{
			m_steerRotationUtil = new RotationUtil(steerTransform, steerAxis, -steerAngle, steerAngle);
			m_pushRotationUtil = new RotationUtil(pushTransform, pushAxis, -pushAngle, pushAngle);
			m_vessel = vessel;

			var collider = steerTransform.GetComponentInChildren<Collider>();

			collider.gameObject.layer = 20;
			m_interactable = Utils.GetOrAddComponent<InteractableBehaviour>(collider.gameObject);

			m_interactable.SkeletonPoser = Utils.GetOrAddComponent<SteamVR_Skeleton_Poser>(steerTransform.gameObject);
			m_interactable.SkeletonPoser.skeletonMainPose = SkeletonPose_HandleRailGrabPose.GetInstance();
			m_interactable.SkeletonPoser.Initialize();

			m_interactable.OnGrab += OnGrab;
			m_interactable.OnRelease += OnRelease;
		}

		private void OnRelease(Hand hand)
		{
			m_vessel.OnPostAutopilotUpdate -= OnPostAutopilotUpdate;
		}

		private void OnGrab(Hand hand)
		{
			m_steerRotationUtil.Grabbed(hand.GripPosition);
			m_pushRotationUtil.Grabbed(hand.GripPosition);

			m_vessel.OnPostAutopilotUpdate += OnPostAutopilotUpdate;

			HapticUtils.Heavy(hand.handType);
		}

		private void OnPostAutopilotUpdate(FlightCtrlState st)
		{
			st.wheelSteer = m_steerRotationUtil.GetInterpolatedPosition() * 2.0f - 1.0f;
			st.wheelThrottle = m_pushRotationUtil.GetInterpolatedPosition() * 2.0f - 1.0f;
		}

		public void OnUpdate()
		{
			if (m_interactable != null && m_interactable.IsGrabbed)
			{
				m_steerRotationUtil.Update(m_interactable.GrabbedHand.GripPosition);
				m_pushRotationUtil.Update(m_interactable.GrabbedHand.GripPosition);
			}
			else
			{
				m_steerRotationUtil.SetInterpolatedPosition((FlightInputHandler.state.wheelSteer + 1.0f) / 2.0f);
				m_pushRotationUtil.SetInterpolatedPosition((FlightInputHandler.state.wheelThrottle + 1.0f) / 2.0f);
			}
		}
	}
}
