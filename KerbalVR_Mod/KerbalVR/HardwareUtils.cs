using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;
using UnityEngine.XR;

namespace KerbalVR
{
	public static class HardwareUtils
	{
		public enum HMDType
		{
			Default,
			IndexHMD,
			Rift,
			Vive,
			Vive_Pro,
			Vive_Cosmos,
			Vive_Tracker_Camera,
			Holographic_HMD,
		}

		public enum ControllerType
		{
			Default,
			Knuckles,
			Oculus_Touch,
			Vive_Controller,
			Vive_Cosmos_Controller,
			Holographic_Controller,
		}

		public static List<string> devices = new List<string>();

		public static HMDType hmdType = HMDType.Default;
		public static ControllerType controllerType = ControllerType.Default;

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

				devices.Add(device);
				deviceId++;

				Debug.Log($"[KerbalVR/HardwareUtils] Detected device {deviceId}: {device}");
			}

			foreach (string device in devices)
			{
				if (Enum.TryParse(device, true, out HMDType hmd))
				{
					hmdType = hmd;
				}
				else if (Enum.TryParse(device, true, out ControllerType controller))
				{
					controllerType = controller;
				}
			}

			Debug.Log($"[KerbalVR/HardwareUtils] HMD: {hmdType}; Controller: {controllerType}");
		}
	}
}
