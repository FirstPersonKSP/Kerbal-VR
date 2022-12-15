using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	public class VRInternalHatch : InternalModule
	{
		// NOTE: this class is basically copy/pasted from VRExternalHatch

		[KSPField]
		public string hatchTransformName = String.Empty;

		[KSPField]
		public Vector3 rotationAxis = Vector3.down;

		[KSPField]
		public float maxRotation = 450.0f;

		InteractableBehaviour m_interactableBehaviour;
		Hand m_grabbedHand;
		RotationUtil m_rotationUtil;

		void Start()
		{
			m_rotationUtil = new RotationUtil(this.FindTransform(hatchTransformName), rotationAxis, 0.0f, maxRotation);
			var collider = m_rotationUtil.Transform.GetComponentInChildren<Collider>();
			m_interactableBehaviour = Utils.GetOrAddComponent<InteractableBehaviour>(collider.gameObject);

			m_interactableBehaviour.SkeletonPoser = Utils.GetOrAddComponent<SteamVR_Skeleton_Poser>(collider.gameObject);
			m_interactableBehaviour.SkeletonPoser.skeletonMainPose = SkeletonPose_HandleRailGrabPose.GetInstance();
			m_interactableBehaviour.SkeletonPoser.Initialize();

			m_interactableBehaviour.OnGrab += OnGrab;
			m_interactableBehaviour.OnRelease += OnRelease;
		}

		public IEnumerator UpdateHatchTransform()
		{
			while (m_grabbedHand)
			{
				m_rotationUtil.Update(m_grabbedHand.GripPosition);

				if (m_rotationUtil.IsAtMax())
				{
					m_grabbedHand.Detach(true);

					m_rotationUtil.Transform.SendMessage("OnMouseDown");

					// TODO: figure out how to force first person (this has to occur after the kerbal spawns)
					// KerbalVR.Scene.EnterFirstPerson();

					break;
				}

				yield return null;
			}

			// TODO: interpolate back to neutral
			m_rotationUtil.Reset();
		}

		private void OnRelease(Hand hand)
		{
			m_grabbedHand = null;
		}

		private void OnGrab(Hand hand)
		{
			m_grabbedHand = hand;
			m_rotationUtil.Grabbed(m_grabbedHand.GripPosition);
			HapticUtils.Heavy(hand.handType);
			StartCoroutine(UpdateHatchTransform());
		}
	}
}
