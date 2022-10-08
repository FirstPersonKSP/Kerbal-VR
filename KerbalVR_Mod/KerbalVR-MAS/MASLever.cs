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

			lastStep = RefreshState();
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
					Utils.LogError($"Unknown lever handler {lever.handler} on {lever.internalProp.propName}");
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
				componentRotation.currentBlend = (float)stepId / (lever.stepCount - 1);
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
