using HarmonyLib;
using KSP.UI;
using System;
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
	internal static class UISystem
	{
		internal static void VRRunningChanged(bool running)
		{
			running = running && !Scene.IsInIVA();

			UIMasterController.Instance.uiCamera.stereoTargetEye = running ? StereoTargetEyeMask.Both : StereoTargetEyeMask.None;
			UIMasterController.Instance.uiCamera.farClipPlane = running ? 2000.0f : 1100.0f;
			UIMasterController.Instance.uiCamera.nearClipPlane = running ? .03f : -300.0f;
			UIMasterController.Instance.uiCamera.orthographic = !running;

			ConfigureCanvas(UIMasterController.Instance.mainCanvas, running);
			ConfigureCanvas(UIMasterController.Instance.actionCanvas, running);
			ConfigureCanvas(UIMasterController.Instance.appCanvas, running);
			ConfigureCanvas(UIMasterController.Instance.dialogCanvas, running);

			ConfigureHand(InteractionSystem.Instance.LeftHand, running);
			ConfigureHand(InteractionSystem.Instance.RightHand, running);
		}

		internal static void ModeChanged()
		{
			VRRunningChanged(Core.IsVrRunning);

			var kerbalEVA = Scene.GetKerbalEVA();

			// open the kerbal eva part action window and attach it to the left hand
			if (Core.IsVrRunning && kerbalEVA != null)
			{
				UIPartActionController.Instance.SelectPart(kerbalEVA.part, false, false);
				UIMasterController.Instance.actionCanvas.transform.SetParent(InteractionSystem.Instance.LeftHand.handObject.transform, false);
				UIMasterController.Instance.actionCanvas.transform.localPosition = Vector3.zero;
				UIMasterController.Instance.actionCanvas.transform.localRotation = Quaternion.identity;
			}
			else
			{

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
			hand.UIHand.VRRunningChanged(running);
		}
	}

	internal class VRUIHand : MonoBehaviour
	{
		Hand m_hand;
		LineRenderer m_lineRenderer;
		SteamVR_Action_Boolean_Source m_interactAction;
		SteamVR_Action_Boolean_Source m_rightClickAction;

		static readonly Vector3 rayDirection = Vector3.Normalize(Vector3.forward - Vector3.up);
		static readonly float rayDistance = 1000.0f;
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
	}

	// This class derived from https://github.com/Raicuparta/two-forks-vr/blob/main/TwoForksVr/src/LaserPointer/LaserInputModule.cs
	internal class VRUIHandInputModule : BaseInputModule
	{
		PointerEventData pointerData;
		Camera EventCamera;
		Vector3 lastHeadPose;

		VRUIHand m_hand; // TODO: switch this when a button is pressed on the other hand
		Collider m_lastHitCollider = null;
		IVRMouseTarget m_mouseTarget;

		internal static VRUIHandInputModule Instance;
		internal VRUIHand VRUIHand => m_hand;

		void Awake()
		{

			m_hand = InteractionSystem.Instance.RightHand.UIHand;
			

			EventCamera = UIMasterController.Instance.uiCamera;

			Instance = this;
		}

		void LateUpdate()
		{

		}

		public override bool ShouldActivateModule()
		{
			return Core.IsVrRunning && m_hand.enabled;
		}

		public override void Process()
		{
			CastRay();
			UpdateCurrentObject();

			var clickDown = m_hand.ClickAction.stateDown;
			var isClicking = m_hand.ClickAction.state;
			var clickUp = m_hand.ClickAction.stateUp;

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
				if (m_hand.RightClickAction.stateDown)
				{
					m_mouseTarget.OnRightMouseButtonDown();
				}
				else if (m_hand.RightClickAction.state)
				{
					m_mouseTarget.OnRightMouseButtonDrag();
				}
				else if (m_hand.RightClickAction.stateUp)
				{
					m_mouseTarget.OnRightMouseButtonUp();
				}
			}
		}

		private void CastRay()
		{
			bool isHit = m_hand.CastRay(out var hit);

			Vector3 interactionRelativeHit = InteractionSystem.Instance.transform.InverseTransformPoint(hit.point);
			Vector3 cameraRelativeHit = EventCamera.transform.parent.TransformPoint(interactionRelativeHit);


			var pointerPosition = EventCamera.WorldToScreenPoint(cameraRelativeHit);

			if (pointerData == null)
			{
				pointerData = new PointerEventData(eventSystem);
				lastHeadPose = pointerPosition;
			}

			// Cast a ray into the scene
			pointerData.Reset();
			pointerData.position = pointerPosition;
			eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
			pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
			m_RaycastResultCache.Clear();
			pointerData.delta = pointerPosition - lastHeadPose;
			lastHeadPose = hit.point;

			// handle colliders in the world that are listening to mouse events
			// If we hit anything on a canvas, ignore this.
			var hitCollider = pointerData.pointerCurrentRaycast.isValid ? null : hit.collider;
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
		}

		private void UpdateCurrentObject()
		{
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
			UpdateMouseButton(Mouse.Left, m_hand.ClickAction);
			UpdateMouseButton(Mouse.Right, m_hand.RightClickAction);

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

	// This interface can be added to things that need to listen for right-click actions
	// Now that the UI System is pushing data into the KSP Mouse class, this is less useful but if we find anything 
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
			VRUIHandInputModule.Instance.UpdateMouse();
		}
	}

	[HarmonyPatch(typeof(Mouse), nameof(Mouse.GetAllMouseButtons))]
	class Mouse_GetAllMouseButtons_Patch
	{
		public static void Postfix(ref Mouse.Buttons __result)
		{
			if (VRUIHandInputModule.Instance.VRUIHand.ClickAction.state) __result |= Mouse.Buttons.Left;
			if (VRUIHandInputModule.Instance.VRUIHand.RightClickAction.state) __result |= Mouse.Buttons.Right;
		}
	}

	[HarmonyPatch(typeof(Mouse), nameof(Mouse.GetAllMouseButtonsDown))]
	class Mouse_GetAllMouseButtonsDown_Patch
	{
		public static void Postfix(ref Mouse.Buttons __result)
		{
			if (VRUIHandInputModule.Instance.VRUIHand.ClickAction.stateDown) __result |= Mouse.Buttons.Left;
			if (VRUIHandInputModule.Instance.VRUIHand.RightClickAction.stateDown) __result |= Mouse.Buttons.Right;
		}
	}

	[HarmonyPatch(typeof(Mouse), nameof(Mouse.GetAllMouseButtonsUp))]
	class Mouse_GetAllMouseButtonsUp_Patch
	{
		public static void Postfix(ref Mouse.Buttons __result)
		{
			if (VRUIHandInputModule.Instance.VRUIHand.ClickAction.stateUp) __result |= Mouse.Buttons.Left;
			if (VRUIHandInputModule.Instance.VRUIHand.RightClickAction.stateUp) __result |= Mouse.Buttons.Right;
		}
	}
}
