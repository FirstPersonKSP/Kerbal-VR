using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KerbalVR.InternalModules;

namespace KerbalVR.IVAAdaptors
{
	// represents an abstraction for an interactive toggle switch
	public class IVASwitch
	{
		public static List<Func<VRSwitch, Transform, IVASwitch>> CreationFunctions = new List<Func<VRSwitch, Transform, IVASwitch>>();

		public static IVASwitch ConstructSwitch(VRSwitch vrSwitch, Transform transform)
		{
			foreach (var creationFunction in CreationFunctions)
			{
				try
				{
					var ivaSwitch = creationFunction(vrSwitch, transform);
					if (ivaSwitch != null)
					{

						return ivaSwitch;
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			return null;
		}

		public virtual void SetAnimationsEnabled(bool enabled) { }

		public virtual bool CurrentState { get; }
		public virtual void SetState(bool newState) { }
	}
}
