using KerbalVR;
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
		static public RPMSwitch TryConstruct(GameObject prop, Transform switchTransform)
		{
			string transformName = switchTransform.name;
			var switchComponents = prop.GetComponents<JSI.JSIActionGroupSwitch>();

			var switchComponent = switchComponents.FirstOrDefault(x => x.switchTransform == transformName);

			if (switchComponent != null)
			{
				return new RPMSwitch(switchComponent);
			}

			return null;
		}

		JSI.JSIActionGroupSwitch m_rpmComponent;

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
			}
		}
	}
}
