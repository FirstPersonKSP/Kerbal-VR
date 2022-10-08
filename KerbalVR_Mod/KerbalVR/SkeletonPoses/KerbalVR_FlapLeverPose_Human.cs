/* ===============================================
 *   This is an auto-generated file for KerbalVR. 
 *   Do not edit by hand.                         
 * ===============================================
 */

using UnityEngine;
using Valve.VR;

namespace KerbalVR {
    public class SkeletonPose_FlapLeverPose_Human {
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
            pose.leftHand.position = new Vector3(-0.008267432f, -0.0020221956f, -0.087410256f);
            pose.leftHand.rotation = new Quaternion(-0.54820305f, 0.7736329f, -0.06691755f, 0.3106246f);
            pose.leftHand.bonePositions = new Vector3[31] {
                new Vector3(-0.0f, 0.0f, 0.0f),
                new Vector3(-0.034037687f, 0.03650266f, 0.16472164f),
                new Vector3(-0.016305087f, 0.027528726f, 0.017799662f),
                new Vector3(0.040405963f, -5.1561553e-08f, 4.5447194e-08f),
                new Vector3(0.032516792f, -5.1137583e-08f, -1.2933195e-08f),
                new Vector3(0.030463902f, 1.6269207e-07f, 7.92839e-08f),
                new Vector3(0.0038021537f, 0.021514153f, 0.01280332f),
                new Vector3(0.0742044f, 0.005002383f, -0.00023445487f),
                new Vector3(0.04328714f, 1.13621354e-07f, -7.55198e-07f),
                new Vector3(0.028275082f, 6.2584877e-07f, -2.9895455e-07f),
                new Vector3(0.022821384f, -1.4365155e-07f, 7.651614e-08f),
                new Vector3(0.005786922f, 0.0068064053f, 0.016533904f),
                new Vector3(0.07095288f, -0.00077883265f, -0.000997186f),
                new Vector3(0.043108486f, -9.950596e-08f, -6.7041825e-09f),
                new Vector3(0.03326598f, -1.7544496e-08f, -2.0628962e-08f),
                new Vector3(0.025892371f, 9.984198e-08f, -2.0352908e-09f),
                new Vector3(0.004123044f, -0.0068582613f, 0.016562859f),
                new Vector3(0.06587581f, -0.0017857892f, -0.00069344096f),
                new Vector3(0.040331207f, -9.449958e-08f, -2.273692e-08f),
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
                new Quaternion(0.015923718f, 0.08243829f, -0.10623138f, 0.9907903f),
                new Quaternion(0.0003115431f, -0.0013711818f, -0.30311087f, 0.9529543f),
                new Quaternion(-1.3877788e-17f, -1.3877788e-17f, -5.551115e-17f, 1.0f),
                new Quaternion(-0.6272518f, -0.5069116f, -0.44850287f, 0.38528055f),
                new Quaternion(-0.012304823f, 0.024455069f, -0.44626448f, 0.8944823f),
                new Quaternion(0.021125717f, 0.11325247f, -0.6964128f, 0.70833385f),
                new Quaternion(-0.0009852009f, 0.029271374f, -0.3772054f, 0.92566645f),
                new Quaternion(6.938894e-18f, 1.9428903e-16f, -1.348151e-33f, 1.0f),
                new Quaternion(-0.53316575f, -0.5044694f, -0.457117f, 0.50228375f),
                new Quaternion(-0.07818202f, -0.032531463f, -0.4401614f, 0.8939168f),
                new Quaternion(-0.0037368936f, -0.097925514f, -0.6841749f, 0.72270423f),
                new Quaternion(-0.06031266f, 0.020404415f, -0.50838506f, 0.8587729f),
                new Quaternion(1.1639192e-17f, -5.602331e-17f, -0.040125635f, 0.9991947f),
                new Quaternion(-0.51022935f, -0.48474824f, -0.5004526f, 0.5042147f),
                new Quaternion(-0.07881625f, 0.04171443f, -0.4307758f, 0.89804244f),
                new Quaternion(-0.004204499f, -0.039609972f, -0.68307704f, 0.7292594f),
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
            pose.rightHand.position = new Vector3(0.008267432f, -0.0020221956f, -0.087410256f);
            pose.rightHand.rotation = new Quaternion(0.54820305f, 0.7736329f, -0.06691755f, -0.3106246f);
            pose.rightHand.bonePositions = new Vector3[31] {
                new Vector3(-0.0f, 0.0f, 0.0f),
                new Vector3(-0.034037687f, 0.03650266f, 0.16472164f),
                new Vector3(-0.016305087f, 0.027528726f, 0.017799662f),
                new Vector3(0.040405963f, -5.1561553e-08f, 4.5447194e-08f),
                new Vector3(0.032516792f, -5.1137583e-08f, -1.2933195e-08f),
                new Vector3(0.030463902f, 1.6269207e-07f, 7.92839e-08f),
                new Vector3(0.0038021537f, 0.021514153f, 0.01280332f),
                new Vector3(0.0742044f, 0.005002383f, -0.00023445487f),
                new Vector3(0.04328714f, 1.13621354e-07f, -7.55198e-07f),
                new Vector3(0.028275082f, 6.2584877e-07f, -2.9895455e-07f),
                new Vector3(0.022821384f, -1.4365155e-07f, 7.651614e-08f),
                new Vector3(0.005786922f, 0.0068064053f, 0.016533904f),
                new Vector3(0.07095288f, -0.00077883265f, -0.000997186f),
                new Vector3(0.043108486f, -9.950596e-08f, -6.7041825e-09f),
                new Vector3(0.03326598f, -1.7544496e-08f, -2.0628962e-08f),
                new Vector3(0.025892371f, 9.984198e-08f, -2.0352908e-09f),
                new Vector3(0.004123044f, -0.0068582613f, 0.016562859f),
                new Vector3(0.06587581f, -0.0017857892f, -0.00069344096f),
                new Vector3(0.040331207f, -9.449958e-08f, -2.273692e-08f),
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
                new Quaternion(0.01592372f, 0.08243829f, -0.10623139f, 0.9907903f),
                new Quaternion(0.0003115431f, -0.0013711818f, -0.30311087f, 0.9529543f),
                new Quaternion(-1.3877788e-17f, -1.3877788e-17f, -5.551115e-17f, 1.0f),
                new Quaternion(-0.6272518f, -0.5069116f, -0.44850287f, 0.38528055f),
                new Quaternion(-0.012304823f, 0.024455069f, -0.44626448f, 0.8944823f),
                new Quaternion(0.021125717f, 0.11325247f, -0.6964128f, 0.70833385f),
                new Quaternion(-0.0009852009f, 0.029271374f, -0.3772054f, 0.92566645f),
                new Quaternion(6.938894e-18f, 1.9428903e-16f, -1.348151e-33f, 1.0f),
                new Quaternion(-0.53316575f, -0.5044694f, -0.457117f, 0.50228375f),
                new Quaternion(-0.07818202f, -0.032531463f, -0.4401614f, 0.8939168f),
                new Quaternion(-0.0037368936f, -0.097925514f, -0.6841749f, 0.72270423f),
                new Quaternion(-0.06031266f, 0.020404415f, -0.50838506f, 0.8587729f),
                new Quaternion(1.1639192e-17f, -5.602331e-17f, -0.040125635f, 0.9991947f),
                new Quaternion(-0.51022935f, -0.48474824f, -0.5004526f, 0.5042147f),
                new Quaternion(-0.07881624f, 0.041714426f, -0.43077576f, 0.8980424f),
                new Quaternion(-0.004204499f, -0.039609972f, -0.68307704f, 0.7292594f),
                new Quaternion(-0.027556734f, -0.0024035904f, -0.7046753f, 0.7089905f),
                new Quaternion(6.938894e-18f, -9.62965e-35f, -1.3877788e-17f, 1.0f),
                new Quaternion(-0.4926854f, -0.3897268f, -0.6201507f, 0.46988016f),
                new Quaternion(-0.064935386f, 0.09249211f, -0.38422334f, 0.91629744f),
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
