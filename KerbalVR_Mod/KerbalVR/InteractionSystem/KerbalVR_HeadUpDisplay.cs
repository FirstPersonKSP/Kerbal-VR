using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using TMPro;

namespace KerbalVR
{
#if true
	public class HeadUpDisplay : MonoBehaviour
	{
		#region Private Members
		// hud game object
		protected Canvas hudCanvas;
		protected Material hudMaterial;
		protected int hudPixelWidth = 1400;
		protected int hudPixelHeight = 1024;
		protected float hudHeight = 1f;

		static readonly float hudDistance = 0.32f;

		protected TextMeshPro altitudeLabel;
		protected Image compassImage;
		#endregion


		public void Awake()
		{
			// create a texture we can draw on the HUD
			hudMaterial = new Material(Shader.Find("KSP/Alpha/Translucent Additive"));

			// create a UI Canvas for this screen.
			var hudCanvasGameObject = new GameObject("KVR_HeadUpDisplay_Canvas");
			DontDestroyOnLoad(hudCanvasGameObject);
			hudCanvas = hudCanvasGameObject.AddComponent<Canvas>();
			hudCanvas.renderMode = RenderMode.WorldSpace;
			hudCanvas.pixelPerfect = false;
			hudCanvas.worldCamera = FlightCamera.fetch.cameras[0];
			hudCanvas.planeDistance = 2f;
			hudCanvasGameObject.AddComponent<CanvasScaler>();

			// create UI elements
			CreateHeadUpDisplayUI();
			Utils.SetLayer(hudCanvasGameObject, 20); // put on internal layer so it renders with the kerbal

			enabled = false;
		}

		public void OnDestroy()
		{
			Utils.Log("HeadUpDisplay being destroyed");
		}

		protected void OnEnable()
		{
			// possible helmet scene path: 		Scene path	"kerbalEVAfemale (Valentina Kerman)/globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_helmet01/be_helmetEnd01"	string
			// var anchor = Scene.GetKerbalEVA().transform.Find("globalMove01/joints01/bn_spA01/bn_spB01/bn_spc01/bn_spD01/be_spE01/bn_helmet01");
			var anchor = FlightCamera.fetch.transform;
			// anchor = Scene.GetKerbalEVA().helmetTransform;

			hudCanvas.transform.SetParent(anchor, false);
			hudCanvas.transform.localPosition = Vector3.forward * hudDistance;
			hudCanvas.transform.localRotation = Quaternion.identity;
			hudCanvas.transform.localScale = 0.5f / hudPixelWidth * Vector3.one;
			hudCanvas.gameObject.SetActive(true);
		}

		protected void OnDisable()
		{
			hudCanvas.transform.SetParent(null);
			hudCanvas.gameObject.SetActive(false);
		}

		protected void Update()
		{
			if (altitudeLabel != null && FlightGlobals.ActiveVessel != null)
			{
				Utils.HumanizeQuantity((float)FlightGlobals.ActiveVessel.radarAltitude, "m", out float altitude, out string altitudeUnits);

				var formatString = FlightGlobals.ActiveVessel.radarAltitude < 10 ? "F1" : "F0";

				altitudeLabel.text = "Altitude: " + altitude.ToString(formatString) + " " + altitudeUnits;
			}
		}

		protected void CreateHeadUpDisplayUI()
		{
			// create a static reticle in the center
			/*
			GameObject reticleImageGO = new GameObject("Reticle");
			reticleImageGO.AddComponent<CanvasRenderer>();
			Image reticleImage = reticleImageGO.AddComponent<Image>();
			string texPath = Path.Combine(Globals.KERBALVR_TEXTURES_DIR, "hud_heading").Replace("\\", "/");
			// string texPath = Path.Combine(Globals.KERBALVR_TEXTURES_DIR, "test_image").Replace("\\", "/");
			Shader texShader = Shader.Find("Unlit/Transparent");
			Material reticleMat = new Material(texShader);
			reticleMat.mainTexture = GameDatabase.Instance.GetTexture(texPath, false);
			reticleImage.material = reticleMat;
			reticleImageGO.transform.SetParent(hudCanvas.transform, false);
			reticleImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			reticleImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			reticleImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
			reticleImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hudPixelHeight);
			reticleImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, hudPixelHeight);
			reticleImage.rectTransform.localPosition = new Vector3(0f, 0f);
			*/

			compassImage = new GameObject("CompassImage").AddComponent<Image>();
			compassImage.material = new Material(Shader.Find("Unlit/Transparent"));
			compassImage.material.mainTexture = GameDatabase.Instance.GetTexture(Path.Combine(Globals.KERBALVR_TEXTURES_DIR, "hud_compass").Replace('\\', '/'), false);
			compassImage.transform.SetParent(hudCanvas.transform, false);
			compassImage.transform.localPosition = new Vector3(0.0f, -hudPixelHeight * 0.4f);
			compassImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1280);
			compassImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 80);

			// information text labels
			TMP_FontAsset font = KerbalVR.AssetLoader.Instance.GetTmpFont("Futura_Medium_BT");
			altitudeLabel = new GameObject("AltitudeLabel").AddComponent<TextMeshPro>();
			altitudeLabel.text = "Altitude: ";
			altitudeLabel.font = font;
			altitudeLabel.fontSize = 600f;
			altitudeLabel.color = Color.white;
			altitudeLabel.alignment = TextAlignmentOptions.TopLeft;
			altitudeLabel.transform.SetParent(hudCanvas.transform, false);
			altitudeLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
			altitudeLabel.rectTransform.anchorMax = new Vector2(0f, 1f);
			altitudeLabel.rectTransform.pivot = new Vector2(0f, 1f);
			altitudeLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1000);
			altitudeLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 300);
			altitudeLabel.rectTransform.localPosition = new Vector3(-hudPixelWidth * 0.45f, hudPixelHeight * 0.4f);
		}
	}
#endif
}
