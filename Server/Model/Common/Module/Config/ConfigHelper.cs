using System;
using System.IO;

namespace ETModel
{
	public static class ConfigHelper
	{
		public static string GetText(string key)
		{
			DirectoryInfo _dirInfo = new DirectoryInfo($"../ServerConfig");

			FileInfo[] _fileInfos=_dirInfo.GetFiles($"{key}.txt", SearchOption.AllDirectories);

			if (_fileInfos == null || _fileInfos.Length == 0)
			{
				throw new Exception($"load server config file fail, path: {key}");
			}

			if (_fileInfos.Length > 1)
			{
				throw new Exception($"load server config file fail, multipe file : {_fileInfos.Length}");
			}

			try
			{
				
				string configStr = File.ReadAllText(_fileInfos[0].FullName);
				return configStr;
			}
			catch (Exception e)
			{
				throw new Exception($"load config file fail, path: {key}");
			}
		}

		public static T ToObject<T>(string str)
		{
			return MongoHelper.FromJson<T>(str);
		}
	}
}
