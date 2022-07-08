using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	class VRThrottleLever : InternalModule
	{
		[KSPField]
		public string leverName = "throttleLever";

		[KSPField]
		public float angleMin = -75.0f;

		[KSPField]
		public float angleMax = 75.0f;

		[KSPField]
		public Vector3 axis = Vector3.right;

		InteractableBehaviour interactable;
		RotationUtil m_rotationUtil;

#if PROP_GIZMOS
		GameObject gizmo;
		GameObject arrow;
#endif

		public void Start()
		{
			// the stock module only supports transforms as a child of this game object
			// try to find a relative path here
			var leverTransform = this.FindTransform(leverName);

			if (leverTransform == null) return;

			// TODO: add generic collider internal module?

			var collider = leverTransform.GetComponentInChildren<Collider>();

			if (collider == null)
			{
				var capsuleCollider = Utils.GetOrAddComponent<CapsuleCollider>(leverTransform.gameObject);

				capsuleCollider.radius = 0.02f;
				capsuleCollider.center = Vector3.up * 0.02f;
				capsuleCollider.height = 0.07f;

				collider = capsuleCollider;
			}

			m_rotationUtil = new RotationUtil(leverTransform, axis, angleMin, angleMax);

			interactable = Utils.GetOrAddComponent<InteractableBehaviour>(collider.gameObject);

			interactable.SkeletonPoser = Utils.GetOrAddComponent<SteamVR_Skeleton_Poser>(m_rotationUtil.Transform.gameObject);
			interactable.SkeletonPoser.skeletonMainPose = SkeletonPose_HandleRailGrabPose.GetInstance();
			interactable.SkeletonPoser.Initialize();

#if PROP_GIZMOS
			if (gizmo == null)
			{
				gizmo = Utils.CreateGizmo();
				gizmo.transform.SetParent(leverTransform.parent, false);
				Utils.SetLayer(gizmo, 20);
				arrow = Utils.CreateArrow(Color.cyan, 0.2f);
				arrow.transform.SetParent(leverTransform.parent, false);
				arrow.transform.localRotation = Quaternion.LookRotation(axis);
				Utils.SetLayer(arrow, 20);

				Utils.GetOrAddComponent<ColliderVisualizer>(leverTransform.gameObject);
			}
#endif

			interactable.OnGrab += OnGrab;
		}

		private void OnGrab(Hand hand)
		{
			m_rotationUtil.Grabbed(hand.GripPosition);
		}

		public override void OnUpdate()
		{
			if (interactable != null && interactable.IsGrabbed)
			{
				m_rotationUtil.Update(interactable.GrabbedHand.GripPosition);

				FlightInputHandler.state.mainThrottle = m_rotationUtil.GetInterpolatedPosition();
			}
			else
			{
				m_rotationUtil.SetInterpolatedPosition(FlightInputHandler.state.mainThrottle);
			}

			base.OnUpdate();
		}
	}
}
