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

		Quaternion m_initialRotation;
		Transform m_hatchTransform;
		InteractableBehaviour m_interactableBehaviour;
		Hand m_grabbedHand;
		float m_currentRotation; // accumulated rotation
		Vector3 m_previousGrabDirection;

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

			m_initialRotation = m_hatchTransform.localRotation;
		}

		Vector3 GetCurrentGrabDirection()
		{
			Vector3 direction = m_grabbedHand.GripPosition - m_hatchTransform.position;
			direction = m_hatchTransform.InverseTransformDirection(direction);

			direction = Vector3.ProjectOnPlane(direction, rotationAxis);

			return direction;
		}

		IEnumerator GoEVA()
		{
			float acLevel = ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.AstronautComplex);
			bool evaUnlocked = GameVariables.Instance.UnlockedEVA(acLevel);
			bool evaPossible = GameVariables.Instance.EVAIsPossible(evaUnlocked, vessel);

			Kerbal kerbal = CameraManager.Instance.IVACameraActiveKerbal;

			if (kerbal != null && evaPossible && HighLogic.CurrentGame.Parameters.Flight.CanEVA)
			{
				var kerbalEVA = FlightEVA.SpawnEVA(kerbal);
				CameraManager.Instance.SetCameraFlight();

				yield return null;

				// wait for kerbal to be ready
				while (FlightGlobals.ActiveVessel == null || FlightGlobals.ActiveVessel.packed ||
					FlightGlobals.ActiveVessel.evaController != gameObject.GetComponent<KerbalEVA>())
				{
					yield return null;
				}

				yield return null;

				var fpCameraManager = FirstPerson.FirstPersonEVA.instance.fpCameraManager;
				fpCameraManager.isFirstPerson = false;
				fpCameraManager.saveCameraState(FlightCamera.fetch);
				fpCameraManager.CheckAndSetFirstPerson(FlightGlobals.ActiveVessel);
			}
		}

		public IEnumerator UpdateHatchTransform()
		{
			while (m_grabbedHand)
			{
				Vector3 newGrabDirection = GetCurrentGrabDirection();
				float deltaAngle = Vector3.SignedAngle(m_previousGrabDirection, newGrabDirection, rotationAxis);
				
				m_currentRotation = Mathf.Clamp(m_currentRotation + deltaAngle, 0.0f, maxRotation);
				m_hatchTransform.localRotation = m_initialRotation * Quaternion.AngleAxis(m_currentRotation, rotationAxis);

				m_previousGrabDirection = GetCurrentGrabDirection();

				if (m_currentRotation == maxRotation)
				{
					yield return StartCoroutine(GoEVA());
					yield break;
				}

				yield return null;
			}

			// TODO: interpolate back to neutral
			m_hatchTransform.localRotation = m_initialRotation;
		}

		private void OnRelease(Hand hand)
		{
			m_grabbedHand = null;
			m_currentRotation = 0.0f; // TODO: interpolate back
			m_hatchTransform.localRotation = m_initialRotation;
		}

		private void OnGrab(Hand hand)
		{
			m_grabbedHand = hand;
			m_previousGrabDirection = GetCurrentGrabDirection();
			StartCoroutine(UpdateHatchTransform());
		}
	}
}
