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
			new Dependency { assemblyName = "FreeIva", minVersion = new Version(0, 2, 18)},
		};

		// not required, but if they exist then verify the version number
		static readonly Dependency[] optionalMods = new Dependency[]
		{
			new Dependency { assemblyName = "TUFX", minVersion = new Version(1, 0, 5)},
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
			CheckScatterer();
			CheckEVE();
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

		static bool CompareVersions(Version required, Version actual)
		{
			if (required.Major != actual.Major) return false;
			if (required.Minor != actual.Minor) return false;
			if (required.Build != -1 && required.Build != actual.Build) return false;
			if (required.Revision != -1 && required.Revision != actual.Revision) return false;

			return true;
		}

		// If a given mod exists, checks it against a list of known good versions.
		// If it's not there, looks up a specific error message for the given version, or else a generic "unsupported" one
		private static void CheckComplexMappings(string assemblyName, Version[] goodVersions, Dictionary<Version, string> errorMessages)
		{
			var assembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.name == assemblyName);
			if (assembly == null) return;

			var assemblyVersion = assembly.assembly.GetName().Version;

			if (goodVersions.Any(v => CompareVersions(v, assemblyVersion)))
			{
				return;
			}
			
			if (errorMessages.TryGetValue(assemblyVersion, out var message))
			{
				Alert(message);
			}
			else
			{
				Alert($"Unsupported {assemblyName} version {assemblyVersion}");
			}
		}

		private static void CheckScatterer()
		{
			CheckComplexMappings("Scatterer",
				new Version[]
				{
					new Version(0, 851, 0, 0), // volclouds v1
					new Version(0, 856, 0, 0), // volclouds v2
					new Version(0, 859, 0, 0), // volclouds v3
					new Version(0, 878, 2, 0), // VR patch on latest publicly available
					new Version(0, 880, 2, 0), // VR patch on volumetrics V4
				},
				new Dictionary<Version, string>()
				{
					{new Version(0, 878, 0, 0), "Install Scatterer from the Optional Mods folder"},
					{new Version(0, 878, 1, 0), "Install Scatterer from the Optional Mods folder"},
					{new Version(0, 880, 0, 0), "Install the files from Optional Mods/VolumetricClouds-v4"},
					{new Version(0, 880, 1, 0), "Install the files from Optional Mods/VolumetricClouds-v4"},
				});
		}

		private static void CheckEVE()
		{
			CheckComplexMappings("Atmosphere",
				new Version[]
				{
					new Version(1, 11, 7, 1), // public version
					new Version(2, 0, 1, 0), // volclouds v1
					new Version(2, 1), // volclouds v2
					new Version(2, 2, 1, 0), // volclouds v3
					new Version(2, 3, 3, 1), // VR patch on volclouds v4
				},
				new Dictionary<Version, string>()
				{
					{ new Version(2, 3, 3, 0), "Install the files from Optional Mods/VolumetricClouds-v4" },
				});
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
