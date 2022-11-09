using KerbalVR.IVAAdaptors;
using System;
using System.Collections;
using UnityEngine;

namespace KerbalVR.InternalModules
{
	public class VRLever : InternalModule
	{
		#region CFG fields

		/// <summary>
		/// Name of the collider transform
		/// </summary>
		[KSPField]
		public string leverTransformName = null;

		/// <summary>
		/// Name of the rotation transform of the lever
		/// </summary>
		[KSPField]
		public string rotationTransformName;

		/// <summary>
		/// Axis of rotation of the rotation transform
		/// </summary>
		[KSPField]
		public Vector3 rotationAxis = Vector3.right;

		/// <summary>
		/// Range of rotation of the rotation transform<br/>
		/// <see cref="Vector2.x"/> is the lower limit<br/>
		/// <see cref="Vector2.y"/> is the upper limit<br/>
		/// </summary>
		[KSPField]
		public Vector2 rotationRange = Vector3.up;

		/// <summary>
		/// Does the lever need to be pulled out when moving
		/// </summary>
		[KSPField]
		public bool pullable = false;

		/// <summary>
		/// Name of the pull transform of the pull transform
		/// </summary>
		[KSPField]
		public string pullTransformName;

		/// <summary>
		/// Direction the pull transform will be pulled to
		/// </summary>
		[KSPField]
		public Vector3 pullDirection = Vector3.forward;

		/// <summary>
		/// Range of the movement of the pull transform<br/>
		/// <see cref="Vector2.x"/> is the normal position<br/>
		/// <see cref="Vector2.y"/> is the pulled position<br/>
		/// </summary>
		[KSPField]
		public Vector2 pullRange = Vector3.up;

		/// <summary>
		/// The time it takes for the lever transform to move in / out
		/// </summary>
		[KSPField]
		public float pullDuration = 0.1f;

		/// <summary>
		/// Number of steps of the lever
		/// </summary>
		[KSPField]
		public int stepCount = 2;

		/// <summary>
		/// Sound that will be played when step changes
		/// </summary>
		[KSPField]
		public string stepSound = string.Empty;

		/// <summary>
		/// What this lever control
		/// </summary>
		[KSPField]
		public string handler = string.Empty;

		#endregion

		[SerializeField] internal AudioSource m_audioSource;
		internal IVALever ivaLever;
		internal VRLeverInteractionListener interactionListener;
		internal Transform rotationTransform;
		internal Transform pullTransform;
		

		public int CurrentStep => interactionListener.CurrentStep;

#if PROP_GIZMOS
		GameObject topGizmo;
		GameObject rotationGizmo;
		GameObject pullGizmo;
#endif

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);

			if (HighLogic.LoadedScene == GameScenes.LOADING)
			{
				if (stepSound != string.Empty)
				{
					m_audioSource = Utils.CreateAudioSourceFromClip(internalProp.gameObject, stepSound);
				}
			}
		}

		public override void OnAwake()
		{
			base.OnAwake();

			rotationTransform = this.FindTransform(rotationTransformName);

			if (rotationTransform)
			{

#if PROP_GIZMOS
				if (!rotationGizmo)
				{
					rotationGizmo = Utils.CreateGizmo();
					rotationGizmo.transform.SetParent(rotationTransform, false);
					rotationGizmo.transform.localRotation = Quaternion.LookRotation(rotationAxis);
					Utils.SetLayer(rotationGizmo, 20);
				}
#endif
			}

			if (pullable)
			{
				pullTransform = this.FindTransform(pullTransformName);

				if (pullTransform)
				{

#if PROP_GIZMOS
					if (!pullGizmo)
					{
						pullGizmo = Utils.CreateGizmo();
						pullGizmo.transform.SetParent(pullTransform, false);
						pullGizmo.transform.localRotation = Quaternion.LookRotation(pullDirection);
						Utils.SetLayer(pullGizmo, 20);
					}
#endif
				}
			}

			Transform leverTransform = this.FindTransform(leverTransformName);

			if (leverTransform && !interactionListener)
			{
				interactionListener = Utils.GetOrAddComponent<VRLeverInteractionListener>(leverTransform.gameObject);
				interactionListener.Initialize(this);

#if PROP_GIZMOS
				if (!topGizmo)
				{
					topGizmo = Utils.CreateGizmo();
					topGizmo.transform.SetParent(leverTransform, false);
					Utils.SetLayer(topGizmo, 20);
				}
#endif
			}
		}

		public void Start()
		{
			stepCount = Math.Max(2, stepCount);
			ivaLever = IVALever.ConstructLever(this);
			interactionListener.OnStart();
		}

		public void OnDestroy()
		{
			ivaLever.OnDestroy();
		}

		internal void PlayStepSound()
		{
			if (m_audioSource != null)
			{
				m_audioSource.Play();
			}
		}
	}

	class VRLeverInteractionListener : MonoBehaviour, IPinchInteractable
	{
		VRLever leverModule;
		RotationUtil rotationUtil;
		Coroutine delayedUpdateEnableCoroutine;

		/// <summary>
		/// 0-based step id
		/// </summary>
		public int CurrentStep
		{
			get
			{
				float rotationFraction = rotationUtil.GetInterpolatedPosition();
				return Mathf.FloorToInt((leverModule.stepCount - 1) * rotationFraction + 0.5f);
			}
		}

		public GameObject GameObject => gameObject;

		public void Initialize(VRLever leverModule)
		{
			this.leverModule = leverModule;
			float currentRotation = Vector3.Scale(leverModule.rotationTransform.localEulerAngles, leverModule.rotationAxis).magnitude;
			rotationUtil = new RotationUtil(leverModule.rotationTransform,
											leverModule.rotationAxis,
											leverModule.rotationRange.x,
											leverModule.rotationRange.y,
											currentRotation);
			
		}

		public void OnStart()
		{
			RotateToCurrentState();
		}

		public void OnPinch(Hand hand)
		{
			if (leverModule.pullable) StartCoroutine(PullOutLever(false));
			leverModule.ivaLever.SetUpdateEnabled(false);

			RotateToCurrentState();
			rotationUtil.Grabbed(hand.GripPosition);

			HapticUtils.Light(hand.handType);
		}

		public void OnHold(Hand hand)
		{
			rotationUtil.Update(hand.GripPosition);
		}

		public void OnRelease(Hand hand)
		{
			if (leverModule.pullable) StartCoroutine(PullOutLever(true));

			leverModule.ivaLever.SetStep(CurrentStep);
			RotateToCurrentState();

			leverModule.PlayStepSound();
			HapticUtils.Snap(hand.handType);

			if (delayedUpdateEnableCoroutine != null)
			{
				StopCoroutine(delayedUpdateEnableCoroutine);
			}
			delayedUpdateEnableCoroutine = StartCoroutine(DelayedUpdateEnable());
		}

		public void RotateToCurrentState()
		{
			rotationUtil.SetInterpolatedPosition((float)leverModule.ivaLever.GetStep() / (leverModule.stepCount - 1));
		}

		IEnumerator DelayedUpdateEnable()
		{
			yield return new WaitForSeconds(1f); // wait for a bit so the animation won't play
			leverModule.ivaLever.SetUpdateEnabled(true);
		}

		IEnumerator PullOutLever(bool reversed)
		{
			Vector2 pullRange = leverModule.pullRange;

			Vector3 startPosition = leverModule.pullDirection * (reversed ? pullRange.y : pullRange.x);
			Vector3 endPosition = leverModule.pullDirection * (reversed ? pullRange.x : pullRange.y);

			float time = 0f;

			while (time < leverModule.pullDuration)
			{
				leverModule.pullTransform.localPosition = Vector3.Lerp(startPosition, endPosition, time / leverModule.pullDuration);
				time += Time.deltaTime;
				yield return null;
			}

			leverModule.pullTransform.localPosition = endPosition;
		}
	}
}
