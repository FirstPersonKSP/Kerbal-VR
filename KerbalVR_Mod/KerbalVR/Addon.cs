using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace FirstPersonKerbal
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class FirstPersonKerbalAddon : MonoBehaviour
	{
		public void Awake()
		{
			Util.LogMessage("Addon Awake");

			// for whatever reason, enabling VR mode during loading makes it super slow
			XRSettings.enabled = false;

			Valve.VR.SteamVR_Settings.instance.trackingSpace = Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated;

			GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);

			DontDestroyOnLoad(this);
		}
		
		public void OnDestroy()
		{
			GameEvents.onLevelWasLoaded.Remove(OnLevelWasLoaded);
			PSystemManager.Instance.OnPSystemReady.Remove(OnPSystemReady);
		}

		public void OnLevelWasLoaded(GameScenes gameScene)
		{
			Util.LogMessage($"OnLevelWasLoaded: {gameScene}");

			if (gameScene == GameScenes.PSYSTEM)
			{
				PSystemManager.Instance.OnPSystemReady.Add(OnPSystemReady);
			}

			if (gameScene == GameScenes.MAINMENU)
			{
				KerbalVR.Core.InitSteamVRInput();

				XRSettings.enabled = true;
				Valve.VR.SteamVR.enabled = true;

				KerbalVR.Core.InitSystems();
			}
		}
		private void OnPSystemReady()
		{
			Util.LogMessage("OnPSystemReady");

			GameObject.Find("UIMainCamera").GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;
			GameObject.Find("UIVectorCamera").GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;

			CameraUtils.AddVRCamera(ScaledCamera.Instance.galaxyCamera, true, true);
			CameraUtils.AddVRCamera(ScaledCamera.Instance.cam, false, true);
		}
	}
	
}