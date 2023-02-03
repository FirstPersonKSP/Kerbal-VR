using UnityEngine;
using Valve.VR;

namespace KerbalVR
{
	public class InteractableBehaviour : MonoBehaviour
	{
		/// <summary>
		/// The skeleton poses that will be applied when interacting with this object.
		/// </summary>
		public SteamVR_Skeleton_Poser SkeletonPoser { get; set; }

		/// <summary>
		/// If true, this object is currently being grabbed by a hand.
		/// False, otherwise.
		/// </summary>
		public bool IsGrabbed
		{
			get { return GrabbedHand != null; }
		}

		/// <summary>
		/// The Hand object that is grabbing this object. Null, if no
		/// hand is grabbing this object.
		/// </summary>
		public Hand GrabbedHand
		{
			get { return lastHand; }
			set
			{
				if (lastHand != value)
				{
					if (lastHand != null && value != null)
					{
						if (OnOtherHandGrab != null)
						{
							OnOtherHandGrab.Invoke(value);
						}
					}
					else if (lastHand != null)
					{
						if (OnRelease != null)
						{
							OnRelease.Invoke(lastHand);
						}
					}
					lastHand = value;
					if (value != null && OnGrab != null)
					{
						OnGrab.Invoke(value);
					}
				}
			}
		}
		private Hand lastHand;

		public delegate void GrabDelegate(Hand hand);
		public GrabDelegate OnGrab;
		public GrabDelegate OnRelease;
		public GrabDelegate OnOtherHandGrab;
	}
}
