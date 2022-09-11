using KSPDev.ConfigUtils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalVR
{
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	[PersistentFieldsDatabase("KerbalVR/settings/KerbalVRConfig")]
	public class MainScreenTweaker : MonoBehaviour
	{
		public class Add
		{
			[PersistentField("prefab")]
			public string prefab;
			[PersistentField("path")]
			public string path;
			[PersistentField("position")]
			public Vector3 position;
			[PersistentField("rotation")]
			public Vector3 rotation;
			[PersistentField("scale")]
			public Vector3 scale;
		}

		public class Hide
		{
			[PersistentField("path")]
			public string path;
		}

		[PersistentField("MainScreenTweaker/enabled")]
		public bool tweakerEnabled = true;

		[PersistentField("MainScreenTweaker/Add", isCollection = true)]
		public List<Add> addItems = new List<Add>();

		[PersistentField("MainScreenTweaker/Hide", isCollection = true)]
		public List<Hide> hideItems = new List<Hide>();

		public void Awake()
		{
			ConfigAccessor.ReadFieldsInType(GetType(), this);
			if (tweakerEnabled)
			{
				ApplyTweaks();
			}
		}

		void ApplyTweaks()
		{
			Debug.Log("[KerbalVR/MainScreenTweaker] Applying Tweaks");

			GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

			foreach (Add add in addItems)
			{
				string[] p = add.path.Split(new[] { '/' }, 2);
				Transform targetTransform = roots.First(x => x.name == p[0]).transform.Find(p[1]);

				if (targetTransform)
				{
					GameObject prefab = AssetLoader.Instance.GetGameObject(add.prefab);
					if (prefab)
					{
						GameObject obj = Instantiate(prefab);
						obj.transform.SetParent(targetTransform);
						obj.transform.localPosition = add.position;
						obj.transform.localRotation = Quaternion.Euler(add.rotation);
						obj.transform.localScale = add.scale;
					}
				}
			}

			foreach (Hide remove in hideItems)
			{
				string[] p = remove.path.Split(new[] { '/' }, 2);
				Transform targetTransform = roots.First(x => x.name == p[0]).transform.Find(p[1]);

				if (targetTransform.gameObject)
				{
					targetTransform.gameObject.SetActive(false);
				}
			}
		}
	}
}
