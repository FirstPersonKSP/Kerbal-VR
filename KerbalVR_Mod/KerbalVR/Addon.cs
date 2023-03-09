using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace KerbalVR
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class FirstPersonKerbalAddon : MonoBehaviour
	{
		static KeyBinding m_vrToggle = new KeyBinding(KeyCode.V);

		public void Awake()
		{
			Utils.Log("Addon Awake");
			DontDestroyOnLoad(this);

			if (XRSettings.enabled)
			{
				Core.InitSteamVRInput();
				SteamVR.Initialize();
				HardwareUtils.Init();

				// for whatever reason, enabling VR mode during loading makes it super slow (vsync maybe?)
				XRSettings.enabled = false;

				ApplyPatches();
			}

			GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);
			GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
		}

		public static void ModuleManagerPostLoad()
		{
			Utils.Log("ModuleManagerPostLoad");

		}
		private static void Scatterer_CreateRenderTextures_Prefix(ref int width, ref int height)
		{
			width = UnityEngine.XR.XRSettings.eyeTextureWidth;
			height = UnityEngine.XR.XRSettings.eyeTextureHeight;
		}

		private static IEnumerable<CodeInstruction> ScreenCopyCommandBuffer_Initialize_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var screenWidthProperty = typeof(Screen).GetProperty(nameof(Screen.width), BindingFlags.Static | BindingFlags.Public).GetGetMethod();
			var screenHeightProperty = typeof(Screen).GetProperty(nameof(Screen.height), BindingFlags.Static | BindingFlags.Public).GetGetMethod();

			var eyeWidthProperty = typeof(XRSettings).GetProperty(nameof(XRSettings.eyeTextureWidth), BindingFlags.Static | BindingFlags.Public).GetGetMethod();
			var eyeHeightProperty = typeof(XRSettings).GetProperty(nameof(XRSettings.eyeTextureHeight), BindingFlags.Static | BindingFlags.Public).GetGetMethod();

			foreach (var instruction in instructions)
			{
				if (instruction.Calls(screenWidthProperty))
				{
					instruction.operand = eyeWidthProperty;
				}
				else if (instruction.Calls(screenHeightProperty))
				{
					instruction.operand = eyeHeightProperty;
				}

				yield return instruction;
			}
		}

		private static void ApplyPatches()
		{
			var harmony = new Harmony("KerbalVR");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			CameraFOVPatch.PatchAll(harmony);

			// I had thought the below code might help with the apparent aliasing issues when using scatterer in vr, but it didn't.
#if false
			var scattererAssembly = AssemblyLoader.loadedAssemblies.assemblies.FirstOrDefault(a => a.name == "scatterer");
			if (scattererAssembly != null)
			{
				var commandBufferType = scattererAssembly.assembly.GetType("Scatterer.ScatteringCommandBuffer");

				if (commandBufferType != null)
				{
					var CreateRenderTextures_MethodInfo = commandBufferType.GetMethod("CreateRenderTextures", BindingFlags.Instance | BindingFlags.NonPublic);
					if (CreateRenderTextures_MethodInfo != null)
					{
						var prefix = new HarmonyMethod(GetType(), nameof(Scatterer_CreateRenderTextures_Prefix));
						var result = harmony.Patch(CreateRenderTextures_MethodInfo, prefix);
						Debug.Log($"patched scatterer: {result}");
					}
				}

				var ScreenCopyCommandBufferType = scattererAssembly.assembly.GetType("Scatterer.ScreenCopyCommandBuffer");
				if (ScreenCopyCommandBufferType != null)
				{
					var Initialize_MethodInfo = ScreenCopyCommandBufferType.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
					if (Initialize_MethodInfo != null)
					{
						var transpiler = new HarmonyMethod(GetType(), nameof(ScreenCopyCommandBuffer_Initialize_Transpiler));
						harmony.Patch(Initialize_MethodInfo, null, null, transpiler);
					}
				}
			}
#endif

		}

		public void LateUpdate()
		{
			if (m_vrToggle.GetKeyDown() && GameSettings.MODIFIER_KEY.GetKey())
			{
				KerbalVR.Core.SetVrRunningDesired(!KerbalVR.Core.IsVrRunning);
			}
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

			if (gameScene == GameScenes.PSYSTEM)
			{
				KerbalVR.Core.InitSystems();
			}

			if (KerbalVR.Core.IsVrEnabled)
			{
				if (gameScene == GameScenes.PSYSTEM)
				{
					Valve.VR.SteamVR_Settings.instance.trackingSpace = Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated;
					Valve.VR.SteamVR_Settings.instance.lockPhysicsUpdateRateToRenderFrequency = false;

					PSystemManager.Instance.OnPSystemReady.Add(OnPSystemReady);
				}

				KerbalVR.Core.SetVrRunningDesired(Core.IsVrRunning && Scene.SceneSupportsVR(gameScene));
			}
		}
		private void OnPSystemReady()
		{
			Utils.Log("OnPSystemReady");

			GameObject.Find("UIMainCamera").GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;
			GameObject.Find("UIVectorCamera").GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;

			Core.InitHeadsetState();
		}

		[HarmonyPatch(typeof(CameraManager), "Update")]
		class CameraManagerPatch
		{
			public static bool Prefix()
			{
				if (m_vrToggle.GetKeyDown() && GameSettings.MODIFIER_KEY.GetKey())
				{
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
				if (KerbalVR.Core.IsVrEnabled)
				{
					Camera scaledCamera = ScaledCamera.Instance.cam;
					Camera galaxyCamera = ScaledCamera.Instance.galaxyCamera;

					// fudge the scaled camera
					var scaledCameraAnchor = CameraUtils.CreateVRAnchor(scaledCamera);
					scaledCameraAnchor.transform.localScale = Vector3.one * ScaledSpace.InverseScaleFactor;
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
	
}