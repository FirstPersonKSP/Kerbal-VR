using HarmonyLib;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	internal class KerbalVR_SpaceCenter : MonoBehaviour
	{
		SpaceCenterCamera2 m_spaceCenterCamera;

		public void Start()
		{
			m_spaceCenterCamera = GameObject.FindObjectOfType<SpaceCenterCamera2>();
			InteractionSystem.Instance.transform.SetParent(FlightCamera.fetch.transform, false);
		}

		public void OnDestroy()
		{
			InteractionSystem.Instance.transform.SetParent(null, false);
		}
	}
}
