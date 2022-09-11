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
			uint deviceId = 0;
			while (true)
			{
				string device = SteamVR.instance.GetStringProperty(ETrackedDeviceProperty.Prop_ControllerType_String, deviceId);

				if (device == "<unknown>")
				{
					break;
				}

				devices.Add(device.ToLower());
				deviceId++;

				Debug.Log($"[KerbalVR/HardwareUtils] Detected device {deviceId}: {device}");
			}
		}
	}
}
