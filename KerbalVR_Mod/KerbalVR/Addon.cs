using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;

namespace KerbalVR
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class FirstPersonKerbalAddon : MonoBehaviour
	{
		bool m_wasEnabled;

		public void Awake()
		{
			Utils.Log("Addon Awake");

			var harmony = new Harmony("KerbalVR");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			// for whatever reason, enabling VR mode during loading makes it super slow
			m_wasEnabled = XRSettings.enabled;
			XRSettings.enabled = false;

			Valve.VR.SteamVR_Settings.instance.trackingSpace = Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated;

			GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);
			GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);

			DontDestroyOnLoad(this);
		}

		private void OnGameSceneLoadRequested(GameScenes data)
		{
			if (InteractionSystem.Instance != null)
			{
				InteractionSystem.Instance.transform.SetParent(null, false);
				GameObject.DontDestroyOnLoad(InteractionSystem.Instance);
			}

		}

		public void OnDestroy()
		{
			GameEvents.onLevelWasLoaded.Remove(OnLevelWasLoaded);
			PSystemManager.Instance.OnPSystemReady.Remove(OnPSystemReady);
		}

		public void OnLevelWasLoaded(GameScenes gameScene)
		{
			Utils.Log($"OnLevelWasLoaded: {gameScene}");

			if (m_wasEnabled)
			{
				if (gameScene == GameScenes.PSYSTEM)
				{
					PSystemManager.Instance.OnPSystemReady.Add(OnPSystemReady);
				}

				if (gameScene == GameScenes.MAINMENU)
				{
					KerbalVR.Core.InitSteamVRInput();

					XRSettings.enabled = true;
					Valve.VR.SteamVR.enabled = true;

					Valve.VR.SteamVR_Settings.instance.lockPhysicsUpdateRateToRenderFrequency = false;

					KerbalVR.Core.InitSystems();
				}
			}
		}
		private void OnPSystemReady()
		{
			Utils.Log("OnPSystemReady");

			GameObject.Find("UIMainCamera").GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;
			GameObject.Find("UIVectorCamera").GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;

			CameraUtils.AddVRCamera(ScaledCamera.Instance.galaxyCamera, true, true);
			CameraUtils.AddVRCamera(ScaledCamera.Instance.cam, false, true);
		}
	}
	
}