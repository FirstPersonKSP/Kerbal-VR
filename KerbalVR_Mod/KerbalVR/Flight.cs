using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace KerbalVR
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class FirstPersonKerbalFlight : MonoBehaviour
	{
		public void Awake()
		{
			Utils.Log("Flight.Awake");

			GameEvents.OnIVACameraKerbalChange.Add(OnIVACameraKerbalChange);
			GameEvents.OnCameraChange.Add(OnCameraChange);


		}

		public void FixedUpdate()
		{
		}

		public void OnDestroy()
		{
			Utils.Log("Flight.OnDestroy");
			GameEvents.OnIVACameraKerbalChange.Remove(OnIVACameraKerbalChange);
			GameEvents.OnCameraChange.Remove(OnCameraChange);
		}

		private void OnIVACameraKerbalChange(Kerbal kerbal)
		{
			FixIVACamera();

		}

		private void OnCameraChange(CameraManager.CameraMode mode)
		{
			if (mode == CameraManager.CameraMode.IVA)
			{
				FixIVACamera();
			}
		}

		static readonly string[] ArmBones = { "bn_l_arm01", "bn_r_arm01" };

		private void FixIVACamera()
		{
			Utils.Log("Flight.FixIVACamera");
			
			// when in VR, the origin of the camera transform generally needs to be on the floor rather than at the eye position
			// Further, the IVA seats have a scale on them and VR camera doesn't seem to respect local scale on the camera's transform itself
			if (InternalCamera.Instance.transform.parent == CameraManager.Instance.IVACameraActiveKerbal.eyeTransform)
			{
				// TODO: how does this work with PCR?

				// Don't really know why, but sometimes the eye transform has a nonzero scale on it
				// this messes with the camera projection and the size/movement of the hands.
				// does this mess with our external view of the rocket...?
				Transform eyeTransform = InternalCamera.Instance.transform.parent;
				eyeTransform.localScale = Vector3.one;
				InternalCamera.Instance.transform.localScale = Vector3.one;
				KerbalVR.InteractionSystem.Instance.transform.localScale = Vector3.one;

				Transform kerbalTransform = InternalCamera.Instance.transform.parent.parent;
				
				var meshRenderer = kerbalTransform.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

				foreach (var b in meshRenderer.bones)
				{
					if (ArmBones.Contains(b.name))
					{
						b.localScale = Vector3.zero;
					}
				}

				KerbalVR.InteractionSystem.Instance.transform.SetParent(InternalCamera.Instance.transform.parent, false);

				SteamVR.settings.trackingSpace = ETrackingUniverseOrigin.TrackingUniverseSeated;

				var chaperone = OpenVR.Chaperone;
				if (chaperone != null)
					chaperone.ResetZeroPose(SteamVR.settings.trackingSpace);
			}
		}
	}
}