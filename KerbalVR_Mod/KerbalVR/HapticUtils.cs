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
		public class HapticLevel
		{
			[PersistentField("name")]
			public string name;
			[PersistentField("duration")]
			public float duration;
			[PersistentField("frequency")]
			public float frequncy;
			[PersistentField("amplitude")]
			public float amplitude;
		}

		[PersistentField("Haptic/enabled")]
		public bool hapticEnabledConfig = true;

		[PersistentField("Haptic/Level", isCollection = true)]
		public readonly List<HapticLevel> hapticLevelsConfig = new List<HapticLevel>();

		private static bool hapticEnabled;
		private static Dictionary<string, HapticLevel> hapticLevels;
		private readonly static SteamVR_Action_Vibration hapticAction = SteamVR_Actions.default_Haptic;

		private void Awake()
		{
			ConfigAccessor.ReadFieldsInType(GetType(), this);

			hapticEnabled = hapticEnabledConfig;
			hapticLevels = hapticLevelsConfig.ToDictionary(x => x.name);
		}

		public static void Light(SteamVR_Input_Sources inputSource)
		{
			Pulse(hapticLevels["Light"], inputSource);
		}

		public static void Heavy(SteamVR_Input_Sources inputSource)
		{
			Pulse(hapticLevels["Heavy"], inputSource);
		}

		public static void Snap(SteamVR_Input_Sources inputSource)
		{
			Pulse(hapticLevels["Snap"], inputSource);
		}

		public static void Pulse(HapticLevel hapticSetting, SteamVR_Input_Sources inputSource)
		{
			if (hapticEnabled)
			{
				hapticAction.Execute(0f, hapticSetting.duration, hapticSetting.frequncy, hapticSetting.amplitude, inputSource);
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
