using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KerbalVR.InternalModules
{
	internal class VRInternalCameraSwitch : InternalCameraSwitch
	{
		InteractableBehaviour interactableBehaviour;

		public override void OnAwake()
		{
			base.OnAwake();

			if (colliderTransform != null)
			{
				interactableBehaviour = Utils.GetOrAddComponent<InteractableBehaviour>(colliderTransform.gameObject);

				interactableBehaviour.OnGrab = OnGrab;

				// TODO: do we need to figure out how to disable the collider on the current camera point?
			}
		}

		private void OnGrab(Hand hand)
		{
			Button_OnDoubleTap();
		}
	}
}
