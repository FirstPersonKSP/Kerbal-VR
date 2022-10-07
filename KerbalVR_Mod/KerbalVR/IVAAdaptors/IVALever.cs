using KerbalVR.InternalModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.IVAAdaptors
{
	public abstract class IVALever
	{
		public static List<Func<VRLever, IVALever>> CreationFunctions = new List<Func<VRLever, IVALever>>();

		public static IVALever ConstructLever(VRLever vrKnob)
		{
			foreach (var creationFunction in CreationFunctions)
			{
				try
				{
					var knob = creationFunction(vrKnob);
					if (knob != null)
					{
						return knob;
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			return null;
		}

		public abstract void SetUpdateEnabled(bool enabled);

		/// <summary>
		/// Change lever step to <paramref name="stepId"/>
		/// </summary>
		/// <param name="stepId">0-based step id</param>
		public abstract void SetStep(int stepId);

		public abstract int GetStep();
	}
}
