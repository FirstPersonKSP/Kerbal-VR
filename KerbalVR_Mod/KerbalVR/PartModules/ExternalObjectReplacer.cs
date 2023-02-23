using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.PartModules
{
	// TODO: this doesn't really need to be a partmodule, but we need to have some kind of behavior that can modify the part prefab
	internal class ExternalObjectReplacer : PartModule
	{
		static readonly char[] TEXTURE_REPLACEMENT_SEPARATORS = new char[3] { ':', ',', ';' };

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);

			foreach (var replacementNode in node.GetNodes("REPLACE"))
			{
				string targetTransformName = replacementNode.GetValue(nameof(targetTransformName));
				string model = replacementNode.GetValue(nameof(model));

				var targetTransform = part.FindModelTransform(targetTransformName);

				if (targetTransform == null)
				{
					Utils.LogError($"No transform named {targetTransformName} found");
					continue;
				}

				GameObject modelPrefab = GameDatabase.Instance.GetModelPrefab(model);

				if (modelPrefab == null)
				{
					Utils.LogError($"No model prefab named {model} found");
					continue;
				}

				GameObject newObject = GameObject.Instantiate(modelPrefab);
				newObject.transform.SetParent(targetTransform.parent, false);
				newObject.transform.localPosition = targetTransform.localPosition;
				newObject.transform.localRotation = targetTransform.localRotation;
				newObject.transform.localScale = targetTransform.localScale;
				newObject.name = targetTransform.name;
				newObject.SetActive(true);

				var textureReplacements = replacementNode.GetValuesList("texture");
				if (textureReplacements.Count > 0)
				{
					List<string> textureNames = new List<string>();
					List<GameDatabase.TextureInfo> newTextures = new List<GameDatabase.TextureInfo>();

					foreach (var textureReplacement in textureReplacements)
					{
						string[] array = textureReplacement.Split(TEXTURE_REPLACEMENT_SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
						
						if (array.Length != 2)
						{
							Utils.LogError($"Invalid texture replacement line: {textureReplacement}");
							continue;
						}
						
						var textureName = array[0].Trim();
						var texturePath = array[1].Trim();

						var textureInfo = GameDatabase.Instance.GetTextureInfo(texturePath);
						if (textureInfo == null)
						{
							Utils.LogError($"No texture named {texturePath} found");
							continue;
						}

						textureNames.Add(textureName);
						newTextures.Add(textureInfo);
					}

					PartLoader.Instance.ReplaceTextures(newObject, textureNames, newTextures);
				}

				GameObject.Destroy(targetTransform.gameObject);
			}
		}
	}
}
