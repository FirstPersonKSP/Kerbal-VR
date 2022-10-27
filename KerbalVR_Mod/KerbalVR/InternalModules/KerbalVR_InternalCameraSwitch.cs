using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KerbalVR.InternalModules
{
	class VRCameraSwitchInteraction : VRSeatBehaviour
	{
		public VRInternalCameraSwitch internalModule;

		public override void OnInteract(Hand hand)
		{
			internalModule.Button_OnDoubleTap();
		}
	}

	internal class VRInternalCameraSwitch : InternalCameraSwitch
	{
		VRCameraSwitchInteraction interactionHandler;

		public override void OnAwake()
		{
			base.OnAwake();

			if (colliderTransform != null)
			{
				interactionHandler = Utils.GetOrAddComponent<VRCameraSwitchInteraction>(colliderTransform.gameObject);
				interactionHandler.internalModule = this;
			}
		}
	}
}
