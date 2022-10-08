using Valve.VR;

namespace KerbalVR 
{
    public class SkeletonPose_FlapLeverPose
	{
		public static SteamVR_Skeleton_Pose GetInstance()
		{
			return HandProfileManager.Instance.IsKerbalHand(true) 
				? SkeletonPose_FlapLeverPose_Kerbal.GetInstance()
				: SkeletonPose_FlapLeverPose_Human.GetInstance();
		}
	}
}
