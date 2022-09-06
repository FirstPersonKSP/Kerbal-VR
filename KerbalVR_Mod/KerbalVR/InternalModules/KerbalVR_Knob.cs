using KerbalVR.IVAAdaptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.InternalModules
{
	public class VRKnobCustomRotation : ScriptableObject
	{
		[Persistent]
		public float minRotation;

		[Persistent]
		public float maxRotation;
	}

    /// <summary>
    /// The InternalModule for a knob that can be manipulated in VR
    /// </summary>
    public class VRKnob : InternalModule
    {
        [KSPField]
        public string knobTransformName = null;

        [KSPField]
        public Vector3 rotationAxis = Vector3.up;

        [KSPField]
        public Vector3 pointerAxis = Vector3.right;

        [KSPField]
        public int stepCount = 2;

        [KSPField]
        public string customRotationHandler = String.Empty;

		public VRKnobCustomRotation customRotation = null;

        VRKnobInteractionListener interactionListener;
        internal float currentAngle = 0;
        internal int lastStep = 0;

        internal IVAKnob m_ivaKnob;

#if PROP_GIZMOS
        GameObject gizmo;
        GameObject arrow;
#endif

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);

			var customRotationNode = node.GetNode("CUSTOMROTATION");
			
			if (customRotationNode != null)
			{
				customRotation = new VRKnobCustomRotation();
				ConfigNode.LoadObjectFromConfig(customRotation, customRotationNode);
			}
		}

		public override void OnAwake()
        {
            base.OnAwake();

            var knobTransform = internalProp.FindModelTransform(knobTransformName);

            if (knobTransform != null && interactionListener == null)
            {
                interactionListener = Utils.GetOrAddComponent<VRKnobInteractionListener>(knobTransform.gameObject);
                interactionListener.knobModule = this;

#if PROP_GIZMOS
                if (arrow == null)
                {
                    arrow = Utils.CreateArrow(Color.cyan, 0.2f);
                    arrow.transform.SetParent(knobTransform.parent, false);
                    arrow.transform.localRotation = Quaternion.LookRotation(rotationAxis);
                    Utils.SetLayer(arrow, 20);
                }

                if (gizmo == null)
                {
                    gizmo = Utils.CreateGizmo();
                    gizmo.transform.SetParent(knobTransform, false);
                    Utils.SetLayer(gizmo, 20);
                }
#endif
            }
        }

        public void Start()
        {
            m_ivaKnob = IVAKnob.ConstructKnob(gameObject, customRotation);
        }
    }

    /// <summary>
    /// The behavior class that gets attached to the movable knob itself and reacts to VR interactions
    /// </summary>
    class VRKnobInteractionListener : MonoBehaviour, IPinchInteractable
    {
        public VRKnob knobModule;

        public GameObject GameObject => gameObject;
        float m_grabbedAngle = 0;

        public void OnHold(Hand hand)
        {
            float newAngle = GetGrabbedAngle(hand);
            float delta = newAngle - m_grabbedAngle;

            if (delta > 180) delta -= 360;
            if (delta < -180) delta += 360;

            float angle = knobModule.currentAngle + delta;
            angle = Mathf.Clamp(angle, knobModule.m_ivaKnob.MinRotation, knobModule.m_ivaKnob.MaxRotation);

            // todo: change state, etc.

            SetAngle(angle);
            m_grabbedAngle = newAngle;

            CheckForStepChange();
        }

        public void OnPinch(Hand hand)
        {
            m_grabbedAngle = GetGrabbedAngle(hand);
            knobModule.m_ivaKnob.SetUpdateEnabled(false);
        }

        // reads from knobModule.currentAngle; returns rotation fraction
        float CheckForStepChange()
        {
            float interp = Mathf.InverseLerp(knobModule.m_ivaKnob.MinRotation, knobModule.m_ivaKnob.MaxRotation, knobModule.currentAngle);
            float rotationFraction = interp;

            if (knobModule.stepCount == 0)
            {
                knobModule.m_ivaKnob.SetRotationFraction(knobModule.customRotationHandler, interp);
            }
            else
            {
                float stepF = interp * (knobModule.stepCount - 1);
                int stepIndex = Mathf.RoundToInt(stepF);
                rotationFraction = stepIndex / (knobModule.stepCount - 1.0f);

                if (knobModule.lastStep != stepIndex)
                {
                    knobModule.lastStep = stepIndex;
                    knobModule.m_ivaKnob.SetRotationFraction(knobModule.customRotationHandler, rotationFraction);
                }
            }

            return rotationFraction;
        }

        public void OnRelease(Hand hand)
        {
            // SetAngle(0);
            // knobModule.m_ivaKnob.SetUpdateEnabled(true);

            float rotationFraction = CheckForStepChange();

            float angle = Mathf.Lerp(knobModule.m_ivaKnob.MinRotation, knobModule.m_ivaKnob.MaxRotation, rotationFraction);

            SetAngle(angle);
        }

        void SetAngle(float angle)
        {
            knobModule.currentAngle = angle;
            transform.localRotation = Quaternion.AngleAxis(knobModule.currentAngle, knobModule.rotationAxis);
        }

        float GetGrabbedAngle(Hand hand)
        {
            Vector3 perpVector = transform.parent.TransformDirection(knobModule.pointerAxis);
            Vector3 localPerpVector = hand.transform.InverseTransformDirection(perpVector);

            return Mathf.Atan2(localPerpVector.y, localPerpVector.x) * Mathf.Rad2Deg;
        }
    }
}
