﻿using HarmonyLib;
using KSP.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;

namespace KerbalVR
{
	internal class UISystem : MonoBehaviour
	{
		internal static UISystem Instance { get; private set; }
		void Awake()
		{
			GameEvents.onPartActionUIShown.Add(OnPartActionUIShown);
			GameEvents.onPartActionUIDismiss.Add(OnPartActionUIDismiss);
			Instance = this;
		}

		private void OnPartActionUIDismiss(Part data)
		{
			if (UIPartActionController.Instance.windows.Count == 0)
			{
				VRFingerTipInputModule.Instance.RemoveCanvas(UIMasterController.Instance.actionCanvas);
			}
		}

		static Quaternion canvasRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

		private void OnPartActionUIShown(UIPartActionWindow window, Part part)
		{
			if (Core.IsVrRunning && Scene.IsFirstPersonEVA())
			{
				window.gameObject.SetLayerRecursive(0);
				window.rectTransform.anchoredPosition3D = Vector3.zero;
				window.rectTransform.localPosition = Vector3.zero;
				window.GetComponentInChildren<Graphic>().material.shader = Shader.Find("UI/Default"); // this defaults to "UI/KSP/Color Overlay" which has z-write on, which causes z-fighting when rendered in worldspace

				VRFingerTipInputModule.Instance.PushCanvas(UIMasterController.Instance.actionCanvas);
			}
			else
			{
				window.gameObject.SetLayerRecursive(5);

				// for some reason, changing the shader back to this will break the rendering of the UI in non-vr mode
				//var graphic = window.GetComponentInChildren<Graphic>();
				//graphic.material.shader = Shader.Find("UI/KSP/Color Overlay");
			}
		}

		internal void VRRunningChanged(bool running)
		{
			running = running && !Scene.IsInIVA();

			ConfigureCanvas(UIMasterController.Instance.actionCanvas, running);
			ConfigureCanvas(UIMasterController.Instance.dialogCanvas, running);
			ConfigureCanvas(UIMasterController.Instance.screenMessageCanvas, running);

#if GUI_ENABLED

			UIMasterController.Instance.uiCamera.stereoTargetEye = running ? StereoTargetEyeMask.Both : StereoTargetEyeMask.None;
			UIMasterController.Instance.uiCamera.farClipPlane = running ? 2000.0f : 1100.0f;
			UIMasterController.Instance.uiCamera.nearClipPlane = running ? .03f : -300.0f;
			UIMasterController.Instance.uiCamera.orthographic = !running;

			ConfigureCanvas(UIMasterController.Instance.mainCanvas, running);
			ConfigureCanvas(UIMasterController.Instance.appCanvas, running);
#endif
			ConfigureHand(InteractionSystem.Instance.LeftHand, running);
			ConfigureHand(InteractionSystem.Instance.RightHand, running);

			var handheldCanvases = new Canvas[]
			{
				UIMasterController.Instance.actionCanvas,
				UIMasterController.Instance.dialogCanvas,
			};

			StartCoroutine(ConfigureHandheldCanvases(handheldCanvases, running));
			StartCoroutine(ConfigureHeadsUpCanvas(UIMasterController.Instance.screenMessageCanvas, running));
		}

		internal void ModeChanged()
		{
			VRRunningChanged(Core.IsVrRunning);
		}

		void LateUpdate()
		{
			bool anydialogsActive = false;
			for (int i = 0; i < UIMasterController.Instance.dialogCanvas.gameObject.transform.childCount; ++i)
			{
				if (UIMasterController.Instance.dialogCanvas.gameObject.transform.GetChild(i).gameObject.activeInHierarchy)
				{
					anydialogsActive = true;
					break;
				}
			}

			if (anydialogsActive)
			{
				VRFingerTipInputModule.Instance.PushCanvas(UIMasterController.Instance.dialogCanvas);
			}
			else
			{
				VRFingerTipInputModule.Instance.RemoveCanvas(UIMasterController.Instance.dialogCanvas);
			}
		}

		IEnumerator ConfigureHandheldCanvases(Canvas[] canvases, bool running)
		{
			yield return null; // wait a frame so that ThroughTheEyes knows whether we are in first person or not
			bool pdaMode = running && Scene.IsFirstPersonEVA();

			foreach (var canvas in canvases)
			{
				if (pdaMode)
				{
					canvas.transform.SetParent(InteractionSystem.Instance.LeftHand.handObject.transform, false);
					canvas.transform.localPosition = Vector3.zero;
					canvas.transform.localRotation = canvasRotation;
					canvas.transform.localScale = Vector3.one * 0.0005f; // arbitrary
					canvas.gameObject.layer = 0;
					canvas.worldCamera = FlightCamera.fetch.mainCamera;
				}
				else
				{
					canvas.transform.SetParent(UIMasterController.Instance.transform, false);
					canvas.transform.localPosition = new Vector3(0, 0, 500); // 500 is the value for the PAW, this might not be correct for other canvases
					canvas.transform.localRotation = Quaternion.identity;
					canvas.transform.localScale = Vector3.one;
					canvas.gameObject.layer = 5;
					canvas.worldCamera = UIMasterController.Instance.uiCamera;
				}
			}
		}

		static Quaternion hudRotation = Quaternion.identity;
		static Vector3 hudPosition = new Vector3(0, 0.0f, 0.3f);
		static float hudScale = 0.0003f;

		private IEnumerator ConfigureHeadsUpCanvas(Canvas canvas, bool running)
		{
			yield return null; // wait a frame so that ThroughTheEyes knows whether we are in first person or not
			bool hudMode= running && Scene.IsFirstPersonEVA();

			if (hudMode)
			{
				var eva = Scene.GetKerbalEVA();

				// TODO: this doesn't actually attach it to the skeleton, so it doesn't move with the helmet.  I tried attaching to the bone, but that didn't work.eva.helmetTransform
				canvas.transform.SetParent(eva.helmetTransform, false);
				canvas.transform.localPosition = hudPosition;
				canvas.transform.localRotation = hudRotation;
				canvas.transform.localScale = Vector3.one * hudScale;
				canvas.gameObject.layer = 0;
				canvas.worldCamera = FlightCamera.fetch.mainCamera;
			}
			else
			{
				canvas.transform.SetParent(UIMasterController.Instance.transform, false);
				canvas.transform.localPosition = new Vector3(0, 0, 500); // 500 is the value for the PAW, this might not be correct for other canvases
				canvas.transform.localRotation = Quaternion.identity;
				canvas.transform.localScale = Vector3.one;
				canvas.gameObject.layer = 5;
				canvas.worldCamera = UIMasterController.Instance.uiCamera;
			}
		}

		static void ConfigureCanvas(Canvas canvas, bool running)
		{
			canvas.renderMode = running ? RenderMode.WorldSpace : RenderMode.ScreenSpaceCamera;

			float scaleFactor = running ? 0.5f : 1.0f;

			canvas.transform.localScale = Vector3.one * scaleFactor;
			
		}

		static void ConfigureHand(Hand hand, bool running)
		{
			// for now, let's only use the right hand as a laser pointer
			hand.UIHand.VRRunningChanged(running && hand.handType == SteamVR_Input_Sources.RightHand);
		}
	}

	internal class VRUIHand : MonoBehaviour
	{
		Hand m_hand;
		LineRenderer m_lineRenderer;
		SteamVR_Action_Boolean_Source m_interactAction;
		SteamVR_Action_Boolean_Source m_rightClickAction;

		static readonly Vector3 rayDirection = Vector3.Normalize(Vector3.forward - Vector3.up);
		static readonly float rayDistance = 1000.0f; // this probably needs to be customized per scene.  This is crazy far for flight scene, but not far enough in KSC
		static readonly int raycastMask =
			1 |
			(1 << 5) | // unity UI
			(1 << 11) | // UI Dialog
			(1 << 12) | // UI Vectors
			(1 << 13) | // UI Mask
			(1 << 14) | // Screens
			(1 << 15) | // KSC buildings
			(1 << 21); // Part triggers

		LineRenderer m_uiLineRenderer;

		public SteamVR_Action_Boolean_Source ClickAction => m_interactAction;
		public SteamVR_Action_Boolean_Source RightClickAction => m_rightClickAction;

		public bool LaserEnabled
		{
			get { return m_lineRenderer.enabled; }
			set { m_lineRenderer.enabled = value; }
		}

		void SetupLineRenderer(LineRenderer lineRenderer)
		{
			lineRenderer.useWorldSpace = false;
			lineRenderer.SetPositions(new[] { rayDirection * 0.5f, rayDirection * rayDistance });
			lineRenderer.startWidth = 0.01f;
			lineRenderer.endWidth = 0.0002f * rayDistance;
			lineRenderer.endColor = new Color(1, 1, 1, 0.8f);
			lineRenderer.startColor = new Color(1, 1, 1, 0.05f);
			lineRenderer.material.shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
			lineRenderer.material.SetColor("_Color", Color.white);
		}

		void Awake()
		{
			m_hand = GetComponent<Hand>();
			m_lineRenderer = gameObject.AddComponent<LineRenderer>();

			SetupLineRenderer(m_lineRenderer);

			m_interactAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI")[m_hand.handType];
			m_rightClickAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("RightClick")[m_hand.handType];

#if false
			var uiLineObject = new GameObject();
			m_uiLineRenderer = uiLineObject.AddComponent<LineRenderer>();
			SetupLineRenderer(m_uiLineRenderer);
			m_uiLineRenderer.startColor = Color.magenta;
			uiLineObject.layer = 5;
			GameObject.DontDestroyOnLoad(uiLineObject);
#endif
		}

		public void LateUpdate()
		{
			if (m_uiLineRenderer)
			{
				var uiCameraTransform = UIMasterController.Instance.uiCamera.transform.parent;

				Vector3 cameraPosition = InteractionSystem.Instance.transform.InverseTransformPoint(m_lineRenderer.transform.position);
				Quaternion cameraDirection = InteractionSystem.Instance.transform.rotation.Inverse() * m_lineRenderer.transform.rotation;

				m_uiLineRenderer.transform.position = uiCameraTransform.TransformPoint(cameraPosition);
				m_uiLineRenderer.transform.rotation = uiCameraTransform.rotation * cameraDirection;
			}
		}

		public void VRRunningChanged(bool running)
		{
			m_lineRenderer.enabled = running;
			enabled = running;
		}

		public bool CastRay(out RaycastHit hit)
		{
			if (LaserEnabled)
			{
				Vector3 dir = transform.TransformDirection(rayDirection);
				bool isHit = Physics.Raycast(
					transform.position,
					dir,
					out hit,
					rayDistance,
					raycastMask);

				float hitDistance = isHit ? hit.distance : rayDistance;
				m_lineRenderer.SetPosition(1, hitDistance * rayDirection);
				m_lineRenderer.endWidth = 0.0002f * hitDistance;

				if (!isHit)
				{
					hit.point = transform.position + dir * rayDistance;
				}

				return isHit;
			}
			else
			{
				hit = default(RaycastHit);
				return false;
			}
		}
	}

	// This class derived from https://github.com/Raicuparta/two-forks-vr/blob/main/TwoForksVr/src/LaserPointer/LaserInputModule.cs under MIT license
	internal abstract class VRBaseInputModule : BaseInputModule
	{
		static internal VRBaseInputModule ActiveInstance;

		PointerEventData pointerData;
		Collider m_lastHitCollider = null;
		IVRMouseTarget m_mouseTarget;

		protected Hand m_hand; // TODO: switch this when a button is pressed on the other hand
		internal VRUIHand VRUIHand => m_hand.UIHand;

		protected override void Awake()
		{
			base.Awake();

			m_hand = InteractionSystem.Instance.RightHand;
			pointerData = new PointerEventData(eventSystem);
		}

		public override void ActivateModule()
		{
			base.ActivateModule();
			ActiveInstance = this;
		}

		public override void DeactivateModule()
		{
			base.DeactivateModule();
			ActiveInstance = null;
		}

		protected virtual void GetClickStates(Mouse.Buttons button, out bool clickDown, out bool isClicking, out bool clickUp)
		{
			if (button == Mouse.Buttons.Left)
			{
				clickDown = m_hand.UIHand.ClickAction.stateDown;
				isClicking = m_hand.UIHand.ClickAction.state;
				clickUp = m_hand.UIHand.ClickAction.stateUp;
			}
			else if (button == Mouse.Buttons.Right)
			{
				clickDown = m_hand.UIHand.RightClickAction.stateDown;
				isClicking = m_hand.UIHand.RightClickAction.state;
				clickUp = m_hand.UIHand.RightClickAction.stateUp;
			}
			else
			{
				clickDown = false;
				isClicking = false;
				clickUp = false;
			}
		}

		public override void Process()
		{
			CastRay(out var hitCollider, ref pointerData);
			UpdateCurrentObject(hitCollider);

			GetClickStates(Mouse.Buttons.Left, out var clickDown, out var isClicking, out var clickUp);

			if (!clickDown && isClicking)
				HandleDrag();
			else if (!pointerData.eligibleForClick && clickDown)
				HandleTrigger();
			else if (clickUp)
				HandlePendingClick();

			if (m_lastHitCollider)
			{
				if (clickDown)
				{
					m_lastHitCollider.gameObject.SendMessage("OnMouseDown");
				}
				else if (isClicking)
				{
					m_lastHitCollider.gameObject.SendMessage("OnMouseDrag");
				}
				else if (clickUp)
				{
					m_lastHitCollider.gameObject.SendMessage("OnMouseUp");
				}
			}

			if (m_mouseTarget != null)
			{
				GetClickStates(Mouse.Buttons.Right, out clickDown, out isClicking, out clickUp);

				if (clickDown)
				{
					m_mouseTarget.OnRightMouseButtonDown();
				}
				else if (isClicking)
				{
					m_mouseTarget.OnRightMouseButtonDrag();
				}
				else if (clickUp)
				{
					m_mouseTarget.OnRightMouseButtonUp();
				}
			}
		}

		protected abstract void CastRay(out Collider hitCollider, ref PointerEventData pointerData);

		private void UpdateCurrentObject(Collider hitCollider)
		{
			if (hitCollider != m_lastHitCollider)
			{
				if (m_lastHitCollider)
				{
					m_lastHitCollider.gameObject.SendMessage("OnMouseExit");
				}
				if (hitCollider)
				{
					hitCollider.gameObject.SendMessage("OnMouseEnter");
				}

				m_lastHitCollider = hitCollider;
				m_mouseTarget = m_lastHitCollider?.GetComponent<IVRMouseTarget>();
			}

			if (m_lastHitCollider)
			{
				m_lastHitCollider.gameObject.SendMessage("OnMouseOver");
			}

			// Send enter events and update the highlight.
			var go = pointerData.pointerCurrentRaycast.gameObject;
			HandlePointerExitAndEnter(pointerData, go);
			// Update the current selection, or clear if it is no longer the current object.
			var selected = ExecuteEvents.GetEventHandler<ISelectHandler>(go);
			if (selected == eventSystem.currentSelectedGameObject)
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, GetBaseEventData(),
					ExecuteEvents.updateSelectedHandler);
			else
				eventSystem.SetSelectedGameObject(null, pointerData);
		}

		private void HandleDrag()
		{
			var moving = pointerData.IsPointerMoving();

			if (moving && pointerData.pointerDrag != null && !pointerData.dragging)
			{
				ExecuteEvents.Execute(pointerData.pointerDrag, pointerData,
					ExecuteEvents.beginDragHandler);
				pointerData.dragging = true;
			}

			if (!pointerData.dragging || !moving || pointerData.pointerDrag == null) return;

			ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
		}

		private void HandlePendingClick()
		{
			if (!pointerData.eligibleForClick) return;

			var go = pointerData.pointerCurrentRaycast.gameObject;

			// Send pointer up and click events.
			ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
			ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);

			if (pointerData.pointerDrag != null)
				ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.dropHandler);

			if (pointerData.pointerDrag != null && pointerData.dragging)
				ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler);

			// Clear the click state.
			pointerData.pointerPress = null;
			pointerData.rawPointerPress = null;
			pointerData.eligibleForClick = false;
			pointerData.clickCount = 0;
			pointerData.pointerDrag = null;
			pointerData.dragging = false;
		}

		private void HandleTrigger()
		{
			var go = pointerData.pointerCurrentRaycast.gameObject;

			// Send pointer down event.
			pointerData.pressPosition = pointerData.position;
			pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
			pointerData.pointerPress =
				ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler)
				?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

			// Save the drag handler as well
			pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(go);
			if (pointerData.pointerDrag != null)
				ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.initializePotentialDrag);

			// Save the pending click state.
			pointerData.rawPointerPress = go;
			pointerData.eligibleForClick = true;
			pointerData.delta = Vector2.zero;
			pointerData.dragging = false;
			pointerData.useDragThreshold = true;
			pointerData.clickCount = 1;
			pointerData.clickTime = Time.unscaledTime;
		}

		// fake some inputs from the VR controllers and store them in the KSP Mouse class
		internal void UpdateMouse()
		{
			UpdateMouseButton(Mouse.Left, m_hand.UIHand.ClickAction);
			UpdateMouseButton(Mouse.Right, m_hand.UIHand.RightClickAction);

			if (m_lastHitCollider != null)
			{
				// TODO: see if we need to be calling Mouse.GetValidHoverPart
				var part = m_lastHitCollider.gameObject.GetComponentUpwards<Part>();
				if (part != null)
				{
					Mouse.HoveredPart = part;
				}
			}
		}

		private void UpdateMouseButton(Mouse.MouseButton button, SteamVR_Action_Boolean_Source action)
		{
			button.up = action.stateUp;
			button.down = action.stateDown;
			button.button = action.state;
		}
	}

	// processes pointer events based on the fingertip vs a canvas
	internal class VRFingerTipInputModule : VRBaseInputModule
	{
		internal static VRFingerTipInputModule Instance;

		List<Canvas> m_canvasStack = new List<Canvas>();

		bool m_lastPAWFingertipState;
		bool m_PAWFingertipState;
		bool m_PAWFingertipLatched;

		protected override void Awake()
		{
			base.Awake();
			Instance = this;
		}

		public void PushCanvas(Canvas canvas)
		{
			m_canvasStack.Remove(canvas);
			m_canvasStack.Add(canvas);
		}

		public void RemoveCanvas(Canvas canvas)
		{
			m_canvasStack.Remove(canvas);
		}

		public override bool ShouldActivateModule()
		{
			return Core.IsVrRunning && m_canvasStack.Count > 0;
		}

		public override void Process()
		{
			base.Process();
			m_lastPAWFingertipState = m_PAWFingertipState;
		}

		protected override void GetClickStates(Mouse.Buttons button, out bool clickDown, out bool isClicking, out bool clickUp)
		{
			base.GetClickStates(button, out clickDown, out isClicking, out clickUp);

			if (button == Mouse.Buttons.Left)
			{
				clickDown = clickDown || (m_PAWFingertipState && !m_lastPAWFingertipState);
				isClicking = isClicking || m_PAWFingertipState;
				clickUp = clickUp || (!m_PAWFingertipState && m_lastPAWFingertipState);
			}
		}

		protected override void CastRay(out Collider hitCollider, ref PointerEventData pointerData)
		{
			m_PAWFingertipState = false;

			if (m_canvasStack.Count > 0)
			{
				var canvas = m_canvasStack.Last();
				var raycaster = canvas.GetComponent<BaseRaycaster>();

				pointerData.Reset();
				pointerData.position = raycaster.eventCamera.WorldToScreenPoint(m_hand.FingertipPosition);
				raycaster.Raycast(pointerData, m_RaycastResultCache);

				Vector3 toFinger = m_hand.FingertipPosition - canvas.transform.position;
				float distance = Vector3.Dot(toFinger, -canvas.transform.forward);

				if (!m_PAWFingertipLatched && distance < m_hand.FingertipRadius && distance > -m_hand.FingertipRadius)
				{
					m_PAWFingertipState = distance < 0;
					if (m_PAWFingertipState)
					{
						m_PAWFingertipLatched = true;

						HapticUtils.Light(m_hand.otherHand.handType);
					}
				}
				else if (m_PAWFingertipLatched && distance > m_hand.FingertipRadius)
				{
					m_PAWFingertipLatched = false;
				}
			}

			pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
			m_RaycastResultCache.Clear();

			// if we're not over the canvas, use the normal laser raycast to find colliders
			if (pointerData.pointerCurrentRaycast.isValid)
			{
				hitCollider = null;
				m_hand.UIHand.LaserEnabled = false;
			}
			else
			{
				m_hand.UIHand.LaserEnabled = true;
				m_hand.UIHand.CastRay(out var hit);
				hitCollider = hit.collider;
			}
		}

	}

	internal class VRLaserInputModule : VRBaseInputModule
	{
		internal static VRLaserInputModule Instance;

		Camera EventCamera;

		protected override void Awake()
		{
			base.Awake();

			EventCamera = UIMasterController.Instance.uiCamera;

			Instance = this;
		}

		public override bool ShouldActivateModule()
		{
			return Core.IsVrRunning && m_hand.UIHand.enabled && !VRFingerTipInputModule.Instance.ShouldActivateModule();
		}

		protected override void CastRay(out Collider hitCollider, ref PointerEventData pointerData)
		{
			// I'm not sure it makes sense to do this raycast first, because I don't think it hits canvases.  If there was a collider on the canvas, this would help get a point right on its surface so that the projection from the eventcamera is more precise
			// as it is right now, I think the canvas raycast is always using the endpoint of this ray
			bool isHit = m_hand.UIHand.CastRay(out var hit);

			Vector3 interactionRelativeHit = InteractionSystem.Instance.transform.InverseTransformPoint(hit.point);
			Vector3 cameraRelativeHit = EventCamera.transform.parent.TransformPoint(interactionRelativeHit);

			var pointerPosition = EventCamera.WorldToScreenPoint(cameraRelativeHit);

			// Cast a ray into the scene
			pointerData.Reset();
			pointerData.position = pointerPosition;
			eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
			pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
			m_RaycastResultCache.Clear();
				
			// TODO: reimplement dragging
			// pointerData.delta = pointerPosition - lastHeadPose;
			
			// handle colliders in the world that are listening to mouse events
			// If we hit anything on a canvas, ignore this.
			hitCollider = pointerData.pointerCurrentRaycast.isValid ? null : hit.collider;
		}
	}

	// This interface can be added to things that need to listen for right-click actions
	// Now that the UI System is pushing data into the KSP Mouse class, this is less useful but if we find anything that needs it
	// This isn't used by anything currently, but leaving it in until I'm more certain that we don't need it
	interface IVRMouseTarget
	{
		void OnRightMouseButtonDown();
		void OnRightMouseButtonUp();
		void OnRightMouseButtonDrag();
	}

	[HarmonyPatch(typeof(Mouse), nameof(Mouse.Update))]
	class Mouse_Update_Patch
	{
		public static void Postfix()
		{
			if (VRBaseInputModule.ActiveInstance)
			{
				VRBaseInputModule.ActiveInstance.UpdateMouse();
			}
		}
	}

	[HarmonyPatch(typeof(Mouse), nameof(Mouse.GetAllMouseButtons))]
	class Mouse_GetAllMouseButtons_Patch
	{
		public static void Postfix(ref Mouse.Buttons __result)
		{
			if (VRBaseInputModule.ActiveInstance)
			{
				if (VRBaseInputModule.ActiveInstance.VRUIHand.ClickAction.state) __result |= Mouse.Buttons.Left;
				if (VRBaseInputModule.ActiveInstance.VRUIHand.RightClickAction.state) __result |= Mouse.Buttons.Right;
			}
		}
	}

	[HarmonyPatch(typeof(Mouse), nameof(Mouse.GetAllMouseButtonsDown))]
	class Mouse_GetAllMouseButtonsDown_Patch
	{
		public static void Postfix(ref Mouse.Buttons __result)
		{
			if (VRBaseInputModule.ActiveInstance)
			{
				if (VRBaseInputModule.ActiveInstance.VRUIHand.ClickAction.stateDown) __result |= Mouse.Buttons.Left;
				if (VRBaseInputModule.ActiveInstance.VRUIHand.RightClickAction.stateDown) __result |= Mouse.Buttons.Right;
			}
		}
	}

	[HarmonyPatch(typeof(Mouse), nameof(Mouse.GetAllMouseButtonsUp))]
	class Mouse_GetAllMouseButtonsUp_Patch
	{
		public static void Postfix(ref Mouse.Buttons __result)
		{
			if (VRBaseInputModule.ActiveInstance)
			{
				if (VRBaseInputModule.ActiveInstance.VRUIHand.ClickAction.stateUp) __result |= Mouse.Buttons.Left;
				if (VRBaseInputModule.ActiveInstance.VRUIHand.RightClickAction.stateUp) __result |= Mouse.Buttons.Right;
			}
		}
	}

	[HarmonyPatch(typeof(UIPartActionController), nameof(UIPartActionController.TrySelect))]
	class UIPartActionController_TrySelect_Patch
	{
		public static void Postfix(UIPartActionController __instance)
		{
			if (VRBaseInputModule.ActiveInstance && VRBaseInputModule.ActiveInstance.VRUIHand.RightClickAction.stateDown)
			{
				__instance.HandleMouseClick(null, HighLogic.LoadedSceneIsFlight);
			}
		}
	}
}
