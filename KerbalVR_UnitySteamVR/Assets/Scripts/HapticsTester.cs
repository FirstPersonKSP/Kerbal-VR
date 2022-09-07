using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class HapticsTester : MonoBehaviour
{
	[Range(0f, 1f)]
	public float duration = 0.005f;

	[Range(0f, 320f)]
	public float frequncy = 0.005f;

	[Range(0f, 1f)]
	public float amplitude = 1.0f;

	private readonly SteamVR_Action_Vibration haptic = SteamVR_Actions.default_Haptic;
	private Material material;

	protected void Awake()
	{
		material = GetComponent<MeshRenderer>().material;
		material.color = Color.white;
	}

	protected void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "finger_index_2_r")
		{
			Pulse(HasParentWithName(other.transform, "HandColliderRight(Clone)") ? SteamVR_Input_Sources.RightHand : SteamVR_Input_Sources.LeftHand);

			material.color = Color.green;
		}
	}

	protected void OnTriggerExit(Collider other)
	{
		material.color = Color.white;
	}

	public void Pulse(SteamVR_Input_Sources inputSource)
	{
		haptic.Execute(0f, duration, frequncy, amplitude, inputSource);
	}

	private bool HasParentWithName(Transform transform, string name)
	{
		if (!transform.parent)
		{
			return false;
		}

		if (transform.parent.name == name)
		{
			return true;
		}
		else
		{
			return HasParentWithName(transform.parent, name);
		}
	}
}
