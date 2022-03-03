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

        VRKnobInteractionListener interactionListener;

        public override void OnAwake()
        {
            base.OnAwake();

            var knobTransform = internalProp.FindModelTransform(knobTransformName);

            if (knobTransform != null && interactionListener == null)
            {
                interactionListener = Utils.GetOrAddComponent<VRKnobInteractionListener>(knobTransform.gameObject);
                interactionListener.knobModule = this;
            }
        }
    }

    class VRKnobInteractionListener : MonoBehaviour, IPinchInteractable
    {
        public VRKnob knobModule;

        public GameObject GameObject => gameObject;
    }
}
