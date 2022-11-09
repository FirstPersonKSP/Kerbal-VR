using JSI;
using KerbalVR;
using KerbalVR.InternalModules;
using KerbalVR.IVAAdaptors;
using UnityEngine;

namespace KerbalVR_RPM
{
	public class RPMLever : IVALever
	{
		public static RPMLever TryConstruct(VRLever vrLever)
		{
			// jSIVariableAnimator might be null but that is OK
			JSIVariableAnimator jSIVariableAnimator = vrLever.GetComponent<JSIVariableAnimator>(); 
			JSIActionGroupSwitch jSIActionGroupSwitch = vrLever.GetComponent<JSIActionGroupSwitch>();

			if (jSIActionGroupSwitch)
			{
				return new RPMLever(jSIVariableAnimator, jSIActionGroupSwitch, vrLever);
			}

			return null;
		}

		VRLever lever;
		JSIVariableAnimator jSIVariableAnimator; // null-able
		JSIActionGroupSwitch jSIActionGroupSwitch;
		RasterPropMonitorComputer rpmComputer;
		Animation cachedActionGroupSwitchAnimation;

		int customAxisNumber = -1;
		float customAxisTarget;
		bool setCustomAxis = false;

		public RPMLever(JSIVariableAnimator jSIVariableAnimator, JSIActionGroupSwitch jSIActionGroupSwitch, VRLever vrLever)
		{
			this.jSIVariableAnimator = jSIVariableAnimator;
			this.jSIActionGroupSwitch = jSIActionGroupSwitch;
			rpmComputer = jSIActionGroupSwitch.rpmComp;
			lever = vrLever;

			if (lever.handler.StartsWith("CustomAxis"))
			{
				if (int.TryParse(lever.handler.Remove(0, 10), out int result))
				{
					customAxisNumber = result - 1;
					FlightInputHandler.OnRawAxisInput += OnRawAxisInput;
				}
				else
				{
					Utils.LogError($"Invalid custom axis name '{lever.handler}' on {lever.internalProp.propName}");
				}
			}
		}

		~RPMLever()
		{
			FlightInputHandler.OnRawAxisInput -= OnRawAxisInput;
		}

		private void OnRawAxisInput(FlightCtrlState st)
		{
			if (lever.vessel.isActiveVessel && setCustomAxis)
			{
				lever.SetCustomAxis(customAxisNumber, customAxisTarget);
				st.custom_axes[customAxisNumber] = customAxisTarget;
			}
		}

		public override void SetStep(int stepId)
		{
			switch (lever.handler)
			{
				case "Gear":
				case "Spoiler":
					if ((jSIActionGroupSwitch.currentState ? 1 : 0) != stepId)
					{
						jSIActionGroupSwitch.Click();
						jSIActionGroupSwitch.currentState = stepId > 0;
					}
					break;
				case "Flap":
					if (rpmComputer.installedModules.Find(x => x.GetType() == typeof(JSIFAR)) is JSIFAR jSIFAR)
					{
						jSIFAR.SetFlaps(stepId);
					}
					break;
				default:
					if (customAxisNumber >= 0)
					{
						customAxisTarget = stepId / (lever.stepCount - 1f);
						setCustomAxis = true;
					}
					else
					{
						Utils.LogError($"Unknown lever handler {lever.handler} on {lever.internalProp.propName}");
					}
					break;
			}
		}

		public override int GetStep()
		{
			switch (lever.handler)
			{
				case "Gear":
				case "Spoiler":
					return jSIActionGroupSwitch.currentState ? 1 : 0;
				case "Flap":
					if (rpmComputer.installedModules.Find(x => x.GetType() == typeof(JSIFAR)) is JSIFAR jSIFAR)
					{
						return (int)jSIFAR.GetFlapSetting();
					}
					break;
				default:
					if (customAxisNumber >= 0)
					{
						return lever.GetCustomAxisState(customAxisNumber);
					}

					Utils.LogError($"Unknown lever handler {lever.handler} on {lever.internalProp.propName}");
					break;
			}

			return 0;
		}

		public override void SetUpdateEnabled(bool enabled)
		{
			if (jSIVariableAnimator)
			{
				// hack: setting useNewMode to true on the JSIVariableAnimator will prevent it from updating on its own
				jSIVariableAnimator.useNewMode = !enabled;
			}
			else
			{
				// remove anim from jSIActionGroupSwitch will prevent it from playing
				if (enabled)
				{
					jSIActionGroupSwitch.anim = cachedActionGroupSwitchAnimation;
				}
				else
				{
					if (jSIActionGroupSwitch.anim)
					{
						cachedActionGroupSwitchAnimation = jSIActionGroupSwitch.anim;
						jSIActionGroupSwitch.anim = null;
					}
				}
			}
		}
	}
}
