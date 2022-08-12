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

		InternalSeat internalSeat
		{
			get
			{
				return gameObject.GetComponentUpwards<InternalModel>().seats[internalSeatIndex];
			}
		}

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
				
			}
		}
	}
}
