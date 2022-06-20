using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR
{
	class ScaledSpaceCameraTransformUpdate : MonoBehaviour
	{
		private void LateUpdate()
		{
			transform.parent.position = ScaledSpace.LocalToScaledSpace(ScaledCamera.Instance.tgtRef.position);
			transform.parent.localRotation = ScaledCamera.Instance.tgtRef.rotation;
		}
	}

	class ChildCameraUpdater : MonoBehaviour
	{
		public Camera ParentCamera;
		public Camera ChildCamera;

		private void LateUpdate()
		{
			if (ParentCamera)
			{
				CameraUtils.CopyCameraSettings(ParentCamera, ChildCamera);

				ParentCamera.enabled = false;
			}
		}
	}

	static class CameraUtils
	{
		public static void DisablePositionTrackingOnCamera(Camera camera)
		{
			if (camera.transform.parent.name != "VRDisablePositionTracking")
			{
				var vrFix = new GameObject("VRDisablePositionTracking").transform;
				vrFix.localScale = Vector3.zero;
				vrFix.SetParent(camera.transform.parent, false);
				camera.transform.SetParent(vrFix, false);
				// camera.gameObject.AddComponent<ScaledSpaceCameraTransformUpdate>();
			}
		}

		static public void CopyCameraSettings(Camera from, Camera to)
		{
			to.allowDynamicResolution = from.allowDynamicResolution;
			to.allowHDR = from.allowHDR;
			to.allowMSAA = from.allowMSAA;
			to.backgroundColor = from.backgroundColor;
			to.clearFlags = from.clearFlags;
			to.clearStencilAfterLightingPass = from.clearStencilAfterLightingPass;
			to.cullingMask = from.cullingMask;
			to.depth = from.depth;
			to.depthTextureMode = from.depthTextureMode;
			to.eventMask = from.eventMask;
			to.farClipPlane = from.farClipPlane;
			to.layerCullDistances = from.layerCullDistances;
			to.nearClipPlane = from.nearClipPlane;
			to.opaqueSortMode = from.opaqueSortMode;
			to.orthographic = from.orthographic;
			to.renderingPath = from.renderingPath;
			to.useOcclusionCulling = from.useOcclusionCulling;
		}

		static public GameObject AddVRCamera(Camera parent, bool disablePositionTracking, bool dontDestroyOnLoad)
		{
			string childName = parent.name;
			parent.name += "(Stock)";
			var childObject = parent.gameObject.GetChild(childName);

			if (childObject != null)
			{
				return childObject;
			}

			var go = new GameObject(childName);

			if (dontDestroyOnLoad)
			{
				GameObject.DontDestroyOnLoad(go);
			}
			var vrCam = go.AddComponent<Camera>();
			go.transform.SetParent(parent.transform, false);

			// copy settings from parent camera
			CopyCameraSettings(parent, vrCam);

			var camUpdater = go.AddComponent<ChildCameraUpdater>();
			camUpdater.ParentCamera = parent;
			camUpdater.ChildCamera = vrCam;

			if (disablePositionTracking)
			{
				parent.transform.localScale = Vector3.zero;
			}

			parent.enabled = false;
			// Destroy(parent);

			return go;
		}
	}
}
