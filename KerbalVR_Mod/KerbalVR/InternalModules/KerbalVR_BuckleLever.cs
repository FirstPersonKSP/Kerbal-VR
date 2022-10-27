using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace KerbalVR.InternalModules
{
	internal class VRBuckleLever : InternalModule
	{
		[KSPField]
		public string leverTransformName = string.Empty;

		[KSPField]
		public Vector3 rotationAxis = Vector3.up;

		[KSPField]
		public float minAngle;
		
		[KSPField]
		public float maxAngle;


		InteractableBehaviour interactable;
		RotationUtil rotationUtil;
		bool latched;

		FreeIva.PropBuckleButton button;

		public void Start()
		{
			var leverTransform = this.FindTransform(leverTransformName);
			if (leverTransform == null) return;

			var collider = leverTransform.GetComponentInChildren<Collider>();

			rotationUtil = new RotationUtil(leverTransform, rotationAxis, minAngle, maxAngle);

			interactable = Utils.GetOrAddComponent<InteractableBehaviour>(collider.gameObject);

			interactable.SkeletonPoser = Utils.GetOrAddComponent<SteamVR_Skeleton_Poser>(rotationUtil.Transform.gameObject);
			interactable.SkeletonPoser.skeletonMainPose = SkeletonPose_HandleRailGrabPose.GetInstance();
			interactable.SkeletonPoser.Initialize();

			interactable.OnGrab += OnGrab;

			button = GetComponent<FreeIva.PropBuckleButton>();
		}

		private void OnGrab(Hand hand)
		{
			rotationUtil.Grabbed(hand.GripPosition);
			HapticUtils.Heavy(hand.handType);
			latched = false;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (interactable != null && interactable.IsGrabbed)
			{
				rotationUtil.Update(interactable.GrabbedHand.GripPosition);

				if (rotationUtil.IsAtMax() && !latched)
				{
					latched = true;
					button.OnClick();
				}
			}
			else
			{
				rotationUtil.SetInterpolatedPosition(0);
			}
		}
	}
}
