using UnityEngine;
using System.Collections;
using Valve.VR;
using UnityEngine.Serialization;

public class KerbalSkeletonHelper : MonoBehaviour
{
	public Retargetable wrist;

	public Thumb thumb;
	public Finger[] fingers = new Finger[3];

	private void Update()
	{
		thumb.Align();

		for (int fingerIndex = 0; fingerIndex < fingers.Length; fingerIndex++)
		{
			fingers[fingerIndex].Align();
		}

		wrist.destination.position = wrist.source.position;
		wrist.destination.rotation = wrist.source.rotation;
	}

	public enum MirrorType
	{
		None,
		LeftToRight,
		RightToLeft
	}

	[System.Serializable]
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

	[System.Serializable]
	public class Thumb
	{
		public Retargetable proximal;
		public Retargetable middle;

		public Thumb(Retargetable proximal, Retargetable middle)
		{
			this.proximal = proximal;
			this.middle = middle;
		}

		public void Align()
		{
			proximal.destination.rotation = proximal.source.rotation;
			middle.destination.rotation = middle.source.rotation;
		}
	}

	[System.Serializable]
	public class Finger
	{
		public Retargetable metacarpal;
		public Retargetable proximal;
		public Retargetable middle;

		public Finger(Retargetable metacarpal, Retargetable proximal, Retargetable middle)
		{
			this.metacarpal = metacarpal;
			this.proximal = proximal;
			this.middle = middle;
		}

		public void Align()
		{
			metacarpal.destination.rotation = metacarpal.source.rotation;
			proximal.destination.rotation = proximal.source.rotation;
			middle.destination.rotation = middle.source.rotation;
		}
	}
}