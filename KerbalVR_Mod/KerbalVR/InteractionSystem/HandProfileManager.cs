using KSPDev.ConfigUtils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalVR
{
	[PersistentFieldsDatabase("KerbalVR/settings/KerbalVRConfig")]
	public class HandProfileManager
	{
		public class Profile
		{
			[PersistentField("name")]
			public string name;

			[PersistentField("scale")]
			public float scale = 1.0f;

			[PersistentField("gripOffset")]
			public Vector3 gripOffset;
			[PersistentField("fingertipOffset")]
			public Vector3 fingertipOffset;
			[PersistentField("palmColliderSize")]
			public float palmColliderSize;
			[PersistentField("fingertipColliderSize")]
			public float fingertipColliderSize;
			[PersistentField("pinchColliderSize")]
			public float pinchColliderSize;

			[PersistentField("prefabNameRight")]
			public string PrefabNameRight;
			[PersistentField("prefabNameLeft")]
			public string PrefabNameLeft;

			[PersistentField("renderModelPath")]
			public string renderModelPath;
			[PersistentField("indexTipTransformPath")]
			public string indexTipTransformPath;
			[PersistentField("thumbTipTransformPath")]
			public string thumbTipTransformPath;
			[PersistentField("gripTransformPath")]
			public string gripTransformPath;

			[PersistentField("skeletonRootTransformPath")]
			public string skeletonRootTransformPath;
			[PersistentField("wrist")]
			public string wrist;
			[PersistentField("joint", isCollection = true)]
			public List<string> joints = new List<string>();
		}

		public static HandProfileManager _instance;
		public static HandProfileManager Instance
		{
			get { return _instance ?? (_instance = new HandProfileManager()); }
		}

		[PersistentField("HandProfile/iva")]
		public string ivaProfile = "profile_name";
		[PersistentField("HandProfile/eva")]
		public string evaProfile = "profile_name";

		[PersistentField("HandProfile/fullRangeOfMotion")]
		public bool fullRangeOfMotion;

		[PersistentField("HandProfile/skeletonPrefabNameRight")]
		public string skeletonPrefabNameRight;
		[PersistentField("HandProfile/skeletonPrefabNameLeft")]
		public string skeletonPrefabNameLeft;
		[PersistentField("HandProfile/skeletonRootPath")]
		public string skeletonRootPath;

		[PersistentField("HandProfile/Profile", isCollection = true)]
		public static List<Profile> profiles = new List<Profile>();

		public void LoadAllProfiles()
		{
			ConfigAccessor.ReadFieldsInType(Instance.GetType(), Instance);

			if (profiles.Count == 0)
			{
				Utils.LogError("No hand profile found!");
			}

			foreach (Profile profile in profiles)
			{
				Utils.Log($"Loaded hand profile \"{profile.name}\" with right prefab \"{profile.PrefabNameRight}\" and left prefab \"{profile.PrefabNameLeft}\"");
			}
			Utils.Log($"IVA profile: \"{ivaProfile}\" EVA profile: \"{evaProfile}\"");
		}
		public Profile GetProfile(bool isIVA)
		{
			return profiles.First(x => x.name == (isIVA ? ivaProfile : evaProfile)) ?? profiles[0];
		}
	}
}
