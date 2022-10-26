using System;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	
	// this will probably eventually be moved to Common so that we can have rails on the exterior too
	public class VRHandRail : InternalModule
	{
		[KSPField]
		public string railTransformName = string.Empty;

		InteractableBehaviour m_interactable;


		void Start()
		{
			var transform = this.FindTransform(railTransformName);
			if (transform != null)
			{
				var colliderTransform = transform.GetComponentInChildren<Collider>();
				if (colliderTransform != null)
				{
					m_interactable = Utils.GetOrAddComponent<InteractableBehaviour>(colliderTransform.gameObject);
					m_interactable.OnGrab += Grabbed;
				}
				else
				{
					Utils.LogError($"VRHandRail: No collider under transform ${railTransformName} found for prop {internalProp.propName}");
				}
			}
		}

		private void Grabbed(Hand hand)
		{
			FreeIva.KerbalIvaController.Instance.Unbuckle();
		}
	}
}
