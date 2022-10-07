using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR_RPM
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class KerbalVR_RPM_Addon : MonoBehaviour
	{
		public void Awake()
		{
			KerbalVR.IVAAdaptors.IVASwitch.CreationFunctions.Add(RPMSwitch.TryConstruct);
			KerbalVR.IVAAdaptors.IVAKnob.CreationFunctions.Add(RPMKnob.TryConstruct);
			KerbalVR.IVAAdaptors.IVALever.CreationFunctions.Add(RPMLever.TryConstruct);
		}
	}
}
