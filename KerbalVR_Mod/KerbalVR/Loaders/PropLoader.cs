using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KerbalVR.Loaders
{
	[HarmonyPatch(typeof(InternalProp), nameof(InternalProp.Load))]
	internal class PropLoader
	{
		public static void Postfix(InternalProp __instance, ConfigNode node)
		{
			string airlockName = null;
			if (node.TryGetValue("airlockName", ref airlockName))
			{
				foreach (InternalModules.VRInternalHatch internalHatch in __instance.internalModules)
				{
					internalHatch.airlockName = airlockName;
				}
			}
		}
	}
}
