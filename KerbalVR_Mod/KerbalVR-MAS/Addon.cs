using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR_MAS
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class KerbalVR_MAS_Addon : MonoBehaviour
    {
		public void Awake()
		{
			KerbalVR.IVAAdaptors.IVASwitch.CreationFunctions.Add(MASSwitch.TryConstruct);
		}
    }
}
