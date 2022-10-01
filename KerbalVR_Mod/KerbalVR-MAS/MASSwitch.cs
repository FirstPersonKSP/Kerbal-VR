using AvionicsSystems;
using KerbalVR.IVAAdaptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR_MAS
{
	internal class MASSwitch : IVASwitch

	{
		static public MASSwitch TryConstruct(GameObject prop, Transform switchTransform)
		{
			var masComponent = prop.GetComponent<MASComponent>();

			if (masComponent != null)
			{
				foreach (var action in masComponent.actions)
				{
					if (action is MASComponentColliderEvent colliderEvent && colliderEvent.buttonObject.transform == switchTransform)
					{
						return new MASSwitch(masComponent, colliderEvent);
					}
				}
			}

			return null;
		}

		MASComponentColliderEvent m_colliderEvent;
		MASComponentRotation m_rotation;
		MASComponentAnimationPlayer m_animationPlayer;
		MASComponentAnimation m_animation;

		public MASSwitch(MASComponent masComponent, MASComponentColliderEvent colliderEvent)
		{
			m_colliderEvent = colliderEvent;

			foreach (var action in masComponent.actions)
			{
				if (action is MASComponentRotation rotation && rotation.transform == colliderEvent.buttonObject.transform)
				{
					m_rotation = rotation;
				}
				else if (action is MASComponentAnimationPlayer animationPlayer && animationPlayer.name.Contains("Switch"))
				{
					// TODO: how to handle props that have more than one animation player?  There's no way to correlate the animation with the switch
					// might need to have something in the VRSwitch cfg that explicitly specifies it
					m_animationPlayer = animationPlayer;
				}
				else if (action is MASComponentAnimation animation && animation.name.Contains("Switch"))
				{
					m_animation = animation;
				}
			}

			if (m_rotation == null && m_animationPlayer == null)
			{
				Debug.LogError($"[KerbalVR] Failed to find MAS rotation, animationPlayer, or animation in prop {masComponent.internalProp.name}");
			}
		}

		// NOTE: the MAS RotationComponent can operate in two modes: blend or snap
		// if blend is true, then the current rotation of the switch is currentBlend
		// if blend is false, the current rotation of the switch is currentState
		// This whole thing will need to be revisited because MAS has switches that use their analog position

		public override bool CurrentState
		{
			get
			{
				if (m_rotation != null)
				{
					return m_rotation.blend ? m_rotation.currentBlend == 1 : m_rotation.currentState;
				}
				else if (m_animationPlayer != null)
				{
					return m_animationPlayer.currentState;
				}
				else if (m_animation != null)
				{
					return m_animation.currentBlend == 1;
				}

				return false;
			}
		}

		public override void SetState(bool newState)
		{
			if (newState != CurrentState)
			{
				m_colliderEvent.buttonObject.onClick();
			}
		}
	}
}
