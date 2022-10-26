using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	internal class KerbalVR_FreeIVA : MonoBehaviour
	{
		void Awake()
		{
			if (!Core.IsVrEnabled)
			{
				Component.Destroy(this);
				return;
			}

			FreeIva.KerbalIvaController.GetInput += FreeIva_GetInput;
		}

		void OnDestroy()
		{
			if (!Core.IsVrEnabled) return;

			FreeIva.KerbalIvaController.GetInput -= FreeIva_GetInput;
		}

		private void FreeIva_GetInput(ref FreeIva.KerbalIvaController.IVAInput input)
		{
			FirstPersonKerbalFlight.Instance.GetKerbalRotationInput(out float yaw, out float pitch, out float roll);

			yaw *= yawRate;
			pitch *= pitchRate;
			roll *= rollRate;

			if (input.MovementThrottle == Vector3.zero)
			{
				input.MovementThrottle = FirstPersonKerbalFlight.Instance.GetKerbalMovementThrottle();
			}
			if (input.RotationInputEuler == Vector3.zero)
			{
				input.RotationInputEuler = new Vector3(pitch, yaw, roll);
			}
		}

		static float pitchRate = 0.5f;
		static float yawRate = 0.5f;
		static float rollRate = 0.5f;
	}

	[HarmonyPatch(typeof(FreeIva.KerbalIvaController), nameof(FreeIva.KerbalIvaController.Unbuckle))]
	class KerbalIvaController_Unbuckle_Patch
	{
		static void Prefix()
		{
			InteractionSystem.Instance.transform.SetParent(null, false);
		}

		static void Postfix(FreeIva.KerbalIvaController __instance)
		{
			FirstPersonKerbalFlight.Instance.FixInternalCamera();
			KerbalVR.Core.SetActionSetActive("EVA", true);
		}
	}

	[HarmonyPatch(typeof(FreeIva.KerbalIvaController), nameof(FreeIva.KerbalIvaController.Buckle))]
	class FreeIvaController_Buckle_Patch
	{
		static void Postfix(FreeIva.KerbalIvaController __instance)
		{
			// buckle is going to need a lot more work, but at a minmum we need to restore the vr anchor
			FirstPersonKerbalFlight.Instance.FixInternalCamera();
			KerbalVR.Core.SetActionSetActive("EVA", false);
		}
	}
}
