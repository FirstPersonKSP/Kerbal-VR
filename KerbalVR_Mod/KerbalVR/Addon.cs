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
		public void Awake()
		{
			Utils.Log("Addon Awake");

			var harmony = new Harmony("KerbalVR");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			KerbalVR.Core.InitSystems(XRSettings.enabled);

			// for whatever reason, enabling VR mode during loading makes it super slow (vsync maybe?)
			KerbalVR.Core.SetVrRunning(false);
			
			Valve.VR.SteamVR_Settings.instance.trackingSpace = Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated;
			Valve.VR.SteamVR_Settings.instance.lockPhysicsUpdateRateToRenderFrequency = false;

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

			if (KerbalVR.Core.IsVrEnabled)
			{
				if (gameScene == GameScenes.PSYSTEM)
				{
					PSystemManager.Instance.OnPSystemReady.Add(OnPSystemReady);
				}

				KerbalVR.Core.SetVrRunning(gameScene == GameScenes.FLIGHT);
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

		[HarmonyPatch(typeof(CameraManager), "Update")]
		class CameraManagerPatch
		{
			public static bool Prefix()
			{
				if (GameSettings.CAMERA_NEXT.GetKeyDown() && GameSettings.MODIFIER_KEY.GetKey())
				{
					KerbalVR.Core.SetVrRunning(!KerbalVR.Core.IsVrRunning);
					return false;
				}
				return true;
			}
		}
	}
	
}