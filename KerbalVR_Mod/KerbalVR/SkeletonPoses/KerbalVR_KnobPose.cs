using Valve.VR;

namespace KerbalVR 
{
    public class SkeletonPose_KnobPose
	{
		public static SteamVR_Skeleton_Pose GetInstance()
		{
			return HandProfileManager.Instance.IsKerbalHand(true) 
				? SkeletonPose_KnobPose_Kerbal.GetInstance()
				: SkeletonPose_KnobPose_Human.GetInstance();
		}
	}
}
