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
			public class RetargetableSetting
			{
				[PersistentField("destinationSkeletonRootPath")]
				public string destinationSkeletonRootPath;
				[PersistentField("wrist")]
				public string wrist;
				[PersistentField("joint", isCollection = true)]
				public List<string> names = new List<string>();
			}

			[PersistentField("name")]
			public string name;

			[PersistentField("gripOffset")]
			public Vector3 gripOffset;
			[PersistentField("fingertipOffset")]
			public Vector3 fingertipOffset;
			[PersistentField("gripColliderSize")]
			public float gripColliderSize;
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

			[PersistentField("sourceSkeletonRootPath")]
			public string sourceSkeletonRootPath;

			[PersistentField("useSkeletonHelper")]
			public bool useSkeletonHelper = false;

			[PersistentField("RetargetableSetting")]
			public RetargetableSetting retargetableSetting = null;
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
		public Profile GetIVAProfile()
		{
			return profiles.First(x => x.name == ivaProfile) ?? profiles[0];
		}

		public Profile GetEVAProfile()
		{
			return profiles.First(x => x.name == evaProfile) ?? profiles[0];
		}
	}
}
