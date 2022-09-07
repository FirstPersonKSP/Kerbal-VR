using Mono.Cecil;
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
        /// Values below this percentage of total angle will be snapped to minAngle
        /// </summary>
        [KSPField]
        public float snapOpenThreshold = 0.3f;

        /// <summary>
        /// Values above this percentage of total angle will be snapped to maxAngle
        /// </summary>
        [KSPField]
        public float snapClosedThreshold = 0.1f;

        VRCoverInteractionListener interactionListener = null;
        internal float currentAngle = 0;
        internal float snappedAngle = 0;
        internal Transform hingeTransform = null;
        internal float snapOnAngle = 0;
        internal float snapOffAngle = 0;

        #region Event

        public delegate void CoverEventHandler();

        /// <summary>
        /// Triggers when cover opens
        /// </summary>
        public event CoverEventHandler OnCoverOpen;

        /// <summary>
        /// Triggers when cover closed
        /// </summary>
        public event CoverEventHandler OnCoverClose;

        #endregion

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

            float totalTravel = (minAngle - maxAngle);
            snapOnAngle = totalTravel * (1 - snapOpenThreshold);
            snapOffAngle = totalTravel * snapOpenThreshold;
        }

        internal void CoverOpen()
        {
            // play a snap sound here?

            if (OnCoverOpen != null)
            {
                OnCoverOpen.Invoke();
            }
        }

        internal void CoverClose()
        {
            // play a snap sound here?

            if (OnCoverClose != null)
            {
                OnCoverClose.Invoke();
            }
        }

        /// <summary>
        /// Set the state of the cover
        /// </summary>
        /// <param name="open"><see langword="true"/> to set cover open, <see langword="false"/> otherwise</param>
        public void SetState(bool open)
        {
            currentAngle = open ? minAngle : maxAngle;
            snappedAngle = currentAngle;
            hingeTransform.localRotation = Quaternion.AngleAxis(currentAngle, rotationAxis);
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

            public void OnEnter(Hand hand, Collider buttonCollider)
            {
                m_contactedAngle = GetFingertipAngle(hand.FingertipPosition);

                HapticUtils.Light(hand.handType);

                enabled = true;
            }

            public void OnExit(Hand hand, Collider buttonCollider)
            {
                // align current angle with visual
                if (coverModule.currentAngle < coverModule.snapOnAngle || coverModule.currentAngle > coverModule.snapOffAngle)
                {
                    coverModule.currentAngle = coverModule.snappedAngle;
                }
            }

            public void OnStay(Hand hand, Collider buttonCollider)
            {
                float newAngle = GetFingertipAngle(hand.FingertipPosition);
                float delta = newAngle - m_contactedAngle;

                if (delta > 180) delta -= 360;
                if (delta < -180) delta += 360;

                float angle = coverModule.currentAngle + delta;

                // set angle
                coverModule.currentAngle = Mathf.Clamp(angle, coverModule.minAngle, coverModule.maxAngle);

                if (coverModule.currentAngle < coverModule.snapOnAngle)
                {
                    if (coverModule.snappedAngle != coverModule.minAngle)
                    {
                        coverModule.snappedAngle = coverModule.minAngle;
                        coverModule.CoverOpen();
                        HapticUtils.Snap(hand.handType);
                    }
                }
                else if (coverModule.currentAngle > coverModule.snapOffAngle)
                {
                    if (coverModule.snappedAngle != coverModule.maxAngle)
                    {
                        coverModule.snappedAngle = coverModule.maxAngle;
                        coverModule.CoverClose();
                        HapticUtils.Snap(hand.handType);
                    }
                }
                else
                {
                    coverModule.snappedAngle = coverModule.currentAngle;
                }

                coverModule.hingeTransform.localRotation = Quaternion.AngleAxis(coverModule.snappedAngle, coverModule.rotationAxis);
            }
        }
    }
}