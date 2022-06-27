using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR
{
	public class VRLadder : PartModule
	{
		Transform m_ladderTransform;
		InteractableBehaviour m_interactableBehavior;

		void Start()
		{
			var colliders = part.FindModelComponents<Collider>();

			foreach (var collider in colliders)
			{
				if (collider.gameObject.activeInHierarchy && collider.enabled && collider.CompareTag("Ladder"))
				{
					// TODO: support more than one ladder per part (requires other refactoring)
					m_ladderTransform = collider.transform;
					m_interactableBehavior = Utils.GetOrAddComponent<InteractableBehaviour>(m_ladderTransform.gameObject);

					m_interactableBehavior.OnGrab += OnGrab;
					m_interactableBehavior.OnRelease += OnRelease;
					break;
				}
			}
		}

		private void OnRelease(Hand hand)
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

		private void OnGrab(Hand hand)
		{
			if (!FlightGlobals.ActiveVessel.isEVA) return;

			var kerbalEVA = FlightGlobals.ActiveVessel.evaController;

			if (!kerbalEVA.OnALadder)
			{
				var ladderTriggers = AccessTools.FieldRefAccess<KerbalEVA, List<Collider>>(kerbalEVA, "currentLadderTriggers");

				ladderTriggers.Clear();
				ladderTriggers.Add(m_ladderTransform.GetComponent<Collider>());

				kerbalEVA.fsm.RunEvent(kerbalEVA.On_ladderGrabStart);
			}
		}
	}
}
