using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ETModel
{
	// 用于字符串转换，减少GC
	public static class AssetBundleHelper
	{

		public static readonly Dictionary<string, string> BundleNameToLowerDict = new Dictionary<string, string>(){{ "StreamingAssets", "StreamingAssets" }};

		// 缓存包依赖，不用每次计算
		public static Dictionary<string, string[]> DependenciesCache = new Dictionary<string, string[]>();


#if UNITY_EDITOR
		/// <summary>
		/// 直接加载工程目录下的资源文件中的资源对象，因为加载必须要后缀名所以在这里处理后缀问题，
		/// 因为资源系统是按照一个AB对应一个文件设计的，这样设置对于开发更为直观高校，所以应该确保资源文件名不要重复
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static UnityEngine.Object LoadResource(string path,Type type)
		{
			UnityEngine.Object _object = null;
			string _fullPath = PathHelper.Combine(PathHelper.EditorResPath, path);
			switch (type.Name)
			{
				case "GameObject":
					return AssetDatabase.LoadAssetAtPath($"{_fullPath}.prefab", type);
				case "SpriteAtlas":
					return AssetDatabase.LoadAssetAtPath($"{_fullPath}.spriteatlas", type); 
				case "Sprite":
					_object= AssetDatabase.LoadAssetAtPath($"{_fullPath}.png", type);
					if(_object==null) _object = AssetDatabase.LoadAssetAtPath($"{_fullPath}.jpg", type);
					return _object;
				case "TextAsset":
					_object = AssetDatabase.LoadAssetAtPath($"{_fullPath}.txt", type);
					if (_object == null) _object = AssetDatabase.LoadAssetAtPath($"{_fullPath}.bytes", type);
					return _object;
				case "AudioClip":
					return AssetDatabase.LoadAssetAtPath($"{_fullPath}.ogg", type); 
				case "Scene":
					return null;// AssetDatabase.LoadAssetAtPath($"{_fullPath}.unity", type);
				case "Material":
					return AssetDatabase.LoadAssetAtPath($"{_fullPath}.mat", type);
				case "VideoClip":
					return AssetDatabase.LoadAssetAtPath($"{_fullPath}.mp4", type);
				case "RuntimeAnimatorController":
					return AssetDatabase.LoadAssetAtPath($"{_fullPath}.controller", type);
				case "TMP_FontAsset":
					return AssetDatabase.LoadAssetAtPath($"{_fullPath}.asset", type);
				case "TMP_SpriteAsset":
					return AssetDatabase.LoadAssetAtPath($"{_fullPath}.asset", type);
				case "PlayableAsset":
					return AssetDatabase.LoadAssetAtPath($"{_fullPath}.playable", type);
				default:
					return null;
			}
		}
#endif

		public static string BundleNameToLower(this string assetBundleName)
		{
			string result;
			if (BundleNameToLowerDict.TryGetValue(assetBundleName, out result))
			{
				return result;
			}

			result = assetBundleName.ToLower();
			BundleNameToLowerDict[assetBundleName] = result;
			return result;
		}

		public static string[] GetDependencies(string assetBundleName, AssetBundleManifest assetBundleManifest)
		{
			string[] dependencies = new string[0];
			if (DependenciesCache.TryGetValue(assetBundleName, out dependencies))
			{
				return dependencies;
			}
			dependencies = assetBundleManifest.GetAllDependencies(assetBundleName);
			DependenciesCache.Add(assetBundleName, dependencies);
			return dependencies;
		}

		public static string[] GetSortedDependencies(string assetBundleName, AssetBundleManifest assetBundleManifest)
		{
			Dictionary<string, int> info = new Dictionary<string, int>();
			List<string> parents = new List<string>();
			CollectDependencies(parents, assetBundleName, info,assetBundleManifest);
			string[] ss = info.OrderBy(x => x.Value).Select(x => x.Key).ToArray();
			return ss;
		}

		public static void CollectDependencies(List<string> parents, string assetBundleName, Dictionary<string, int> info, AssetBundleManifest assetBundleManifest)
		{
			parents.Add(assetBundleName);
			string[] deps = GetDependencies(assetBundleName, assetBundleManifest);
			foreach (string parent in parents)
			{
				if (!info.ContainsKey(parent))
				{
					info[parent] = 0;
				}
				info[parent] += deps.Length;
			}


			foreach (string dep in deps)
			{
				if (parents.Contains(dep))
				{
					throw new Exception($"包有循环依赖，请重新标记: {assetBundleName} {dep}");
				}
				CollectDependencies(parents, dep, info, assetBundleManifest);
			}
			parents.RemoveAt(parents.Count - 1);
		}

		public static string GetWithOutPathPatten(string pathPrefix)
		{
			return $"^(?!{pathPrefix.ToLower()}).*$";
		}

		public static string GetWithPathPatten(string pathPrefix)
		{
			return $"^({pathPrefix.ToLower()}).*$";
		}
	}
}
