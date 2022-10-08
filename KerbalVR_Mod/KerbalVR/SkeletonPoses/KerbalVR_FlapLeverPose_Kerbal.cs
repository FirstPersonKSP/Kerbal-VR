/* ===============================================
 *   This is an auto-generated file for KerbalVR. 
 *   Do not edit by hand.                         
 * ===============================================
 */

using UnityEngine;
using Valve.VR;

namespace KerbalVR {
    public class SkeletonPose_FlapLeverPose_Kerbal {
        public static SteamVR_Skeleton_Pose GetInstance() {
            SteamVR_Skeleton_Pose pose = ScriptableObject.CreateInstance<SteamVR_Skeleton_Pose>();
            pose.applyToSkeletonRoot = true;
            pose.leftHand.inputSource = SteamVR_Input_Sources.LeftHand;
            pose.leftHand.thumbFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Extend;
            pose.leftHand.indexFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Static;
            pose.leftHand.middleFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Static;
            pose.leftHand.ringFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Extend;
            pose.leftHand.pinkyFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Extend;
            pose.leftHand.ignoreRootPoseData = true;
            pose.leftHand.ignoreWristPoseData = true;
            pose.leftHand.position = new Vector3(-0.014504595f, -0.018290346f, -0.078659855f);
            pose.leftHand.rotation = new Quaternion(-0.5646052f, 0.73248154f, -0.020919688f, 0.3798083f);
            pose.leftHand.bonePositions = new Vector3[31] {
                new Vector3(-0.0f, 0.0f, 0.0f),
                new Vector3(-0.034037687f, 0.03650266f, 0.16472164f),
                new Vector3(-0.016305087f, 0.027528726f, 0.017799662f),
                new Vector3(0.04040411f, -8.1956387e-07f, 1.8961728e-06f),
                new Vector3(0.032516792f, -5.1137583e-08f, -1.2933195e-08f),
                new Vector3(0.030463902f, 1.6269207e-07f, 7.92839e-08f),
                new Vector3(0.0038021537f, 0.021514153f, 0.01280332f),
                new Vector3(0.07420434f, 0.0050025005f, -0.00023509562f),
                new Vector3(0.043287136f, 1.3038516e-07f, -8.260831e-07f),
                new Vector3(0.028275082f, 6.2584877e-07f, -2.9895455e-07f),
                new Vector3(0.022821384f, -1.4365155e-07f, 7.651614e-08f),
                new Vector3(0.0057867505f, 0.0068061496f, 0.016533677f),
                new Vector3(0.07095276f, -0.0007789526f, -0.0009976402f),
                new Vector3(0.043108463f, -8.568168e-08f, 1.1641532e-08f),
                new Vector3(0.03326598f, -1.7544496e-08f, -2.0628962e-08f),
                new Vector3(0.025892371f, 9.984198e-08f, -2.0352908e-09f),
                new Vector3(0.0041222945f, -0.0068591386f, 0.016562505f),
                new Vector3(0.06587541f, -0.0017873449f, -0.0006982684f),
                new Vector3(0.04033173f, -4.33065e-07f, -1.5227124e-06f),
                new Vector3(0.028488781f, 1.01152565e-07f, 4.5493586e-08f),
                new Vector3(0.022430236f, 1.0846127e-07f, -1.7428562e-08f),
                new Vector3(0.0011314574f, -0.019294508f, 0.01542875f),
                new Vector3(0.0628784f, -0.0028440945f, -0.0003315112f),
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
                new Quaternion(-0.3585664f, -0.68047774f, 0.18095611f, 0.61289084f),
                new Quaternion(0.07268018f, 0.06751024f, -0.18202181f, 0.97827816f),
                new Quaternion(0.0003115431f, -0.0013711818f, -0.30311087f, 0.9529543f),
                new Quaternion(-1.3877788e-17f, -1.3877788e-17f, -5.551115e-17f, 1.0f),
                new Quaternion(-0.6272518f, -0.5069116f, -0.44850287f, 0.38528055f),
                new Quaternion(-0.13862298f, 0.07948979f, -0.4672699f, 0.8695538f),
                new Quaternion(0.011292377f, 0.11950289f, -0.7159537f, 0.6877513f),
                new Quaternion(-0.0009852009f, 0.029271374f, -0.3772054f, 0.92566645f),
                new Quaternion(6.938894e-18f, 1.9428903e-16f, -1.348151e-33f, 1.0f),
                new Quaternion(-0.54437625f, -0.5050685f, -0.45655742f, 0.4900161f),
                new Quaternion(0.01126574f, -0.08199929f, -0.37974033f, 0.92138296f),
                new Quaternion(-0.028051848f, -0.07601203f, -0.7434372f, 0.66387993f),
                new Quaternion(-0.06031266f, 0.020404415f, -0.50838506f, 0.8587729f),
                new Quaternion(1.1639192e-17f, -5.602331e-17f, -0.040125635f, 0.9991947f),
                new Quaternion(-0.5496514f, -0.48921078f, -0.49861708f, 0.45818904f),
                new Quaternion(0.10546419f, 0.024266804f, -0.38748458f, 0.91550213f),
                new Quaternion(0.01033391f, 0.009739143f, -0.7796215f, 0.62609005f),
                new Quaternion(-0.027556734f, -0.0024035904f, -0.7046753f, 0.7089905f),
                new Quaternion(6.938894e-18f, -9.62965e-35f, -1.3877788e-17f, 1.0f),
                new Quaternion(-0.4926854f, -0.3897268f, -0.6201507f, 0.46988016f),
                new Quaternion(-0.06493539f, 0.09249212f, -0.38422337f, 0.9162975f),
                new Quaternion(0.00313052f, 0.03775632f, -0.7113834f, 0.7017823f),
                new Quaternion(-0.008416769f, -0.0019025599f, -0.6392362f, 0.7689621f),
                new Quaternion(0.0f, 0.0f, 1.9081958e-17f, 1.0f),
                new Quaternion(-0.54886997f, 0.1177861f, -0.7578353f, 0.33249632f),
                new Quaternion(0.13243657f, -0.8730836f, -0.45493412f, -0.114980996f),
                new Quaternion(0.17098099f, -0.92266804f, -0.34507802f, -0.019245595f),
                new Quaternion(0.15011512f, -0.952169f, -0.25831383f, -0.064137466f),
                new Quaternion(0.07684197f, -0.97957754f, -0.18576658f, -0.0037347008f),
            };
            pose.rightHand.inputSource = SteamVR_Input_Sources.RightHand;
            pose.rightHand.thumbFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Extend;
            pose.rightHand.indexFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Static;
            pose.rightHand.middleFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Static;
            pose.rightHand.ringFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Extend;
            pose.rightHand.pinkyFingerMovementType = SteamVR_Skeleton_FingerExtensionTypes.Extend;
            pose.rightHand.ignoreRootPoseData = true;
            pose.rightHand.ignoreWristPoseData = true;
            pose.rightHand.position = new Vector3(0.014504595f, -0.018290346f, -0.078659855f);
            pose.rightHand.rotation = new Quaternion(0.5646052f, 0.73248154f, -0.020919688f, -0.3798083f);
            pose.rightHand.bonePositions = new Vector3[31] {
                new Vector3(-0.0f, 0.0f, 0.0f),
                new Vector3(-0.034037687f, 0.03650266f, 0.16472164f),
                new Vector3(-0.016305087f, 0.027528726f, 0.017799662f),
                new Vector3(0.04040411f, -8.1956387e-07f, 1.8961728e-06f),
                new Vector3(0.032516792f, -5.1137583e-08f, -1.2933195e-08f),
                new Vector3(0.030463902f, 1.6269207e-07f, 7.92839e-08f),
                new Vector3(0.0038021537f, 0.021514153f, 0.01280332f),
                new Vector3(0.07420434f, 0.0050025005f, -0.00023509562f),
                new Vector3(0.043287136f, 1.3038516e-07f, -8.260831e-07f),
                new Vector3(0.028275082f, 6.2584877e-07f, -2.9895455e-07f),
                new Vector3(0.022821384f, -1.4365155e-07f, 7.651614e-08f),
                new Vector3(0.0057867505f, 0.0068061496f, 0.016533677f),
                new Vector3(0.07095276f, -0.0007789526f, -0.0009976402f),
                new Vector3(0.043108463f, -8.568168e-08f, 1.1641532e-08f),
                new Vector3(0.03326598f, -1.7544496e-08f, -2.0628962e-08f),
                new Vector3(0.025892371f, 9.984198e-08f, -2.0352908e-09f),
                new Vector3(0.0041222945f, -0.0068591386f, 0.016562505f),
                new Vector3(0.06587541f, -0.0017873449f, -0.0006982684f),
                new Vector3(0.04033173f, -4.33065e-07f, -1.5227124e-06f),
                new Vector3(0.028488781f, 1.01152565e-07f, 4.5493586e-08f),
                new Vector3(0.022430236f, 1.0846127e-07f, -1.7428562e-08f),
                new Vector3(0.0011314574f, -0.019294508f, 0.01542875f),
                new Vector3(0.0628784f, -0.0028440945f, -0.0003315112f),
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
                new Quaternion(-0.3585664f, -0.68047774f, 0.18095611f, 0.61289084f),
                new Quaternion(0.07268018f, 0.06751024f, -0.18202181f, 0.97827816f),
                new Quaternion(0.0003115431f, -0.0013711818f, -0.30311087f, 0.9529543f),
                new Quaternion(-1.3877788e-17f, -1.3877788e-17f, -5.551115e-17f, 1.0f),
                new Quaternion(-0.6272518f, -0.5069116f, -0.44850287f, 0.38528055f),
                new Quaternion(-0.13862298f, 0.07948979f, -0.4672699f, 0.8695538f),
                new Quaternion(0.011292377f, 0.11950289f, -0.7159537f, 0.6877513f),
                new Quaternion(-0.0009852009f, 0.029271374f, -0.3772054f, 0.92566645f),
                new Quaternion(6.938894e-18f, 1.9428903e-16f, -1.348151e-33f, 1.0f),
                new Quaternion(-0.54437625f, -0.5050685f, -0.45655742f, 0.4900161f),
                new Quaternion(0.011265739f, -0.08199928f, -0.3797403f, 0.9213829f),
                new Quaternion(-0.028051848f, -0.07601203f, -0.7434372f, 0.66387993f),
                new Quaternion(-0.06031266f, 0.020404415f, -0.50838506f, 0.8587729f),
                new Quaternion(1.1639192e-17f, -5.602331e-17f, -0.040125635f, 0.9991947f),
                new Quaternion(-0.5496514f, -0.48921075f, -0.49861708f, 0.458189f),
                new Quaternion(0.10546419f, 0.024266804f, -0.38748458f, 0.91550213f),
                new Quaternion(0.01033391f, 0.009739143f, -0.7796215f, 0.62609005f),
                new Quaternion(-0.027556734f, -0.0024035904f, -0.7046753f, 0.7089905f),
                new Quaternion(6.938894e-18f, -9.62965e-35f, -1.3877788e-17f, 1.0f),
                new Quaternion(-0.4926854f, -0.3897268f, -0.6201507f, 0.46988016f),
                new Quaternion(-0.06493539f, 0.09249212f, -0.38422337f, 0.9162975f),
                new Quaternion(0.00313052f, 0.03775632f, -0.7113834f, 0.7017823f),
                new Quaternion(-0.008416769f, -0.0019025599f, -0.6392362f, 0.7689621f),
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
