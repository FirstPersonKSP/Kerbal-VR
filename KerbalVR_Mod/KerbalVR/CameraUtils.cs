using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR
{
	static class CameraUtils
	{

		public static GameObject CreateVRAnchor(Camera camera)
		{
			var anchor = new GameObject(camera.name + " VRAnchor");
			anchor.transform.localPosition = camera.transform.localPosition;
			anchor.transform.localRotation = camera.transform.localRotation;
			anchor.transform.localScale = camera.transform.localScale;
			anchor.transform.SetParent(camera.transform.parent, false);
			camera.transform.SetParent(anchor.transform, false);
			camera.transform.localPosition = Vector3.zero;
			camera.transform.localRotation = Quaternion.identity;
			camera.transform.localScale = Vector3.one;

			return anchor;
		}

		public static T MoveComponent<T>(GameObject from, GameObject to) where T : Component
		{
			var oldComponent = from.GetComponent<T>();
			if (oldComponent != null)
			{
				var newComponent = to.AddComponent<T>();

				FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (var field in fields)
				{
					object value = field.GetValue(oldComponent);
					field.SetValue(newComponent, value);
				}

				Component.DestroyImmediate(oldComponent);

				return newComponent;
			}

			return null;
		}

	}

	[HarmonyPatch(typeof(FXCamera), "LateUpdate")]
	[HarmonyPatch(typeof(FXDepthCamera), "LateUpdate")]
	[HarmonyPatch(typeof(InternalCamera), nameof(InternalCamera.SetFOV))]
	[HarmonyPatch(typeof(InternalCamera), nameof(InternalCamera.UpdateState))]
	[HarmonyPatch(typeof(FlightCamera), nameof(FlightCamera.SetFoV))]
	[HarmonyPatch(typeof(ScaledCamera), nameof(ScaledCamera.SetFoV))]
	[HarmonyPatch(typeof(GalaxyCameraControl), nameof(GalaxyCameraControl.SetFoV))]
	[HarmonyPatch(typeof(InternalSpaceOverlay), nameof(InternalSpaceOverlay.LateUpdate))]
	[HarmonyPatch(typeof(IVACamera), "UpdateState")]
	[HarmonyPatch(typeof(VehiclePhysics.CameraFree), nameof(VehiclePhysics.CameraFree.Update))]
	[HarmonyPatch(typeof(VehiclePhysics.CameraLookAt), nameof(VehiclePhysics.CameraLookAt.Update))]
	class CameraFOVPatch
	{
		private static MethodInfo set_fieldOfView = AccessTools.DeclaredPropertySetter(typeof(Camera), "fieldOfView");

		static void SetCameraFOV(Camera camera, float fov)
		{
			if (!camera.stereoEnabled)
			{
				// camera.fieldOfView = fov;
			}
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Callvirt && ReferenceEquals(instruction.operand, set_fieldOfView))
				{
					instruction.opcode = OpCodes.Call;
					instruction.operand = AccessTools.Method(typeof(CameraFOVPatch), nameof(SetCameraFOV));
				}
				
				yield return instruction;
			}
		}
	}

	[HarmonyPatch(typeof(FXCamera), nameof(FXCamera.Start))]
	class FXCameraPatch_Start
	{
		public static void Postfix(FXCamera __instance)
		{
			__instance.velocityCam.stereoTargetEye = StereoTargetEyeMask.None;
			__instance.velocityCam.transform.SetParent(FlightCamera.fetch.transform.parent, false);

			__instance.transform.SetParent(FlightCamera.fetch.transform.parent, false);
		}
	}

	[HarmonyPatch(typeof(FXCamera), nameof(FXCamera.LateUpdate))]
	class FXCameraPatch_LateUpdate
	{
		public static void Postfix(FXCamera __instance)
		{
			__instance.transform.localPosition = FlightCamera.fetch.transform.localPosition;
			__instance.transform.localRotation = FlightCamera.fetch.transform.localRotation;
			__instance.transform.localScale = FlightCamera.fetch.transform.localScale;
		}
	}

	// [harmonyPatch(typeof(PlanetariumCamera), nameof(PlanetariumCamera.Activate))]

}
