using HarmonyLib;
using KSPAchievements;
using System;
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
		public static InteractionSystem Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<InteractionSystem>();
					if (_instance == null)
					{
						Utils.LogError("The scene needs to have one active GameObject with an InteractionSystem script attached!");
					}
					else
					{
						_instance.Initialize();
					}
				}
				return _instance;
			}
		}

		/// <summary>
		/// One-time initialization for this singleton class.
		/// </summary>
		private void Initialize()
		{
			InitializeHandScripts();

			GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
		}

		private void OnGameSceneLoadRequested(GameScenes data)
		{
			LeftHand.Detach(true);
			RightHand.Detach(true);
		}
		#endregion

		private void OnDestroy()
		{
			Utils.LogError("Interaction System being destroyed");
		}

		#region Properties

		private bool useIVAProfile = false;
		public bool UseIVAProfile
		{
			get => useIVAProfile;
			set
			{
				useIVAProfile = value;

				LeftHand.UseIVAProfile = useIVAProfile;
				RightHand.UseIVAProfile = useIVAProfile;
			}
		}

		public Hand LeftHand;
		public Hand RightHand;

		//public GameObject HeadUpDisplay { get; private set; }
		#endregion


		#region Private Members
		// head up display
		//protected KerbalVR.HeadUpDisplay hud;

		// device behaviors and actions
		protected bool isHandsInitialized = false;
		#endregion

		public Hand GetHand(SteamVR_Input_Sources handType)
		{
			switch (handType)
			{
				case SteamVR_Input_Sources.LeftHand: return LeftHand;
				case SteamVR_Input_Sources.RightHand: return RightHand;
				default: return null;
			}
		}

		private Hand SetupHand(string name)
		{
			GameObject handGameObject = new GameObject(name);
			DontDestroyOnLoad(handGameObject);
			handGameObject.transform.SetParent(transform, false);
			return handGameObject.AddComponent<Hand>();
		}

		protected void InitializeHandScripts()
		{
			HandProfileManager.Instance.LoadAllProfiles();

			// set up the hand objects
			LeftHand = SetupHand("KVR_HandL");
			RightHand = SetupHand("KVR_HandR");

			// can init the skeleton behavior now
			LeftHand.Initialize(SteamVR_Input_Sources.LeftHand, RightHand);
			RightHand.Initialize(SteamVR_Input_Sources.RightHand, LeftHand);

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
			if (Core.IsVrEnabled && InteractionSystem.Instance.gameObject.GetComponentUpwards<Part>() != null)
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

	[HarmonyPatch(typeof(FirstPerson.FirstPersonCameraManager), "enableRenderers")]
	class FirstPerson_enableRenderers_Patch
	{
		public static void Prefix(ref Transform __state)
		{
			if (Core.IsVrEnabled)
			{
				__state = InteractionSystem.Instance.transform.parent;
				InteractionSystem.Instance.transform.SetParent(null, false);
			}
		}

		public static void Postfix(Transform __state)
		{
			if (__state != null)
			{
				InteractionSystem.Instance.transform.SetParent(__state, false);
			}
		}
	}
} // namespace KerbalVR
