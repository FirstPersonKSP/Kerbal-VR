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

				KerbalVR.Scene.EnterFirstPerson();
			}
		}

		public IEnumerator UpdateHatchTransform()
		{
			while (m_grabbedHand)
			{
				m_rotationUtil.Update(m_grabbedHand.GripPosition);

				if (m_rotationUtil.IsAtMax())
				{
					yield return StartCoroutine(GoEVA());
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
			StartCoroutine(UpdateHatchTransform());
		}
	}
}
