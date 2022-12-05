using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ETModel
{
	public static partial class ResourcesHelper
	{
		public static Sprite LoadSprite(string toyDirName,string atlasName, string spriteName)
		{
			return (ResourcesComponent.Instance.LoadAsset($"{toyDirName}/Atlas/{atlasName}", typeof(SpriteAtlas)) as SpriteAtlas).GetSprite(spriteName);
		}

		public static Sprite LoadSpriteLan(string toyDirName, string atlasName, string spriteName)
		{
			return (ResourcesComponent.Instance.LoadAsset($"{toyDirName}/Atlas/{atlasName}", typeof(SpriteAtlas)) as SpriteAtlas).GetSprite($"{spriteName}_{LanguageHelper.GetCurrentLan()}");
		}
	}
}
