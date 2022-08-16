using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

		private void OnReleased(Hand hand)
		{
			
		}

		private void OnGrabbed(Hand hand)
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
			var collider = gameObject.GetComponent<Collider>();
			collider.enabled = !KerbalVR.Scene.IsInIVA();
		}

		void OnDestroy()
		{
			GameEvents.OnCameraChange.Remove(OnCameraChange);
		}

		private void OnReleased(Hand hand)
		{
		}

		private void OnGrabbed(Hand hand)
		{
			var part = this.gameObject.GetComponentUpwards<Part>();
			var seatModule = part.FindModuleImplementing<KerbalSeat>();

			if (seatModule.Occupant)
			{
				// exit the seat
				if (part == FlightGlobals.ActiveVessel.GetReferenceTransformPart())
				{
					var actionParam = new KSPActionParam(KSPActionGroup.None, KSPActionType.Activate);
					seatModule.LeaveSeat(actionParam);
				}
				// switch to them
				else
				{
					FlightGlobals.SetActiveVessel(seatModule.Occupant.vessel);
				}
			}
			else
			{
				seatModule.BoardSeat();

				var fpCameraManager = FirstPerson.FirstPersonEVA.instance.fpCameraManager;
				fpCameraManager.isFirstPerson = false;
				fpCameraManager.saveCameraState(FlightCamera.fetch);
				fpCameraManager.CheckAndSetFirstPerson(FlightGlobals.ActiveVessel);
			}
		}
	}
}
