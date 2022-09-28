using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using static Valve.VR.SteamVR_Events;

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
		public SteamVR_Input_Sources handType;

		/// <summary>
		/// The SteamVR_Input action for the hand pose data.
		/// </summary>
		public SteamVR_Action_Pose handActionPose;

		/// <summary>
		/// The other hand's GameObject
		/// </summary>
		public Hand otherHand;

		/// <summary>
		/// Hand profile
		/// </summary>
		public HandProfileManager.Profile profile;

		#endregion


		#region Private Members
		// hand game objects
		public GameObject handObject;
		public Vector3 GripOffset = new Vector3(0, 0, -0.1f);
		public Vector3 GripPosition
		{
			get { return transform.TransformPoint(GripOffset); }
		}

		public Vector3 FingertipPosition
		{
			get { return fingertipCollider.FingertipCenter; }
		}

		public float FingertipRadius => fingertipCollider.FingertipRadius;
		public bool FingertipEnabled
		{
			get { return fingertipCollider.InteractionsEnabled; }
			set { fingertipCollider.InteractionsEnabled = value; }
		}

		protected SkinnedMeshRenderer handRenderer;
		protected SteamVR_Behaviour_Skeleton handSkeleton;

		// keep tracking of render state
		protected Types.ShiftRegister<bool> isRenderingHands = new Types.ShiftRegister<bool>(2);
		protected Types.ShiftRegister<int> renderLayerHands = new Types.ShiftRegister<int>(2);

		// keep track of held objects
		protected Transform handTransform;
		protected HandCollider handCollider;
		protected InteractableBehaviour heldObject;

		// interacting with mouse-clickable objects
		protected Transform fingertipTransform;
		protected FingertipCollider fingertipCollider;

		// interacting with pinchable objects
		protected Transform thumbTransform;
		protected Transform pinchTransform;
		protected PinchCollider pinchCollider;

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
		public void Initialize(SteamVR_Input_Sources handType, Hand otherHand, bool isIVA)
		{
			handActionPose = SteamVR_Input.GetPoseAction("default", "Pose");
			this.handType = handType;
			this.otherHand = otherHand;

			// verify members are set correctly
			if (handType != SteamVR_Input_Sources.RightHand && handType != SteamVR_Input_Sources.LeftHand)
			{
				throw new ArgumentException("handType must be LeftHand or RightHand");
			}
			bool isRightHand = handType == SteamVR_Input_Sources.RightHand;

			// load hand profile
			profile = HandProfileManager.Instance.GetProfile(isIVA);

			GripOffset = profile.gripOffset;

			// load hand prefab assets
			string prefabName = isRightHand ? profile.PrefabNameRight : profile.PrefabNameLeft;
			GameObject handPrefab = AssetLoader.Instance.GetGameObject(prefabName);
			if (handPrefab == null)
			{
				throw new MissingReferenceException($"Could not load prefab: {prefabName}");
			}

			// make instance object out of the hand prefab
			handObject = Instantiate(handPrefab);
			if (handObject == null)
			{
				throw new Exception("Could not Instantiate prefab for " + handType);
			}
			handObject.name = "KVR_HandObject_" + handType;
			DontDestroyOnLoad(handObject);
			handObject.SetActive(false); // default to inactive, to match the default in Update

			// cache the hand renderers
			handRenderer = handObject.transform.Find(profile.renderModelPath).gameObject.GetComponent<SkinnedMeshRenderer>();
			Utils.SetLayer(handObject, 0);

			// add behavior scripts to the hands
			handSkeleton = handObject.AddComponent<SteamVR_Behaviour_Skeleton>();
			handSkeleton.skeletonRoot = handObject.transform.Find(profile.sourceSkeletonRootPath);
			handSkeleton.inputSource = handType;
			handSkeleton.rangeOfMotion = HandProfileManager.Instance.fullRangeOfMotion ? EVRSkeletalMotionRange.WithoutController : EVRSkeletalMotionRange.WithController;
			handSkeleton.mirroring = isRightHand ? SteamVR_Behaviour_Skeleton.MirrorType.None : SteamVR_Behaviour_Skeleton.MirrorType.RightToLeft;
			handSkeleton.updatePose = false;
			string skeletonActionName = (handType == SteamVR_Input_Sources.LeftHand) ? "SkeletonLeftHand" : "SkeletonRightHand";
			handSkeleton.skeletonAction = SteamVR_Input.GetSkeletonAction("default", skeletonActionName, false);
			handSkeleton.fallbackCurlAction = SteamVR_Input.GetSingleAction("default", "Squeeze", false);

			// add fallback pose scripts
			Transform handFallback = handObject.transform.Find("fallback");
			SteamVR_Skeleton_Poser handPoser = handFallback.gameObject.AddComponent<SteamVR_Skeleton_Poser>();
			handPoser.skeletonMainPose = SkeletonPose_FallbackRelaxedPose.GetInstance();
			handPoser.Initialize();
			handSkeleton.fallbackPoser = handPoser;
			handSkeleton.Initialize();

			// add tracking
			SteamVR_Behaviour_Pose pose = gameObject.AddComponent<SteamVR_Behaviour_Pose>();
			pose.inputSource = handType;

			// set up actions
			SteamVR_Action_Boolean actionGrab = SteamVR_Input.GetBooleanAction("default", "GrabGrip");
			actionGrab[handType].onChange += OnChangeGrab;

			// set up skeleton helper
			if (profile.useSkeletonHelper)
			{
				KerbalSkeletonHelper kerbalSkeletonHelper = handObject.AddComponent<KerbalSkeletonHelper>();
				kerbalSkeletonHelper.retargetableSetting = profile.retargetableSetting;
				kerbalSkeletonHelper.sourceSkeletonRoot = handSkeleton.skeletonRoot;
				kerbalSkeletonHelper.destinationSkeletonRoot = handObject.transform.Find(profile.retargetableSetting.destinationSkeletonRootPath);
			}

			// set up ladder
			ladder = gameObject.AddComponent<VRLadder>();

			// set up UI hand  
			UIHand = gameObject.AddComponent<VRUIHand>();

			#region Setup Colliders

			// add fingertip collider for "mouse clicks"
			fingertipTransform = handObject.transform.Find(profile.indexTipTransformPath);
			fingertipTransform.localPosition += profile.fingertipOffset;
			fingertipCollider = fingertipTransform.gameObject.AddComponent<FingertipCollider>();
			fingertipCollider.Initialize(this);

			// this has to be after the fingertip collider is initialized and before the hand collider is initialized (for ladder setup)
			Detach();

			// create a child object for the colider so that it can be on a different layer
			handTransform = new GameObject("handTransform").transform;
			handTransform.SetParent(handObject.transform.Find(profile.gripTransformPath), false);
			handTransform.rotation = handObject.transform.rotation;
			handTransform.localPosition = GripOffset;
			handCollider = handTransform.gameObject.AddComponent<HandCollider>();
			handCollider.Initialize(this);

			// thumb is used to calculate position of pinch collider
			thumbTransform = handObject.transform.Find(profile.thumbTipTransformPath);

			// set up pinch behavior
			pinchTransform = new GameObject("pinchTransform").transform;
			pinchTransform.SetParent(handObject.transform, false);
			pinchCollider = pinchTransform.gameObject.AddComponent<PinchCollider>();
			pinchCollider.Initialize(this);

			#endregion
		}

		public void Attach(Transform parent)
		{
			handObject.transform.SetParent(parent, true);
			Vector3 scale = handObject.transform.parent.lossyScale.Reciprocal();
			scale.Scale(transform.lossyScale);
			handObject.transform.localScale = scale;
			FingertipEnabled = false;

			var detacher = parent.gameObject.AddComponent<HandDetacher>();
			detacher.hand = this;
		}

		public void Detach()
		{
			var handDetacher = handObject.GetComponentInParent<HandDetacher>();
			if (handDetacher)
			{
				Component.Destroy(handDetacher);
			}

			handSkeleton.BlendToSkeleton();
			handObject.transform.SetParent(transform, false);
			handObject.transform.localPosition = Vector3.zero;
			handObject.transform.localRotation = Quaternion.identity;
			handObject.transform.localScale = Vector3.one;
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
			ChangeGrab(newState);
		}

		public void ChangeGrab(bool newState)
		{
			if (newState)
			{
				if (handCollider.HoveredObject != null)
				{
					heldObject = handCollider.HoveredObject;
					heldObject.GrabbedHand = this;
					if (heldObject.SkeletonPoser != null)
					{
						handSkeleton.BlendToPoser(heldObject.SkeletonPoser);
					}
					Attach(heldObject.transform);
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

		protected void Update()
		{
			// should we render the hands in the current scene?
			bool isRendering = KerbalVR.Core.IsVrRunning;
			if (isRendering)
			{
				int renderLayer = 0;

				switch (HighLogic.LoadedScene)
				{
					case GameScenes.FLIGHT:

						if (KerbalVR.Scene.IsInIVA())
						{
							// IVA-specific settings
							renderLayer = 20;
						}
						break;
				}

				renderLayerHands.Push(renderLayer);
			}

			// if rendering, update the hand positions
			if (isRendering)
			{
				// get device indices for each hand, then set the transform
				bool isConnected = handActionPose.GetDeviceIsConnected(handType);
				if (isConnected)
				{
					// update position of the pinch transform to the middle between the tip of the index finger and the tip of the thumb
					pinchTransform.position = Vector3.Lerp(fingertipTransform.position, thumbTransform.position, 0.5f);
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
			if (renderLayerHands.IsChanged())
			{
				Utils.SetLayer(this.gameObject, renderLayerHands.Value);
				Utils.SetLayer(handObject, renderLayerHands.Value);

				handTransform.gameObject.layer = renderLayerHands.Value == 20 ? 20 : 3;
			}
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
