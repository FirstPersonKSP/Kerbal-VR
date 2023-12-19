using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace KerbalVR
{
	public static class HardwareUtils
	{
		public static List<string> devices = new List<string>();

		public static void Init()
		{
			for (uint deviceId = 0; deviceId < 1024; ++deviceId)
			{
				string device = SteamVR.instance.GetStringProperty(ETrackedDeviceProperty.Prop_ControllerType_String, deviceId);

				if (device == "<unknown>" || device == "TrackedProp_InvalidDevice")
				{
					break;
				}

				devices.Add(device.ToLower());

				Debug.Log($"[KerbalVR/HardwareUtils] Detected device {deviceId}: {device}");
			}
		}
	}
}
