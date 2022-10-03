using UnityEngine;
using System.Collections.Generic;

namespace KerbalVR
{
	public class KerbalSkeletonHelper : MonoBehaviour
	{
		private Retargetable wrist;
		private List<Retargetable> retargetables;

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

		public void Initialize(HandProfileManager.Profile profile, Transform sourceSkeletonRoot, Transform destinationSkeletonRoot)
		{
			wrist = new Retargetable(Utils.RecursiveFindChild(sourceSkeletonRoot, profile.wrist), Utils.RecursiveFindChild(destinationSkeletonRoot, profile.wrist));

			retargetables = new List<Retargetable>(profile.joints.Count);
			foreach (string name in profile.joints)
			{
				retargetables.Add(new Retargetable(Utils.RecursiveFindChild(sourceSkeletonRoot, name), Utils.RecursiveFindChild(destinationSkeletonRoot, name)));
			}
		}

		private void LateUpdate()
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
