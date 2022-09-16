using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR
{
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class KerbalVR_MainMenu : MonoBehaviour
	{
		MainMenu m_mainMenu;
		MainMenuEnvLogic m_mainMenuEnvLogic;

		public void Awake()
		{
			m_mainMenu = GameObject.FindObjectOfType<MainMenu>();
			m_mainMenuEnvLogic = GameObject.FindObjectOfType<MainMenuEnvLogic>();

			// the main menu tries to drive the transform of the landscapeCamera around directly.
			// so we need to clone the camera and attach it as a child object, then disable the original
			var landscapeCameraAnchor = CameraUtils.CreateVRAnchor(m_mainMenuEnvLogic.landscapeCamera);
			var dummyCamera = CameraUtils.CloneComponent(m_mainMenuEnvLogic.landscapeCamera, landscapeCameraAnchor);
			dummyCamera.enabled = false;
			m_mainMenuEnvLogic.landscapeCamera = dummyCamera;

			InteractionSystem.Instance.transform.SetParent(m_mainMenuEnvLogic.landscapeCamera.transform, false);
		}
	}
}
