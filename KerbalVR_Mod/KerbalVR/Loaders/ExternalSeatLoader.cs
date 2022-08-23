using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.Loaders
{
	// adds the colliders and behaviors to the prefab at load time
	[HarmonyPatch(typeof(KerbalSeat), nameof(KerbalSeat.OnLoad))]
	internal class ExternalSeatLoader
	{
		public static void Postfix(KerbalSeat __instance, ConfigNode node)
		{
			if (HighLogic.LoadedScene == GameScenes.LOADING)
			{
				var seatTransform = __instance.part.FindModelTransform(__instance.seatPivotName);

				var collider = VRInternalSeat.AddSeatCollider(seatTransform, Vector3.zero, 21);
				collider.gameObject.AddComponent<VRExternalSeat>();
			}
		}
	}
}
