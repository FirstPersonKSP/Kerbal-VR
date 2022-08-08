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
		#region static members
		static readonly Type x_jsiVariableAnimatorType;
		static readonly FieldInfo x_useNewModeField;
		static readonly FieldInfo x_variableSetsField;

		static readonly Type x_variableAnimationSetType;
		static readonly FieldInfo x_variableField;
		static readonly FieldInfo x_vectorStartField;
		static readonly FieldInfo x_vectorEndField;

		static readonly Type x_variableOrNumberType;
		static readonly MethodInfo x_inverseLerpMethod;
		static readonly MethodInfo x_variableOrNumberAsFloatMethod;

		static readonly Type x_jsiNumericInputType;
		static readonly FieldInfo x_perPodPersistenceNameField;
		static readonly FieldInfo x_perPodPersistenceIsGlobalField;
		static readonly FieldInfo x_rpmCompField;
		static readonly FieldInfo x_minRangeField;
		static readonly FieldInfo x_maxRangeField;

		static readonly Type x_rasterPropMonitorComputerType;
		static readonly MethodInfo x_rpmCompSetPersistentVariableMethod;
		static readonly MethodInfo x_rpmCompGetInternalMethodMethod;

		static RPMKnob()
		{
			x_jsiVariableAnimatorType = AssemblyLoader.GetClassByName(typeof(InternalModule), "JSIVariableAnimator");

			if (x_jsiVariableAnimatorType != null)
			{
				x_useNewModeField = x_jsiVariableAnimatorType.GetField("useNewMode", BindingFlags.Instance | BindingFlags.NonPublic);
				x_variableSetsField = x_jsiVariableAnimatorType.GetField("variableSets", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			x_variableAnimationSetType = x_jsiVariableAnimatorType.Assembly.GetTypes().FirstOrDefault(type => type.FullName == "JSI.VariableAnimationSet");

			if (x_variableAnimationSetType != null)
			{
				x_variableField = x_variableAnimationSetType.GetField("variable", BindingFlags.Instance | BindingFlags.NonPublic);
				x_vectorStartField = x_variableAnimationSetType.GetField("vectorStart", BindingFlags.Instance | BindingFlags.NonPublic);
				x_vectorEndField = x_variableAnimationSetType.GetField("vectorEnd", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			x_variableOrNumberType = x_jsiVariableAnimatorType.Assembly.GetTypes().FirstOrDefault(type => type.FullName == "JSI.VariableOrNumber");

			if (x_variableOrNumberType != null)
			{
				x_inverseLerpMethod = x_variableOrNumberType.GetMethod("InverseLerp", BindingFlags.Instance | BindingFlags.Public);
				x_variableOrNumberAsFloatMethod = x_variableOrNumberType.GetMethod("AsFloat", BindingFlags.Instance | BindingFlags.Public);
			}

			x_jsiNumericInputType = x_jsiVariableAnimatorType.Assembly.GetTypes().FirstOrDefault(type => type.FullName == "JSI.JSINumericInput");

			if (x_jsiNumericInputType != null)
			{
				x_perPodPersistenceNameField = x_jsiNumericInputType.GetField("perPodPersistenceName", BindingFlags.Instance | BindingFlags.Public);
				x_perPodPersistenceIsGlobalField = x_jsiNumericInputType.GetField("perPodPersistenceIsGlobal", BindingFlags.Instance | BindingFlags.Public);
				x_rpmCompField = x_jsiNumericInputType.GetField("rpmComp", BindingFlags.Instance | BindingFlags.NonPublic);
				x_minRangeField = x_jsiNumericInputType.GetField("minRange", BindingFlags.Instance | BindingFlags.NonPublic);
				x_maxRangeField = x_jsiNumericInputType.GetField("maxRange", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			x_rasterPropMonitorComputerType = x_jsiVariableAnimatorType.Assembly.GetTypes().FirstOrDefault(type => type.FullName == "JSI.RasterPropMonitorComputer");
			if (x_rasterPropMonitorComputerType != null)
			{
				x_rpmCompSetPersistentVariableMethod = x_rasterPropMonitorComputerType.GetMethod("SetPersistentVariable", BindingFlags.Instance | BindingFlags.NonPublic);
				x_rpmCompGetInternalMethodMethod = x_rasterPropMonitorComputerType.GetMethod("GetInternalMethod", BindingFlags.Instance | BindingFlags.Public);
			}
		}

		static public RPMKnob TryConstruct(GameObject gameObject, VRKnobCustomRotation customRotation)
		{
			var rpmComponent = gameObject.GetComponent(x_jsiVariableAnimatorType);

			if (rpmComponent != null)
			{
				return new RPMKnob(rpmComponent, customRotation);
			}

			return null;
		}

		#endregion

		Component m_jsiVariableAnimator;
		object m_variableAnimationSet;
		object m_jsiNumericInput;

		object m_rpmComp;
		string m_perPodPersistenceName;
		bool m_perPodPersistenceIsGlobal;
		JSI.JSIActionGroupSwitch m_jsiActionGroupSwitch;

		VRKnobCustomRotation m_customRotation;

		public override float MinRotation { get; protected set; }

		public override float MaxRotation { get; protected set; }

		public override void SetUpdateEnabled(bool enabled)
		{
			// hack: setting useNewMode to true on the JSIVariableAnimator will prevent it from updating on its own
			x_useNewModeField.SetValue(m_jsiVariableAnimator, !enabled);
		}

		static float GetValueFromVariableOrNumberField(object obj, FieldInfo variableOrNumberField)
		{
			object variableOrNumber = variableOrNumberField.GetValue(obj);
			var result = x_variableOrNumberAsFloatMethod.Invoke(variableOrNumber, null);
			return (float)result;
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

		public override void SetRotationFraction(string customRotationFunction, float fraction)
		{
			if (!string.IsNullOrEmpty(customRotationFunction))
			{
				//var im = m_jsiVariableAnimator as InternalModule;
				//var vessel = im.vessel;

				// TODO: build a registry for these
				switch (customRotationFunction)
				{
					case "SpeedDisplayMode":
						FlightGlobals.SetSpeedMode(SpeedDisplayModeFromRotationFraction(fraction));
						break;
					case "ThrustLimit":
						var del = x_rpmCompGetInternalMethodMethod.Invoke(m_rpmComp, new object[] { "JSIInternalRPMButtons:SetThrottleLimit", typeof(Action<double>) }) as Delegate;
						if (del != null)
						{
							((Action<double>)del).Invoke(fraction * 100.0f);
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
			else
			{
				float minValue = GetValueFromVariableOrNumberField(m_jsiNumericInput, x_minRangeField);
				float maxValue = GetValueFromVariableOrNumberField(m_jsiNumericInput, x_maxRangeField);

				var val = Mathf.Lerp(minValue, maxValue, fraction);

				x_rpmCompSetPersistentVariableMethod.Invoke(m_rpmComp, new object[] { m_perPodPersistenceName, val, m_perPodPersistenceIsGlobal });
			}
		}

		public RPMKnob(Component knobComponent, VRKnobCustomRotation customRotation)
		{
			m_jsiVariableAnimator = knobComponent;
			m_customRotation = customRotation;

			if (m_customRotation != null)
			{
				MinRotation = customRotation.minRotation;
				MaxRotation = customRotation.maxRotation;
			}
			else
			{
				var variableSets = x_variableSetsField.GetValue(m_jsiVariableAnimator) as IList;
				if (variableSets != null && variableSets.Count > 0)
				{
					m_variableAnimationSet = variableSets[0];

					// note: VariableAnimationSet uses vectorStart/vectorEnd or rotationStart / rotationEnd depending on whether longPath is true
					// but I think longPath is true for all the props we care about
					Vector3 vectorStart = (Vector3)x_vectorStartField.GetValue(m_variableAnimationSet);
					Vector3 vectorEnd = (Vector3)x_vectorEndField.GetValue(m_variableAnimationSet);

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

			m_jsiNumericInput = knobComponent.gameObject.GetComponent(x_jsiNumericInputType);

			if (m_jsiNumericInput != null)
			{
				m_rpmComp = x_rpmCompField.GetValue(m_jsiNumericInput);
				m_perPodPersistenceName = x_perPodPersistenceNameField.GetValue(m_jsiNumericInput) as string;
				m_perPodPersistenceIsGlobal = (bool)x_perPodPersistenceIsGlobalField.GetValue(m_jsiNumericInput);
			}
		}
	}
}
