using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR
{
	internal class KerbalVR_ArmScaler : MonoBehaviour
	{
		static readonly string[] ArmBoneNames = { "bn_l_arm01 1", "bn_r_arm01 1" };
		KerbalEVA m_eva;
		List<Transform> m_armTransforms = new List<Transform>();

		void Awake()
		{
			var skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
			foreach (var b in skinnedMeshRenderer.bones)
			{
				if (ArmBoneNames.Contains(b.name))
				{
					m_armTransforms.Add(b);
				}
			}

			m_eva = GetComponent<KerbalEVA>();
		}

		void LateUpdate()
		{
			if (Core.IsVrRunning && Scene.IsFirstPersonEVA() && Scene.GetKerbalEVA() == m_eva)
			{
				foreach (var armTransform in m_armTransforms)
				{
					armTransform.localScale = Vector3.zero;
				}
			}
		}
	}
}
