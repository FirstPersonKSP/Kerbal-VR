using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.PartModules
{
	public class VRThrottleLever : PartModule
	{
		[SerializeField]
		InteractionCommon.VRThrottleLever m_throttleLever;

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			InteractionCommon.VRThrottleLever.Create(gameObject, ref m_throttleLever, node);
		}

		public void Start()
		{
			var leverTransform = part.FindModelTransform(m_throttleLever.leverName);
			if (leverTransform == null)
			{
				Utils.LogError($"VRThrottleLever: Unable to find transform named {m_throttleLever.leverName} on part {part.partName}");
				return;
			}

			m_throttleLever.OnStart(leverTransform);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			m_throttleLever.OnUpdate();
		}
	}
}
