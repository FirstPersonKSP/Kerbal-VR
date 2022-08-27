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
        public string coverTransformName = "";

        [KSPField]
        public string hingeTransformName = "";

        [KSPField]
        public Vector3 rotationAxis = Vector3.right;

        [KSPField]
        public Vector3 zeroVector = Vector3.up;

        /// <summary>
        /// Fully opened Angle
        /// </summary>
        [KSPField]
        public float minAngle = -110;

        /// <summary>
        /// Fully Closed Angle
        /// </summary>
        [KSPField]
        public float maxAngle = 0;

		/// <summary>
		/// Values below this limit will be snapped to minAngle
		/// </summary>
		[KSPField]
		public float snappingLimit = -90;

		VRCoverInteractionListener interactionListener = null;
        internal float currentAngle = 0;
        internal float snappedAngle = 0;
        internal Transform hingeTransform = null;

        public bool IsOpen
        {
            get { return currentAngle == minAngle; }
        }

        static Transform FirstAncestorWithName(Transform transform, string name)
        {
            while (transform != null && transform.name != name)
            {
                transform = transform.parent;
            }

            return transform;
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
                snappedAngle = currentAngle;
			}

            if (hingeTransform == null && coverTransform != null)
            {
                if (hingeTransformName != "")
                {
                    hingeTransform = FirstAncestorWithName(coverTransform, hingeTransformName);
                }

                if (hingeTransform == null)
                {
                    hingeTransform = coverTransform;
                }
            }
        }
    }

    class VRCoverInteractionListener : MonoBehaviour, IFingertipInteractable
    {
        public VRCover coverModule;

        float m_contactedAngle;

        float GetFingertipAngle(Vector3 fingertipCenter)
        {
            Vector3 vec = fingertipCenter - coverModule.hingeTransform.position;
            vec = coverModule.hingeTransform.parent.InverseTransformDirection(vec);
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
			// align current angle with visual
			if (coverModule.currentAngle < coverModule.snappingLimit)
            {
                coverModule.currentAngle = coverModule.snappedAngle;
			}
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

			if (coverModule.currentAngle < coverModule.snappingLimit)
			{
				coverModule.snappedAngle = coverModule.minAngle;

                // play a snap sound here?
			}
			else
			{
				coverModule.snappedAngle = coverModule.currentAngle;
			}

			coverModule.hingeTransform.localRotation = Quaternion.AngleAxis(coverModule.snappedAngle, coverModule.rotationAxis);
        }
    }
}
