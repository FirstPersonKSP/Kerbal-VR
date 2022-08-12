using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.Loaders
{
	[HarmonyPatch(typeof(InternalModel), nameof(InternalModel.Load))]
	internal class InternalModelLoader
	{
		public static void Postfix(InternalModel __instance)
		{
			// InternalModel.Load gets called both at load time and when spawning the IVA
			// for now, we only need to modify the prefabs
			if (HighLogic.LoadedScene != GameScenes.LOADING) return;

			Utils.Log($"Patching {__instance.name}");

			// add colliders for internal seats
			for (int seatIndex = 0; seatIndex < __instance.seats.Count; seatIndex++)
			{
				var seat = __instance.seats[seatIndex];

				if (seat.seatTransform != null)
				{
					var collider = seat.seatTransform.gameObject.AddComponent<SphereCollider>();
					collider.radius = 0.15f;
					collider.center = Vector3.up * 0.3f + seat.kerbalOffset;
					collider.isTrigger = true;

					seat.transform.gameObject.layer = 20;

					// Utils.GetOrAddComponent<ColliderVisualizer>(collider.gameObject);

					var vrSeat = collider.gameObject.AddComponent<VRSeat>();
					vrSeat.internalSeatIndex = seatIndex;
				}
			}
		}
		

	}
}
