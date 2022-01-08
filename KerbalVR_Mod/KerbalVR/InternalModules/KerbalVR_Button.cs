using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	class VRButton : InternalModule, IFingertipInteractable
	{
		[KSPField]
		public string buttonTransformName = null;



#if PROP_GIZMOS
		GameObject gizmo;
#endif

		public override void OnAwake()
		{
			base.OnAwake();

#if PROP_GIZMOS
			if (gizmo == null)
			{
				var buttonTransform = internalProp.FindModelTransform(buttonTransformName);

				if (buttonTransform == null) return;

				gizmo = Utils.CreateGizmo();
				gizmo.transform.SetParent(buttonTransform, false);
				Utils.SetLayer(gizmo, 20);

				Utils.GetOrAddComponent<ColliderVisualizer>(buttonTransform.gameObject);
			}
#endif
		}

		public void OnEnter(Collider collider, SteamVR_Input_Sources inputSource)
		{
			
		}

		public void OnExit(Collider collider, SteamVR_Input_Sources inputSource)
		{
			
		}

		public void OnStay(Collider collider, SteamVR_Input_Sources inputSource)
		{
			
		}
	}
}
