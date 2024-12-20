﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR
{
	internal struct ColliderParams
	{
#pragma warning disable 0649
		[Persistent] public string shapeType;
		[Persistent] public Vector3 center;
		[Persistent] public float height; // Note: only used for Capsule primitives
		[Persistent] public float radius; // Note: only used for Capsule, Sphere primitives
		[Persistent] public Vector3 boxDimensions; // Note: only used for Box primitives
		[Persistent] public string parentTransformName;
		[Persistent] public string colliderTransformName;
		[Persistent] public Vector3 localRotation; // if a new transform is being created, this is its local rotation
		[Persistent] public bool isTrigger;
		[Persistent] public int layer;
		[Persistent] public string tag;
#pragma warning restore 0649

		public Collider Create(Transform root, ConfigNode node)
		{
			Transform parentTransform = string.IsNullOrEmpty(parentTransformName)
					? root
					: Utils.FindTransform(root, parentTransformName);

			if (parentTransform == null)
			{
				Utils.LogError($"Unable to find transform named {parentTransformName} in transform {root.name}");
				return null;
			}

			Transform childTransform = parentTransform;

			if (!string.IsNullOrEmpty(colliderTransformName))
			{
				// return null; // no crash here

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

			if (childTransform == null) return null;

			var collider = childTransform.GetComponent<Collider>();
			if (collider == null)
			{
				switch (shapeType)
				{
					case "Box":
						var boxCollider = childTransform.gameObject.AddComponent<BoxCollider>();
						boxCollider.center = center;
						boxCollider.size = boxDimensions;
						if (boxDimensions == Vector3.zero)
						{
							Utils.LogError($"Invalid boxDimensions for collider ${colliderTransformName} in {root.name}");
						}
						collider = boxCollider;
						break;
					case "Sphere":
						var sphereCollider = childTransform.gameObject.AddComponent<SphereCollider>();
						sphereCollider.center = center;
						sphereCollider.radius = radius;
						if (radius == 0)
						{
							Utils.LogError($"Invalid radius for collider ${colliderTransformName} in {root.name}");
						}
						collider = sphereCollider;
						break;
					case "Capsule":
						var capsuleCollider = childTransform.gameObject.AddComponent<CapsuleCollider>();
						capsuleCollider.center = center;
						capsuleCollider.height = height;
						capsuleCollider.radius = radius;

						var axis = (FreeIva.ColliderUtil.CapsuleAxis)capsuleCollider.direction;
						if (node.TryGetEnum("axis", ref axis, axis))
						{
							capsuleCollider.direction = (int)axis;
						}

						if (radius == 0)
						{
							Utils.LogError($"Invalid radius for collider ${colliderTransformName} in {root.name}");
						}
						if (height == 0)
						{
							Utils.LogError($"Invalid height for collider ${colliderTransformName} in {root.name}");
						}
						collider = capsuleCollider;
						break;
					default:
						Utils.LogError($"Unrecognized primitive type {shapeType} - must be Box, Sphere, or Capsule");
						return null;
				}

				if (collider != null)
				{
					collider.isTrigger = isTrigger;
					collider.gameObject.layer = layer;

					if (!string.IsNullOrEmpty(tag))
					{
						collider.tag = tag;
					}
				}
			}

			return collider;
		}
	}
}
