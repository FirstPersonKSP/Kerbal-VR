using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.InternalModules
{
	class VRSwitch : InternalModule
	{
		[KSPField]
		public string switchTransformName = null;

		[KSPField]
		public Vector3 rotationAxis = Vector3.right;

		[KSPField]
		public float minAngle = -30;

		[KSPField]
		public float maxAngle = 30;

#if PROP_GIZMOS
		GameObject gizmo;
#endif
	}
}
