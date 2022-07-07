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

		Transform m_hatchTransform;
		InteractableBehaviour m_interactableBehaviour;
		Hand m_grabbedHand;
		float m_grabbedAngle;

		void Start()
		{

			var firstSlashIndex = hatchTransformName.IndexOf('/');
			if (firstSlashIndex > 0)
			{
				var root = hatchTransformName.Substring(0, firstSlashIndex);
				var rootTransform = part.FindModelTransform(root);
				var rest = hatchTransformName.Substring(firstSlashIndex + 1);
				m_hatchTransform = rootTransform.Find(rest);
			}
			else
			{
				m_hatchTransform = part.FindModelTransform(hatchTransformName);
			}

			var collider = m_hatchTransform.GetComponentInChildren<Collider>();
			m_interactableBehaviour = Utils.GetOrAddComponent<InteractableBehaviour>(collider.gameObject);

			m_interactableBehaviour.SkeletonPoser = Utils.GetOrAddComponent<SteamVR_Skeleton_Poser>(collider.gameObject);
			m_interactableBehaviour.SkeletonPoser.skeletonMainPose = SkeletonPose_HandleRailGrabPose.GetInstance();
			m_interactableBehaviour.SkeletonPoser.Initialize();

			m_interactableBehaviour.OnGrab += OnGrab;
			m_interactableBehaviour.OnRelease += OnRelease;
		}

		float GetCurrentAngle()
		{
			Vector3 relativePosition = m_hatchTransform.parent.transform.InverseTransformPoint(m_grabbedHand.GripPosition);
			Vector3 forward = rotationAxis == Vector3.down ? Vector3.forward : Vector3.up;
			Vector3 right = Vector3.Cross(forward, rotationAxis);

			float f = Vector3.Dot(forward, relativePosition);
			float r = -Vector3.Dot(right, relativePosition);

			return Mathf.Atan2(r, f) * Mathf.Rad2Deg;
		}

		public IEnumerator UpdateHatchTransform()
		{
			while (m_grabbedHand)
			{
				float newAngle = GetCurrentAngle();
				float rotation = Mathf.Clamp(newAngle - m_grabbedAngle, 0.0f, maxRotation);
				m_hatchTransform.localRotation = Quaternion.AngleAxis(rotation, rotationAxis);

				if (rotation == maxRotation)
				{
					var kerbalEVA = FlightGlobals.ActiveVessel.evaController;
					var protoCrewMember = kerbalEVA.part.protoModuleCrew[0];
					kerbalEVA.BoardPart(part);
					
					yield return null; // we have to wait a frame so the kerbal gets set up

					CameraManager.Instance.SetCameraIVA(protoCrewMember.KerbalRef, false);
					yield break;
				}

				yield return null;
			}
			
			// TODO: interpolate back to neutral
			m_hatchTransform.localRotation = Quaternion.identity;
		}

		private void OnRelease(Hand hand)
		{
			m_grabbedHand = null;
		}

		private void OnGrab(Hand hand)
		{
			m_grabbedHand = hand;
			m_grabbedAngle = GetCurrentAngle();
			StartCoroutine(UpdateHatchTransform());
		}
	}
}
