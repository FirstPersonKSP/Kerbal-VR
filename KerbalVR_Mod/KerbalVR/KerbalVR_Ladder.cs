using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR
{
	// This component gets added to each hand, and represents the ladder that hand is grabbing
	public class VRLadder : InteractableBehaviour
	{
		public Transform LadderTransform;

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
			if (!FlightGlobals.ActiveVessel.isEVA) return;

			var kerbalEVA = FlightGlobals.ActiveVessel.evaController;

			// TODO: we probably don't want to let go on the first release after grabbing the ladder,
			// but that state should probably also be in some kind of EVA controller class
			// and have this class just send it notifications
			if (kerbalEVA.OnALadder)
			{
				kerbalEVA.fsm.RunEvent(kerbalEVA.On_ladderLetGo);
			}
		}

		private void OnGrabbed(Hand hand)
		{
			if (!FlightGlobals.ActiveVessel.isEVA) return;

			var kerbalEVA = FlightGlobals.ActiveVessel.evaController;

			if (!kerbalEVA.OnALadder)
			{
				var ladderTriggers = AccessTools.FieldRefAccess<KerbalEVA, List<Collider>>(kerbalEVA, "currentLadderTriggers");

				ladderTriggers.Clear();
				ladderTriggers.Add(LadderTransform.GetComponent<Collider>());

				kerbalEVA.fsm.RunEvent(kerbalEVA.On_ladderGrabStart);
			}
		}
	}
}
