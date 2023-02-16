using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace InstallCheck
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class InstallCheck : MonoBehaviour
	{
		// these mods must exist and pass the version requirements
		static readonly Dependency[] dependencies = new Dependency[]
		{
			new Dependency { assemblyName = "0Harmony", minVersion = new Version(2, 0)},
			new Dependency { assemblyName = "ModuleManager", minVersion = new Version(4, 2, 2) },
			new Dependency { assemblyName = "ThroughTheEyes", minVersion = new Version(2, 0, 4, 1) },
			new Dependency { assemblyName = "FreeIva", minVersion = new Version(0, 2, 9)},
		};

		// not required, but if they exist then verify the version number
		static readonly Dependency[] optionalMods = new Dependency[]
		{
			new Dependency { assemblyName = "EVEManager", minVersion = new Version(1, 11, 7, 1)},
			new Dependency { assemblyName = "TUFX", minVersion = new Version(1, 0, 5)},
			new Dependency { assemblyName = "Scatterer", minVersion = new Version(0, 838, 1)},
			new Dependency { assemblyName = "AvionicsSystems", minVersion = new Version(1, 3, 6)},
			new Dependency { assemblyName = "RasterPropMonitor", minVersion = new Version(0, 31, 10, 2)},
		};

		// these files must exist
		static readonly string[] requiredFiles = new string[]
		{
			"KSP_x64_Data/Managed/System.Runtime.Serialization.dll",
			"KSP_x64_Data/Managed/System.ServiceModel.Internals.dll",
			"KSP_x64_Data/Plugins/openvr_api.dll",
			"KSP_X64_Data/Plugins/XRSDKOpenVR.dll"
		};

		public void Awake()
		{
			Debug.Log("[KerbalVR] InstallCheck Awake");

			CheckVREnabled();
			CheckDependencies();
			CheckOptionalMods();
			CheckRequiredFiles();
		}

		private static void CheckVREnabled()
		{
			if (XRSettings.supportedDevices.IndexOf("OpenVR") == -1)
			{
				Alert("Unity VR is not enabled.  Please run VRInstaller.exe and point it at your KSP directory:\r\n" + Path.GetFullPath(KSPUtil.ApplicationRootPath));
			}
		}

		struct Dependency
		{
			public string assemblyName;
			public Version requiredVersion;
			public Version minVersion;
			public Version maxVersion;

			public void CheckVersion(Version installedVersion, ref string errorMessage)
			{
				if (requiredVersion != null && requiredVersion != installedVersion)
				{
					errorMessage += $"{assemblyName} must be version {requiredVersion} but you have {installedVersion} installed{Environment.NewLine}";
				}
				else if (minVersion != null && minVersion > installedVersion)
				{
					errorMessage += $"{assemblyName} must be at least version {minVersion} but you have {installedVersion} installed{Environment.NewLine}";
				}
				else if (maxVersion != null && maxVersion < installedVersion)
				{
					errorMessage += $"{assemblyName} must be not greater than {maxVersion} but you have {installedVersion} installed{Environment.NewLine}";
				}
			}
		}

		private static void CheckDependencies()
		{
			string errorMessage = string.Empty;
			foreach (var dependency in dependencies)
			{
				var assembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.name == dependency.assemblyName);
				if (assembly == null)
				{
					errorMessage += "Missing dependency: " + dependency.assemblyName + Environment.NewLine;
				}
				else
				{
					dependency.CheckVersion(assembly.assembly.GetName().Version, ref errorMessage);
				}
			}

			if (errorMessage != string.Empty)
			{
				Alert(errorMessage);
			}
		}

		private static void CheckOptionalMods()
		{
			string errorMessage = string.Empty;
			foreach (var dependency in optionalMods)
			{
				var assembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.name == dependency.assemblyName);
				if (assembly != null)
				{
					dependency.CheckVersion(assembly.assembly.GetName().Version, ref errorMessage);
				}
			}

			if (errorMessage != string.Empty)
			{
				Alert(errorMessage);
			}
		}

		private static void CheckRequiredFiles()
		{
			string errorMessage = string.Empty;
			foreach (var filePath in requiredFiles)
			{
				if (!File.Exists(Path.Combine(KSPUtil.ApplicationRootPath, filePath)))
				{
					errorMessage += "Missing file: " + filePath + Environment.NewLine;
				}
			}

			if (errorMessage != string.Empty)
			{
				Alert(errorMessage);
			}
		}

		private static void Alert(string message)
		{
			XRSettings.enabled = false;

			Debug.LogError($"[KerbalVR] - {message}");

			var dialog = new MultiOptionDialog(
				"KerbalVRFatalError",
				$"KerbalVR has detected the following fatal problems.  Please refer to the installation guide.\n\n{message}",
				"KerbalVR Fatal Error",
				HighLogic.UISkin,
				new DialogGUIButton("Quit", Application.Quit));

			PopupDialog.SpawnPopupDialog(dialog, true, HighLogic.UISkin);
		}
	}
}
