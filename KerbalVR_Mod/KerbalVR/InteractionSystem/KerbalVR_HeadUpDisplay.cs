using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using TMPro;
using KSP.UI.Screens.Flight;
using KSP.UI.Screens;

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

		TMP_FontAsset font;
		protected TextMeshPro altitudeLabel;
		protected Image compassImage;
		protected TextMeshPro horizontalSpeed;
		protected TextMeshPro verticalSpeed;
		protected TextMeshPro totalSpeed;
		HUDResourceMeter resourceMeter;
		#endregion


		public void Awake()
		{
			hudMaterial = new Material(Shader.Find("KSP/Alpha/Translucent Additive"));
			font = KerbalVR.AssetLoader.Instance.GetTmpFont("Futura_Medium_BT");

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

		string GetSpeedString(float speed)
		{
			Utils.HumanizeQuantity(speed, "m/s", out float displaySpeed, out string displayUnits);
			return displaySpeed.ToString("F1") + " " + displayUnits;
		}

		protected void Update()
		{
			if (altitudeLabel != null && FlightGlobals.ActiveVessel != null)
			{
				var activeVessel = FlightGlobals.ActiveVessel;

				Utils.HumanizeQuantity((float)activeVessel.radarAltitude, "m", out float altitude, out string altitudeUnits);

				var formatString = activeVessel.radarAltitude < 10 ? "F1" : "F0";

				altitudeLabel.text = "Altitude: " + altitude.ToString(formatString) + " " + altitudeUnits;


				horizontalSpeed.text = "H: " + GetSpeedString((float)activeVessel.horizontalSrfSpeed);
				verticalSpeed.text = "V: " + GetSpeedString((float)activeVessel.verticalSpeed);
				totalSpeed.text = "T: " + GetSpeedString((float)activeVessel.speed);

				Quaternion quaternion = Quaternion.LookRotation(activeVessel.north, activeVessel.upAxis);
				Quaternion quaternion2 = Quaternion.Inverse(Quaternion.Euler(90f, 0f, 0f) * Quaternion.Inverse(activeVessel.GetTransform().rotation) * quaternion);
				float heading = quaternion2.eulerAngles.y;
				compassImage.material.mainTextureScale = new Vector2(0.2f, 1.0f);
				compassImage.material.mainTextureOffset = new Vector2(heading / 360.0f - 0.5f * 0.2f, 0);

				UpdateResources();
			}
		}

		private void UpdateResources()
		{
			var kerbalEVA = KerbalVR.Scene.GetKerbalEVA();

			var resourceInfo = kerbalEVA.propellantResource;

			resourceMeter.SetData(resourceInfo.info.displayName, resourceInfo.amount, resourceInfo.maxAmount);
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
			
			altitudeLabel = new GameObject("AltitudeLabel").AddComponent<TextMeshPro>();
			altitudeLabel.text = "Altitude: ";
			altitudeLabel.font = font;
			altitudeLabel.fontSize = 200f;
			altitudeLabel.color = Color.white;
			altitudeLabel.alignment = TextAlignmentOptions.TopLeft;
			altitudeLabel.transform.SetParent(hudCanvas.transform, false);
			altitudeLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
			altitudeLabel.rectTransform.anchorMax = new Vector2(0f, 1f);
			altitudeLabel.rectTransform.pivot = new Vector2(0f, 1f);
			altitudeLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1000);
			altitudeLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 300);
			altitudeLabel.rectTransform.localPosition = new Vector3(-hudPixelWidth * 0.45f, hudPixelHeight * 0.4f);

			// speed labels
			horizontalSpeed = CreateLabel("HorizontalSpeed", 200, 0.92f, 0.15f, 240, 32);
			verticalSpeed = CreateLabel("VerticalSpeed", 200, 0.92f, 0.17f, 240, 32);
			totalSpeed = CreateLabel("TotalSpeed", 200, 0.92f, 0.19f, 240, 32);

			// resources
			resourceMeter = new GameObject("resourceMeter").AddComponent<HUDResourceMeter>();
			resourceMeter.transform.SetParent(hudCanvas.transform, false);
			resourceMeter.rectTransform.anchorMin = new Vector2(1f, 1f);
			resourceMeter.rectTransform.anchorMax = new Vector2(1f, 1f);
			resourceMeter.rectTransform.localPosition = new Vector3(hudPixelWidth * 0.4f, hudPixelHeight * 0.45f);

		}

		TextMeshPro CreateLabel(string name, float fontSize, float relPositionX, float relPositionY, float width, float height)
		{
			var label = new GameObject(name).AddComponent<TextMeshPro>();
			label.font = font;
			label.fontSize = fontSize;
			label.color = Color.white;
			label.transform.SetParent(hudCanvas.transform, false);
			label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
			label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
			label.rectTransform.localPosition = new Vector3(hudPixelWidth * (relPositionX - 0.5f), hudPixelHeight * (relPositionY -0.5f));

			return label;
		}
	}

	public class HUDResourceMeter : Image
	{
		Image m_barImage;
		TextMeshPro m_nameLabel;
		TextMeshPro m_amountLabel;

		public void Start()
		{
			this.color = Color.white;
			
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20);

			m_barImage = new GameObject("bar").AddComponent<Image>();
			m_barImage.transform.SetParent(transform, false);
			m_barImage.rectTransform.anchorMin = new Vector2(0, 0);
			m_barImage.rectTransform.anchorMax = new Vector2(1, 1);
			m_barImage.rectTransform.sizeDelta = new Vector2(0.8f, 0.8f);
			m_barImage.color = Color.green;
			m_barImage.type = Type.Filled;
			m_barImage.fillOrigin = (int)Image.OriginHorizontal.Left;
			m_barImage.fillMethod = FillMethod.Horizontal;
			m_barImage.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);

			m_nameLabel = new GameObject("name").AddComponent<TextMeshPro>();
			m_nameLabel.transform.SetParent(transform, false);
			m_nameLabel.color = Color.white;
			m_nameLabel.fontSize = 100;
			m_nameLabel.rectTransform.anchoredPosition = new Vector2(-400, 0);
			//m_nameLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
			//m_nameLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);
			m_nameLabel.enabled = false;

			m_amountLabel = new GameObject("amount").AddComponent<TextMeshPro>();
			m_amountLabel.transform.SetParent(m_barImage.transform, false);
			m_amountLabel.color = Color.black;
			m_amountLabel.fontSize = 100;
			m_amountLabel.enabled = false;

		}

		public string ResourceName
		{
			get { return m_nameLabel.text; }
			set { 
				// m_nameLabel.text = value;
				}
		}

		public void SetData(string resourceName, double currentAmount, double capacity)
		{
			m_barImage.fillAmount = (float)(currentAmount / capacity);
			m_nameLabel.text = resourceName;
			m_amountLabel.text = currentAmount.ToString("F2") + " / " + capacity.ToString("F2");
		}
	}
#endif
}
