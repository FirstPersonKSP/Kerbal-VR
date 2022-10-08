using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR
{
	internal struct RotationUtil
	{
		public readonly Transform Transform;

		readonly float MinRotation;
		readonly float MaxRotation;
		readonly Vector3 RotationAxis;
		readonly Quaternion InitialRotation;

		float m_currentRotation;
		Vector3 m_previousGrabDirection;

		public RotationUtil(Transform transform, Vector3 rotationAxis, float minRotation, float maxRotation) : this()
		{
			m_currentRotation = Mathf.Clamp(0.0f, minRotation, maxRotation);
			Transform = transform;
			MinRotation = minRotation;
			MaxRotation = maxRotation;
			RotationAxis = rotationAxis;
			InitialRotation = transform.localRotation;
		}

		public RotationUtil(Transform transform, Vector3 rotationAxis, float minRotation, float maxRotation, float currentRotation) 
			: this(transform, rotationAxis, minRotation, maxRotation)
		{
			m_currentRotation = Mathf.Clamp(currentRotation, minRotation, maxRotation);
			InitialRotation = transform.localRotation * Quaternion.AngleAxis((maxRotation - minRotation)/2f, RotationAxis);
		}


		Vector3 GetCurrentGrabDirection(Vector3 gripPosition)
		{
			Vector3 direction = gripPosition - Transform.position;
			direction = Transform.InverseTransformDirection(direction);

			direction = Vector3.ProjectOnPlane(direction, RotationAxis);

			return direction;
		}

		public void Update(Vector3 gripPosition)
		{
			Vector3 newGrabDirection = GetCurrentGrabDirection(gripPosition);
			float deltaAngle = Vector3.SignedAngle(m_previousGrabDirection, newGrabDirection, RotationAxis);

			m_currentRotation += deltaAngle;
			float effectiveRotation = Mathf.Clamp(m_currentRotation, MinRotation, MaxRotation);
			Transform.localRotation = InitialRotation * Quaternion.AngleAxis(effectiveRotation, RotationAxis);

			m_previousGrabDirection = GetCurrentGrabDirection(gripPosition);
		}

		public void Grabbed(Vector3 gripPosition)
		{
			m_previousGrabDirection = GetCurrentGrabDirection(gripPosition);
		}

		public void Reset()
		{
			Transform.localRotation = InitialRotation;
			m_currentRotation = 0.0f;
		}

		public bool IsAtMax()
		{
			return m_currentRotation >= MaxRotation;
		}

		public bool IsAtMin()
		{
			return m_currentRotation <= MinRotation;
		}

		public bool IsAtZero()
		{
			return m_currentRotation == 0.0f;
		}

		public float GetInterpolatedPosition()
		{
			return Mathf.Clamp01(Mathf.InverseLerp(MinRotation, MaxRotation, m_currentRotation));
		}

		public void SetInterpolatedPosition(float position)
		{
			m_currentRotation = Mathf.Lerp(MinRotation, MaxRotation, position);
			Transform.localRotation = InitialRotation * Quaternion.AngleAxis(m_currentRotation, RotationAxis);
		}
	}
}
