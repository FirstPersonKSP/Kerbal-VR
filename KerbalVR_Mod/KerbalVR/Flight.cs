
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
using HarmonyLib;

namespace KerbalVR
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class FirstPersonKerbalFlight : MonoBehaviour
	{
		public static FirstPersonKerbalFlight Instance { get; private set; }

		Transform m_lastKerbalTransform = null;

		SteamVR_Action_Vector2 m_moveStickAction;
		SteamVR_Action_Vector2 m_lookStickAction;
		SteamVR_Action_Single m_rcsUpAction;
		SteamVR_Action_Single m_rcsDownAction;
		SteamVR_Action_Boolean m_toggleRCSAction;
		SteamVR_Action_Boolean m_toggleLightAction;
		SteamVR_Action_Boolean m_jumpAction;

		public void Awake()
		{
			Utils.Log("Flight.Awake");

			Instance = this;

			GameEvents.OnIVACameraKerbalChange.Add(OnIVACameraKerbalChange);
			GameEvents.OnCameraChange.Add(OnCameraChange);
			GameEvents.onVesselChange.Add(OnVesselChange);

			m_moveStickAction = SteamVR_Input.GetVector2Action("MoveStick");
			m_lookStickAction = SteamVR_Input.GetVector2Action("LookStick");
			m_rcsUpAction = SteamVR_Input.GetSingleAction("RCSUp");
			m_rcsDownAction = SteamVR_Input.GetSingleAction("RCSDown");
			m_toggleRCSAction = SteamVR_Input.GetBooleanAction("ToggleRCS");
			m_toggleLightAction = SteamVR_Input.GetBooleanAction("ToggleLight");
			m_jumpAction = SteamVR_Input.GetBooleanAction("Jump");

			m_toggleRCSAction.onStateDown += ToggleRCS_OnStateDown;
			m_toggleLightAction.onStateDown += ToggleLight_OnStateDown;
		}

		private void OnVesselChange(Vessel data)
		{
			OnCameraChange(CameraManager.Instance.currentCameraMode);
		}

		public void FixedUpdate()
		{
			
		}

		public void OnDestroy()
		{
			Utils.Log("Flight.OnDestroy");
			GameEvents.OnIVACameraKerbalChange.Remove(OnIVACameraKerbalChange);
			GameEvents.OnCameraChange.Remove(OnCameraChange);

			if (KerbalVR.InteractionSystem.Instance != null)
			{
				KerbalVR.InteractionSystem.Instance.transform.parent = null;
			}

			m_toggleRCSAction.onStateDown -= ToggleRCS_OnStateDown;
			m_toggleLightAction.onStateDown -= ToggleLight_OnStateDown;

			Instance = null;
		}

		private void OnIVACameraKerbalChange(Kerbal kerbal)
		{
			SetArmBoneScale(m_lastKerbalTransform, Vector3.one);

			FixIVACamera();
		}

		void ResetVRPosition()
		{
			if (SteamVR.active)
			{
				// seated mode means the transform of the Unity Camera is the nominal eye position for the VR headset
				// in standing mode the transform needs to be at ground level
				SteamVR.settings.trackingSpace = ETrackingUniverseOrigin.TrackingUniverseSeated;

				var chaperone = OpenVR.Chaperone;
				if (chaperone != null)
					chaperone.ResetZeroPose(SteamVR.settings.trackingSpace);
			}
		}

		private void OnCameraChange(CameraManager.CameraMode mode)
		{
			// restore kerbal arms
			SetArmBoneScale(m_lastKerbalTransform, Vector3.one);
			m_lastKerbalTransform = null;

			bool isEVA = FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.isEVA;

			KerbalVR.Core.SetActionSetActive("EVA", isEVA);

			if (mode == CameraManager.CameraMode.IVA)
			{
				ResetVRPosition();
				FixIVACamera();
			}
			else if (isEVA)
			{
				ResetVRPosition();
				FixEVACamera();
			}
		}

		static readonly string[] ArmBones = { "bn_l_arm01", "bn_r_arm01" };

		void SetArmBoneScale(Transform kerbalTransform, Vector3 scale)
		{
			if (kerbalTransform != null)
			{
				var meshRenderer = kerbalTransform.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

				foreach (var b in meshRenderer.bones)
				{
					if (ArmBones.Contains(b.name))
					{
						b.localScale = scale;
					}
				}
			}
		}

		private void FixIVACamera()
		{
			Utils.Log("Flight.FixIVACamera");

			if (KerbalVR.InteractionSystem.Instance == null) return;

			// The IVA seats have a scale on them and VR camera doesn't seem to respect local scale on the camera's transform itself
			if (InternalCamera.Instance.transform.parent == CameraManager.Instance.IVACameraActiveKerbal.eyeTransform)
			{
				// TODO: how does this work with PCR?

				// Don't really know why, but sometimes the eye transform has a nonzero scale on it
				// this messes with the camera projection and the size/movement of the hands.
				// does this mess with our external view of the rocket...?
				Transform eyeTransform = InternalCamera.Instance.transform.parent;
				eyeTransform.localScale = Vector3.one;
				InternalCamera.Instance.transform.localScale = Vector3.one;
				KerbalVR.InteractionSystem.Instance.transform.localScale = Vector3.one;

				KerbalVR.InteractionSystem.Instance.transform.SetParent(InternalCamera.Instance.transform.parent, false);

				m_lastKerbalTransform = eyeTransform.parent;

				SetArmBoneScale(m_lastKerbalTransform, Vector3.zero);
			}
		}

		private void FixEVACamera()
		{
			Utils.Log("Flight.FixEVACamera");

			if (KerbalVR.InteractionSystem.Instance == null) return;

			var kerbalEVA = FlightGlobals.fetch.activeVessel.evaController;

			KerbalVR.InteractionSystem.Instance.transform.SetParent(FlightCamera.fetch.transform, false);

			// force ThroughTheEyes to run its modifications on the kerbal's FSM.  These should be safe to run more than once
			FirstPerson.FirstPersonEVA.instance.fpStateWalkRun.evt_OnEnterFirstPerson(kerbalEVA);
			FirstPerson.FirstPersonEVA.instance.fpStateFloating.evt_OnEnterFirstPerson(kerbalEVA);

			kerbalEVA.On_jump_start.OnCheckCondition = (KFSMState currentState) => m_jumpAction.state && !kerbalEVA.PartPlacementMode && !EVAConstructionModeController.MovementRestricted;
			
		}

		public void HandleMovementInput_Prefix(KerbalEVA kerbalEVA)
		{
			// this enables "FPS" controls so the character will strafe instead of turning and walking
			kerbalEVA.CharacterFrameModeToggle = true;

			Vector2 moveStickInput = m_moveStickAction.GetAxis(SteamVR_Input_Sources.Any);
			float verticalInput = m_rcsUpAction.GetAxis(SteamVR_Input_Sources.Any) - m_rcsDownAction.GetAxis(SteamVR_Input_Sources.Any);

			// TODO: deadzone/exponent? builtin response seems OK

			// TODO: sprint?

			Vector3 tgtRpos =
				moveStickInput.y * kerbalEVA.transform.forward +
				moveStickInput.x * kerbalEVA.transform.right;

			Vector3 packTgtRpos = tgtRpos + verticalInput * kerbalEVA.transform.up;

			FirstPerson.ReflectedMembers.eva_tgtRpos.SetValue(kerbalEVA, tgtRpos);
			FirstPerson.ReflectedMembers.eva_packTgtRPos.SetValue(kerbalEVA, packTgtRpos);
			FirstPerson.ReflectedMembers.eva_ladderTgtRPos.SetValue(kerbalEVA, packTgtRpos); // for now, same as jetpack

			// TODO: parachute input
		}

		public void HandleMovementInput_Postfix(KerbalEVA kerbalEVA)
		{
			// rotation needs to be done after the main method or else it will get overwritten
			Vector2 lookStickInput = m_lookStickAction.GetAxis(SteamVR_Input_Sources.Any);

			float yaw = lookStickInput.x;

			// TODO: pitch/roll for space

			const float YAW_SCALE = 0.5f;

			FirstPerson.FirstPersonEVA.instance.fpStateWalkRun.current_turn += YAW_SCALE * yaw * kerbalEVA.turnRate * Time.fixedDeltaTime * Mathf.Rad2Deg;

			if (FirstPerson.FirstPersonEVA.instance.fpStateWalkRun.current_turn < 0)
			{
				FirstPerson.FirstPersonEVA.instance.fpStateWalkRun.current_turn += 360.0f;
			}
			if (FirstPerson.FirstPersonEVA.instance.fpStateWalkRun.current_turn > 360.0f)
			{
				FirstPerson.FirstPersonEVA.instance.fpStateWalkRun.current_turn -= 360.0f;
			}
		}

		private void ToggleLight_OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			var kerbalEVA = FlightGlobals.ActiveVessel.evaController;
			kerbalEVA.ToggleLamp();
		}

		private void ToggleRCS_OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			var kerbalEVA = FlightGlobals.ActiveVessel.evaController;
			kerbalEVA.ToggleJetpack();
		}
	}

	[HarmonyPatch(typeof(KerbalEVA))]
	[HarmonyPatch("HandleMovementInput")]
	class KerbalEVAPatch
	{
		
		static void Prefix(KerbalEVA __instance)
		{
			//if (!kerbalEVA.VesselUnderControl || (!kerbalEVA.SurfaceOrSplashed() && kerbalEVA.JetpackDeployed && EVAConstructionModeController.MovementRestricted))
			//{
			//	return;
			//}

			FirstPersonKerbalFlight.Instance.HandleMovementInput_Prefix(__instance);
		}

		static void Postfix(KerbalEVA __instance)
		{
			FirstPersonKerbalFlight.Instance.HandleMovementInput_Postfix(__instance);
		}
	}
}