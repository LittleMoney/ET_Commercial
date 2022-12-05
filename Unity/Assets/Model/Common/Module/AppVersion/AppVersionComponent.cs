using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
	/// <summary>
	/// 更新类型
	/// </summary>
	public enum UpdateType
	{
		/// <summary>
		/// 无需更新
		/// </summary>
		None,

		/// <summary>
		/// 热更新
		/// </summary>
		Hotfix,

		/// <summary>
		/// 冷更新
		/// </summary>
		Cold
	}



	public class AppVersionComponent : Component
	{
		public AppVersionConfig localAppVersionConfig;

		public AppVersionConfig serverAppVersionConfig;

		public void Awake()
		{

		}
	}

	[ObjectSystem]
	public class AppVersionComponentComponentAwakeSystem : AwakeSystem<AppVersionComponent>
	{
		public override void Awake(AppVersionComponent t)
		{
			t.Awake();
		}
	}


	/// <summary>
	/// 
	/// </summary>
	public static class AppVersionComponentSystem
	{
		/// <summary>
		/// 加载服务端appVersion配置
		/// </summary>
		/// <param name="self"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public static async ETTask LoadServerAppVersionConfigAsync(this AppVersionComponent self)
		{
			if(self.localAppVersionConfig==null)
            {
				throw new Exception("对不起app的本地版本配置文件不存在");
            }

			using (UnityWebRequestAsync webRequestAsync = ComponentFactory.Create<UnityWebRequestAsync>())
			{
				await webRequestAsync.DownloadAsync(self.localAppVersionConfig.AppVersionConfigRequestUrl);
				//downloadHandler.text 默认uft8编码
				self.serverAppVersionConfig = JsonHelper.FromJson<AppVersionConfig>(webRequestAsync.Request.downloadHandler.text);
			}
		}

		/// <summary>
		/// 加载本地appVersion数据
		/// </summary>
		/// <param name="self"></param>
		/// <param name="url"></param>
		public static async ETTask LoadLocalAppVersionConfigAsync(this AppVersionComponent self)
		{
			//如果本地有配置就读表
			if (File.Exists(PathHelper.HotfixAppVersionConfigPath))
			{
				using (FileStream fs = File.Open(PathHelper.HotfixAppVersionConfigPath, FileMode.Open))
				using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8))
				{
					string _text = await sr.ReadToEndAsync();
					self.localAppVersionConfig = JsonHelper.FromJson<AppVersionConfig>(_text);
				}
			}
			else
			{
				try
				{
					using (UnityWebRequestAsync webRequestAsync = ComponentFactory.Create<UnityWebRequestAsync>())
					{
						await webRequestAsync.DownloadAsync(PathHelper.BuildInAppVersionConfigPath4Web);
						byte[] _data = webRequestAsync.Request.downloadHandler.data;
						self.localAppVersionConfig = JsonHelper.FromJson<AppVersionConfig>(webRequestAsync.Request.downloadHandler.text); //默认utf8 
					}
				}
				catch (Exception e)
				{
					//版本文件拉去失败
					throw new Exception($"url: {PathHelper.BuildInAppVersionConfigPath4Web}", e);
				}
			}
		}

		/// <summary>
		/// 服务器是否停止服务
		/// </summary>
		/// <param name="self"></param>
		/// <param name=""></param>
		/// <returns></returns>
		public static bool IsServerNormal(this AppVersionComponent self)
		{
			return self.serverAppVersionConfig.IsServerNormal;
		}

		/// <summary>
		/// 服务器公告
		/// </summary>
		/// <param name="self"></param>
		/// <param name=""></param>
		/// <returns></returns>
		public static string GetServerAnnouncement(this AppVersionComponent self)
		{
			return self.serverAppVersionConfig.MaintainAnnouncement;
		}

		/// <summary>
		/// 获取app更新url
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static string GetAppDownloadUrl(this AppVersionComponent self)
		{
			return self.serverAppVersionConfig.AppDownloadUrl;
		}

		/// <summary>
		/// 获取app更新url
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static string GetAppHotfixUrl(this AppVersionComponent self)
		{
			return self.serverAppVersionConfig.HotfixDownloadUrl;
		}


		/// <summary>
		/// 判断App是否需要更新
		/// </summary>
		/// <returns></returns>
		public static UpdateType CensorAppNeedUpdate(this AppVersionComponent self)
		{
			if (!string.IsNullOrEmpty(UnityEngine.Application.version))
			{
				string[] _versionStr = UnityEngine.Application.version.Split('.');
				long _version = 0;
				if (_versionStr.Length == 2 && int.TryParse(_versionStr[0], out int _mainVer) && int.TryParse(_versionStr[1], out int _subVer))
				{
					_version = ((long)_mainVer << 32) + (long)_subVer;

					if (_version < self.serverAppVersionConfig.ColdUpdateVersion)
					{
						return UpdateType.Cold;
					}
					else if (self.localAppVersionConfig.Version != self.serverAppVersionConfig.Version)
					{
						return UpdateType.Hotfix;
					}
					else
					{
						return UpdateType.None;
					}
				}
			}

			throw new Exception("无效的APP版本号");

		}


		/// <summary>
		/// 判断toy资源包目录是否需要更新
		/// </summary>
		/// <param name="relativePath"></param>
		/// <returns></returns>
		public static bool CensorToyNeedUpdate(this AppVersionComponent self, string toyDirName)
		{
			if (self.localAppVersionConfig.ToyVersionConfigs.TryGetValue(toyDirName, out ToyVersionConfig _localToyVersionConfig))
			{
				if (self.serverAppVersionConfig.ToyVersionConfigs.TryGetValue(toyDirName, out ToyVersionConfig _serverToyVersionConfig))
				{
					if (_localToyVersionConfig.SignMD5 != _serverToyVersionConfig.SignMD5)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					throw new Exception("模块在服务端配置中不存在");
				}
			}
			else
			{
				if (self.serverAppVersionConfig.ToyVersionConfigs.TryGetValue(toyDirName, out ToyVersionConfig _serverToyVersionConfig))
				{
					return true;
				}
				else
				{
					throw new Exception("模块不存在");
				}
			}
		}

		/// <summary>
		/// 同步服务端的ToyVersion数据到本地配置
		/// </summary>
		/// <param name="self"></param>
		/// <param name="toyDirName"></param>
		public static void UpdateLocalToyVersionConfig(this AppVersionComponent self, string toyDirName)
		{
			if (self.serverAppVersionConfig == null || self.localAppVersionConfig == null) throw new Exception("serverAppVersionConfig or localAppVersionConfig is null");

			self.localAppVersionConfig.ToyVersionConfigs[toyDirName] = self.serverAppVersionConfig.ToyVersionConfigs[toyDirName];
		}

		/// <summary>
		/// 拷贝服务端appVersionConfig数据到本地appVersionConfig ，但不拷贝toy数据
		/// 因为app版本一致，但可能toy版本不一致，比如用户从始至终都没有打开过某个toy模块，则这个toy的信息在本地版本文件中就不存在
		/// </summary>
		/// <param name="self"></param>
		public static void UpdateLocalAppVersionConfigWithOutToyVersionConfigs(this AppVersionComponent self)
		{
			if (self.serverAppVersionConfig == null || self.localAppVersionConfig == null) throw new Exception("serverAppVersionConfig or localAppVersionConfig is null");

			self.localAppVersionConfig.AppVersionConfigRequestUrl = self.serverAppVersionConfig.AppVersionConfigRequestUrl;
			self.localAppVersionConfig.HotfixDownloadUrl = self.serverAppVersionConfig.HotfixDownloadUrl;
			self.localAppVersionConfig.AppDownloadUrl = self.serverAppVersionConfig.AppDownloadUrl;
			self.localAppVersionConfig.Version = self.serverAppVersionConfig.Version;
		}


		/// <summary>
		/// 清理到已经无用的ToyVersionConfig
		/// </summary>
		/// <returns></returns>
		public static void ClearInvalidLocalToyVersionConfigs(this AppVersionComponent self)
		{
			List<ToyVersionConfig> _list = new List<ToyVersionConfig>();

			foreach (KeyValuePair<string, ToyVersionConfig> pair in self.localAppVersionConfig.ToyVersionConfigs)
			{
				//如果本地配置中的toy在服务端已经不存在了
				if (!self.serverAppVersionConfig.ToyVersionConfigs.ContainsKey(pair.Key))
				{
					_list.Add(pair.Value);
				}
			}

			foreach(ToyVersionConfig tvc in _list)
            {
				self.localAppVersionConfig.ToyVersionConfigs.Remove(tvc.Name);
            }

		}

		/// <summary>
		/// 获取所有本地toy配置
		/// </summary>
		/// <returns></returns>
		public static string[] GetAllLocalToyDirNames(this AppVersionComponent self)
		{
			string[] _toyDirNames = new string[self.localAppVersionConfig.ToyVersionConfigs.Count];
			int _index = 0;
			foreach(string _name in self.localAppVersionConfig.ToyVersionConfigs.Keys)
            {
				_toyDirNames[_index++] = _name;
			}
			return _toyDirNames;
		}

		/// <summary>
		/// 获取所有本地toy配置
		/// </summary>
		/// <returns></returns>
		public static ToyVersionConfig[] GetAllLocalToyVersions(this AppVersionComponent self)
		{
			return self.localAppVersionConfig.ToyVersionConfigs.Values.ToArray();
		}

		/// <summary>
		/// 获取本地的toy配置
		/// </summary>
		/// <param name="self"></param>
		/// <param name="toyDirName"></param>
		/// <returns></returns>
		public static ToyVersionConfig GetLocalToyVersionConfig(this AppVersionComponent self,string toyDirName)
		{
			return self.localAppVersionConfig.ToyVersionConfigs[toyDirName];
		}

		/// <summary>
		/// 保存当前本地的AppVersionConfig到Hotfix目录中
		/// </summary>
		/// <returns></returns>
		public static async ETTask UpdateHotfixAppVersionConfigAsync(this AppVersionComponent self)
		{
			//删除旧文件
			if(File.Exists(PathHelper.HotfixAppVersionConfigPath))
            {
				File.Delete(PathHelper.HotfixAppVersionConfigPath);
            }

			//存储到本地
			using (StreamWriter sw = new StreamWriter(File.Create(PathHelper.HotfixAppVersionConfigPath), System.Text.Encoding.UTF8))
			{
				string _text = JsonHelper.ToJson(self.localAppVersionConfig).Replace("\\", "\\\\");
				await sw.WriteAsync(_text);
			}
		}

		/// <summary>
		/// 移除本地版本文件
		/// </summary>
		public static void RemoveHotfixAppVersionConfig(this AppVersionComponent self)
		{
			if (!File.Exists(PathHelper.HotfixAppVersionConfigPath))
			{
				File.Delete(PathHelper.HotfixAppVersionConfigPath); 
			}
		}
	}
}
