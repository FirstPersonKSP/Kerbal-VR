using AvionicsSystems;
using KerbalVR;
using KerbalVR.InternalModules;
using KerbalVR.IVAAdaptors;
using System;

namespace KerbalVR_MAS
{
	public class MASLever : IVALever
	{
		public static MASLever TryConstruct(VRLever vrLever)
		{
			MASComponent component = vrLever.GetComponent<MASComponent>(); 

			if (component)
			{
				return new MASLever(component, vrLever);
			}

			return null;
		}

		VRLever lever;
		MASComponentAnimation componentAnimation;
		MASComponentAnimationPlayer componentAnimationPlayer;
		MASComponentRotation componentRotation;
		MASFlightComputer flightComputer;

		int customAxisNumber = -1;
		float customAxisTarget;
		bool setCustomAxis = false;

		// cache it because some MAS functions won't update value instantly
		int lastStep;

		public MASLever(MASComponent component, VRLever vrLever)
		{
			lever = vrLever;

			foreach (IMASSubComponent action in component.actions)
			{
				if (action is MASComponentAnimation componentAnimation)
				{
					this.componentAnimation = componentAnimation;
				}
				else if (action is MASComponentAnimationPlayer componentAnimationPlayer)
				{
					this.componentAnimationPlayer = componentAnimationPlayer;
				}
				else if (action is MASComponentRotation componentRotation)
				{
					this.componentRotation = componentRotation;
				}
			}

			flightComputer = vrLever.part.GetComponent<MASFlightComputer>();

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

			lastStep = RefreshState();
		}

		~MASLever()
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
					flightComputer.fcProxy.SetGear(stepId == 1);
					break;
				case "Spoiler":
					flightComputer.farProxy.SetSpoilers(stepId == 1);
					break;
				case "Flap":
					int currentSetting = (int)Math.Round(flightComputer.farProxy.GetFlapSetting());
					if (currentSetting >= 0)
					{
						int delta = stepId - currentSetting;
						if (delta < 0)
						{
							for (int i = 0; i > delta; --i)
							{
								flightComputer.farProxy.DecreaseFlapSetting();
							}
						}
						else if (delta > 0)
						{
							for (int i = 0; i < delta; ++i)
							{
								flightComputer.farProxy.IncreaseFlapSetting();
							}
						}
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

			lastStep = stepId;

			// Manually set correct state for these modules so they won't play
			if (componentAnimation != null)
			{
				componentAnimation.currentBlend = stepId;
				componentAnimation.animationState.time = stepId * componentAnimation.animationState.length;
			}
			if (componentAnimationPlayer != null)
			{
				componentAnimationPlayer.currentState = stepId > 0;
			}
			if (componentRotation != null)
			{
				componentRotation.currentBlend = stepId / (lever.stepCount - 1f);
			}
		}

		public override int GetStep()
		{
			return lastStep;
		}

		public int RefreshState()
		{
			switch (lever.handler)
			{
				case "Gear":
					return (int)Math.Round(flightComputer.fcProxy.GetGear());
				case "Spoiler":
					return (int)Math.Round(flightComputer.farProxy.GetSpoilerSetting());
				case "Flap":
					return (int)Math.Round(flightComputer.farProxy.GetFlapSetting());
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
			if (componentAnimation != null && componentAnimation.variableRegistrar.isEnabled != enabled)
			{
				componentAnimation.variableRegistrar.EnableCallbacks(enabled);
				componentAnimation.animationState.enabled = enabled;
			}
			if (componentAnimationPlayer != null && componentAnimationPlayer.variableRegistrar.isEnabled != enabled)
			{
				componentAnimationPlayer.variableRegistrar.EnableCallbacks(enabled);
			}
			if (componentRotation != null && componentRotation.variableRegistrar.isEnabled != enabled)
			{
				componentRotation.variableRegistrar.EnableCallbacks(enabled);
			}

			lastStep = RefreshState();
		}
	}
}
