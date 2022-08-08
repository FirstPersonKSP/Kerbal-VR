using KerbalVR.InternalModules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KerbalVR.IVAAdaptors
{
	/// <summary>
	/// Represents the interface between the VR mod and the IVA mod (RPM or MAS).
	/// </summary>
	public abstract class IVAKnob
	{
		public static List<Func<GameObject, VRKnobCustomRotation, IVAKnob>> CreationFunctions = new List<Func<GameObject, VRKnobCustomRotation, IVAKnob>>();

		public static IVAKnob ConstructKnob(GameObject gameObject, VRKnobCustomRotation customRotation)
		{
			foreach (var creationFunction in CreationFunctions)
			{
				try
				{
					var knob = creationFunction(gameObject, customRotation);
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

		public abstract float MinRotation { get; protected set; }
		public abstract float MaxRotation { get; protected set; }
		public abstract void SetUpdateEnabled(bool enabled);
		public abstract void SetRotationFraction(string customRotationFunction, float fraction);
	}
}