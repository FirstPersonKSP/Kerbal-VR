using AvionicsSystems;
using KerbalVR.InternalModules;
using KerbalVR.IVAAdaptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR_MAS
{
	internal class MASKnob : IVAKnob
	{
		static public MASKnob TryConstruct(VRKnob vrKnob)
		{
			var masComponent = vrKnob.GetComponent<MASComponent>();

			if (masComponent != null)
			{
				foreach (var action in masComponent.actions)
				{
					// TODO: how do we handle props with more than one knob (e.g. ASET HUD) ?
					if (action is MASComponentRotation rotation)
					{
						return new MASKnob(vrKnob, rotation);
					}
				}
			}

			return null;
		}

		VRKnob m_vrKnob;
		MASComponentRotation m_rotation;
		MASFlightComputer m_computer;

		MASKnob(VRKnob vrKnob, MASComponentRotation rotation)
		{
			m_vrKnob = vrKnob;
			m_rotation = rotation;
			m_computer = vrKnob.part.GetComponent<MASFlightComputer>();

			if (vrKnob.userVariable == String.Empty && vrKnob.customRotationHandler == String.Empty)
			{
				Debug.LogError($"[KerbalVR] MAS knob {vrKnob.internalProp.name} is missing both userVariable and customRotationHandler; won't be usable");
			}

			MinRotation = m_rotation.startRotation.eulerAngles.y; // TOOD: other axes?
			MaxRotation = m_rotation.endRotation.eulerAngles.y;

			if (MinRotation > MaxRotation)
			{
				MinRotation -= 360;
			}
		}

		public override float MinRotation { get; protected set; }
		public override float MaxRotation { get; protected set; }

		// Note this is backwards from the RPM one
		FlightGlobals.SpeedDisplayModes SpeedDisplayModeFromRotationFraction(float fraction)
		{
			int step = Mathf.RoundToInt(fraction * 2);
			switch (step)
			{
				case 0: return FlightGlobals.SpeedDisplayModes.Target;
				case 1: return FlightGlobals.SpeedDisplayModes.Surface;
				case 2: return FlightGlobals.SpeedDisplayModes.Orbit;
			}

			throw new ArgumentException("Invalid fraction");
		}

		public override void SetRotationFraction(float fraction)
		{
			var val = Mathf.Lerp(m_rotation.range1, m_rotation.range2, fraction);

			if (!string.IsNullOrEmpty(m_vrKnob.customRotationHandler))
			{
				switch (m_vrKnob.customRotationHandler)
				{
					case "ThrustLimit":
						m_computer.fcProxy.SetThrottleLimit(val);
						break;
					case "SpeedDisplayMode":
						// MAS doesn't seem to have a function for setting the speed mode directly (only toggle)
						FlightGlobals.SetSpeedMode(SpeedDisplayModeFromRotationFraction(fraction));
						break;
					case "GimbalLock":
						m_computer.fcProxy.SetGimbalLock(val == 1.0f);
						break;
					case "PrecisionMode":
						m_computer.fcProxy.SetPrecisionMode(val == 1.0f);
						break;
					case "StageLocked":
						m_computer.fcProxy.SetStageLocked(val == 1.0f);
						break;
					case "MultiEngineMode":
						m_computer.fcProxy.SetMultiModeEngineMode(val == 1.0f);
						break;
					default:
						Debug.LogError($"[KerbalVR] MAS knob {m_vrKnob.internalProp.name} has an unrecognized customRotationFunction '{m_vrKnob.customRotationHandler}'");
						break;
				}
			}
			else if (!string.IsNullOrEmpty(m_vrKnob.userVariable))
			{
				m_computer.SetPersistent(m_vrKnob.userVariable, val);
			}
		}

		public override void SetUpdateEnabled(bool enabled)
		{
			m_rotation.variableRegistrar.EnableCallbacks(enabled);
		}
	}
}
