using KerbalVR;
using KerbalVR.InternalModules;
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
		static public IVASwitch TryConstruct(VRSwitch prop, Transform switchTransform)
		{
			string transformName = switchTransform.name;

			if (prop.triState)
			{
				var numericInputComponents = prop.GetComponents<JSI.JSINumericInput>();
				if (numericInputComponents.Length > 0)
				{
					return new RPMTriStateSwitch(numericInputComponents);
				}
			}
			else
			{
				var switchComponents = prop.GetComponents<JSI.JSIActionGroupSwitch>();
				var switchComponent = switchComponents.FirstOrDefault(x => x.switchTransform == transformName);

				if (switchComponent != null)
				{
					return new RPMSwitch(switchComponent);
				}
			}

			return null;
		}

		JSI.JSIActionGroupSwitch m_rpmComponent;
		Animation m_animation;

		public RPMSwitch(JSI.JSIActionGroupSwitch switchComponent)
		{
			m_rpmComponent = switchComponent;
		}

		public override bool CurrentState
		{
			get { return m_rpmComponent.currentState; }
		}

		public override void SetState(bool newState)
		{
			if (newState != CurrentState)
			{
				m_rpmComponent.Click();
				m_rpmComponent.currentState = newState;
			}
		}

		public override void SetAnimationsEnabled(bool enabled)
		{
			if (enabled)
			{
				m_rpmComponent.anim = m_animation;
			}
			else if (m_rpmComponent.anim != null)
			{
				m_animation = m_rpmComponent.anim;
				m_rpmComponent.anim = null;
			}
		}
	}

	internal class RPMTriStateSwitch : IVASwitch
	{
		JSI.JSINumericInput.NumericInput m_incrementInput;
		JSI.JSINumericInput.NumericInput m_decrementInput;

		Animation m_incrementAnimation;
		Animation m_decrementAnimation;

		public RPMTriStateSwitch(JSI.JSINumericInput[] inputs)
		{
			foreach (var input in inputs)
			{
				foreach(var numericInput in input.numericInputs)
				{
					if (numericInput.increment.AsDouble() > 0)
					{
						m_incrementInput = numericInput;
					}
					else if (numericInput.increment.AsDouble() < 0)
					{
						m_decrementInput = numericInput;
					}
				}
			}
		}

		public override void SetState(bool newState)
		{
			var input = newState ? m_incrementInput : m_decrementInput;
			input.Click();
		}

		public override void SetAnimationsEnabled(bool enabled)
		{
			if (enabled)
			{
				m_incrementInput.anim = m_incrementAnimation;
				m_decrementInput.anim = m_decrementAnimation;
			}
			else
			{
				m_incrementAnimation = m_incrementInput.anim;
				m_incrementInput.anim = null;
				m_decrementAnimation = m_decrementInput.anim;
				m_decrementInput.anim = null;
			}
		}
	}
}
