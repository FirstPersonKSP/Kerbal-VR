using UnityEngine;
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
        protected SteamVR_Action_Boolean headsetOnAction;
        #endregion


        protected void InitializeHandScripts() {
            // store actions for these devices
            handActionPose = SteamVR_Input.GetPoseAction("default", "Pose");
            headsetOnAction = SteamVR_Input.GetBooleanAction("default", "HeadsetOnHead");
            headsetOnAction.onChange += OnChangeHeadsetOnAction;

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

            RightHand.otherHand = LeftHand.gameObject;
            LeftHand.otherHand = RightHand.gameObject;

            // can init the skeleton behavior now
            LeftHand.Initialize();
            RightHand.Initialize();

            // init the head up display
            //HeadUpDisplay = new GameObject("KVR_HeadUpDisplay");
            //DontDestroyOnLoad(HeadUpDisplay);
            //hud = HeadUpDisplay.AddComponent<KerbalVR.HeadUpDisplay>();
            //hud.Initialize();
        }

        /// <summary>
        /// Activate or deactivate VR when the headset is worn or not, respectively.
        /// </summary
        /// <param name="fromAction">The HeadsetOnHead action</param>
        /// <param name="fromSource">The source for the event</param>
        /// <param name="newState">True if the headset is being worn by the user, false otherwise</param>
        protected void OnChangeHeadsetOnAction(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            KerbalVR.Core.IsVrEnabled = newState;
        }

    } // class InteractionSystem
} // namespace KerbalVR
