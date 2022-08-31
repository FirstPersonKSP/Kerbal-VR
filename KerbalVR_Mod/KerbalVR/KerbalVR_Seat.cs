using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR
{
	// interactable behavior that gets attached to internal seats
	public class VRInternalSeat : InteractableBehaviour
	{
		public int internalSeatIndex = -1;

		void Awake()
		{
			OnGrab += OnGrabbed;
			OnRelease += OnReleased;
		}

		private void OnReleased(Hand hand, SteamVR_Input_Sources source)
		{
			
		}

		private void OnGrabbed(Hand hand, SteamVR_Input_Sources source)
		{
			var internalModel = gameObject.GetComponentUpwards<InternalModel>();
			var internalSeat = internalModel.seats[internalSeatIndex];

			var oldSetting = GameSettings.IVA_RETAIN_CONTROL_POINT;
			GameSettings.IVA_RETAIN_CONTROL_POINT = true;

			// if there is a kerbal here, switch to them
			if (internalSeat.kerbalRef != null)
			{
				// .. as long as it's not us..
				if (internalSeat.kerbalRef != CameraManager.Instance.IVACameraActiveKerbal)
				{
					CameraManager.Instance.SetCameraIVA(internalSeat.kerbalRef, true);
					GameEvents.OnIVACameraKerbalChange.Fire(internalSeat.kerbalRef);
					FirstPersonKerbalFlight.Instance.OnIVACameraKerbalChange();
				}
			}
			else
			{
				var kerbal = CameraManager.Instance.IVACameraActiveKerbal;
				kerbal.protoCrewMember.seat.kerbalRef = null;

				internalModel.UnseatKerbalAt(kerbal.protoCrewMember.seat);
				internalModel.SitKerbalAt(kerbal.protoCrewMember, internalSeat);

				kerbal.transform.parent = internalSeat.seatTransform;
				kerbal.transform.localPosition = internalSeat.kerbalOffset;
				kerbal.transform.localScale = Vector3.Scale(kerbal.transform.localScale, internalSeat.kerbalScale);
				kerbal.transform.localRotation = Quaternion.identity;
				kerbal.InPart = internalModel.part;
				kerbal.ShowHelmet(internalSeat.allowCrewHelmet);
				internalSeat.kerbalRef = kerbal;

				GameEvents.OnIVACameraKerbalChange.Fire(internalSeat.kerbalRef);
				FirstPersonKerbalFlight.Instance.OnIVACameraKerbalChange();
			}

			GameSettings.IVA_RETAIN_CONTROL_POINT = oldSetting;
		}

		public static VRInternalSeat CreateInternalSeat(InternalSeat seat, int seatIndex)
		{
			var collider = AddSeatCollider(seat.seatTransform, seat.kerbalOffset + Vector3.up * 0.3f, 20);
			var vrSeat = collider.gameObject.AddComponent<VRInternalSeat>();
			vrSeat.internalSeatIndex = seatIndex;
			return vrSeat;
		}

		public static Collider AddSeatCollider(Transform seatTransform, Vector3 kerbalOffset, int layer)
		{
			var collider = seatTransform.gameObject.AddComponent<SphereCollider>();
			collider.radius = 0.15f;
			collider.center = kerbalOffset;
			collider.isTrigger = true;

			seatTransform.gameObject.layer = layer;

			// Utils.GetOrAddComponent<ColliderVisualizer>(collider.gameObject);

			return collider;
		}
	}

	// interactable behavior that gets attached to external seats (i.e. command chair)
	public class VRExternalSeat : InteractableBehaviour
	{
		void Awake()
		{
			if (!HighLogic.LoadedSceneIsFlight) return;

			OnGrab += OnGrabbed;
			OnRelease += OnReleased;

			GameEvents.OnCameraChange.Add(OnCameraChange);
		}

		private void OnCameraChange(CameraManager.CameraMode cameraMode)
		{
			UpdateSeatCollision();
		}

		public void UpdateSeatCollision()
		{
			var part = this.gameObject.GetComponentUpwards<Part>();
			var seatModule = part.FindModuleImplementing<KerbalSeat>();
			var kerbal = InteractionSystem.Instance.gameObject.GetComponentUpwards<KerbalEVA>();

			bool currentlyInSeat = kerbal != null && kerbal.part == seatModule.Occupant;

			var collider = gameObject.GetComponent<Collider>();
			collider.enabled = !KerbalVR.Scene.IsInIVA() && !currentlyInSeat;
		}

		void OnDestroy()
		{
			GameEvents.OnCameraChange.Remove(OnCameraChange);
		}

		private void OnReleased(Hand hand, SteamVR_Input_Sources source)
		{
		}

		private void OnGrabbed(Hand hand, SteamVR_Input_Sources source)
		{
			var part = this.gameObject.GetComponentUpwards<Part>();
			var seatModule = part.FindModuleImplementing<KerbalSeat>();

			if (seatModule.Occupant)
			{
				// switch to them
				// what about different command chairs on the same vessel...?
				FlightGlobals.SetActiveVessel(seatModule.Occupant.vessel);
			}
			else
			{
				seatModule.BoardSeat();
				var collider = gameObject.GetComponent<Collider>();
				collider.enabled = false;
				KerbalVR.FirstPersonKerbalFlight.Instance.OnSeatBoarded(); // during the boarding process we switch vessels which confuses the flight system; restore it here

				KerbalVR.Scene.EnterFirstPerson();
			}
		}
	}

	[HarmonyPatch(typeof(KerbalEVA), nameof(KerbalEVA.EjectFromSeat))]
	class KerbalEVA_EjectFromSeat
	{
		public static void Postfix(KerbalEVA __instance)
		{
			// oddly, kerbalSeat never seems to get set back to null.
			var vrSeat = __instance.kerbalSeat.GetComponentInChildren<VRExternalSeat>();
			if (vrSeat != null)
			{
				var collider = vrSeat.gameObject.GetComponent<Collider>();
				if (collider != null)
				{
					collider.enabled = true;
				}
			}
		}
	}
}
