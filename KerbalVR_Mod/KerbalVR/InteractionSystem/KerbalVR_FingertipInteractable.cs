using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR
{
	interface IFingertipInteractable
	{
		void OnEnter(Hand hand, Collider buttonCollider, SteamVR_Input_Sources inputSource);
		void OnStay(Hand hand, Collider buttonCollider, SteamVR_Input_Sources inputSource);
		void OnExit(Hand hand, Collider buttonCollider, SteamVR_Input_Sources inputSource);
	}
}
