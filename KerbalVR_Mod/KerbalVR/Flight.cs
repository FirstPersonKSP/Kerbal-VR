
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

		static readonly float EVA_PRECISION_MODE_SCALE = 0.5f;
		static float EVA_FLOATING_ROTATION_SCALE = 0.25f;

		bool m_jetpackPrecisionMode = true;
		bool JetpackPrecisionMode
		{
			get { return m_jetpackPrecisionMode; }
			set
			{
				m_jetpackPrecisionMode = value;
				FirstPerson.FirstPersonEVA.instance.state.eva_throttle = m_jetpackPrecisionMode ? EVA_PRECISION_MODE_SCALE : 1.0f;
			}
		}

		bool m_isSprinting = false;
		bool m_lookStickIsRoll = false;

		SteamVR_Action_Vector2 m_moveStickAction;
		SteamVR_Action_Vector2 m_lookStickAction;
		SteamVR_Action_Single m_rcsUpAction;
		SteamVR_Action_Single m_rcsDownAction;
		SteamVR_Action_Boolean m_toggleRCSAction;
		SteamVR_Action_Boolean m_toggleLightAction;
		SteamVR_Action_Boolean m_jumpAction;
		SteamVR_Action_Boolean m_sprintAction;
		SteamVR_Action_Boolean m_swapRollYawAction;

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
			m_sprintAction = SteamVR_Input.GetBooleanAction("Sprint");
			m_swapRollYawAction = SteamVR_Input.GetBooleanAction("SwapRollYaw");

			m_toggleRCSAction.onStateDown += ToggleRCS_OnStateDown;
			m_toggleLightAction.onStateDown += ToggleLight_OnStateDown;
			m_sprintAction.onStateDown += Sprint_OnStateDown;
			m_swapRollYawAction.onStateDown += SwapRollYaw_OnStateDown;
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
			m_sprintAction.onStateDown -= Sprint_OnStateDown;
			m_swapRollYawAction.onStateDown -= SwapRollYaw_OnStateDown;

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
			FirstPerson.FirstPersonEVA.instance.fpCameraManager.nearPlaneDistance = 0.02f;

			kerbalEVA.On_jump_start.OnCheckCondition = (KFSMState currentState) => m_jumpAction.state && !kerbalEVA.PartPlacementMode && !EVAConstructionModeController.MovementRestricted;

			kerbalEVA.On_startRun.OnCheckCondition = (KFSMState currentState) => m_isSprinting;
			kerbalEVA.On_endRun.OnCheckCondition = (KFSMState currentState) => !m_isSprinting;

			JetpackPrecisionMode = true;
			m_isSprinting = false;
		}

		public void HandleMovementInput_Prefix(KerbalEVA kerbalEVA)
		{
			// this enables "FPS" controls so the character will strafe instead of turning and walking
			kerbalEVA.CharacterFrameModeToggle = true;

			Vector2 moveStickInput = m_moveStickAction.GetAxis(SteamVR_Input_Sources.Any);
			float verticalInput = m_rcsUpAction.GetAxis(SteamVR_Input_Sources.Any) - m_rcsDownAction.GetAxis(SteamVR_Input_Sources.Any);

			// TODO: deadzone/exponent? builtin response seems OK

			if (m_sprintAction.state && moveStickInput.y > 0.5f)
			{
				m_isSprinting = true;
			}

			if (m_isSprinting && moveStickInput.y < 0.5f || kerbalEVA.JetpackDeployed)
			{
				m_isSprinting = false;
			}

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

		public void FPStateFloating_PreOnFixedUpdate_Postfix(KerbalEVA kerbalEVA)
		{
			if (kerbalEVA.vessel.situation == Vessel.Situations.SPLASHED || 
				kerbalEVA.vessel.situation == Vessel.Situations.LANDED ||
				!kerbalEVA.JetpackDeployed)
			{
				return;
			}

			// ----- rotation

			Vector2 lookStickInput = m_lookStickAction.GetAxis(SteamVR_Input_Sources.Any);

			float yaw = m_lookStickIsRoll ? 0.0f : lookStickInput.x; // rotation around up
			float pitch = lookStickInput.y; // rotation around right
			float roll = m_lookStickIsRoll ? -lookStickInput.x : 0.0f; // rotation around forward

			Vector3 cmdRot =
				yaw * kerbalEVA.transform.up +
				pitch * kerbalEVA.transform.right +
				roll * kerbalEVA.transform.forward;

			// we want rotation strength to be the same regardless of precision mode, so scale this up to make it consistent
			if (m_jetpackPrecisionMode)
			{
				cmdRot /= EVA_PRECISION_MODE_SCALE;
			}

			cmdRot *= EVA_FLOATING_ROTATION_SCALE;

			if (cmdRot != Vector3.zero)
			{
				FirstPerson.ReflectedMembers.eva_cmdRot.SetValue(kerbalEVA, cmdRot);
				FirstPerson.FirstPersonEVA.instance.state.rotationpid_previouserror = Vector3.zero;
				FirstPerson.FirstPersonEVA.instance.state.rotationpid_integral = Vector3.zero;
			}

			// ----- translation

			Vector2 moveStickInput = m_moveStickAction.GetAxis(SteamVR_Input_Sources.Any);
			float verticalInput = m_rcsUpAction.GetAxis(SteamVR_Input_Sources.Any) - m_rcsDownAction.GetAxis(SteamVR_Input_Sources.Any);

			// TODO: deadzone/exponent? builtin response seems OK

			Vector3 tgtRpos =
				moveStickInput.y * kerbalEVA.transform.forward +
				moveStickInput.x * kerbalEVA.transform.right;

			Vector3 packTgtRpos = tgtRpos + verticalInput * kerbalEVA.transform.up;

			packTgtRpos.Normalize();

			FirstPerson.ReflectedMembers.eva_packTgtRPos.SetValue(kerbalEVA, packTgtRpos);

		}

		private void ToggleLight_OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			FlightGlobals.ActiveVessel.evaController.ToggleLamp();
		}

		private void ToggleRCS_OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			FlightGlobals.ActiveVessel.evaController.ToggleJetpack();
		}
		private void Sprint_OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			KerbalEVA kerbalEVA = FlightGlobals.ActiveVessel.evaController;

			if (!kerbalEVA) return;

			if (kerbalEVA.vessel.situation == Vessel.Situations.SPLASHED ||
				kerbalEVA.vessel.situation == Vessel.Situations.LANDED ||
				!kerbalEVA.JetpackDeployed)
			{
				// TODO: actual sprint support
				return;
			}

			// maybe this should be a separate action?
			JetpackPrecisionMode = !JetpackPrecisionMode;
		}

		private void SwapRollYaw_OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			m_lookStickIsRoll = !m_lookStickIsRoll;
		}
	}

	[HarmonyPatch(typeof(KerbalEVA), "HandleMovementInput")]
	class KerbalEVAPatch
	{
		
		static void Prefix(KerbalEVA __instance)
		{
			FirstPersonKerbalFlight.Instance.HandleMovementInput_Prefix(__instance);
		}

		static void Postfix(KerbalEVA __instance)
		{
			FirstPersonKerbalFlight.Instance.HandleMovementInput_Postfix(__instance);
		}
	}

	[HarmonyPatch(typeof(FirstPerson.FPStateFloating), "evtHook_PreOnFixedUpdate")]
	class FPStateFloatingPatch
	{
		static void Postfix(FirstPerson.FPStateFloating __instance, KerbalEVA eva)
		{
			FirstPersonKerbalFlight.Instance.FPStateFloating_PreOnFixedUpdate_Postfix(eva);
		}
	}

}