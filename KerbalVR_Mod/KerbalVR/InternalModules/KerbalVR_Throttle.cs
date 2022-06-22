using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	class VRThrottleLever : InternalLeverThrottle
	{
		InteractableBehaviour interactable;

		bool initialized = false;
		float grabbedAngle; // what was the hand angle when we grabbed it?
		float grabbedThrottleAngle; // what was the mapped angle from the current throttle when we grabbed it?

#if PROP_GIZMOS
		GameObject gizmo;
		GameObject arrow;
#endif

		public override void OnAwake()
		{
			try
			{
				base.OnAwake();
			}
			catch (Exception)
			{
				// Utils.LogError(e);
			}

			if (HighLogic.LoadedScene == GameScenes.LOADING) return;

			// the stock module only supports transforms as a child of this game object
			// try to find a relative path here
			if (leverObject == null)
			{
				var leverTransform = transform.parent.Find("model").GetChild(0).Find(leverName);

				if (leverTransform == null) return;

				var anchorTransform = new GameObject("VRThrottleAnchor").transform;
				anchorTransform.localPosition = leverTransform.localPosition;
				anchorTransform.localRotation = leverTransform.localRotation;
				anchorTransform.localScale = leverTransform.localScale;

				anchorTransform.SetParent(leverTransform.parent, false);

				leverTransform.localPosition = Vector3.zero;
				leverTransform.localRotation = Quaternion.identity;
				leverTransform.localScale = Vector3.one;
				leverTransform.SetParent(anchorTransform, false);

				// TODO: add generic collider internal module?
				var collider = Utils.GetOrAddComponent<CapsuleCollider>(leverTransform.gameObject);
				leverObject = collider;

				collider.radius = 0.02f;
				collider.center = Vector3.up * 0.02f;
				collider.height = 0.07f;

				leverInitial = leverTransform.rotation;
			}

			if (!initialized)
			{
				var leverTransform = leverObject.transform;
				interactable = Utils.GetOrAddComponent<InteractableBehaviour>(leverTransform.gameObject);

				interactable.SkeletonPoser = Utils.GetOrAddComponent<SteamVR_Skeleton_Poser>(leverTransform.gameObject);
				interactable.SkeletonPoser.skeletonMainPose = SkeletonPose_HandleRailGrabPose.GetInstance();
				interactable.SkeletonPoser.Initialize();

#if PROP_GIZMOS
				if (gizmo == null)
				{
					gizmo = Utils.CreateGizmo();
					gizmo.transform.SetParent(leverTransform.parent, false);
					Utils.SetLayer(gizmo, 20);
					arrow = Utils.CreateArrow(Color.cyan, 0.2f);
					arrow.transform.SetParent(leverTransform.parent, false);
					arrow.transform.localRotation = Quaternion.LookRotation(axis);
					Utils.SetLayer(arrow, 20);

					Utils.GetOrAddComponent<ColliderVisualizer>(leverTransform.gameObject);
				}
#endif

				interactable.OnGrab += OnGrab;

				initialized = true;
			}
		}

		private float GetHandAngle()
		{
			//Vector3 handPosition = interactable.GrabbedHand.handActionPose.GetLocalPosition(interactable.GrabbedHand.handType);
			Vector3 handPosition = interactable.GrabbedHand.GripPosition;
			//Vector3 relativeHandPosition = handPosition - leverObject.transform.position;
			Vector3 relativeHandPosition = leverObject.transform.parent.InverseTransformPoint(handPosition);

			Vector3 zeroRotation = leverObject.transform.parent.rotation * leverInitial * Vector3.forward;
			Vector3 basisVector = Vector3.Cross(axis, zeroRotation);

			float x = Vector3.Dot(relativeHandPosition, zeroRotation);
			float y = Vector3.Dot(relativeHandPosition, basisVector);

			float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

			return angle;

		}

		private void OnGrab(Hand hand)
		{
			grabbedAngle = GetHandAngle();
			grabbedThrottleAngle = Mathf.Lerp(angleMin, angleMax, FlightInputHandler.state.mainThrottle);
		}

		public override void OnUpdate()
		{
			if (interactable != null && interactable.IsGrabbed)
			{
				float currentAngle = GetHandAngle();
				float deltaAngle = currentAngle - grabbedAngle;

				if (deltaAngle > 180) deltaAngle -= 360;
				if (deltaAngle < -180) deltaAngle += 360;

				float newAngle = grabbedThrottleAngle + deltaAngle;

				float desiredThrottle = Mathf.InverseLerp(angleMin, angleMax, newAngle);

				FlightInputHandler.state.mainThrottle = Mathf.Clamp01(desiredThrottle);
			}

			base.OnUpdate();
		}
	}
}
