using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KerbalVR.InternalModules
{
	class InternalObjectDeleter : InternalModule
	{
		public string[] m_objectNames;

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			m_objectNames = node.GetValues("objectName");
		}

		private void Start()
		{
			foreach (var name in m_objectNames)
			{
				var transform = this.FindTransform(name);

				if (transform != null)
				{
					Destroy(transform.gameObject);
				}
			}
		}
	}
}
