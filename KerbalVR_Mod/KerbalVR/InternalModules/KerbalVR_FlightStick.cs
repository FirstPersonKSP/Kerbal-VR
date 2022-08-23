using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	public class VRFlightStick : InternalModule
	{
		[SerializeField]
		InteractionCommon.VRFlightStick m_flightStick;

#if PROP_GIZMOS
		GameObject gizmo;
#endif

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);

			InteractionCommon.VRFlightStick.Create(gameObject, ref m_flightStick, node);
		}

		private void Start()
		{
			var stickTransform = this.FindTransform(m_flightStick.stickTransformName);
			if (stickTransform == null) { return; }

			m_flightStick.OnStart(stickTransform, vessel, false);

#if PROP_GIZMOS
			if (gizmo == null)
			{
				gizmo = Utils.CreateGizmo();
				gizmo.transform.SetParent(stickTransform.parent, false);
				Utils.SetLayer(gizmo, 20);
					
				Utils.GetOrAddComponent<ColliderVisualizer>(stickTransform.gameObject);
			}
#endif
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			m_flightStick.OnUpdate();
		}
	}
}
