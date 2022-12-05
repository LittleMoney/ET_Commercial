using System;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
	[ObjectSystem]
	public class ConfigComponentAwakeSystem : AwakeSystem<ConfigComponent>
	{
		public override void Awake(ConfigComponent self)
		{
			ConfigComponent.Instance = self;
			self.Awake();
		}
	}


	/// <summary>
	/// Config组件会扫描所有的有ConfigAttribute标签的配置,加载进来
	/// </summary>
	public class ConfigComponent: Component
	{
		public static ConfigComponent Instance;

		private Dictionary<string, ACategory> allConfigDict = new Dictionary<string, ACategory>();

		public void Awake()
		{

		}

		/// <summary>
		/// 异步加载配置
		/// </summary>
		/// <param name="path">配置子类型为路径的文件名不要后缀</param>
		/// <returns></returns>
		public  ACategory Load(string path)
		{
			ACategory _acategory = null;

			if (allConfigDict.TryGetValue(path, out _acategory)) return _acategory;

			TextAsset _textAsset =  ResourcesComponent.Instance.LoadAsset(path, typeof(TextAsset)) as TextAsset;

			{
				Type _type = Type.GetType($"ETModel.{path.GetPureFileName()}Category");

				if (_type == null || !typeof(ACategory).IsAssignableFrom(_type))
				{
					throw new Exception("无法解析的配置资源");
				}

				object[] attrs = _type.GetCustomAttributes(typeof(ConfigAttribute), false);
				if (attrs.Length == 0)
				{
					throw new Exception("无法解析的配置资源");
				}

				ConfigAttribute configAttribute = attrs[0] as ConfigAttribute;

				// 只加载指定的配置
				if (!configAttribute.Type.Is(AppType.ClientM))
				{
					throw new Exception("无法解析的配置资源");
				}

				object obj = Activator.CreateInstance(_type);

				ACategory iCategory = obj as ACategory;
				if (iCategory == null)
				{
					throw new Exception($"class: {_type.Name} not inherit from ACategory");
				}
				iCategory.BeginInit(_textAsset.text);
				iCategory.EndInit();

				_acategory = iCategory;
			}

			ResourcesComponent.Instance.UnloadBundle(path);

			this.allConfigDict[path] = _acategory;

			return _acategory;
		}

		/// <summary>
		/// 异步加载配置
		/// </summary>
		/// <param name="path">配置子类型为路径的文件名不要后缀</param>
		/// <returns></returns>
		public async ETTask<ACategory> LoadAsync(string path)
		{
			ACategory _acategory = null;

			if (allConfigDict.TryGetValue(path,out _acategory)) return _acategory;

			string _text = (await ResourcesComponent.Instance.LoadAssetAsync(path, typeof(TextAsset)) as TextAsset).text;

			await System.Threading.Tasks.Task.Run(() =>
            {
				Type _type = Type.GetType($"ETModel.{path.GetPureFileName()}Category");

				if (_type == null || !typeof(ACategory).IsAssignableFrom(_type))
				{
					throw new Exception("无法解析的配置资源");
				}

				object[] attrs = _type.GetCustomAttributes(typeof(ConfigAttribute), false);
				if (attrs.Length == 0)
				{
					throw new Exception("无法解析的配置资源");
				}

				ConfigAttribute configAttribute = attrs[0] as ConfigAttribute;

				// 只加载指定的配置
				if (!configAttribute.Type.Is(AppType.ClientM))
				{
					throw new Exception("无法解析的配置资源");
				}

				object obj = Activator.CreateInstance(_type);

				ACategory iCategory = obj as ACategory;
				if (iCategory == null)
				{
					throw new Exception($"class: {_type.Name} not inherit from ACategory");
				}
				iCategory.BeginInit(_text);
				iCategory.EndInit();

				_acategory = iCategory;
			});

			ResourcesComponent.Instance.UnloadBundle(path);

			this.allConfigDict[path] = _acategory;

			return _acategory;
        }

		/// <summary>
		/// 移除配置
		/// </summary>
		/// <param name="configName"></param>
		public void Remove(string path)
		{
			if (allConfigDict.ContainsKey(path))
			{
				allConfigDict.Remove(path);
			}
		}

		/// <summary>
		/// 移除配置
		/// </summary>
		/// <param name="configName"></param>
		public void RemoveForRegex(string regexPattern)
		{
			System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex(regexPattern);
			List<string> _tempList = new List<string>();
			foreach (KeyValuePair<string, ACategory> item in allConfigDict)
			{
				if (_regex.IsMatch(item.Key))
				{
					_tempList.Add(item.Key);
				}
			}

			foreach (string key in _tempList)
			{
				allConfigDict.Remove(key);
			}
		}

	}
}