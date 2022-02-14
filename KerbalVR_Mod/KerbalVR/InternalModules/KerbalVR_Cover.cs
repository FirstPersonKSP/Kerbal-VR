using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
    class VRCover : InternalModule
    {
        [KSPField]
        public string coverTransformName = null;

        [KSPField]
        public Vector3 rotationAxis = Vector3.right;

        [KSPField]
        public Vector3 zeroVector = Vector3.up;

        [KSPField]
        public float minAngle = -110;

        [KSPField]
        public float maxAngle = 0;

        VRCoverInteractionListener interactionListener = null;
        internal float currentAngle = 0;

        public bool IsOpen
        {
            get { return currentAngle == minAngle; }
        }

        public override void OnAwake()
        {
            base.OnAwake();

            var coverTransform = internalProp.FindModelTransform(coverTransformName);

            if (coverTransform != null && interactionListener == null)
            {
                interactionListener = coverTransform.gameObject.AddComponent<VRCoverInteractionListener>();
                interactionListener.coverModule = this;

                currentAngle = maxAngle;
            }
        }
    }

    class VRCoverInteractionListener : MonoBehaviour, IFingertipInteractable
    {
        public VRCover coverModule;

        float m_contactedAngle;

        // the pushbutton cover's collider is on a child of the transform that actually forms the hinge
        // In the future we may want to data-drive this
        Transform HingeTransform {  get { return transform.parent; } }

        float GetFingertipAngle(Vector3 fingertipCenter)
        {
            Vector3 vec = fingertipCenter - HingeTransform.position;
            vec = HingeTransform.parent.InverseTransformDirection(vec);
            vec -= vec * Vector3.Dot(vec, coverModule.rotationAxis);
            vec = Vector3.Normalize(vec);

            Vector3 yaxis = Vector3.Cross(coverModule.rotationAxis, coverModule.zeroVector);

            float x = Vector3.Dot(vec, coverModule.zeroVector);
            float y = Vector3.Dot(vec, yaxis);

            return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        }

        public void OnEnter(Hand hand, Collider buttonCollider, SteamVR_Input_Sources inputSource)
        {
            m_contactedAngle = GetFingertipAngle(hand.FingertipPosition);

            enabled = true;
        }

        public void OnExit(Hand hand, Collider buttonCollider, SteamVR_Input_Sources inputSource)
        {
            
        }

        public void OnStay(Hand hand, Collider buttonCollider, SteamVR_Input_Sources inputSource)
        {
            float newAngle = GetFingertipAngle(hand.FingertipPosition);
            float delta = newAngle - m_contactedAngle;

            if (delta > 180) delta -= 360;
            if (delta < -180) delta += 360;

            float angle = coverModule.currentAngle + delta;
            float clampedAngle = Mathf.Clamp(angle, coverModule.minAngle, coverModule.maxAngle);

            /*
            if (angle != clampedAngle)
            {
                m_isMoving = false;

                bool newState = clampedAngle == switchModule.maxAngle;

                switchModule.m_ivaSwitch.SetState(newState);
            }
            */

            SetAngle(clampedAngle);
        }

        void SetAngle(float angle)
        {
            coverModule.currentAngle = angle;
            HingeTransform.localRotation = Quaternion.AngleAxis(coverModule.currentAngle, coverModule.rotationAxis);
        }
    }
}
