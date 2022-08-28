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

		[HarmonyPatch(typeof(PSystemManager), nameof(PSystemManager.SetupScaledSpace))]
		class PSystemManagerPatch
		{
			public static void Postfix()
			{
				Camera scaledCamera = ScaledCamera.Instance.cam;
				Camera galaxyCamera = ScaledCamera.Instance.galaxyCamera;

				// fudge the scaled camera
				var scaledCameraAnchor = CameraUtils.CreateVRAnchor(scaledCamera);
				var dummyListener = scaledCameraAnchor.AddComponent<AudioListener>(); // PlanetariumCamera.Awake requires an audiolistener
				var dummyCamera = scaledCameraAnchor.AddComponent<Camera>(); // PlanetariumCamera.Deactivate requires a camera object
				PlanetariumCamera._fetch = CameraUtils.MoveComponent<PlanetariumCamera>(scaledCamera.gameObject, scaledCameraAnchor);
				PSystemManager.Instance.scaledSpaceCamera = PlanetariumCamera.fetch;
				Component.DestroyImmediate(dummyListener);
				dummyCamera.enabled = false;

				PlanetariumCamera.camRef = scaledCamera;
				var scaledCameraDriver = CameraUtils.MoveComponent<ScaledCamera>(scaledCamera.gameObject, scaledCameraAnchor);

				// disable position tracking on the galaxy camera
				var galaxyCameraAnchor = CameraUtils.CreateVRAnchor(galaxyCamera);
				galaxyCameraAnchor.transform.localScale = Vector3.zero;
				var followRot = CameraUtils.MoveComponent<FollowRot>(galaxyCamera.gameObject, galaxyCameraAnchor);
				followRot.tgt = scaledCameraAnchor.transform;
			}
		}
	}
	
}