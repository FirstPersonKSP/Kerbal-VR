using Valve.VR;

namespace KerbalVR 
{
    public class SkeletonPose_GearLeverPose
	{
		public static SteamVR_Skeleton_Pose GetInstance()
		{
			return HandProfileManager.Instance.IsKerbalHand(true) 
				? SkeletonPose_GearLeverPose_Kerbal.GetInstance()
				: SkeletonPose_GearLeverPose_Human.GetInstance();
		}
	}
}
