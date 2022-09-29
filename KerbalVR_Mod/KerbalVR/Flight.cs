
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
using HarmonyLib;
using KSP.UI;
using System.Reflection;
using System.Reflection.Emit;

namespace KerbalVR
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class FirstPersonKerbalFlight : MonoBehaviour
	{
		public static FirstPersonKerbalFlight Instance { get; private set; }

		Transform m_lastKerbalTransform = null;
		Collider m_lastSeatCollider = null;

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
		SteamVR_Action_Boolean m_plantFlagAction;

		public void Awake()
		{
			Utils.Log("Flight.Awake");

			Instance = this;

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
			m_plantFlagAction = SteamVR_Input.GetBooleanAction("PlantFlag");

			m_toggleRCSAction.onStateDown += ToggleRCS_OnStateDown;
			m_toggleLightAction.onStateDown += ToggleLight_OnStateDown;
			m_sprintAction.onStateDown += Sprint_OnStateDown;
			m_swapRollYawAction.onStateDown += SwapRollYaw_OnStateDown;
			m_jumpAction.onStateDown += Jump_OnStateDown;
			m_plantFlagAction.onStateDown += PlantFlag_OnStateDown;
		}

		private void OnVesselChange(Vessel data)
		{
			OnCameraChange(CameraManager.Instance.currentCameraMode);
		}

		public void LateUpdate()
		{
			if (GameSettings.CAMERA_MODE.GetKeyDown())
			{
				KerbalVR.Core.ResetVRPosition();
			}
		}

		public void OnDestroy()
		{
			Utils.Log("Flight.OnDestroy");
			GameEvents.OnCameraChange.Remove(OnCameraChange);

			if (KerbalVR.InteractionSystem.Instance != null)
			{
				KerbalVR.InteractionSystem.Instance.transform.parent = null;
			}

			m_toggleRCSAction.onStateDown -= ToggleRCS_OnStateDown;
			m_toggleLightAction.onStateDown -= ToggleLight_OnStateDown;
			m_sprintAction.onStateDown -= Sprint_OnStateDown;
			m_swapRollYawAction.onStateDown -= SwapRollYaw_OnStateDown;
			m_jumpAction.onStateDown -= Jump_OnStateDown;
			m_plantFlagAction.onStateDown -= PlantFlag_OnStateDown;

			Instance = null;
		}

		// Note, jumping in EVA isn't handled here; it's hooked into the kerbal FSM.  This is just for exiting an external seat or getting off a ladder
		private void Jump_OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			var kerbalEVA = KerbalVR.Scene.GetKerbalEVA();

			if (kerbalEVA != null)
			{
				if (kerbalEVA.IsSeated())
				{
					kerbalEVA.OnDeboardSeat();
					KerbalVR.Scene.EnterFirstPerson();
				}
				else if (kerbalEVA.OnALadder)
				{
					kerbalEVA.fsm.RunEvent(kerbalEVA.On_ladderLetGo);
				}
			}
		}

		private void PlantFlag_OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			var kerbalEVA = KerbalVR.Scene.GetKerbalEVA();

			if (kerbalEVA != null && kerbalEVA.CanPlantFlag())
			{
				kerbalEVA.PlantFlag();
			}
		}

		public void OnSeatBoarded()
		{
			OnCameraChange(CameraManager.Instance.currentCameraMode);
		}

		void RestoreLastKerbal()
		{
			// restore kerbal arms
			if (m_lastKerbalTransform != null)
			{
				SetArmBoneScale(m_lastKerbalTransform, Vector3.one);
				m_lastKerbalTransform = null;
			}

			if (m_lastSeatCollider != null)
			{
				m_lastSeatCollider.enabled = true;
				m_lastSeatCollider = null;
			}
		}

		public void OnIVACameraKerbalChange()
		{
			RestoreLastKerbal();

			FixIVACamera();
		}

		private void OnCameraChange(CameraManager.CameraMode mode)
		{
			RestoreLastKerbal();
			UISystem.Instance.ModeChanged();

			var kerbalEVA = KerbalVR.Scene.GetKerbalEVA();

			if (KerbalVR.Core.IsVrRunning)
			{
				KerbalVR.Core.SetVrRunningDesired(mode != CameraManager.CameraMode.Map);
			}
			KerbalVR.Core.SetActionSetActive("EVA", kerbalEVA != null);

			if (kerbalEVA != null)
			{
				FixEVACamera(kerbalEVA);
			}
			else if (mode == CameraManager.CameraMode.IVA)
			{
				// no longer resetting position between camera swaps because the camera changes orientation between iva/eva
				// e.g. you turn 180 to reach a hatch behind you; you don't want that becoming the new base position
				//ResetVRPosition();
				FixIVACamera();
			}
			else if (mode == CameraManager.CameraMode.Internal)
			{
				// this is mainly for ProbeControlRoom
				FixInternalCamera();
			}
			else if (mode == CameraManager.CameraMode.Flight)
			{
				FixFlightCamera();
			}

			InteractionSystem.Instance.UseIVAProfile = (mode == CameraManager.CameraMode.IVA);
		}
		
		private void FixFlightCamera()
		{
			KerbalVR.InteractionSystem.Instance.transform.SetParent(FlightCamera.fetch.transform, false);
			KerbalVR.InteractionSystem.Instance.transform.localPosition = Vector3.zero;
			KerbalVR.InteractionSystem.Instance.transform.localRotation = Quaternion.identity;
		}

		private void FixInternalCamera()
		{
			Utils.Log("Flight.FixInternalCamera");

			if (KerbalVR.InteractionSystem.Instance == null) return;

			Transform eyeTransform = InternalCamera.Instance.transform.parent;
			eyeTransform.localScale = Vector3.one;
			InternalCamera.Instance.transform.localScale = Vector3.one;
			KerbalVR.InteractionSystem.Instance.transform.localScale = Vector3.one;

			KerbalVR.InteractionSystem.Instance.transform.SetParent(InternalCamera.Instance.transform.parent, false);
			KerbalVR.InteractionSystem.Instance.transform.localPosition = Vector3.zero;
			KerbalVR.InteractionSystem.Instance.transform.localRotation = Quaternion.identity;
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
			// if (InternalCamera.Instance.transform.parent == CameraManager.Instance.IVACameraActiveKerbal.eyeTransform)
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

				var seatTransform = m_lastKerbalTransform.parent;
				m_lastSeatCollider = seatTransform.GetComponent<Collider>();
				if (m_lastSeatCollider != null)
				{
					m_lastSeatCollider.enabled = false;
				}
			}
		}

		private void FixEVACamera(KerbalEVA kerbalEVA)
		{
			Utils.Log("Flight.FixEVACamera");

			if (KerbalVR.InteractionSystem.Instance == null) return;

			KerbalVR.InteractionSystem.Instance.transform.SetParent(FlightCamera.fetch.transform, false);

			Utils.GetOrAddComponent<KerbalVR_ArmScaler>(kerbalEVA.gameObject);

			if (!kerbalEVA.IsSeated())
			{
				// force ThroughTheEyes to run its modifications on the kerbal's FSM.  These should be safe to run more than once
				FirstPerson.FirstPersonEVA.instance.fpStateWalkRun.evt_OnEnterFirstPerson(kerbalEVA);
				FirstPerson.FirstPersonEVA.instance.fpStateFloating.evt_OnEnterFirstPerson(kerbalEVA);
				FirstPerson.FirstPersonEVA.instance.fpCameraManager.nearPlaneDistance = 0.02f;

				kerbalEVA.On_jump_start.OnCheckCondition = (KFSMState currentState) => kerbalEVA.VesselUnderControl && m_jumpAction.state && !kerbalEVA.PartPlacementMode && !EVAConstructionModeController.MovementRestricted;


				kerbalEVA.On_startRun.OnCheckCondition = (KFSMState currentState) => kerbalEVA.VesselUnderControl && m_isSprinting;
				kerbalEVA.On_endRun.OnCheckCondition = (KFSMState currentState) => kerbalEVA.VesselUnderControl && !m_isSprinting;

				JetpackPrecisionMode = true;
				m_isSprinting = false;
			}
		}

		public void HandleMovementInput_Prefix(KerbalEVA kerbalEVA)
		{
			if (!kerbalEVA.VesselUnderControl || !Core.IsVrRunning)
			{
				return;
			}

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

			kerbalEVA.tgtRpos = tgtRpos;
			kerbalEVA.packTgtRPos = packTgtRpos;
			kerbalEVA.ladderTgtRPos = packTgtRpos; // for now, same as jetpack (so up/down match)
		}

		public void HandleMovementInput_Postfix(KerbalEVA kerbalEVA)
		{
			if (!kerbalEVA.VesselUnderControl || !Core.IsVrRunning)
			{
				return;
			}

			// rotation needs to be done after the main method or else it will get overwritten
			Vector2 lookStickInput = m_lookStickAction.GetAxis(SteamVR_Input_Sources.Any);
			Vector2 moveStickInput = m_moveStickAction.GetAxis(SteamVR_Input_Sources.Any);

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

			// parachuteInput gets cleared in handleMovementInput, so we need to set it in postfix
			// use both sticks the same for now
			kerbalEVA.parachuteInput.x = Mathf.Clamp(lookStickInput.y + moveStickInput.y, -1.0f, 1.0f);
			kerbalEVA.parachuteInput.y = Mathf.Clamp(lookStickInput.x + moveStickInput.x, -1.0f, 1.0f);
		}

		public void FPStateFloating_PreOnFixedUpdate_Postfix(KerbalEVA kerbalEVA)
		{
			if (!kerbalEVA.VesselUnderControl ||
				!kerbalEVA.JetpackDeployed ||
				kerbalEVA.SurfaceOrSplashed() ||
				!Core.IsVrRunning)
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
				kerbalEVA.cmdRot = cmdRot;
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

			kerbalEVA.packTgtRPos = packTgtRpos;
		}

		private void ToggleLight_OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			var eva = KerbalVR.Scene.GetKerbalEVA();
			if (eva != null)
			{
				eva.ToggleLamp();
			}
		}

		private void ToggleRCS_OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			var eva = KerbalVR.Scene.GetKerbalEVA();
			if (eva != null)
			{
				eva.ToggleJetpack();
			}
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

	[HarmonyPatch(typeof(InternalSeat), nameof(InternalSeat.DespawnCrew))]
	class InternalSeatPatch
	{
		static void Prefix(InternalSeat __instance)
		{
			if (KerbalVR.InteractionSystem.Instance != null)
			{
				if (__instance.kerbalRef != null && CameraManager.Instance.IVACameraActiveKerbal == __instance.kerbalRef)
				{
					KerbalVR.InteractionSystem.Instance.transform.SetParent(null, false);
					GameObject.DontDestroyOnLoad(KerbalVR.InteractionSystem.Instance);
				}
			}
		}
	}

	[HarmonyPatch(typeof(CameraManager), nameof(CameraManager.NextCameraIVA))]
	class CameraManagerPatch
	{
		static void Prefix()
		{
			// disconnect the interaction system early, so that JSI doesn't disable it during the event callbacks
			KerbalVR.InteractionSystem.Instance.transform.SetParent(null, false);
			GameObject.DontDestroyOnLoad(KerbalVR.InteractionSystem.Instance);
		}

		static void Postfix()
		{
			FirstPersonKerbalFlight.Instance.OnIVACameraKerbalChange();
		}
	}

	[HarmonyPatch(typeof(FlagSite), nameof(FlagSite.OnPlacementComplete))]
	class FlagSitePatch
	{
		static void Postfix(FlagSite __instance)
		{
			__instance.DismissSiteRename();
		}
	}

	// support for deploying inventory items in EVA
	// this function checks the EVA_Jump key directly
	[HarmonyPatch(typeof(ModuleInventoryPart), nameof(ModuleInventoryPart.OnUpdate))]
	class ModuleInventoryPart_Update_Patch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			bool replaceNextKeyDown = false;
			bool replacementSuccessful = false;
			FieldInfo EVA_Jump_fieldInfo = AccessTools.DeclaredField(typeof(GameSettings), nameof(GameSettings.EVA_Jump));
			MethodInfo KeyDown_methodInfo = AccessTools.DeclaredMethod(typeof(KeyBinding), nameof(KeyBinding.GetKeyDown));
			MethodInfo replacement_methodInfo = AccessTools.DeclaredMethod(typeof(ModuleInventoryPart_Update_Patch), nameof(ShouldDeployPart));

			foreach (var code in instructions)
			{
				if (code.LoadsField(EVA_Jump_fieldInfo))
				{
					replaceNextKeyDown = true;
				}

				if (replaceNextKeyDown && code.Calls(KeyDown_methodInfo))
				{
					code.opcode = OpCodes.Call;
					code.operand = replacement_methodInfo;
					replaceNextKeyDown = false;
					replacementSuccessful = true;
				}

				yield return code;
			}

			if (!replacementSuccessful)
			{
				throw new Exception("[KerbalVR]: failed to patch ModuleInventoryPart.OnUpdate");
			}
		}

		static bool ShouldDeployPart(KeyBinding keyBinding, bool ignoreInputLock)
		{
			// TODO: extend this to check for VR inputs
			if (keyBinding.GetKeyDown(ignoreInputLock))
			{
				return true;
			}

			if (Core.IsVrRunning && SteamVR_Actions.eVA_Jump.stateDown)
			{
				return true;
			}

			return false;
		}
	}
}