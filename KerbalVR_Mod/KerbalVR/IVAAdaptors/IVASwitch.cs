using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.IVAAdaptors
{
	// represents an abstraction for an interactive toggle switch
	public class IVASwitch
	{
		public static List<Func<GameObject, Transform, IVASwitch>> CreationFunctions = new List<Func<GameObject, Transform, IVASwitch>>();

		public static IVASwitch ConstructSwitch(GameObject gameObject, Transform transform)
		{
			foreach (var creationFunction in CreationFunctions)
			{
				try
				{
					var ivaSwitch = creationFunction(gameObject, transform);
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


		public virtual bool CurrentState { get; }
		public virtual void SetState(bool newState) { }
	}
}
