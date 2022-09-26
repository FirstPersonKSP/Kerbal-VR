using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

namespace KerbalVR
{
    /// <summary>
    /// InteractionSystem is a singleton class that encapsulates
    /// the code that manages interaction systems via SteamVR_Input,
    /// i.e. the interaction game objects (hands), and input actions.
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        #region Singleton
        /// <summary>
        /// This is a singleton class, and there must be exactly one GameObject with this Component in the scene.
        /// </summary>
        private static InteractionSystem _instance;
        public static InteractionSystem Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<InteractionSystem>();
                    if (_instance == null) {
                        Utils.LogError("The scene needs to have one active GameObject with an InteractionSystem script attached!");
                    } else {
                        _instance.Initialize();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// One-time initialization for this singleton class.
        /// </summary>
        private void Initialize() {
            // load glove prefab assets
            glovePrefabL = AssetLoader.Instance.GetGameObject("vr_glove_left_model_slim");
            if (glovePrefabL == null) {
                Utils.LogWarning("Could not load prefab: vr_glove_left_model_slim");
                return;
            }
            glovePrefabR = AssetLoader.Instance.GetGameObject("vr_glove_right_model_slim");
            if (glovePrefabR == null) {
                Utils.LogWarning("Could not load prefab: vr_glove_right_model_slim");
                return;
            }

			InitializeHandScripts();
			EventSystem.current.gameObject.AddComponent<VRLaserInputModule>();
			EventSystem.current.gameObject.AddComponent<VRFingerTipInputModule>();
		}
        #endregion

        private void OnDestroy()
        {
            Utils.LogError("Interaction System being destroyed");
        }

        #region Properties
        public Hand LeftHand { get; private set; }
        public Hand RightHand { get; private set; }
        //public GameObject HeadUpDisplay { get; private set; }
        #endregion


        #region Private Members
        // hand game objects
        protected GameObject glovePrefabL;
        protected GameObject glovePrefabR;

        // head up display
        //protected KerbalVR.HeadUpDisplay hud;

        // device behaviors and actions
        protected bool isHandsInitialized = false;
        protected SteamVR_Action_Pose handActionPose;
        #endregion


        protected void InitializeHandScripts() {
            // store actions for these devices
            handActionPose = SteamVR_Input.GetPoseAction("default", "Pose");

            // set up the hand objects
            var lhGameObject = new GameObject("KVR_HandL");
            lhGameObject.transform.SetParent(transform, false);
            DontDestroyOnLoad(lhGameObject);
            LeftHand = lhGameObject.AddComponent<KerbalVR.Hand>();
            LeftHand.handPrefab = glovePrefabL;
            LeftHand.handType = SteamVR_Input_Sources.LeftHand;
            LeftHand.handActionPose = handActionPose;

            var rhGameObject = new GameObject("KVR_HandR");
            rhGameObject.transform.SetParent(transform, false);
            DontDestroyOnLoad(rhGameObject);
            RightHand = rhGameObject.AddComponent<KerbalVR.Hand>();
            RightHand.handPrefab = glovePrefabR;
            RightHand.handType = SteamVR_Input_Sources.RightHand;
            RightHand.handActionPose = handActionPose;

            RightHand.otherHand = LeftHand;
            LeftHand.otherHand = RightHand;

            // can init the skeleton behavior now
            LeftHand.Initialize();
            RightHand.Initialize();

            // init the head up display
            //HeadUpDisplay = new GameObject("KVR_HeadUpDisplay");
            //DontDestroyOnLoad(HeadUpDisplay);
            //hud = HeadUpDisplay.AddComponent<KerbalVR.HeadUpDisplay>();
            //hud.Initialize();
        }

    } // class InteractionSystem

	// The interaction system is sometimes a child of a part (e.g. when in a command chair), in which case the CollisionManager will try to disable collisions between the hands and other colliders on the vessel
	[HarmonyPatch(typeof(CollisionManager), nameof(CollisionManager.GetAllVesselColliders))]
	class GetAllVesselColliders_Patch
	{
		public static void Prefix(CollisionManager __instance, ref Transform __state)
		{
			if (InteractionSystem.Instance.gameObject.GetComponentUpwards<Part>() != null)
			{
				__state = InteractionSystem.Instance.transform.parent;
				InteractionSystem.Instance.transform.SetParent(null, false);
			}
		}

		public static void Postfix(CollisionManager __instance, Transform __state)
		{
			if (__state != null)
			{
				InteractionSystem.Instance.transform.SetParent(__state, false);
			}
		}
	}
} // namespace KerbalVR
