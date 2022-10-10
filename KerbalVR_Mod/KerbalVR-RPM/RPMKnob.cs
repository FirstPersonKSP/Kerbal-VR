using KerbalVR.InternalModules;
using KerbalVR.IVAAdaptors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR_RPM
{
	public class RPMKnob : IVAKnob
	{
		static public RPMKnob TryConstruct(VRKnob vrKnob)
		{
			var rpmComponent = vrKnob.GetComponent<JSI.JSIVariableAnimator>();

			if (rpmComponent != null)
			{
				return new RPMKnob(rpmComponent, vrKnob);
			}

			return null;
		}

		JSI.JSIVariableAnimator m_jsiVariableAnimator;
		JSI.VariableAnimationSet m_variableAnimationSet;
		JSI.JSINumericInput m_jsiNumericInput;

		JSI.RasterPropMonitorComputer m_rpmComp;
		string m_perPodPersistenceName;
		bool m_perPodPersistenceIsGlobal;
		JSI.JSIActionGroupSwitch m_jsiActionGroupSwitch;

		VRKnob m_knob;

		public override float MinRotation { get; protected set; }

		public override float MaxRotation { get; protected set; }

		public override void SetUpdateEnabled(bool enabled)
		{
			// hack: setting useNewMode to true on the JSIVariableAnimator will prevent it from updating on its own
			m_jsiVariableAnimator.useNewMode = !enabled;
		}

		FlightGlobals.SpeedDisplayModes SpeedDisplayModeFromRotationFraction(float fraction)
		{
			int step = Mathf.RoundToInt(fraction * 2);
			switch (step)
			{
				case 0: return FlightGlobals.SpeedDisplayModes.Orbit;
				case 1: return FlightGlobals.SpeedDisplayModes.Surface;
				case 2: return FlightGlobals.SpeedDisplayModes.Target;
			}

			throw new ArgumentException("Invalid fraction");
		}

		public override void SetRotationFraction(float fraction)
		{
			if (!string.IsNullOrEmpty(m_knob.customRotationHandler))
			{
				//var im = m_jsiVariableAnimator as InternalModule;
				//var vessel = im.vessel;

				// TODO: build a registry for these
				switch (m_knob.customRotationHandler)
				{
					case "SpeedDisplayMode":
						FlightGlobals.SetSpeedMode(SpeedDisplayModeFromRotationFraction(fraction));
						break;
					case "ThrustLimit":
						var thrustLImitDel = m_rpmComp.GetInternalMethod("JSIInternalRPMButtons:SetThrottleLimit", typeof(Action<double>));
						if (thrustLImitDel != null)
						{
							((Action<double>)thrustLImitDel).Invoke(fraction * 100.0f);
						}
						break;
					case "StageLocked":
						var stageLockedDel = m_rpmComp.GetInternalMethod("JSIInternalRPMButtons:ButtonStageLock", typeof(Action<bool>));
						if (stageLockedDel != null)
						{
							((Action<bool>)stageLockedDel).Invoke(fraction == 1.0f);
						}
						break;
					case "PrecisionMode":
						var precModeDel = m_rpmComp.GetInternalMethod("JSIInternalRPMButtons:ButtonPrecisionMode", typeof(Action<bool>));
						if (precModeDel != null)
						{
							((Action<bool>)precModeDel).Invoke(fraction == 1.0f);
						}
						break;
					case "IntLight":
						if (m_jsiVariableAnimator.gameObject.GetComponentCached(ref m_jsiActionGroupSwitch) != null &&
							m_jsiActionGroupSwitch.lightObjects != null)
						{
							foreach (var light in m_jsiActionGroupSwitch.lightObjects)
							{
								light.intensity = fraction;
								light.enabled = fraction > 0;
							}
						}
						break;
				}
			}
			else if (m_jsiNumericInput != null)
			{
				float minValue = m_jsiNumericInput.minRange.AsFloat();
				float maxValue = m_jsiNumericInput.maxRange.AsFloat();

				var val = Mathf.Lerp(minValue, maxValue, fraction);

				m_rpmComp.SetPersistentVariable(m_perPodPersistenceName, val, m_perPodPersistenceIsGlobal);
			}
		}

		public RPMKnob(JSI.JSIVariableAnimator knobComponent, VRKnob vrKnob)
		{
			m_jsiVariableAnimator = knobComponent;
			m_knob = vrKnob;
			m_rpmComp = knobComponent.rpmComp;

			// try to calculate min/max rotation angles
			if (m_knob.customRotation != null)
			{
				MinRotation = m_knob.customRotation.minRotation;
				MaxRotation = m_knob.customRotation.maxRotation;
			}
			else
			{
				var variableSets = m_jsiVariableAnimator.variableSets;
				if (variableSets != null && variableSets.Count > 0)
				{
					// this isn't correct for props with more than one variable set!
					m_variableAnimationSet = variableSets[0];

					// note: VariableAnimationSet uses vectorStart/vectorEnd or rotationStart / rotationEnd depending on whether longPath is true
					// but I think longPath is true for all the props we care about
					Vector3 vectorStart = m_variableAnimationSet.vectorStart;
					Vector3 vectorEnd = m_variableAnimationSet.vectorEnd;

					MinRotation = vectorStart.x;
					MaxRotation = vectorEnd.x;

					if (MinRotation == 0 && MaxRotation == 0)
					{
						MinRotation = vectorStart.y;
						MaxRotation = vectorEnd.y;

						if (MinRotation == 0 && MaxRotation == 0)
						{
							MinRotation = vectorStart.z;
							MaxRotation = vectorEnd.z;
						}
					}
				}
			}

			m_jsiNumericInput = knobComponent.gameObject.GetComponent<JSI.JSINumericInput>();

			if (m_jsiNumericInput != null)
			{
				m_perPodPersistenceName = m_jsiNumericInput.perPodPersistenceName;
				m_perPodPersistenceIsGlobal = m_jsiNumericInput.perPodPersistenceIsGlobal;
			}
		}
	}
}
