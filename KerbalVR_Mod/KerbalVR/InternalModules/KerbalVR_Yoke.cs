using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.InternalModules
{
	internal class VRYoke : InternalModule
	{
		[SerializeField]
		InteractionCommon.VRYoke m_yoke;

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			InteractionCommon.VRYoke.Create(gameObject, ref m_yoke, node);
		}

		public void Start()
		{
			var steerTransform = this.FindTransform(m_yoke.steerTransformName);
			var pushTransform = this.FindTransform(m_yoke.pushTransformName);

			if (steerTransform == null || pushTransform == null)
			{
				return;
			}

			m_yoke.OnStart(steerTransform, pushTransform, vessel);

#if PROP_GIZMOS
			Utils.GetOrAddComponent<ColliderVisualizer>(steerTransform.gameObject);
			Utils.GetOrAddComponent<ColliderVisualizer>(pushTransform.gameObject);
#endif
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			m_yoke.OnUpdate();
		}
	}
}
