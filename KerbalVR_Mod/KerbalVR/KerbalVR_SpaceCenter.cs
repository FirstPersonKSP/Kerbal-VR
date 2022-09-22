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

	// this class gets added to each building's colliders.
	// The stock KSP code listens for OnMouseOver events and marks the building as hovered.
	// Then the building's update code polls for the mouse being down while hovered.
	// To make this work with VR, call the internal functions directly when the VR UISystem's actions are activated via IVRMouseTarget
	class VRBuildingMouseHandler : MonoBehaviour, IVRMouseTarget
	{
		public SpaceCenterBuildingCollider m_spaceCenterBuildingCollider;

		public void OnMouseDown()
		{
			m_spaceCenterBuildingCollider.building.OnLeftClick();
		}

		public void OnRightMouseButtonDown()
		{
			m_spaceCenterBuildingCollider.building.OnRightClick();
		}

		public void OnRightMouseButtonDrag() {}

		public void OnRightMouseButtonUp() {}
	}

	[HarmonyPatch(typeof(SpaceCenterBuildingCollider), nameof(SpaceCenterBuildingCollider.Setup))]
	class SpaceCenterBuildingColliderPatch : HarmonyPatch
	{
		public static void Postfix(SpaceCenterBuildingCollider __instance)
		{
			var mouseHandler = __instance.gameObject.AddComponent<VRBuildingMouseHandler>();
			mouseHandler.m_spaceCenterBuildingCollider = __instance;
		}
	}
}
