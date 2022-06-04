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
		public float tiltAngle = 30.0f;

		[KSPField]
		public float twistAngle = 30.0f;

		[KSPField]
		public float tiltDeadzoneAngle = 3.0f;

		[KSPField]
		public float twistDeadzoneAngle = 3.0f;

		[KSPField]
		public float tiltExponent = 3.0f;

		[KSPField]
		public float twistExponent = 3.0f;

		InteractableBehaviour interactable;
		Transform stickTransform = null;

		Quaternion grabbedOrientation; // the worldspace orientation of the hand at the moment the stick was grabbed

		SpaceNavigator previousSpaceNavigator;
		FakeSpaceNavigator spaceNavigator = new FakeSpaceNavigator();

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

			if (stickTransform == null)
            {
				Utils.LogError($"Unable to find transform named {stickTransformName} in iva {this.internalModel.name} prop {this.internalProp.name}");
				return;
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
			interactable.OnRelease += OnRelease;

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

		private static float ApplyDeadZone(float raw, float deadZoneFraction, float exponent)
		{
			float sign = Mathf.Sign(raw);
			float deadZoned = Mathf.Max(0, Mathf.Abs(raw) - deadZoneFraction) * (1.0f / (1.0f - deadZoneFraction));

			return sign * Mathf.Pow(deadZoned, exponent);
		}

		// gets the input amount in each axis in a [-1,1] range
		// x = forward/back
		// y = twist
		// z = left/right
		private Vector3 GetInputAxes()
		{
			Vector3 result = Vector3.zero;

			if (interactable.IsGrabbed)
			{
				Quaternion currentHandOrientation = Quaternion.Inverse(interactable.GrabbedHand.handObject.transform.rotation) * stickTransform.parent.rotation;

				Quaternion deltaRotation = Quaternion.Inverse(grabbedOrientation) * currentHandOrientation;

				Vector3 deltaAngles = deltaRotation.eulerAngles;

				if (deltaAngles.x > 180) deltaAngles.x -= 360;
				if (deltaAngles.y > 180) deltaAngles.y -= 360;
				if (deltaAngles.z > 180) deltaAngles.z -= 360;

				Vector3 raw = new Vector3(
					Mathf.InverseLerp(-tiltAngle, tiltAngle, deltaAngles.x) * 2.0f - 1.0f,
					Mathf.InverseLerp(twistAngle, -twistAngle, deltaAngles.y) * 2.0f - 1.0f,
					Mathf.InverseLerp(-tiltAngle, tiltAngle, deltaAngles.z) * 2.0f - 1.0f);

				float twistDeadzoneFraction = twistDeadzoneAngle / twistAngle;
				float tiltDeadzoneFraction = tiltDeadzoneAngle / tiltAngle;

				result = new Vector3(
					ApplyDeadZone(raw.x, tiltDeadzoneFraction, tiltExponent),
					ApplyDeadZone(raw.y, twistDeadzoneFraction, twistExponent),
					ApplyDeadZone(raw.z, tiltDeadzoneFraction, tiltExponent));
			}

			return result;
		}

		private void GetInput(FlightCtrlState st)
		{
			if (interactable.IsGrabbed)
			{
				Vector3 inputAxes = GetInputAxes();
				
				st.pitch = inputAxes.x;
				st.roll = inputAxes.y;
				st.yaw = inputAxes.z;
			}
		}

		private void OnDestroy()
		{
			// FlightInputHandler.OnRawAxisInput -= GetInput;
		}

		private void OnGrab(Hand hand)
		{
			grabbedOrientation = Quaternion.Inverse(hand.handObject.transform.rotation) * stickTransform.parent.rotation;

			vessel.OnPreAutopilotUpdate += OnPreAutopilotUpdate;
			vessel.OnPostAutopilotUpdate += OnPostAutopilotUpdate;
		}

		private void OnPreAutopilotUpdate(FlightCtrlState st)
		{
			// install our fake space navigator class so that SAS can understand when we want to inject inputs
			FlightInputHandler.SPACENAV_USE_AS_FLIGHT_CONTROL = true;
			GameSettings.SPACENAV_FLIGHT_SENS_ROT = 1.0f;
			previousSpaceNavigator = SpaceNavigator.Instance;
			SpaceNavigator.Instance = spaceNavigator;
		}

		private void OnPostAutopilotUpdate(FlightCtrlState st)
		{
			// remove the fake space navigator
			FlightInputHandler.SPACENAV_USE_AS_FLIGHT_CONTROL = false;
			SpaceNavigator.Instance = previousSpaceNavigator;
		}

		private void OnRelease(Hand hand)
		{
			vessel.OnPreAutopilotUpdate -= OnPreAutopilotUpdate;
			vessel.OnPostAutopilotUpdate -= OnPostAutopilotUpdate;
		}

		public override void OnUpdate()
		{

			if (stickTransform != null)
			{
				stickTransform.localRotation = Quaternion.Euler(
					-FlightInputHandler.state.pitch * tiltAngle,
					FlightInputHandler.state.roll * twistAngle,
					-FlightInputHandler.state.yaw * tiltAngle);
			}
		}

		class FakeSpaceNavigator : SpaceNavigator
		{
			public override void Dispose()
			{
			}

			public override Quaternion GetRotation()
			{
				// a bit silly: this class isn't used to generate any input, only to tell SAS that we are trying to turn the ship.
				// so all we have to do is return a non-zero rotation
				return Quaternion.Euler(30, 30, 30);
			}

			public override Vector3 GetTranslation()
			{
				return Vector3.zero;
			}
		}
	}
}
