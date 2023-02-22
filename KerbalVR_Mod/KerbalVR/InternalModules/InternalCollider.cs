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
		ColliderParams colliderParams;

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			if (HighLogic.LoadedScene == GameScenes.LOADING)
			{

				object result = new ColliderParams();
				ConfigNode.LoadObjectFromConfig(result, node);
				colliderParams = (ColliderParams)result;

				// TODO: find a way to make these defaults so they can be overridden
				colliderParams.isTrigger = true;
				colliderParams.layer = 20;

				var collider = colliderParams.Create(internalProp.hasModel ? internalProp.transform : internalProp.internalModel.transform, node);

				if (collider != null)
				{
#if PROP_GIZMOS
					Utils.GetOrAddComponent<ColliderVisualizer>(collider.gameObject);
#endif
				}

				enabled = false;
			}
		}
	}
}
