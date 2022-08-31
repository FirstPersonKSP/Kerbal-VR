using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	// very similar to VRFlightStick, except it tracks the *translation* of the hand instead of the rotation
	// Also each axis can be mapped to a different RCS translation axis, so this module can be used for both left/down/up/right and forward/back props
	internal class VRTranslationControl : InternalModule
	{
		[KSPField]
		public string stickCollidername = String.Empty;

		// the ASET translation control is set up to have the objects rotate, but eventually we'll want one that can translate in 3 dimensions
		// For now I'm going to have the *output* side of this control (the visual movement of the prop) still controlled by ASET
		// but we'll eventually want to have this module also (optionally) drive the transform of the stick itself

		[KSPField]
		public Vector3 movementBounds = Vector3.zero;

		[KSPField]
		public Vector3 deadzoneFractionVector = new Vector3(0.1f, 0.1f, 0.1f);

		[KSPField]
		public Vector3 exponentVector = new Vector3(3.0f, 3.0f, 3.0f);

		[KSPField]
		public string axisBindingX = string.Empty;

		[KSPField]
		public string axisBindingY = string.Empty;

		[KSPField]
		public string axisBindingZ = string.Empty;

		InteractableBehaviour interactable;
		Transform stickTransform;
		Vector3 grabbedPosition; // the position in prop-space where the collider was grabbed

#if PROP_GIZMOS
		GameObject gizmo;
#endif

		private void Start()
		{
			stickTransform = this.FindTransform(stickCollidername);
			if (stickTransform == null) { return; }

			interactable = Utils.GetOrAddComponent<InteractableBehaviour>(stickTransform.gameObject);

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

		private void OnDestroy()
		{
			FlightInputHandler.OnRawAxisInput -= GetInput;
		}

		float NormalizeTranslation(float bounds, float position)
		{
			return bounds == 0.0f ? 0.0f : Mathf.InverseLerp(-bounds, bounds, position) * 2.0f - 1.0f;
		}

		Vector3 GetNormalizedInput()
		{
			Vector3 result = Vector3.zero;

			if (interactable.IsGrabbed)
			{
				Vector3 localPosition = transform.InverseTransformPoint(interactable.GrabbedHand.GripPosition);
				Vector3 rawPosition = localPosition - grabbedPosition;

				Vector3 raw = new Vector3(
					NormalizeTranslation(movementBounds.x, rawPosition.x),
					NormalizeTranslation(movementBounds.y, rawPosition.y),
					NormalizeTranslation(movementBounds.z, rawPosition.z));

				result = new Vector3(
					InteractionCommon.VRFlightStick.ApplyDeadZone(raw.x, deadzoneFractionVector.x, exponentVector.x),
					InteractionCommon.VRFlightStick.ApplyDeadZone(raw.y, deadzoneFractionVector.y, exponentVector.y),
					InteractionCommon.VRFlightStick.ApplyDeadZone(raw.z, deadzoneFractionVector.z, exponentVector.z));
			}

			return result;
		}

		private void RemapAxis(FlightCtrlState st, string axisBinding, float value)
		{
			switch(axisBinding)
			{
				case "X": st.X = value; break;
				case "Y": st.Y = value; break;
				case "Z": st.Z = value; break;
			}
		}

		private void GetInput(FlightCtrlState st)
		{
			if (interactable.IsGrabbed)
			{
				Vector3 normalizedInput = GetNormalizedInput();
				RemapAxis(st, axisBindingX, normalizedInput.x);
				RemapAxis(st, axisBindingY, normalizedInput.y);
				RemapAxis(st, axisBindingZ, normalizedInput.z);
			}
		}

		private void OnGrab(Hand hand, SteamVR_Input_Sources source)
		{
			grabbedPosition = transform.InverseTransformPoint(hand.GripPosition);
		}
	}
}
