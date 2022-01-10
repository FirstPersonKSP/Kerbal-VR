using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.InternalModules
{
	class InternalCollider : InternalModule
	{
		[KSPField]
		public string shapeType = "Box";

		[KSPField]
		public Vector3 center = Vector3.zero;

		[KSPField]
		public float height; // Note: only used for Capsule primitives

		[KSPField]
		public float radius; // Note: only used for Capsule, Sphere primitives

		[KSPField]
		public Vector3 boxDimensions; // Note: only used for Box primitives

		[KSPField]
		public string parentTransformName = "";

		[KSPField]
		public string colliderTransformName = "";

		[KSPField]
		public Vector3 localRotation; // if a new transform is being created, this is its local rotation

		Collider collider;

		public override void OnAwake()
		{
			base.OnAwake();

			if (collider == null)
			{
				Transform parentTransform = parentTransformName == ""
					? internalProp.transform
					: internalProp.FindModelTransform(parentTransformName);

				if (parentTransform == null) return;

				Transform childTransform = parentTransform;

				if (colliderTransformName != "")
				{
					var existingChild = parentTransform.Find(colliderTransformName);

					if (existingChild == null)
					{
						childTransform = new GameObject(colliderTransformName).transform;
						childTransform.SetParent(parentTransform, false);
						childTransform.localRotation = Quaternion.Euler(localRotation);
					}
					else
					{
						childTransform = existingChild;
					}
				}

				if (childTransform == null) return;

				switch (shapeType)
				{
					case "Box":
						var boxCollider = childTransform.gameObject.AddComponent<BoxCollider>();
						boxCollider.center = center;
						boxCollider.size = boxDimensions;
						collider = boxCollider;
						break;
					case "Sphere":
						var sphereCollider = childTransform.gameObject.AddComponent<SphereCollider>();
						sphereCollider.center = center;
						sphereCollider.radius = radius;
						collider = sphereCollider;
						break;
					case "Capsule":
						var capsuleCollider = childTransform.gameObject.AddComponent<CapsuleCollider>();
						capsuleCollider.center = center;
						capsuleCollider.height = height;
						capsuleCollider.radius = radius;
						collider = capsuleCollider;
						break;
					default:
						Utils.LogError($"Unrecognized primitive type {shapeType} - must be Box, Sphere, or Capsule");
						return;
				}

				collider.isTrigger = true;

#if PROP_GIZMOS
				Utils.GetOrAddComponent<ColliderVisualizer>(collider.gameObject);
#endif
			}

		}
	}
}
