using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InteractionCommon
{
	internal class VRThrottleLever : MonoBehaviour
	{
		[Persistent]
		public string leverName = "throttleLever";
		[Persistent]
		public float angleMin = -75.0f;
		[Persistent]
		public float angleMax = 75.0f;
		[Persistent]
		public Vector3 axis = Vector3.right;

		InteractableBehaviour interactable;
		RotationUtil m_rotationUtil;

		public static void Create(GameObject gameObject, ref VRThrottleLever throttleLever, ConfigNode node)
		{
			if (HighLogic.LoadedScene == GameScenes.LOADING)
			{
				throttleLever = gameObject.AddComponent<InteractionCommon.VRThrottleLever>();
				ConfigNode.LoadObjectFromConfig(throttleLever, node);
			}
		}

		public void OnStart(Transform leverTransform)
		{
			// TODO: add generic collider internal module?

			var collider = leverTransform.GetComponentInChildren<Collider>();

			if (collider == null)
			{
				var boxCollider = Utils.GetOrAddComponent<BoxCollider>(leverTransform.gameObject);

				boxCollider.size = new Vector3(0.11f, 0.03f, 0.03f);
				boxCollider.center = new Vector3(0.045f, 0.07f, 0.0f);

				collider = boxCollider;
			}

			m_rotationUtil = new RotationUtil(leverTransform, axis, angleMin, angleMax);

			collider.gameObject.layer = 20;
			interactable = Utils.GetOrAddComponent<InteractableBehaviour>(collider.gameObject);

			interactable.SkeletonPoser = Utils.GetOrAddComponent<SteamVR_Skeleton_Poser>(m_rotationUtil.Transform.gameObject);
			interactable.SkeletonPoser.skeletonMainPose = SkeletonPose_HandleRailGrabPose.GetInstance();
			interactable.SkeletonPoser.Initialize();

			interactable.OnGrab += OnGrab;
		}

		private void OnGrab(Hand hand)
		{
			m_rotationUtil.Grabbed(hand.GripPosition);
			HapticUtils.Heavy(hand.handType);
		}

		public void OnUpdate()
		{
			if (interactable != null && interactable.IsGrabbed)
			{
				m_rotationUtil.Update(interactable.GrabbedHand.GripPosition);

				FlightInputHandler.state.mainThrottle = m_rotationUtil.GetInterpolatedPosition();
			}
			else
			{
				m_rotationUtil.SetInterpolatedPosition(FlightInputHandler.state.mainThrottle);
			}
		}
	}
}
