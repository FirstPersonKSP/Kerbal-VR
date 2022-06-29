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
		public float maxRotation = 175.0f;

		Quaternion initialRotation;
		Transform m_hatchTransform;
		InteractableBehaviour m_interactableBehaviour;
		Hand m_grabbedHand;
		float m_grabbedAngle;

		void Start()
		{
			m_hatchTransform = this.FindTransform(hatchTransformName);
			var collider = m_hatchTransform.GetComponentInChildren<Collider>();
			m_interactableBehaviour = Utils.GetOrAddComponent<InteractableBehaviour>(collider.gameObject);

			m_interactableBehaviour.SkeletonPoser = Utils.GetOrAddComponent<SteamVR_Skeleton_Poser>(collider.gameObject);
			m_interactableBehaviour.SkeletonPoser.skeletonMainPose = SkeletonPose_HandleRailGrabPose.GetInstance();
			m_interactableBehaviour.SkeletonPoser.Initialize();

			m_interactableBehaviour.OnGrab += OnGrab;
			m_interactableBehaviour.OnRelease += OnRelease;

			initialRotation = m_hatchTransform.localRotation;
		}

		float GetCurrentAngle()
		{
			Vector3 direction = m_grabbedHand.GripPosition - m_hatchTransform.position;
			Vector3 relativePosition = initialRotation.Inverse() * direction;
			relativePosition =  m_hatchTransform.parent.transform.InverseTransformVector(relativePosition);

			bool rotationAxisIsParellelToUp = Mathf.Approximately(Vector3.Magnitude(Vector3.Cross(rotationAxis, Vector3.up)), 0.0f);
			Vector3 forward = rotationAxisIsParellelToUp ? Vector3.forward : Vector3.down;
			Vector3 right = Vector3.Normalize(Vector3.Cross(forward, rotationAxis));
			forward = Vector3.Normalize(Vector3.Cross(right, rotationAxis));

			float f = Vector3.Dot(forward, relativePosition);
			float r = Vector3.Dot(right, relativePosition);

			return Mathf.Atan2(r, f) * Mathf.Rad2Deg;
		}

		void GoEVA()
		{
			float acLevel = ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.AstronautComplex);
			bool evaUnlocked = GameVariables.Instance.UnlockedEVA(acLevel);
			bool evaPossible = GameVariables.Instance.EVAIsPossible(evaUnlocked, vessel);

			Kerbal kerbal = CameraManager.Instance.IVACameraActiveKerbal;

			if (kerbal != null && evaPossible && HighLogic.CurrentGame.Parameters.Flight.CanEVA)
			{
				FlightEVA.SpawnEVA(kerbal);
				CameraManager.Instance.SetCameraFlight();
			}
		}

		public IEnumerator UpdateHatchTransform()
		{
			while (m_grabbedHand)
			{
				float newAngle = GetCurrentAngle();
				float rotation = Mathf.Clamp(newAngle - m_grabbedAngle, 0.0f, maxRotation);
				m_hatchTransform.localRotation = Quaternion.AngleAxis(rotation, rotationAxis) * initialRotation;

				if (rotation == maxRotation)
				{
					GoEVA();
					yield break;
				}

				yield return null;
			}

			// TODO: interpolate back to neutral
			m_hatchTransform.localRotation = initialRotation;
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
