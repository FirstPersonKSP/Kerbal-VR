using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace KerbalVR
{
	static class CameraUtils
	{

		public static GameObject CreateVRAnchor(Camera camera)
		{
			var anchorName = camera.name + "VRAnchor";
			
			Transform anchorTransform;
			if (camera.transform.parent.name == anchorName)
			{
				anchorTransform = camera.transform.parent;
			}
			else
			{
				anchorTransform = camera.transform.parent?.Find(anchorName);
				if (anchorTransform == null)
				{
					anchorTransform = new GameObject(anchorName).transform;
					anchorTransform.SetParent(camera.transform.parent, false);
				}

				anchorTransform.localPosition = camera.transform.localPosition;
				anchorTransform.localRotation = camera.transform.localRotation;
				anchorTransform.localScale = camera.transform.localScale;
				camera.transform.SetParent(anchorTransform, false);
			}
			
			camera.transform.localPosition = Vector3.zero;
			camera.transform.localRotation = Quaternion.identity;
			camera.transform.localScale = Vector3.one;

			return anchorTransform.gameObject;
		}

		public static T CloneComponent<T>(T oldComponent, GameObject to) where T : Component
		{
			if (oldComponent != null)
			{
				var newComponent = to.AddComponent<T>();

				FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (var field in fields)
				{
					object value = field.GetValue(oldComponent);
					field.SetValue(newComponent, value);
				}

				return newComponent;
			}

			return null;
		}

		public static T MoveComponent<T>(GameObject from, GameObject to) where T : Component
		{
			var oldComponent = from.GetComponent<T>();
			if (oldComponent != null)
			{
				var newComponent = CloneComponent<T>(oldComponent, to);

				Component.DestroyImmediate(oldComponent);

				return newComponent;
			}

			return null;
		}

	}

	class CameraFOVPatch
	{
		private static MethodInfo set_fieldOfView = AccessTools.DeclaredPropertySetter(typeof(Camera), nameof(Camera.fieldOfView));

		static void SetCameraFOV(Camera camera, float fov)
		{
			if (!camera.stereoEnabled)
			{
				// camera.fieldOfView = fov;
			}
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			bool found = false;

			foreach (var instruction in instructions)
			{
				if (instruction.Calls(set_fieldOfView))
				{
					instruction.opcode = OpCodes.Call;
					instruction.operand = AccessTools.Method(typeof(CameraFOVPatch), nameof(SetCameraFOV));
					found = true;
				}

				yield return instruction;
			}

			if (!found)
			{
				Utils.LogError("Failed to patch set_fieldOfView call");
			}
		}

		public static void PatchAll(Harmony harmony)
		{
			Utils.Log("Patching Camera FOV");

			MethodInfo transpiler = AccessTools.Method(typeof(CameraFOVPatch), nameof(CameraFOVPatch.Transpiler));
			(Type, string)[] targets =
			{
				(typeof(FXCamera), nameof(FXCamera.LateUpdate)),
				(typeof(FXDepthCamera), nameof(FXDepthCamera.LateUpdate)),
				(typeof(InternalCamera), nameof(InternalCamera.SetFOV)),
				(typeof(InternalCamera), nameof(InternalCamera.UpdateState)),
				(typeof(FlightCamera), nameof(FlightCamera.SetFoV)),
				(typeof(FlightCamera), nameof(FlightCamera.EnableCamera)),
				(typeof(ScaledCamera), nameof(ScaledCamera.SetFoV)),
				(typeof(GalaxyCameraControl), nameof(GalaxyCameraControl.SetFoV)),
				(typeof(InternalSpaceOverlay), nameof(InternalSpaceOverlay.LateUpdate)),
				(typeof(IVACamera), nameof(IVACamera.UpdateState)),
				(typeof(VehiclePhysics.CameraFree), nameof(VehiclePhysics.CameraFree.Update)),
				(typeof(VehiclePhysics.CameraLookAt), nameof(VehiclePhysics.CameraLookAt.Update)),
			};

			foreach ((Type type, string method) in targets)
			{
				harmony.Patch(AccessTools.Method(type, method), transpiler: new HarmonyMethod(transpiler));
			}
		}
	}

	[HarmonyPatch(typeof(AerodynamicsFX), nameof(AerodynamicsFX.Start))]
	class AerodynamicsFX_Patch
	{
		public static void Postfix(AerodynamicsFX __instance)
		{
			if (KerbalVR.Core.IsVrEnabled)
			{
				// these cameras are children of Camera 00, which means they would get head tracking double-applied
				// Move them up one transform (child of Camera LocalSpace) so they are peers of the other cameras and get the same tracking
				__instance.fxCamera.transform.SetParent(__instance.fxCamera.transform.parent.parent, false);
				__instance.fxDepthCamera.transform.SetParent(__instance.fxDepthCamera.transform.parent.parent, false);
			}
		}
	}

	[HarmonyPatch(typeof(InternalCamera), nameof(InternalCamera.Update))]
	class InternalCamera_Patch
	{
		public static void Prefix(InternalCamera __instance, ref float __state)
		{
			__state = __instance.orbitSensitivity;

			if (Core.IsVrRunning)
			{
				__instance.currentPitch = 0.0f;
				__instance.currentRot = 0.0f;
				__instance.orbitSensitivity = 0.0f;
			}
		}

		public static void Postfix(InternalCamera __instance, float __state)
		{
			__instance.orbitSensitivity = __state;
		}
	}

	[HarmonyPatch(typeof(InternalCamera), nameof(InternalCamera.UpdateState))]
	class InternalCamera_UpdateState_Patch
	{
		public static void Postfix(InternalCamera __instance)
		{
			// copy the camera wobble offsets to the parent transform since it can't drive the transform that is controlled by the VR rig
			if (Core.IsVrRunning && Scene.IsInIVA())
			{
				__instance.transform.parent.localPosition = __instance.transform.localPosition;
				__instance.transform.parent.localRotation = __instance.transform.localRotation;
				__instance.transform.localPosition = Vector3.zero;
				__instance.transform.localRotation = Quaternion.identity;
			}
		}
	}

	// [harmonyPatch(typeof(PlanetariumCamera), nameof(PlanetariumCamera.Activate))]

}
