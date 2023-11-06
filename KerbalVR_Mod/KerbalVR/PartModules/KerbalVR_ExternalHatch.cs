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

		public override void OnLoad(ConfigNode node)
		{
			if (HighLogic.LoadedScene != GameScenes.LOADING) return;

			base.OnLoad(node);
		}

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

			if (hatchTransform == null)
			{
				Utils.LogError($"Unable to find transform {hatchTransformName} on part {part.partInfo.name}");
				return;
			}

			m_rotationUtil = new RotationUtil(hatchTransform, rotationAxis, 0.0f, maxRotation);

			var collider = m_rotationUtil.Transform.GetComponentInChildren<Collider>(true);

			if (collider == null)
			{
				Utils.LogError($"No collider found on transform {hatchTransform} on part {part.partInfo.name}");
				return;
			}

			m_interactableBehaviour = Utils.GetOrAddComponent<InteractableBehaviour>(collider.gameObject);

			m_interactableBehaviour.SkeletonPoser = Utils.GetOrAddComponent<SteamVR_Skeleton_Poser>(collider.gameObject);
			m_interactableBehaviour.SkeletonPoser.skeletonMainPose = SkeletonPose_HandleRailGrabPose.GetInstance();
			m_interactableBehaviour.SkeletonPoser.Initialize();

			m_interactableBehaviour.OnGrab += OnGrab;
			m_interactableBehaviour.OnRelease += OnRelease;
		}

		InternalSeat FindSeat(string name)
		{
			foreach (var seat in part.internalModel.seats)
			{
				if (seat.seatTransformName == name)
				{
					return seat;
				}
			}

			Debug.LogError($"Seat {name} not found in internal space {part.internalModel.internalName} for hatch on part {part.name}");
			return null;
		}

		public IEnumerator UpdateHatchTransform()
		{
			while (m_grabbedHand)
			{
				m_rotationUtil.Update(m_grabbedHand.GripPosition);

				if (m_rotationUtil.IsAtMax())
				{
					m_grabbedHand.Detach(true);
					m_rotationUtil.Reset();

					var kerbalEVA = FlightGlobals.ActiveVessel.evaController;

					// find a nearby airlock collider to use
					if (kerbalEVA.currentAirlockTrigger == null)
					{
						Transform[] airlocks = part.FindModelTransformsWithTag(FreeIva.FreeIvaHatch.AIRLOCK_TAG);
						float closestDistance = float.MaxValue;
						Collider closestAirlock = null;

						foreach (var airlockTransform in airlocks)
						{
							Collider airlockCollider = airlockTransform.GetComponent<Collider>();
							if (airlockCollider != null)
							{
								Vector3 closestPoint = airlockCollider.ClosestPoint(m_rotationUtil.Transform.position);
								float distance = Vector3.Distance(closestPoint, m_rotationUtil.Transform.position);
								if (distance < closestDistance)
								{
									closestDistance = distance;
									closestAirlock = airlockCollider;
								}
							}
						}

						kerbalEVA.currentAirlockTrigger = closestAirlock;
						kerbalEVA.currentAirlockPart = part;
					}

					FreeIva.FreeIva.BoardPartFromAirlock(kerbalEVA, false);

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
			HapticUtils.Heavy(hand.handType);
			StartCoroutine(UpdateHatchTransform());
		}
	}
}
