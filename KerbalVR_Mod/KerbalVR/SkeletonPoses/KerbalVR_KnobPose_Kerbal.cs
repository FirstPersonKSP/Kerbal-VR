/* ===============================================
 *   This is an auto-generated file for KerbalVR. 
 *   Do not edit by hand.                         
 * ===============================================
 */

using UnityEngine;
using Valve.VR;

namespace KerbalVR {
    public class SkeletonPose_KnobPose_Kerbal {
        public static SteamVR_Skeleton_Pose GetInstance() {
            SteamVR_Skeleton_Pose pose = ScriptableObject.CreateInstance<SteamVR_Skeleton_Pose>();
            pose.applyToSkeletonRoot = true;
            pose.leftHand.inputSource = SteamVR_Input_Sources.LeftHand;
            pose.leftHand.thumbFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Static;
            pose.leftHand.indexFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Static;
            pose.leftHand.middleFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Extend;
            pose.leftHand.ringFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Extend;
            pose.leftHand.pinkyFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Extend;
            pose.leftHand.ignoreRootPoseData = true;
            pose.leftHand.ignoreWristPoseData = true;
            pose.leftHand.position = new Vector3(-0.0065933466f, -0.01570997f, -0.056148954f);
            pose.leftHand.rotation = new Quaternion(-0.9380342f, 0.23019055f, 0.15627712f, 0.20659529f);
            pose.leftHand.bonePositions = new Vector3[31] {
                new Vector3(-0.0f, 0.0f, 0.0f),
                new Vector3(-0.034037687f, 0.03650266f, 0.16472164f),
                new Vector3(-0.016308408f, 0.027525874f, 0.017796827f),
                new Vector3(0.040403105f, 1.097098e-06f, 1.6838312e-06f),
                new Vector3(0.032516714f, -1.5733531e-07f, 1.4877878e-07f),
                new Vector3(0.030463902f, 1.6269207e-07f, 7.92839e-08f),
                new Vector3(0.0038021489f, 0.021514187f, 0.012803366f),
                new Vector3(0.074204326f, 0.0050021997f, -0.00023406744f),
                new Vector3(0.043286677f, 5.9333324e-08f, 1.8320057e-07f),
                new Vector3(0.028275194f, -9.297885e-08f, -1.2653295e-07f),
                new Vector3(0.022821384f, -1.4365155e-07f, 7.651614e-08f),
                new Vector3(0.005786922f, 0.0068064053f, 0.016533904f),
                new Vector3(0.07094864f, -0.00077799754f, -0.000998877f),
                new Vector3(0.043108486f, -9.950596e-08f, -6.7041825e-09f),
                new Vector3(0.03326598f, -1.7544496e-08f, -2.0628962e-08f),
                new Vector3(0.025892371f, 9.984198e-08f, -2.0352908e-09f),
                new Vector3(0.004123044f, -0.0068582613f, 0.016562859f),
                new Vector3(0.06587544f, -0.0017856425f, -0.00069358945f),
                new Vector3(0.040331207f, -9.449958e-08f, -2.273692e-08f),
                new Vector3(0.028488781f, 1.01152565e-07f, 4.5493586e-08f),
                new Vector3(0.022430236f, 1.0846127e-07f, -1.7428562e-08f),
                new Vector3(0.0011314574f, -0.019294508f, 0.01542875f),
                new Vector3(0.062878005f, -0.00284403f, -0.00033179298f),
                new Vector3(0.029874247f, -3.4247638e-08f, -9.126629e-08f),
                new Vector3(0.017978692f, -2.8448923e-09f, -2.0797508e-07f),
                new Vector3(0.01801794f, -2.00012e-08f, 6.59746e-08f),
                new Vector3(0.019716311f, 0.002801723f, 0.093936935f),
                new Vector3(-0.0075385696f, 0.01764465f, 0.10240429f),
                new Vector3(-0.0031984635f, 0.0072115273f, 0.11665362f),
                new Vector3(2.6269245e-05f, -0.007118772f, 0.13072418f),
                new Vector3(-0.0018780098f, -0.02256182f, 0.14003526f),
            };
            pose.leftHand.boneRotations = new Quaternion[31] {
                new Quaternion(-6.123234e-17f, 1.0f, 6.123234e-17f, -4.371139e-08f),
                new Quaternion(-0.078608155f, -0.92027926f, 0.3792963f, -0.055146642f),
                new Quaternion(-0.404508f, -0.72348076f, 0.18921566f, 0.526447f),
                new Quaternion(-0.0093529215f, 0.11486061f, -0.31798467f, 0.9410661f),
                new Quaternion(0.00026501055f, -0.0013811882f, -0.27068567f, 0.9626668f),
                new Quaternion(-1.3877788e-17f, -1.3877788e-17f, -5.551115e-17f, 1.0f),
                new Quaternion(-0.6173145f, -0.44918522f, -0.5108743f, 0.39517453f),
                new Quaternion(-0.011441967f, 0.118833154f, -0.5260608f, 0.842026f),
                new Quaternion(-0.0005700487f, 0.115204416f, -0.81729656f, 0.56458294f),
                new Quaternion(-0.010756178f, 0.027241308f, -0.66610956f, 0.7452787f),
                new Quaternion(6.938894e-18f, 1.9428903e-16f, -1.348151e-33f, 1.0f),
                new Quaternion(-0.5142028f, -0.4836996f, -0.47834843f, 0.522315f),
                new Quaternion(-0.10113799f, -0.04138155f, -0.6276664f, 0.7707747f),
                new Quaternion(0.0076794685f, -0.09769542f, -0.7635977f, 0.6382125f),
                new Quaternion(-0.06366954f, 0.00036316764f, -0.7530614f, 0.6548623f),
                new Quaternion(1.1639192e-17f, -5.602331e-17f, -0.040125635f, 0.9991947f),
                new Quaternion(-0.489609f, -0.46399677f, -0.52064353f, 0.523374f),
                new Quaternion(-0.085860096f, 0.024085924f, -0.61110616f, 0.7865096f),
                new Quaternion(-0.0005935501f, -0.039828163f, -0.74642265f, 0.66427904f),
                new Quaternion(-0.027121458f, -0.005438834f, -0.7788164f, 0.62664175f),
                new Quaternion(6.938894e-18f, -9.62965e-35f, -1.3877788e-17f, 1.0f),
                new Quaternion(-0.47976637f, -0.37993452f, -0.63019824f, 0.47783276f),
                new Quaternion(-0.08508587f, 0.0743764f, -0.5912949f, 0.7984979f),
                new Quaternion(0.00313052f, 0.03775632f, -0.7113834f, 0.7017823f),
                new Quaternion(-0.008087321f, -0.003009417f, -0.7361885f, 0.6767216f),
                new Quaternion(0.0f, 0.0f, 1.9081958e-17f, 1.0f),
                new Quaternion(-0.54886997f, 0.1177861f, -0.7578353f, 0.33249632f),
                new Quaternion(0.13243657f, -0.8730836f, -0.45493412f, -0.114980996f),
                new Quaternion(0.17098099f, -0.92266804f, -0.34507802f, -0.019245595f),
                new Quaternion(0.15011512f, -0.952169f, -0.25831383f, -0.064137466f),
                new Quaternion(0.07684197f, -0.97957754f, -0.18576658f, -0.0037347008f),
            };
            pose.rightHand.inputSource = SteamVR_Input_Sources.RightHand;
            pose.rightHand.thumbFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Static;
            pose.rightHand.indexFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Static;
            pose.rightHand.middleFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Extend;
            pose.rightHand.ringFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Extend;
            pose.rightHand.pinkyFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Extend;
            pose.rightHand.ignoreRootPoseData = true;
            pose.rightHand.ignoreWristPoseData = true;
            pose.rightHand.position = new Vector3(0.0060321167f, -0.016053978f, -0.05645269f);
            pose.rightHand.rotation = new Quaternion(-0.23012567f, 0.938079f, 0.20657897f, 0.15612544f);
            pose.rightHand.bonePositions = new Vector3[31] {
                new Vector3(-0.0f, 0.0f, 0.0f),
                new Vector3(-0.034037687f, 0.03650266f, 0.16472164f),
                new Vector3(-0.016308408f, 0.027525874f, 0.017796827f),
                new Vector3(0.040403105f, 1.097098e-06f, 1.6838312e-06f),
                new Vector3(0.032516714f, -1.5733531e-07f, 1.4877878e-07f),
                new Vector3(0.030463902f, 1.6269207e-07f, 7.92839e-08f),
                new Vector3(0.0038021489f, 0.021514187f, 0.012803366f),
                new Vector3(0.074204326f, 0.0050021997f, -0.00023406744f),
                new Vector3(0.043286677f, 5.9333324e-08f, 1.8320057e-07f),
                new Vector3(0.028275194f, -9.297885e-08f, -1.2653295e-07f),
                new Vector3(0.022821384f, -1.4365155e-07f, 7.651614e-08f),
                new Vector3(0.005786922f, 0.0068064053f, 0.016533904f),
                new Vector3(0.07094864f, -0.00077799754f, -0.000998877f),
                new Vector3(0.043108486f, -9.950596e-08f, -6.7041825e-09f),
                new Vector3(0.03326598f, -1.7544496e-08f, -2.0628962e-08f),
                new Vector3(0.025892371f, 9.984198e-08f, -2.0352908e-09f),
                new Vector3(0.004123044f, -0.0068582613f, 0.016562859f),
                new Vector3(0.06587544f, -0.0017856425f, -0.00069358945f),
                new Vector3(0.040331207f, -9.449958e-08f, -2.273692e-08f),
                new Vector3(0.028488781f, 1.01152565e-07f, 4.5493586e-08f),
                new Vector3(0.022430236f, 1.0846127e-07f, -1.7428562e-08f),
                new Vector3(0.0011314574f, -0.019294508f, 0.01542875f),
                new Vector3(0.062878005f, -0.00284403f, -0.00033179298f),
                new Vector3(0.029874247f, -3.4247638e-08f, -9.126629e-08f),
                new Vector3(0.017978692f, -2.8448923e-09f, -2.0797508e-07f),
                new Vector3(0.01801794f, -2.00012e-08f, 6.59746e-08f),
                new Vector3(0.019716311f, 0.002801723f, 0.093936935f),
                new Vector3(-0.0075385696f, 0.01764465f, 0.10240429f),
                new Vector3(-0.0031984635f, 0.0072115273f, 0.11665362f),
                new Vector3(2.6269245e-05f, -0.007118772f, 0.13072418f),
                new Vector3(-0.0018780098f, -0.02256182f, 0.14003526f),
            };
            pose.rightHand.boneRotations = new Quaternion[31] {
                new Quaternion(-6.123234e-17f, 1.0f, 6.123234e-17f, -4.371139e-08f),
                new Quaternion(-0.078608155f, -0.92027926f, 0.3792963f, -0.055146642f),
                new Quaternion(-0.404508f, -0.72348076f, 0.18921566f, 0.526447f),
                new Quaternion(-0.0093529215f, 0.11486061f, -0.31798467f, 0.9410661f),
                new Quaternion(0.00026501055f, -0.0013811882f, -0.27068567f, 0.9626668f),
                new Quaternion(-1.3877788e-17f, -1.3877788e-17f, -5.551115e-17f, 1.0f),
                new Quaternion(-0.6173145f, -0.44918522f, -0.5108743f, 0.39517453f),
                new Quaternion(-0.011441967f, 0.118833154f, -0.5260608f, 0.842026f),
                new Quaternion(-0.0005700487f, 0.115204416f, -0.81729656f, 0.56458294f),
                new Quaternion(-0.010756178f, 0.027241308f, -0.66610956f, 0.7452787f),
                new Quaternion(6.938894e-18f, 1.9428903e-16f, -1.348151e-33f, 1.0f),
                new Quaternion(-0.5142028f, -0.4836996f, -0.47834843f, 0.522315f),
                new Quaternion(-0.10113799f, -0.04138155f, -0.6276664f, 0.7707747f),
                new Quaternion(0.0076794685f, -0.09769542f, -0.7635977f, 0.6382125f),
                new Quaternion(-0.06366954f, 0.00036316764f, -0.7530614f, 0.6548623f),
                new Quaternion(1.1639192e-17f, -5.602331e-17f, -0.040125635f, 0.9991947f),
                new Quaternion(-0.489609f, -0.46399677f, -0.52064353f, 0.523374f),
                new Quaternion(-0.085860096f, 0.024085924f, -0.61110616f, 0.7865096f),
                new Quaternion(-0.0005935501f, -0.039828163f, -0.74642265f, 0.66427904f),
                new Quaternion(-0.027121458f, -0.005438834f, -0.7788164f, 0.62664175f),
                new Quaternion(6.938894e-18f, -9.62965e-35f, -1.3877788e-17f, 1.0f),
                new Quaternion(-0.47976637f, -0.37993452f, -0.63019824f, 0.47783276f),
                new Quaternion(-0.08508587f, 0.0743764f, -0.5912949f, 0.7984979f),
                new Quaternion(0.00313052f, 0.03775632f, -0.7113834f, 0.7017823f),
                new Quaternion(-0.008087321f, -0.003009417f, -0.7361885f, 0.6767216f),
                new Quaternion(0.0f, 0.0f, 1.9081958e-17f, 1.0f),
                new Quaternion(-0.54886997f, 0.1177861f, -0.7578353f, 0.33249632f),
                new Quaternion(0.13243657f, -0.8730836f, -0.45493412f, -0.114980996f),
                new Quaternion(0.17098099f, -0.92266804f, -0.34507802f, -0.019245595f),
                new Quaternion(0.15011512f, -0.952169f, -0.25831383f, -0.064137466f),
                new Quaternion(0.07684197f, -0.97957754f, -0.18576658f, -0.0037347008f),
            };
            return pose;
        }
    }
}
