using KSP.UI.Screens.DebugToolbar.Screens.Cheats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR
{
	internal struct PushUtil
	{
		public readonly Transform Transform;

		readonly float MinTranslation;
		readonly float MaxTranslation;
		readonly Vector3 PushAxis;
		readonly float InitialPosition;

		float m_currentPosition; // unclamped, relative to initial
		float m_previousGrabPosition;

		public PushUtil(Transform transform, Vector3 localPushAxis, float minTranslation, float maxTranslation) : this()
		{
			m_currentPosition = Mathf.Clamp(0.0f, minTranslation, maxTranslation);
			Transform = transform;
			MinTranslation = minTranslation;
			MaxTranslation = maxTranslation;
			PushAxis = transform.TransformDirection(localPushAxis);
			InitialPosition = GetAxisPosition(transform.position);
		}

		public void Update(Vector3 gripPosition)
		{
			float newPosition = GetAxisPosition(gripPosition);
			float delta = newPosition - m_previousGrabPosition;
			m_currentPosition += delta;
			m_previousGrabPosition = newPosition;

			SetTransformToCurrent();
		}

		float GetAxisPosition(Vector3 gripPosition)
		{
			return Vector3.Dot(PushAxis, gripPosition) - InitialPosition;
		}

		public void Grabbed(Vector3 gripPosition)
		{
			m_previousGrabPosition = GetAxisPosition(gripPosition);
		}

		public void Reset()
		{
			m_currentPosition = 0.0f;
			SetTransformToCurrent();
		}

		public bool IsAtMax()
		{
			return m_currentPosition >= MaxTranslation;
		}

		public bool IsAtMin()
		{
			return m_currentPosition <= MinTranslation;
		}

		public bool IsAtZero()
		{
			return m_currentPosition == 0.0f;
		}

		public float GetInterpolatedPosition()
		{
			return Mathf.Clamp01(Mathf.InverseLerp(MinTranslation, MaxTranslation, m_currentPosition));
		}

		public void SetInterpolatedPosition(float normalizedValue)
		{
			m_currentPosition = Mathf.Lerp(MinTranslation, MaxTranslation, normalizedValue);
			SetTransformToCurrent();
		}

		void SetTransformToCurrent()
		{
			Transform.position = Vector3.Exclude(PushAxis, Transform.position) + (InitialPosition + Mathf.Clamp(m_currentPosition, MinTranslation, MaxTranslation)) * PushAxis;
		}
	}
}
