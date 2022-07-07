using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	class VRThrottleLever : InternalLeverThrottle
	{
		InteractableBehaviour interactable;

		bool initialized = false;
		RotationUtil m_rotationUtil;

#if PROP_GIZMOS
		GameObject gizmo;
		GameObject arrow;
#endif

		public override void OnAwake()
		{
			try
			{
				base.OnAwake();
			}
			catch (Exception)
			{
				// Utils.LogError(e);
			}

			if (HighLogic.LoadedScene == GameScenes.LOADING) return;

			// the stock module only supports transforms as a child of this game object
			// try to find a relative path here
			if (leverObject == null)
			{
				var leverTransform = transform.parent.Find("model").GetChild(0).Find(leverName);

				if (leverTransform == null) return;

				// TODO: add generic collider internal module?
				var collider = Utils.GetOrAddComponent<CapsuleCollider>(leverTransform.gameObject);
				leverObject = collider;

				collider.radius = 0.02f;
				collider.center = Vector3.up * 0.02f;
				collider.height = 0.07f;

				leverInitial = leverTransform.rotation;
			}
		}

		public void Start()
		{
			m_rotationUtil = new RotationUtil(leverObject.transform, base.axis, base.angleMin, base.angleMax);

			if (!initialized)
			{
				interactable = Utils.GetOrAddComponent<InteractableBehaviour>(m_rotationUtil.Transform.gameObject);

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

				initialized = true;
			}
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

			base.OnUpdate();
		}
	}
}
