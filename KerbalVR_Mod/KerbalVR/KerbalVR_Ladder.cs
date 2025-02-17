﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR
{
	static class UnityObjectExtensions
	{
		// if an object reference isn't actually null but pointing to a destroyed object, then obj?.name will throw a NullReferenceException.  Use this instead.
		public static string SafeName(this UnityEngine.Object obj)
		{
			return obj == null ? null : obj.name;
		}
	}

	// This component gets added to each hand, and represents the ladder that hand is grabbing (both IVA and EVA)
	public class VRLadder : InteractableBehaviour
	{
		public Transform LadderTransform;

		Vector3 m_grabbedPosition;
		Vector3 velocity;

		public static readonly string COLLIDER_TAG = "Ladder";

		void Start()
		{
			SkeletonPoser = Utils.GetOrAddComponent<SteamVR_Skeleton_Poser>(gameObject);
			SkeletonPoser.skeletonMainPose = SkeletonPose_HandleRailGrabPose.GetInstance();
			SkeletonPoser.Initialize();

			OnGrab += OnGrabbed;
			OnRelease += OnReleased;
		}

		private void OnReleased(Hand hand)
		{
			Utils.Log($"VRLadder.OnReleased: {hand.handType} - LadderTransform {LadderTransform.SafeName()}");

			if (hand.otherHand.heldObject is VRLadder otherLadder && otherLadder.GrabbedHand == hand.otherHand)
			{
				Utils.Log($"VRLadder.OnReleased: other hand is holding ladder {otherLadder.LadderTransform?.name}");
				return;
			}

			var kerbalEVA = FlightGlobals.ActiveVessel.evaController;

			if (kerbalEVA != null)
			{
				var evafsm = kerbalEVA.GetComponent<KerbalVR_EVAFSM>();
				// TODO: we probably don't want to let go on the first release after grabbing the ladder,
				// but that state should probably also be in some kind of EVA controller class
				// and have this class just send it notifications
				if (kerbalEVA.OnALadder)
				{
					//kerbalEVA.fsm.RunEvent(kerbalEVA.On_ladderLetGo);
				}

				kerbalEVA.fsm.RunEvent(evafsm.m_vrReleaseLadderEvent);
				kerbalEVA.vessel.gravityMultiplier = 1;
				kerbalEVA._rigidbody.detectCollisions = true;
			}
			else
			{
				FreeIva.KerbalIvaAddon.Instance.KerbalIva.FreezeUpdates = false;
				FreeIva.KerbalIvaAddon.Instance.KerbalIva.KerbalRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				FreeIva.KerbalIvaAddon.Instance.KerbalIva.KerbalRigidbody.velocity = velocity;
				FreeIva.KerbalIvaAddon.Instance.KerbalIva.KerbalRigidbody.WakeUp();
				velocity = Vector3.zero;
			}
		}

		private void OnGrabbed(Hand hand)
		{
			m_grabbedPosition = LadderTransform.InverseTransformPoint(hand.GripPosition);
			Utils.Log($"VRLadder.OnGrabbed: {hand.handType} - LadderTransform {LadderTransform.SafeName()}; LadderTransform {LadderTransform.position}; grabbedPosition {m_grabbedPosition}");
			HapticUtils.Heavy(hand.handType);

			// VRLadder is weird because the interactable is on the hand, not the ladder - so the normal OnOtherHandGrab even won't be used
			// if the other hand was already holding a ladder, detach it because only one hand can control the kerbal's motion at once (for now)
			if (hand.otherHand.heldObject is VRLadder otherLadder)
			{
				otherLadder.GrabbedHand = null;
			}

			var kerbalEVA = FlightGlobals.ActiveVessel.evaController;

			if (kerbalEVA != null)
			{
				var ladderTriggers = AccessTools.FieldRefAccess<KerbalEVA, List<Collider>>(kerbalEVA, "currentLadderTriggers");
				var evafsm = kerbalEVA.GetComponent<KerbalVR_EVAFSM>();

				ladderTriggers.Clear();
				ladderTriggers.Add(LadderTransform.GetComponent<Collider>());

				//kerbalEVA.fsm.RunEvent(kerbalEVA.On_ladderGrabStart);
				kerbalEVA.fsm.RunEvent(evafsm.m_vrGrabLadderEvent);
			}
			else if (FreeIva.KerbalIvaAddon.Instance.buckled)
			{
				FreeIva.KerbalIvaAddon.Instance.Unbuckle();
				FreeIva.KerbalIvaAddon.Instance.KerbalIva.FreezeUpdates = true;
			}
		}

		static float x_gain = 10f;
		static float x_maxOffset = 0.1f;

		void FixedUpdate()
		{
			if (IsGrabbed)
			{
				var kerbalEVA = FlightGlobals.ActiveVessel.evaController;

				if (LadderTransform == null)
				{
					GrabbedHand = null;
					return;
				}

				Vector3 offset = GrabbedHand.GripPosition - LadderTransform.TransformPoint(m_grabbedPosition);
				Rigidbody rigidBody;

				if (kerbalEVA != null)
				{
					rigidBody = kerbalEVA._rigidbody;

					var ladderRigidBody = LadderTransform.GetComponent<Collider>()?.attachedRigidbody;

					Vector3 Vtgt = (ladderRigidBody 
						? (ladderRigidBody.velocity + Vector3.Cross(ladderRigidBody.angularVelocity, rigidBody.worldCenterOfMass - ladderRigidBody.worldCenterOfMass)) 
						: Vector3.zero);

					rigidBody.velocity = -offset / Time.fixedDeltaTime + Vtgt;

					// todo: orientation?
				}
				else
				{
					float offsetMag = offset.magnitude;
					if (offsetMag > x_maxOffset)
					{
						offset = offset.normalized * x_maxOffset;
					}

					rigidBody = FreeIva.KerbalIvaAddon.Instance.KerbalIva.KerbalRigidbody;
					//rigidBody.MovePosition(rigidBody.position - offset);
					// rigidBody.AddForce(-offset / Time.fixedDeltaTime, ForceMode.VelocityChange);
					rigidBody.velocity = -offset / Time.fixedDeltaTime;

					// Debug.Log($"KerbalVR ladder: Offset {offset.magnitude}; Floating Origin: {FloatingOrigin.Offset.magnitude}; thisFrame: {FloatingOrigin.fetch.SetOffsetThisFrame}");
				}

				velocity = (x_gain * -offset / Time.fixedDeltaTime + velocity) / (1 + x_gain);
			}
		}
	}
}
