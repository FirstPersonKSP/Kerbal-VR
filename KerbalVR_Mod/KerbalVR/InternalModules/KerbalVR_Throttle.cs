using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	class VRThrottleLever : InternalModule
	{
		[SerializeField]
		InteractionCommon.VRThrottleLever m_throttleLever;

#if PROP_GIZMOS
		GameObject gizmo;
		GameObject arrow;
#endif

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			InteractionCommon.VRThrottleLever.Create(gameObject, ref m_throttleLever, node);
		}

		public void Start()
		{
			// the stock module only supports transforms as a child of this game object
			// try to find a relative path here
			var leverTransform = this.FindTransform(m_throttleLever.leverName);
			if (leverTransform == null) return;

			m_throttleLever.OnStart(leverTransform);

#if PROP_GIZMOS
			if (gizmo == null)
			{
				gizmo = Utils.CreateGizmo();
				gizmo.transform.SetParent(leverTransform.parent, false);
				Utils.SetLayer(gizmo, 20);
				arrow = Utils.CreateArrow(Color.cyan, 0.2f);
				arrow.transform.SetParent(leverTransform.parent, false);
				arrow.transform.localRotation = Quaternion.LookRotation(m_throttleLever.axis);
				Utils.SetLayer(arrow, 20);

				Utils.GetOrAddComponent<ColliderVisualizer>(leverTransform.gameObject);
			}
#endif
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			m_throttleLever.OnUpdate();
		}
	}
}
