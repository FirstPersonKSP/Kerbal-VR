using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.InternalModules
{
    class VRKnob : InternalModule
    {
        [KSPField]
        public string knobTransformName = null;

        [KSPField]
        public Vector3 rotationAxis = Vector3.up;

        [KSPField]
        public Vector3 pointerAxis = Vector3.right;

        VRKnobInteractionListener interactionListener;
        internal float currentAngle = 0;

#if PROP_GIZMOS
        GameObject gizmo;
        GameObject arrow;
#endif

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
    }

    class VRKnobInteractionListener : MonoBehaviour, IPinchInteractable
    {
        public VRKnob knobModule;

        public GameObject GameObject => gameObject;

        public void OnHold(Hand hand)
        {
            SetAngle(knobModule.currentAngle + 0.5f);
        }

        public void OnPinch(Hand hand)
        {
            
        }

        public void OnRelease(Hand hand)
        {
            SetAngle(0);
        }

        void SetAngle(float angle)
        {
            knobModule.currentAngle = angle;
            transform.localRotation = Quaternion.AngleAxis(knobModule.currentAngle, knobModule.rotationAxis);
        }
    }
}
