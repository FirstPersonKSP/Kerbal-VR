using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace KerbalVR
{
	/// <summary>
	/// The Hand component is applied to each of the two hand GameObjects.
	/// It handles all the interactions related to using the hands in VR.
	/// </summary>
	public class Hand : MonoBehaviour
	{

		#region Public Members
		/// <summary>
		/// Either the LeftHand or RightHand for this object.
		/// </summary>
		public SteamVR_Input_Sources handType = SteamVR_Input_Sources.RightHand;

		/// <summary>
		/// The SteamVR_Input action for the hand pose data.
		/// </summary>
		public SteamVR_Action_Pose handActionPose;

		/// <summary>
		/// The other hand's GameObject
		/// </summary>
		public Hand otherHand;

		public HandProfileManager.Profile IVAProfile;
		public HandProfileManager.Profile EVAProfile;
		public HandProfileManager.Profile CurrentProfile => UseIVAProfile ? IVAProfile : EVAProfile;

		#endregion


		#region Private Members

		private bool isRightHand;

		// hand game objects
		public GameObject handObject;
		public GameObject IVAObject;
		public GameObject EVAObject;

		public GameObject CurrentHandObject => UseIVAProfile ? IVAObject : EVAObject;

		private bool useIVAProfile = false;
		public bool UseIVAProfile
		{
			get => useIVAProfile;
			set
			{
				useIVAProfile = value;
				SwitchProfile();
			}
		}

		public Vector3 GripPosition
		{
			get
			{
				// the grip collider can now be attached to some bone other than the hand root
				// when the hand is attached to an object, the colliders on the hand aren't moving, so we need to use the transform from the tracking object instead
				// find the collider center relative to the hand object root, then apply that offset to the tracked hand
				Vector3 relativeGripLocation = handObject.transform.InverseTransformPoint(palmCollider.transform.position);
				return transform.TransformPoint(relativeGripLocation);
			}
		}

		public Vector3 FingertipPosition => fingertipTransform.position;
		public float FingertipRadius => fingertipCollider.collider.radius;
		public bool FingertipEnabled
		{
			get => fingertipCollider.InteractionsEnabled;
			set => fingertipCollider.InteractionsEnabled = value;
		}

		protected SkinnedMeshRenderer handRenderer;
		public SteamVR_Behaviour_Skeleton handSkeleton { get; protected set; }

		// keep tracking of render state
		protected Types.ShiftRegister<bool> isRenderingHands = new Types.ShiftRegister<bool>(2);
		protected Types.ShiftRegister<int> renderLayerHands = new Types.ShiftRegister<int>(2);

		// keep track of held objects
		protected Transform palmTransform;
		protected HandCollider palmCollider;
		protected InteractableBehaviour heldObject;

		// interacting with mouse-clickable objects
		protected Transform fingertipTransform;
		protected FingertipCollider fingertipCollider;

		// interacting with pinchable objects
		protected Transform thumbTransform;
		protected Transform pinchTransform;
		protected PinchCollider pinchCollider;

		protected SkinnedMeshRenderer renderModel;

		protected VRLadder ladder;

		// For interacting with the UI
		internal VRUIHand UIHand { get; private set; }

		#endregion

		/// <summary>
		/// Creates the render models for the hands; sets up behavior scripts on the hands,
		/// including colliders for interacting with objects.
		/// </summary>
		/// <param name="handType"></param>
		/// <param name="otherHand"></param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="MissingReferenceException"></exception>
		/// <exception cref="Exception"></exception>
		public void Initialize(SteamVR_Input_Sources handType, Hand otherHand)
		{
			this.handType = handType;
			this.otherHand = otherHand;

			// verify members are set correctly
			if (handType != SteamVR_Input_Sources.RightHand && handType != SteamVR_Input_Sources.LeftHand)
			{
				throw new ArgumentException("handType must be LeftHand or RightHand");
			}
			isRightHand = handType == SteamVR_Input_Sources.RightHand;

			if (!Core.IsVrEnabled)
			{
				enabled = false;
				return;
			}

			handActionPose = SteamVR_Input.GetPoseAction("default", "Pose");

			// load hand profile
			HandProfileManager handSetting = HandProfileManager.Instance;

			#region Setup tracking skeleton

			string trackingSkeletonPrefabName = isRightHand ?
												handSetting.skeletonPrefabNameRight :
												handSetting.skeletonPrefabNameLeft;
			GameObject trackingSkeleton = AssetLoader.Instance.GetGameObject(trackingSkeletonPrefabName);

			if (trackingSkeleton == null)
			{
				throw new MissingReferenceException($"Could not load tracking skeleton '{trackingSkeletonPrefabName}'");
			}

			handObject = Instantiate(trackingSkeleton, Vector3.zero, Quaternion.identity, transform);
			GameObject.DontDestroyOnLoad(handObject);
			handObject.name = "KVR_HandObject_" + handType;
			handObject.SetActive(false); // default to inactive, to match the default in Update
			// add behavior scripts to the hands
			handSkeleton = handObject.AddComponent<SteamVR_Behaviour_Skeleton>();
			handSkeleton.skeletonRoot = handObject.transform.Find(handSetting.skeletonRootPath);
			handSkeleton.inputSource = handType;
			handSkeleton.rangeOfMotion = handSetting.fullRangeOfMotion ? EVRSkeletalMotionRange.WithoutController : EVRSkeletalMotionRange.WithController;
			handSkeleton.mirroring = isRightHand ? SteamVR_Behaviour_Skeleton.MirrorType.None : SteamVR_Behaviour_Skeleton.MirrorType.RightToLeft;
			handSkeleton.updatePose = false;
			string skeletonActionName = isRightHand ? "SkeletonRightHand" : "SkeletonLeftHand";
			handSkeleton.skeletonAction = SteamVR_Input.GetSkeletonAction("default", skeletonActionName, false);
			handSkeleton.fallbackCurlAction = SteamVR_Input.GetSingleAction("default", "Squeeze", false);

			// add fallback pose scripts
			GameObject handFallback = new GameObject("fallback");
			handFallback.transform.parent = handObject.transform;
			SteamVR_Skeleton_Poser handPoser = handFallback.AddComponent<SteamVR_Skeleton_Poser>();
			handPoser.skeletonMainPose = SkeletonPose_FallbackRelaxedPose.GetInstance();
			handPoser.Initialize();
			handSkeleton.fallbackPoser = handPoser;
			handSkeleton.Initialize();

			// add tracking
			SteamVR_Behaviour_Pose pose = gameObject.AddComponent<SteamVR_Behaviour_Pose>();
			pose.inputSource = handType;

			// set up actions
			try
			{
				SteamVR_Action_Boolean actionGrab = SteamVR_Actions.default_GrabGrip;
				actionGrab[handType].onChange += OnChangeGrab;
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			try
			{
				var seatInteract = SteamVR_Actions.flight_SeatInteraction;
				seatInteract[handType].onStateDown += SeatInteract_OnStateDown;
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			#endregion

			#region Setup hand model

			IVAProfile = handSetting.GetProfile(true);
			SetupModel(IVAProfile, ref IVAObject);

			EVAProfile = handSetting.GetProfile(false);
			SetupModel(EVAProfile, ref EVAObject);

			#endregion

			#region Setup other stuffs

			// set up ladder
			ladder = gameObject.AddComponent<VRLadder>();

			// set up UI hand  
			UIHand = gameObject.AddComponent<VRUIHand>();

			#endregion

			#region Setup Collider

			// add fingertip collider for "mouse clicks"
			fingertipTransform = new GameObject("KVR_FingertipCollider").transform;
			fingertipTransform.SetParent(handObject.transform); // This will be modified in SwitchProfile
			fingertipCollider = fingertipTransform.gameObject.AddComponent<FingertipCollider>();
			fingertipCollider.Initialize(this);

			// this has to be after the fingertip collider is initialized and before the hand collider is initialized (for ladder setup)
			Detach(true);

			// create a child object for the colider so that it can be on a different layer
			palmTransform = new GameObject("KVR_PalmCollider").transform;
			palmTransform.SetParent(handObject.transform); // This will be modified in SwitchProfile
			palmCollider = palmTransform.gameObject.AddComponent<HandCollider>();
			palmCollider.Initialize(this, ladder);

			// thumb is used to calculate position of pinch collider
			thumbTransform = new GameObject("KVR_Thumb").transform;
			thumbTransform.SetParent(handObject.transform); // This will be modified in SwitchProfile

			// set up pinch behavior
			pinchTransform = new GameObject("KVR_PinchCollider").transform;
			pinchTransform.SetParent(handObject.transform); // This will be modified in SwitchProfile
			pinchCollider = pinchTransform.gameObject.AddComponent<PinchCollider>();
			pinchCollider.Initialize(this);

			#endregion
			
			#region Setup skeleton helper

			KerbalSkeletonHelper ivaSkeletonHelper = IVAObject.AddComponent<KerbalSkeletonHelper>();
			ivaSkeletonHelper.Initialize(IVAProfile, handSkeleton.skeletonRoot, IVAObject.transform.Find(IVAProfile.skeletonRootTransformPath));

			KerbalSkeletonHelper evaSkeletonHelper = EVAObject.AddComponent<KerbalSkeletonHelper>();
			evaSkeletonHelper.Initialize(EVAProfile, handSkeleton.skeletonRoot, EVAObject.transform.Find(EVAProfile.skeletonRootTransformPath));

			#endregion

			UseIVAProfile = false;

#if HAND_GIZMOS
			var g = Utils.CreateGizmo(0.05f);
			g.transform.SetParent(handObject.transform);
			var g2 = Utils.CreateGizmo(0.2f);
			g2.transform.SetParent(transform);
#endif
		}

		public void OnDestroy()
		{
			Utils.LogWarning("Hand being destroyed");
		}

		private void SetupModel(HandProfileManager.Profile profile, ref GameObject profileObject)
		{
			string prefabName = isRightHand ? profile.PrefabNameRight : profile.PrefabNameLeft;
			GameObject prefab = AssetLoader.Instance.GetGameObject(prefabName);
			if (prefab == null)
			{
				throw new MissingReferenceException($"Could not load prefab: {prefabName}");
			}
			profileObject = Instantiate(prefab, Vector3.zero, Quaternion.identity, handObject.transform);
			profileObject.transform.localScale = Vector3.one * profile.scale;
		}

		public void Attach(Transform parent)
		{
			var scaleObject = new GameObject().transform;
			scaleObject.transform.SetParent(parent, false);
			scaleObject.localScale = parent.lossyScale.Reciprocal();


			handObject.transform.SetParent(scaleObject, true);
			
			FingertipEnabled = false;

			var detacher = parent.gameObject.AddComponent<HandDetacher>();
			detacher.hand = this;
		}

		public void Detach(bool immediate = false)
		{
			if (handObject == null) return;

			var handDetacher = handObject.GetComponentsInParent<HandDetacher>(true).FirstOrDefault();
			if (handDetacher)
			{
				Destroy(handDetacher);
			}

			if (handObject.transform.parent != transform)
			{
				var scaleObject = handObject.transform.parent;

				if (immediate)
				{
					handSkeleton.skeletonBlend = 1.0f;
					handSkeleton.UpdateSkeletonTransforms();
				}
				else
				{
					handSkeleton.BlendToSkeleton();
				}
				handObject.transform.SetParent(transform, false);
				handObject.transform.localPosition = Vector3.zero;
				handObject.transform.localRotation = Quaternion.identity;
				handObject.transform.localScale = Vector3.one;

				Destroy(scaleObject.gameObject);
			}

			FingertipEnabled = true;
		}

		/// <summary>
		/// Handle grabbing Interactable objects
		/// </summary>
		/// <param name="fromAction">SteamVR action that triggered this callback</param>
		/// <param name="fromSource">Hand type that triggered this callback</param>
		/// <param name="newState">New state of this action</param>
		protected void OnChangeGrab(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
		{
			if (newState)
			{
				if (palmCollider.HoveredObject != null && palmCollider.HoveredObject.isActiveAndEnabled)
				{
					heldObject = palmCollider.HoveredObject;
					heldObject.GrabbedHand = this;

					// the grab callbacks might end up destroying or disabling the object
					if (!heldObject.isActiveAndEnabled)
					{
						palmCollider.ClearHoveredObject();
					}
					else if (heldObject != null)
					{
						if (heldObject.SkeletonPoser != null)
						{
							handSkeleton.BlendToPoser(heldObject.SkeletonPoser);
						}
						Attach(heldObject.transform);
					}
				}
			}
			else
			{
				if (heldObject != null)
				{
					heldObject.GrabbedHand = null;
					heldObject = null;
					Detach();
				}
			}
		}

		private void SeatInteract_OnStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			if (Scene.IsInIVA())
			{
				if (palmCollider.HoveredSeat != null)
				{
					palmCollider.HoveredSeat.OnInteract(this);
				}
				else if (FreeIva.KerbalIvaAddon.Instance.buckled)
				{
					FreeIva.KerbalIvaAddon.Instance.Unbuckle();
				}
			}
		}

		public bool IsFingerTrackingPinching()
		{
			// if we're running partial tracking, activate pinch whenever the fingertips are close together
			if (handSkeleton.skeletalTrackingLevel >= EVRSkeletalTrackingLevel.VRSkeletalTracking_Partial && handSkeleton.skeletonBlend >= 1.0f)
			{
				var fingertipDistance = Vector3.Distance(handSkeleton.indexTip.position, handSkeleton.thumbTip.position);

				if (fingertipDistance <= CurrentProfile.pinchColliderSize * 4.0f)
				{
					return true;
				}
			}

			return false;
		}

		protected void Update()
		{
			// should we render the hands in the current scene?
			bool isRendering = KerbalVR.Core.IsVrRunning;

			// if rendering, update the hand positions
			if (isRendering)
			{
				// get device indices for each hand, then set the transform
				bool isConnected = handActionPose.GetDeviceIsConnected(handType);
				if (isConnected)
				{
					UpdateCollider();
#if false
					// keep this object (Hand script) always tracking the device
					SteamVR_Utils.RigidTransform handTransform = new SteamVR_Utils.RigidTransform(KerbalVR.Core.GamePoses[deviceIndex].mDeviceToAbsoluteTracking);
					Vector3 handTransformPos = KerbalVR.Scene.Instance.DevicePoseToWorld(handTransform.pos);
					Quaternion handTransformRot = KerbalVR.Scene.Instance.DevicePoseToWorld(handTransform.rot);
					this.transform.position = handTransformPos;
					this.transform.rotation = handTransformRot;

					// determine if the rendered hand object needs to track device, or the interacting object
					if (heldObject != null && heldObject.SkeletonPoser != null) {
						// this equation looks messy because of the way the SteamVR_Skeleton_Pose_Hand object
						// records the skeleton offset position/rotation. it's like a negative offset, relative
						// to the object we are interacting with. i think it boils down to this:
						//
						// handTransform + skeletonOffset = heldTransform
						//
						// so, we are solving for `handTransform`, taking into account the rotations of the various frames of reference.
						//
						Quaternion skeletonRotInv = (handType == SteamVR_Input_Sources.LeftHand) ?
							Quaternion.Inverse(heldObject.SkeletonPoser.skeletonMainPose.leftHand.rotation) :
							Quaternion.Inverse(heldObject.SkeletonPoser.skeletonMainPose.rightHand.rotation);
						Vector3 skeletonPos = (handType == SteamVR_Input_Sources.LeftHand) ?
							heldObject.SkeletonPoser.skeletonMainPose.leftHand.position :
							heldObject.SkeletonPoser.skeletonMainPose.rightHand.position;

						handObject.transform.position = heldObject.transform.position - heldObject.transform.rotation * skeletonRotInv * skeletonPos;
						handObject.transform.rotation = heldObject.transform.rotation * skeletonRotInv;
					} else {
						handObject.transform.position = handTransformPos;
						handObject.transform.rotation = handTransformRot;
					}
#endif
				}
				else
				{
					isRendering = false;
				}
			}

			// makes changes as necessary
			isRenderingHands.Push(isRendering);
			if (isRenderingHands.IsChanged())
			{
				handObject.SetActive(isRendering);

			}
		}

		private void UpdateCollider()
		{
			// update position of the pinch transform to the middle between the tip of the index finger and the tip of the thumb
			pinchTransform.position = Vector3.Lerp(fingertipTransform.position, thumbTransform.position, 0.5f);
		}

		private void SwitchProfile()
		{
			if (!Core.IsVrEnabled) return;

			Utils.SetLayer(gameObject, UseIVAProfile ? 20 : 0);
			palmTransform.gameObject.layer = 3; //  UseIVAProfile ? 20 : 3; // layer 3 interacts with everything, and we want to be able to grab both layer 16 and layer 20 in IVA

			IVAObject.SetActive(UseIVAProfile);
			EVAObject.SetActive(!UseIVAProfile);

			// update collider size
			fingertipCollider.collider.radius = CurrentProfile.fingertipColliderSize;
			palmCollider.collider.radius = CurrentProfile.palmColliderSize;
			pinchCollider.collider.radius = CurrentProfile.pinchColliderSize;

			// re-parent index tip collider
			fingertipTransform.SetParent(CurrentHandObject.transform.Find(CurrentProfile.indexTipTransformPath));
			fingertipTransform.localRotation = Quaternion.identity;
			fingertipTransform.localPosition = CurrentProfile.fingertipOffset;

			// re-parent thumb tip collider
			thumbTransform.SetParent(CurrentHandObject.transform.Find(CurrentProfile.thumbTipTransformPath));
			thumbTransform.localRotation = Quaternion.identity;
			thumbTransform.localPosition = Vector3.zero;

			// re-parent palm collider
			palmTransform.SetParent(CurrentHandObject.transform.Find(CurrentProfile.gripTransformPath));
			palmTransform.localRotation = Quaternion.identity;
			palmTransform.localPosition = CurrentProfile.gripOffset;

			// update render model
			renderModel = CurrentHandObject.transform.Find(CurrentProfile.renderModelPath).gameObject.GetComponent<SkinnedMeshRenderer>();
		}
	}

	class HandDetacher : MonoBehaviour
	{
		public Hand hand;

		public void OnDestroy()
		{
			if (hand)
			{
				hand.Detach();
			}
		}
	}
}
