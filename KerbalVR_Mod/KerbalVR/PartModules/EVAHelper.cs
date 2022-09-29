using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.PartModules
{
	public class VREVAHelper : PartModule
	{
		KerbalEVA m_eva;
		InteractableBehaviour m_interactableBehaviour;

		void Start()
		{
			m_eva = GetComponent<KerbalEVA>();
			LampChanged();
			JetpackChanged();

			m_interactableBehaviour = m_eva.ladderCollider.gameObject.AddComponent<InteractableBehaviour>();

			m_interactableBehaviour.OnGrab += OnGrabbed;
			m_interactableBehaviour.enabled = !vessel.isActiveVessel;

			GameEvents.onVesselChange.Add(OnVesselChange);
		}

		void OnDestroy()
		{
			GameEvents.onVesselChange.Remove(OnVesselChange);
		}

		private void OnVesselChange(Vessel activeVessel)
		{
			m_interactableBehaviour.enabled = activeVessel != vessel;
		}

		private void OnGrabbed(Hand hand)
		{
			SwitchTo();
		}

		[KSPEvent(guiActive = true)]
		void ToggleLamp()
		{
			m_eva.ToggleLamp();
			LampChanged();
		}

		void LampChanged()
		{
			// really this should be a postfix or something..
			base.Events["ToggleLamp"].guiName = m_eva.lampOn ? "Deactivate Lamp" : "Activate Lamp";
		}

		[KSPEvent(guiActive = true)]
		void ToggleJetpack()
		{
			m_eva.ToggleJetpack();
			JetpackChanged();
		}

		void JetpackChanged()
		{
			base.Events["ToggleJetpack"].guiName = m_eva.JetpackDeployed ? "Deactivate Jetpack" : "Activate Jetpack";
		}

		[KSPEvent(guiActive = false, guiActiveUnfocused = true, unfocusedRange = 2000)]
		void SwitchTo()
		{
			FlightGlobals.SetActiveVessel(vessel);
			Scene.EnterFirstPerson();
		}
	}
}
