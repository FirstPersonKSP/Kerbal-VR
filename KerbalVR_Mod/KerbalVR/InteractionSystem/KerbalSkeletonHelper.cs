using UnityEngine;
using System.Collections.Generic;

namespace KerbalVR
{
	public class KerbalSkeletonHelper : MonoBehaviour
	{
		public Transform sourceSkeletonRoot;
		public Transform destinationSkeletonRoot;

		public HandProfileManager.Profile.RetargetableSetting retargetableSetting;

		public Retargetable wrist;
		public List<Retargetable> retargetables;

		public class Retargetable
		{
			public Transform source;
			public Transform destination;

			public Retargetable(Transform source, Transform destination)
			{
				this.source = source;
				this.destination = destination;
			}
		}

		private void Awake()
		{
			wrist = new Retargetable(Utils.RecursiveFindChild(sourceSkeletonRoot, retargetableSetting.wrist), Utils.RecursiveFindChild(destinationSkeletonRoot,retargetableSetting.wrist));

			retargetables = new List<Retargetable>(retargetableSetting.names.Count);
			foreach (string name in retargetableSetting.names)
			{
				retargetables.Add(new Retargetable(Utils.RecursiveFindChild(sourceSkeletonRoot, name), Utils.RecursiveFindChild(destinationSkeletonRoot, name)));
			}
		}

		private void Update()
		{
			wrist.destination.position = wrist.source.position;
			wrist.destination.rotation = wrist.source.rotation;

			foreach (Retargetable retargetable in retargetables)
			{
				retargetable.destination.rotation = retargetable.source.rotation;
			}
		}
	}
}
