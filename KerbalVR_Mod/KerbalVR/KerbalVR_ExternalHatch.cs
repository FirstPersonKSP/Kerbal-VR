using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR
{
	// we may want to eventually just make this a regular behavior class that gets dynamically attached to stuff instead of a partmodule
	public class VRExternalHatch : PartModule
	{
		[KSPField]
		public string hatchTransformName = String.Empty;

		[KSPField]
		public Vector3 rotationAxis = Vector3.down;

		[KSPField]
		public float maxRotation = 175.0f;

		InteractableBehaviour m_interactableBehaviour;
		Hand m_grabbedHand;
		RotationUtil m_rotationUtil;

		void Start()
		{
			Transform hatchTransform = null;
			var firstSlashIndex = hatchTransformName.IndexOf('/');
			if (firstSlashIndex > 0)
			{
				var root = hatchTransformName.Substring(0, firstSlashIndex);
				var rootTransform = part.FindModelTransform(root);
				var rest = hatchTransformName.Substring(firstSlashIndex + 1);
				hatchTransform = rootTransform.Find(rest);
			}
			else
			{
				hatchTransform = part.FindModelTransform(hatchTransformName);
			}

			m_rotationUtil = new RotationUtil(hatchTransform, rotationAxis, 0.0f, maxRotation);

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
					var kerbalEVA = FlightGlobals.ActiveVessel.evaController;
					var protoCrewMember = kerbalEVA.part.protoModuleCrew[0];
					kerbalEVA.BoardPart(part);
					
					yield return null; // we have to wait a frame so the kerbal gets set up

					CameraManager.Instance.SetCameraIVA(protoCrewMember.KerbalRef, false);
					m_rotationUtil.Reset();
					yield break;
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
			StartCoroutine(UpdateHatchTransform());
		}
	}
}
