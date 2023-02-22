using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.InternalModules
{
	internal static class InternalModuleExtensions
	{
		public static Transform FindTransform(this InternalModule internalModule, string nameOrPath)
		{
			Transform result = null;

			if (string.IsNullOrEmpty(nameOrPath))
			{
				return internalModule.internalProp.transform;
			}
			else if (nameOrPath.StartsWith("/") || !internalModule.internalProp.hasModel)
			{
				// try to find the path relative to the entire IVA
				var internalModelInstance = internalModule.internalModel.transform.Find("model").GetChild(0);
				result = internalModelInstance.Find(nameOrPath.TrimStart('/'));

				if (result == null)
				{
					result = internalModule.internalModel.FindModelTransform(nameOrPath);
				}
			}
			else if (nameOrPath.Contains('/'))
			{
				// try to find the path relative to the prop's model
				var modelRoot = internalModule.internalProp.transform.Find("model");
				var propModelInstance = modelRoot.GetChild(0);
				result = propModelInstance.Find(nameOrPath) ?? modelRoot.Find(nameOrPath);
			}
			else
			{
				// use KSP's default search - recursively finds the first transform with the given name
				result = internalModule.internalProp.FindModelTransform(nameOrPath);
			}

			if (result == null && !String.IsNullOrEmpty(nameOrPath))
			{
				Utils.LogError($"Unable to find transform named {nameOrPath} in iva {internalModule.internalProp.internalModel?.name} for prop {internalModule.internalProp.propName}");
			}

			return result;
		}
	}
}
