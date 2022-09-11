using KSPDev.ConfigUtils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace KerbalVR
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	[PersistentFieldsDatabase("KerbalVR/settings/KerbalVRConfig")]
	public class HapticUtils : MonoBehaviour
	{
		public class PulseSetting
		{
			[PersistentField("duration")]
			public float duration = 0.005f;
			[PersistentField("frequency")]
			public float frequncy = 0.005f;
			[PersistentField("amplitude")]
			public float amplitude = 1.0f;
		}

		public class HapticProfile
		{
			[PersistentField("controller")]
			public string controller = "default";
			[PersistentField("Light")]
			public PulseSetting light = new PulseSetting();
			[PersistentField("Heavy")]
			public PulseSetting heavy = new PulseSetting();
			[PersistentField("Snap")]
			public PulseSetting snap = new PulseSetting();
		}

		[PersistentField("Haptic/enabled")]
		public bool hapticEnabledConfig = true;

		[PersistentField("Haptic/Profile", isCollection = true)]
		public List<HapticProfile> hapticProfiles = new List<HapticProfile>();

		private static bool hapticEnabled;
		private static HapticProfile currentProfile = null;
		private readonly static SteamVR_Action_Vibration hapticAction = SteamVR_Actions.default_Haptic;

		private void Awake()
		{
			ConfigAccessor.ReadFieldsInType(GetType(), this);

			hapticEnabled = hapticEnabledConfig;
			hapticEnabled &= Core.IsVrEnabled;

			if (hapticEnabled)
			{
				foreach (HapticProfile profile in hapticProfiles)
				{
					Debug.Log($"[KerbalVR/HapticUtils] Profile '{profile.controller}' loaded");

					if (HardwareUtils.devices.Contains(profile.controller.ToLower()))
					{
						currentProfile = profile;
					}
				}

				if (currentProfile == null) // no device-specific profile found
				{
					HapticProfile defaultProfile = hapticProfiles.FirstOrDefault(x => x.controller.ToLower() == "default");
					if (defaultProfile != null) // if default profile found
					{
						currentProfile = defaultProfile;
					}
					else
					{
						currentProfile = new HapticProfile();
					}
				}

				Debug.Log($"[KerbalVR/HapticUtils] Using profile '{currentProfile.controller}'");
			}
		}

		public static void Light(SteamVR_Input_Sources inputSource)
		{
			Pulse(currentProfile.light, inputSource);
		}

		public static void Heavy(SteamVR_Input_Sources inputSource)
		{
			Pulse(currentProfile.heavy, inputSource);
		}

		public static void Snap(SteamVR_Input_Sources inputSource)
		{
			Pulse(currentProfile.snap, inputSource);
		}

		public static void Pulse(PulseSetting setting, SteamVR_Input_Sources inputSource)
		{
			if (hapticEnabled)
			{
				hapticAction.Execute(0f, setting.duration, setting.frequncy, setting.amplitude, inputSource);
			}
		}

		public static void Pulse(float duration, float frequncy, float amplitude, SteamVR_Input_Sources inputSource)
		{
			if (hapticEnabled)
			{
				hapticAction.Execute(0f, duration, frequncy, amplitude, inputSource);
			}	
		}
	}
}
