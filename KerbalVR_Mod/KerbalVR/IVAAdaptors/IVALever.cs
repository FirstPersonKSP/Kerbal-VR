using KerbalVR.InternalModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.IVAAdaptors
{
	public abstract class IVALever
	{
		public static List<Func<VRLever, IVALever>> CreationFunctions = new List<Func<VRLever, IVALever>>();

		public static IVALever ConstructLever(VRLever vrLever)
		{
			foreach (Func<VRLever, IVALever> creationFunction in CreationFunctions)
			{
				try
				{
					IVALever lever = creationFunction(vrLever);
					if (lever != null)
					{
						return lever;
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

		protected VRLever lever;

		// custom axis stuff
		protected AxisGroupsModule axisGroupsModule;
		protected int customAxisNumber = -1;
		protected float customAxisTarget;
		protected bool setCustomAxis = false;
		protected const string customAxisPrefix = "CustomAxis";

		protected virtual bool UsingCustomAxis => customAxisNumber >= 0;

		/// <summary>
		/// Setup custom axis if available, call in constructor after setting lever
		/// </summary>
		protected virtual void SetupCustomAxis() 
		{
			if (lever.handler.StartsWith(customAxisPrefix))
			{
				if (int.TryParse(lever.handler.Substring(customAxisPrefix.Length), out int result))
				{
					customAxisNumber = result - 1;
					FlightInputHandler.OnRawAxisInput += OnRawAxisInput;

					// https://forum.kerbalspaceprogram.com/index.php?/topic/200955-cant-change-custom_axes-from-code-solved/
					axisGroupsModule = lever.vessel.FindVesselModuleImplementing<AxisGroupsModule>();

					if (!axisGroupsModule)
					{
						Utils.LogError($"Cannot find AxisGroupsModule on vessel {lever.vessel.GetDisplayName()}");
					}
				}
				else
				{
					Utils.LogError($"Invalid custom axis name '{lever.handler}' on {lever.internalProp.propName}");
				}
			}
		}

		/// <summary>
		/// Set target for custom axis so it can be applied when OnRawAxisInput is called
		/// </summary>
		/// <param name="stepId"></param>
		protected virtual void SetCustomAxisTarget(int stepId)
		{
			customAxisTarget = stepId / (lever.stepCount - 1f);
			setCustomAxis = true;
		}

		/// <summary>
		/// Get which step the lever is on when using custom axis
		/// </summary>
		/// <returns>value from 0 to </returns>
		protected virtual int GetCustomAxisState()
		{
			float axisValue = FlightInputHandler.state.custom_axes[customAxisNumber];
			return Math.Max(0, Mathf.FloorToInt((lever.stepCount - 1) * axisValue + 0.5f));
		}

		private void OnRawAxisInput(FlightCtrlState st)
		{
			if (lever.vessel.isActiveVessel && setCustomAxis)
			{
				axisGroupsModule.SetAxisGroup((KSPAxisGroup)(1 << (9 + customAxisNumber)), customAxisTarget);
				st.custom_axes[customAxisNumber] = customAxisTarget;
			}
		}

		public virtual void OnDestory()
		{
			if (UsingCustomAxis)
			{
				FlightInputHandler.OnRawAxisInput -= OnRawAxisInput;
			}
		}
	}
}
