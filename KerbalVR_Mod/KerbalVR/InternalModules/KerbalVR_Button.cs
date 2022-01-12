using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	class VRButton : InternalModule
	{
		[KSPField]
		public string buttonTransformName = null;

		[KSPField]
		public Vector3 axis = Vector3.down;

		[KSPField]
		public float pressThreshold = 0.004f;

		VRButtonInteractionListener interactionListener = null;

#if PROP_GIZMOS
		GameObject gizmo;
#endif

		public override void OnAwake()
		{
			base.OnAwake();

			var buttonTransform = internalProp.transform.Find(buttonTransformName);

			if (buttonTransform != null && interactionListener == null)
			{
				interactionListener = buttonTransform.gameObject.AddComponent<VRButtonInteractionListener>();
				interactionListener.buttonModule = this;

#if PROP_GIZMOS
				if (gizmo == null)
				{
					gizmo = Utils.CreateGizmo();
					gizmo.transform.SetParent(transform, false);
					Utils.SetLayer(gizmo, 20);
				}
#endif
			}
		}

		class VRButtonInteractionListener : MonoBehaviour, IFingertipInteractable
		{
			public VRButton buttonModule;

			// when the fingertip initially makes contact, where is its center along the axis?
			float initialContactOffset = 0.0f;
			Vector3 initialLocalPosition; // the button's localPosition at rest
			bool latched = false;

			public void Awake()
			{
				initialLocalPosition = transform.localPosition;

#if PROP_GIZMOS
				Utils.GetOrAddComponent<ColliderVisualizer>(gameObject);
#endif
			}

			float GetFingertipPosition(Vector3 fingertipCenter)
			{
				Vector3 localFingertipPosition = buttonModule.transform.InverseTransformPoint(fingertipCenter);
				return Vector3.Dot(localFingertipPosition, buttonModule.axis);
			}

			public void OnEnter(Vector3 fingertipCenter, Collider buttonCollider, SteamVR_Input_Sources inputSource)
			{
				initialContactOffset = GetFingertipPosition(fingertipCenter);
			}

			public void OnExit(Vector3 fingertipCenter, Collider buttonCollider, SteamVR_Input_Sources inputSource)
			{
				transform.localPosition = initialLocalPosition;

				if (latched)
				{
					buttonModule.internalProp.SendMessage("OnMouseUp");
				}

				latched = false;
			}

			public void OnStay(Vector3 fingertipCenter, Collider buttonCollider, SteamVR_Input_Sources inputSource)
			{
				float currentFingerPosition = GetFingertipPosition(fingertipCenter);
				float delta = Mathf.Max(0.0f, currentFingerPosition - initialContactOffset);

				if (delta > buttonModule.pressThreshold)
				{
					delta = buttonModule.pressThreshold;

					if (!latched)
					{
						latched = true;
						gameObject.SendMessage("OnMouseDown");
					}
				}

				transform.localPosition = initialLocalPosition + buttonModule.axis * delta;
			}
		}
	}
}
