using KSP.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalVR
{
	internal static class UISystem
	{
		internal static void VRRunningChanged(bool running)
		{
			UIMasterController.Instance.uiCamera.stereoTargetEye = running ? StereoTargetEyeMask.Both : StereoTargetEyeMask.None;
			UIMasterController.Instance.uiCamera.farClipPlane = running ? 0.0f : 1100.0f;

			ConfigureCanvas(UIMasterController.Instance.mainCanvas, running);
			ConfigureCanvas(UIMasterController.Instance.actionCanvas, running);
			ConfigureCanvas(UIMasterController.Instance.appCanvas, running);
			ConfigureCanvas(UIMasterController.Instance.dialogCanvas, running);
		}

		static void ConfigureCanvas(Canvas canvas, bool running)
		{
			canvas.renderMode = running ? RenderMode.WorldSpace : RenderMode.ScreenSpaceCamera;
		}
	}
}
