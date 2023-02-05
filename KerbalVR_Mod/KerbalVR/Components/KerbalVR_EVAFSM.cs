using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR
{
	internal class KerbalVR_EVAFSM : MonoBehaviour
	{

		KFSMState m_vrOnLadderState;
		public KFSMEvent m_vrGrabLadderEvent;
		public KFSMEvent m_vrReleaseLadderEvent;

		void Awake()
		{
			var kerbalEVA = GetComponent<KerbalEVA>();

			m_vrOnLadderState = new KFSMState("VR_ladder");
			m_vrOnLadderState.OnEnter = (KFSMState s) =>
			{
				kerbalEVA.vessel.gravityMultiplier = 0;
				kerbalEVA._rigidbody.detectCollisions = false;
			};
			m_vrOnLadderState.OnLeave = (KFSMState s) => { kerbalEVA.vessel.gravityMultiplier = 1; };

			m_vrGrabLadderEvent = new KFSMEvent("VR Ladder Grab Start");
			m_vrGrabLadderEvent.GoToStateOnEvent = m_vrOnLadderState;

			m_vrReleaseLadderEvent = new KFSMEvent("VR Ladder Release");
			m_vrReleaseLadderEvent.GoToStateOnEvent = kerbalEVA.st_idle_fl;

			kerbalEVA.fsm.AddState(m_vrOnLadderState);

			// add the VR ladder grab event to the same states that the default ladder grab event is
			kerbalEVA.fsm.AddEvent(m_vrGrabLadderEvent, kerbalEVA.st_idle_fl, kerbalEVA.st_idle_gr, kerbalEVA.st_idle_b_gr, kerbalEVA.st_swim_idle);
			kerbalEVA.fsm.AddEvent(m_vrReleaseLadderEvent, m_vrOnLadderState);
		}
	}
}
