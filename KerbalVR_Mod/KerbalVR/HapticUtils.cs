using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace KerbalVR
{
	public static class HapticUtils
	{
		private readonly static SteamVR_Action_Vibration hapticAction = SteamVR_Actions.default_Haptic;

		public static void Light(SteamVR_Input_Sources inputSource)
		{
			Pulse(0.005f, 0.005f, 1f, inputSource);
		}

		public static void Heavy(SteamVR_Input_Sources inputSource)
		{
			Pulse(0.05f, 100f, 1f, inputSource);
		}

		public static void Snap(SteamVR_Input_Sources inputSource)
		{
			Pulse(0.01f, 200f, 1f, inputSource);
		}

		public static void Pulse(float duration, float frequncy, float amplitude, SteamVR_Input_Sources inputSource)
		{
			hapticAction.Execute(0f, duration, frequncy, amplitude, inputSource);
		}
	}
}
