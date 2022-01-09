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
		void OnEnter(Vector3 fingertipCenter, Collider buttonCollider, SteamVR_Input_Sources inputSource);
		void OnStay(Vector3 fingertipCenter, Collider buttonCollider, SteamVR_Input_Sources inputSource);
		void OnExit(Vector3 fingertipCenter, Collider buttonCollider, SteamVR_Input_Sources inputSource);
	}
}
