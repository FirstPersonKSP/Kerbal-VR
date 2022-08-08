using KerbalVR.IVAAdaptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR_RPM
{
	internal class RPMSwitch : IVASwitch
	{
		static readonly Type x_jsiActionGroupSwitchType;
		static readonly FieldInfo x_currentStateField;
		static readonly FieldInfo x_switchTransformField;
		static readonly MethodInfo x_clickMethod;

		static RPMSwitch()
		{
			x_jsiActionGroupSwitchType = AssemblyLoader.GetClassByName(typeof(InternalModule), "JSIActionGroupSwitch");

			if (x_jsiActionGroupSwitchType != null)
			{
				x_currentStateField = x_jsiActionGroupSwitchType.GetField("currentState", BindingFlags.Instance | BindingFlags.NonPublic);
				x_clickMethod = x_jsiActionGroupSwitchType.GetMethod("Click", BindingFlags.Instance | BindingFlags.Public);
				x_switchTransformField = x_jsiActionGroupSwitchType.GetField("switchTransform", BindingFlags.Instance | BindingFlags.Public);
			}

			IVASwitch.CreationFunctions.Add(TryConstruct);
		}

		static public RPMSwitch TryConstruct(GameObject prop, Transform switchTransform)
		{
			string transformName = switchTransform.name;
			var switchComponents = prop.GetComponents(x_jsiActionGroupSwitchType);

			var switchComponent = switchComponents.FirstOrDefault(x => (string)x_switchTransformField.GetValue(x) == transformName);

			if (switchComponent != null)
			{
				return new RPMSwitch(switchComponent);
			}

			return null;
		}

		Component m_rpmComponent;

		public RPMSwitch(Component switchComponent)
		{
			m_rpmComponent = switchComponent;
		}

		public override bool CurrentState
		{
			get { return (bool)x_currentStateField.GetValue(m_rpmComponent); }
		}

		public override void SetState(bool newState)
		{
			if (newState != CurrentState)
			{
				x_clickMethod.Invoke(m_rpmComponent, null);
			}
		}
	}
}
