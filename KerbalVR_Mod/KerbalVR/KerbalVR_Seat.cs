using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR
{
	public class VRSeat : InteractableBehaviour
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
	}
}
