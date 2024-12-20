﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	public class VRButton : InternalModule
	{
		[KSPField]
		public string buttonTransformName = String.Empty;

		[KSPField]
		public Vector3 axis = Vector3.down;

		[KSPField]
		public float pressThreshold = 0.004f;

		[KSPField]
		public string coverTransformName = String.Empty;

		VRButtonInteractionListener interactionListener = null;
		VRCover cover = null;

#if PROP_GIZMOS
		GameObject gizmo;
#endif

		private void Start()
		{
			var buttonTransform = this.FindTransform(buttonTransformName);

			if (buttonTransform != null && interactionListener == null)
			{
				interactionListener = Utils.GetOrAddComponent<VRButtonInteractionListener>(buttonTransform.gameObject);
				interactionListener.buttonModule = this;

#if PROP_GIZMOS
				if (gizmo == null)
				{
					gizmo = Utils.CreateGizmo();
					gizmo.transform.SetParent(buttonTransform, false);
					Utils.SetLayer(gizmo, 20);
				}
#endif
			}

			if (coverTransformName == String.Empty)
			{
				cover = gameObject.GetComponent<VRCover>();
			}
			else if (coverTransformName != "none")
			{
				foreach (var c in gameObject.GetComponents<VRCover>())
				{
					if (c.coverTransformName == coverTransformName)
					{
						cover = c;
						break;
					}
				}

				if (cover == null)
				{
					Utils.LogError($"VRButton - unable to find cover with transform '{coverTransformName}' for prop {internalProp.propName}");
				}
			}
		}

		class VRButtonInteractionListener : MonoBehaviour, IFingertipInteractable
		{
			public VRButton buttonModule;

			// when the fingertip initially makes contact, where is its center along the axis?
			float initialContactOffset = 0.0f;
			Vector3 initialLocalPosition; // the button's localPosition at rest
			bool latched = false;
			bool latchedCover = false;

			public void Awake()
			{
				initialLocalPosition = transform.localPosition;

#if PROP_GIZMOS
				Utils.GetOrAddComponent<ColliderVisualizer>(gameObject);
#endif
			}

			float GetFingertipPosition(Vector3 fingertipCenter)
			{
				Vector3 localFingertipPosition = transform.InverseTransformPoint(fingertipCenter);
				return Vector3.Dot(localFingertipPosition, buttonModule.axis);
			}

			public void OnEnter(Hand hand, Collider buttonCollider)
			{
				initialContactOffset = GetFingertipPosition(hand.FingertipPosition);

				latchedCover = buttonModule.cover != null && !buttonModule.cover.IsOpen;

				//if (!latchedCover)
				//{
				//	HapticUtils.Light(hand.handType);
				//}
			}

			public void OnExit(Hand hand, Collider buttonCollider)
			{
				if (latchedCover) return;

				transform.localPosition = initialLocalPosition;

				if (latched)
				{
					gameObject.SendMessage("OnMouseUp");
				}

				latched = false;
			}

			public void OnStay(Hand hand, Collider buttonCollider)
			{
				if (latchedCover) return;

				float currentFingerPosition = GetFingertipPosition(hand.FingertipPosition);
				float delta = Mathf.Max(0.0f, currentFingerPosition - initialContactOffset);

				if (delta > buttonModule.pressThreshold)
				{
					delta = buttonModule.pressThreshold;

					if (!latched)
					{
						latched = true;
						
						// some buttons do things like change scenes (revert to launch, quickload) which cannot be called from an OnStay callback - so delay a frame and this will be executed during coroutine evaluation
						StartCoroutine(CallbackUtil.DelayedCallback(1, delegate
						{
							gameObject.SendMessage("OnMouseDown");
						}));
						
						HapticUtils.Light(hand.handType);
					}
				}

				Vector3 axisInParentSpace = transform.parent.InverseTransformDirection(transform.TransformDirection(buttonModule.axis));

				transform.localPosition = initialLocalPosition + axisInParentSpace * delta;
			}
		}
	}
}
