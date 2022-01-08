using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	class VRFlightStick : InternalModule
	{
		[KSPField]
		public string stickTransformName = null;

		[KSPField]
		public float deflectionAngle = 30.0f;

		[KSPField]
		public float twistAngle = 30.0f;

		InteractableBehaviour interactable;
		Transform stickTransform = null;

		Quaternion grabbedOrientation; // the worldspace orientation of the hand at the moment the stick was grabbed

#if PROP_GIZMOS
		GameObject gizmo;
#endif

		public override void OnAwake()
		{
			if (HighLogic.LoadedScene == GameScenes.LOADING) return;

			if (stickTransform != null) return;
			// first, assume the transform is relative to this prop
			stickTransform = transform.Find(stickTransformName);

			// otherwise, try to find one in the entire IVA
			if (stickTransform == null)
			{
				stickTransform = transform.parent.Find("model").GetChild(0).Find(stickTransformName);
			}

			var anchorTransform = new GameObject("VRFlightStickAnchor").transform;
			anchorTransform.localPosition = stickTransform.localPosition;
			anchorTransform.localRotation = stickTransform.localRotation;
			anchorTransform.localScale = stickTransform.localScale;

			anchorTransform.SetParent(stickTransform.parent, false);

			stickTransform.localPosition = Vector3.zero;
			stickTransform.localRotation = Quaternion.identity;
			stickTransform.localScale = Vector3.one;
			stickTransform.SetParent(anchorTransform, false);

			var collider = Utils.GetOrAddComponent<CapsuleCollider>(stickTransform.gameObject);

			collider.radius = 0.02f;
			collider.center = Vector3.up * 0.02f;
			collider.height = 0.07f;

			interactable = Utils.GetOrAddComponent<InteractableBehaviour>(stickTransform.gameObject);

			interactable.SkeletonPoser = Utils.GetOrAddComponent<SteamVR_Skeleton_Poser>(stickTransform.gameObject);
			interactable.SkeletonPoser.skeletonMainPose = SkeletonPose_HandleRailGrabPose.GetInstance();
			interactable.SkeletonPoser.Initialize();

			interactable.OnGrab += OnGrab;

			FlightInputHandler.OnRawAxisInput += GetInput;

#if PROP_GIZMOS
			if (gizmo == null)
			{
				gizmo = Utils.CreateGizmo();
				gizmo.transform.SetParent(stickTransform.parent, false);
				Utils.SetLayer(gizmo, 20);
					
				Utils.GetOrAddComponent<ColliderVisualizer>(stickTransform.gameObject);
			}
#endif
		}

		private void GetInput(FlightCtrlState st)
		{
			if (interactable.IsGrabbed)
			{
				Quaternion currentHandOrientation = Quaternion.Inverse(interactable.GrabbedHand.handObject.transform.rotation) * stickTransform.parent.rotation;

				Quaternion deltaRotation = Quaternion.Inverse(grabbedOrientation) * currentHandOrientation;

				Vector3 deltaAngles = deltaRotation.eulerAngles;

				if (deltaAngles.x > 180) deltaAngles.x -= 360;
				if (deltaAngles.y > 180) deltaAngles.y -= 360;
				if (deltaAngles.z > 180) deltaAngles.z -= 360;

				st.yaw = Mathf.InverseLerp(-deflectionAngle, deflectionAngle, deltaAngles.z) * 2.0f - 1.0f;
				st.pitch = Mathf.InverseLerp(-deflectionAngle, deflectionAngle, deltaAngles.x) * 2.0f - 1.0f;
				st.roll = Mathf.InverseLerp(twistAngle, -twistAngle, deltaAngles.y) * 2.0f - 1.0f;
			}
		}

		private void OnDestroy()
		{
			FlightInputHandler.OnRawAxisInput -= GetInput;
		}

		private void OnGrab(Hand hand)
		{
			grabbedOrientation = Quaternion.Inverse(hand.handObject.transform.rotation) * stickTransform.parent.rotation;
		}

		public override void OnUpdate()
		{
			

			if (stickTransform != null)
			{
				stickTransform.localRotation = Quaternion.Euler(
					-FlightInputHandler.state.pitch * deflectionAngle,
					FlightInputHandler.state.roll * twistAngle,
					-FlightInputHandler.state.yaw * deflectionAngle);
			}
		}
	}
}
