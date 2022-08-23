using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.PartModules
{
	internal class VRFlightStick : PartModule
	{
		[SerializeField]
		InteractionCommon.VRFlightStick m_flightStick;

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			InteractionCommon.VRFlightStick.Create(gameObject, ref m_flightStick, node);
		}

		void Start()
		{
			var transform = part.FindModelTransform(m_flightStick.stickTransformName);
			if (transform == null)
			{
				Utils.LogError($"VRFlightStick: Unable to find transform named {m_flightStick.stickTransformName} on part {part.partName}");
				return;
			}

			m_flightStick.OnStart(transform, vessel, true);

			var gizmo = Utils.CreateGizmo();
			gizmo.transform.SetParent(transform.parent, false);

			Utils.GetOrAddComponent<ColliderVisualizer>(transform.gameObject);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			m_flightStick.OnUpdate();
		}
	}
}
