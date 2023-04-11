using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace KerbalVR
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class AssetLoader : FreeIva.AssetLoaderBase
	{
		readonly string[] assetBundlePaths = 
		{
			Path.Combine(Globals.KERBALVR_ASSETBUNDLES_DIR, "kerbalvr.dat"),
			Path.Combine(Globals.KERBALVR_ASSETBUNDLES_DIR, "kerbalvr_ui.dat"),
			Path.Combine(Globals.KERBALVR_ASSETBUNDLES_DIR, "kerbalvr_font.dat"),
		};

		public static AssetLoader Instance { get; private set; }

		void Start()
		{
			Instance = this;
		}

		public void ModuleManagerPostLoad()
		{
			LoadAssets(assetBundlePaths);

			// load TextMeshPro fonts
			tmpFontsDictionary.Add("Futura_Medium_BT", KerbalVR.Fonts.TMPFont_Futura_Medium_BT_SDF.GetInstance());
			tmpFontsDictionary.Add("SpaceMono_Regular", KerbalVR.Fonts.TMPFont_SpaceMono_Regular_SDF.GetInstance());
		}
	}
}
