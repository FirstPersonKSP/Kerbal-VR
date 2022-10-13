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

		protected TextMeshPro label;
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
			Utils.SetLayer(hudCanvasGameObject, 0);

			enabled = false;
		}

		public void OnDestroy()
		{
			Utils.Log("HeadUpDisplay being destroyed");
		}

		protected void OnEnable()
		{
			hudCanvas.transform.SetParent(FlightCamera.fetch.transform, false);
			hudCanvas.transform.localPosition = Vector3.forward * 1f;
			hudCanvas.transform.localRotation = Quaternion.identity;
			hudCanvas.transform.localScale = 1.0f / hudPixelWidth * Vector3.one;
			hudCanvas.gameObject.SetActive(true);
		}

		protected void OnDisable()
		{
			hudCanvas.transform.SetParent(null);
			hudCanvas.gameObject.SetActive(false);
		}

		protected void Update()
		{
			if (label != null && FlightGlobals.ActiveVessel != null)
			{
				Utils.HumanizeQuantity((float)FlightGlobals.ActiveVessel.radarAltitude, "m", out float altitude, out string altitudeUnits);

				var formatString = FlightGlobals.ActiveVessel.radarAltitude < 10 ? "F1" : "F0";

				label.text = "Altitude: " + altitude.ToString(formatString) + " " + altitudeUnits;
			}
		}

		protected void CreateHeadUpDisplayUI()
		{
			// create a static reticle in the center
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

			// information text labels
			TMP_FontAsset font = KerbalVR.AssetLoader.Instance.GetTmpFont("Futura_Medium_BT");
			GameObject altitudeLabel = new GameObject("AltitudeLabel");
			altitudeLabel.AddComponent<CanvasRenderer>();
			label = altitudeLabel.AddComponent<TextMeshPro>();
			label.text = "Altitude: ";
			label.font = font;
			label.fontSize = 600f;
			label.color = Color.white;
			label.alignment = TextAlignmentOptions.TopLeft;
			label.transform.SetParent(hudCanvas.transform, false);
			label.rectTransform.anchorMin = new Vector2(0f, 1f);
			label.rectTransform.anchorMax = new Vector2(0f, 1f);
			label.rectTransform.pivot = new Vector2(0f, 1f);
			label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1000);
			label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 300);
			label.rectTransform.localPosition = new Vector3(-hudPixelWidth * 0.45f, hudPixelHeight * 0.4f);
		}
	}
#endif
}
