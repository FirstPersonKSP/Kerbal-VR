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
		void OnEnter(Collider collider, SteamVR_Input_Sources inputSource);
		void OnStay(Collider collider, SteamVR_Input_Sources inputSource);
		void OnExit(Collider collider, SteamVR_Input_Sources inputSource);
	}
}
