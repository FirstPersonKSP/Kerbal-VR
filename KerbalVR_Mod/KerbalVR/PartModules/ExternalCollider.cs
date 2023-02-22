using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KerbalVR
{
	public class ExternalCollider : PartModule
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

				colliderParams.Create(part.transform, node);
			}

			enabled = false;
		}
	}
}
