using System.Collections.Generic;
using System.IO;
using ETModel;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
	public static class BuildHelper
	{
		private const string relativeDirPrefix = "../Release";

		public static string BuildDirPathTemplate = "../Release/{0}";

		public static string BuildAppDirPathTemplate = "../Release/{0}_app";

		public static string BuildBundlesDirPathTemplate = "../Release/{0}/gameres";

		public static string BuildBundlesDirName = "gameres";

		public static string BuildCommonDirName = "common";

		public static string CommonDirName = "Common";

		public static string BuildOtherDirName = "common/Other";

		public static string AppVersionConfigFileName = "AppVersion.json";

		public static string FileVersionConfigFileName = "FileVersion.json";

		private const string BundlesDir = "Assets/Bundles";

		private const string ResDir = "Assets/Res";


		[MenuItem("Tools/打包/清理PlayerPrefs数据")]
		public static void ClearPlayerPrefs()
		{
			UnityEngine.PlayerPrefs.DeleteAll();
			Debug.Log("clear PlayerPrefs success!");
		}

		[MenuItem("Tools/打包/清理PersistentDataPath目录")]
		public static void ClearPersistentDataPath()
		{
			Directory.Delete(Application.persistentDataPath, true);
			Debug.Log(string.Format("clear path success! {0}", Application.persistentDataPath));
		}

		[MenuItem("Tools/打包/web资源服务器")]
		public static void OpenFileServer()
		{
			ProcessHelper.Run("dotnet", "FileServer.dll", "../FileServer/");
		}

		public static void Build(PlatformType type,BuildOptions buildOptions, bool isBuildExe, bool isContainAB)
		{
			BuildTarget buildTarget = BuildTarget.StandaloneWindows;
			string exeName = "ET";
			string _buildDirPath=string.Format(BuildDirPathTemplate, type);
			string _buildBundlesDirPath = string.Format(BuildBundlesDirPathTemplate, type);
			string _buildAppDirPath = string.Format(BuildAppDirPathTemplate, type);
			
			switch (type)
			{
				case PlatformType.PC:
					buildTarget = BuildTarget.StandaloneWindows64;
					exeName += ".exe";
					break;
				case PlatformType.Android:
					buildTarget = BuildTarget.Android;
					exeName += ".apk";
					break;
				case PlatformType.IOS:
					buildTarget = BuildTarget.iOS;
					break;
				case PlatformType.MacOS:
					buildTarget = BuildTarget.StandaloneOSX;
					break;
			}



			EditorUtility.DisplayProgressBar("开始打包", "...", 0);
			BuildAssetBundle(_buildBundlesDirPath, buildTarget);

			EditorUtility.DisplayProgressBar("开始生成文件版本列表", "...", 0);
			GenerateFileVersionConfigs(_buildBundlesDirPath);

			EditorUtility.DisplayProgressBar("更新 AppVersion.json 模块配置数据", "...", 0);
			UpdateToyVersionConfigAll(_buildBundlesDirPath, type);

			EditorUtility.DisplayProgressBar("完成资源打包", "...", 1);

			if (isContainAB)
			{
				//包含AB包的情况下，只需要把生成目录下所有文件拷贝过去即可
				EditorUtility.DisplayProgressBar("拷贝 所有AB包到 StreamingAssets", "...", 0);
				FileHelper.CleanDirectory("Assets/StreamingAssets");
				FileHelper.CopyDirectory(_buildDirPath, "Assets/StreamingAssets");
			}
            else
            {
				//不含AB包的情况下，需要吧 common 目录 和 AppVersion.json 文件拷贝到 StreamingAssets 目录下
				EditorUtility.DisplayProgressBar("拷贝 Common模块AB包 和 AppVersion.json文件 到 StreamingAssets", "...", 0);
				FileHelper.CleanDirectory("Assets/StreamingAssets");
				FileHelper.CopyDirectory($"{_buildBundlesDirPath}/{BuildCommonDirName}", $"Assets/StreamingAssets/{BuildBundlesDirName}/{BuildCommonDirName}");
				
				//生成只包含Common模块的AppVersionConfig 到StreamingAssets
				AppVersionConfig _appVersionConfig=AppVersionEditor.LoadAppVersionConfig(type);
				string[] _toyDirNames = new string[_appVersionConfig.ToyVersionConfigs.Count];
				_appVersionConfig.ToyVersionConfigs.Keys.CopyTo(_toyDirNames, 0);
				for(int i=0;i<_toyDirNames.Length;i++)
                {
					if (_toyDirNames[i] != CommonDirName)
                    {
						_appVersionConfig.ToyVersionConfigs.Remove(_toyDirNames[i]);
                    }
                }
				AppVersionEditor.SaveAppVersionConfig(_appVersionConfig, $"Assets/StreamingAssets/{AppVersionConfigFileName}");
			}

			AssetDatabase.Refresh();

			if (isBuildExe)
			{
				EditorUtility.DisplayProgressBar("开始EXE打包", "...", 0);
				AssetDatabase.Refresh();
				string[] levels = {"Assets/Init.unity",};
				BuildPipeline.BuildPlayer(levels, $"{_buildAppDirPath}/{exeName}", buildTarget, buildOptions);
			}

			EditorUtility.ClearProgressBar();
		}

		/// <summary>
		/// 拷贝清单文件到common目录下
		/// </summary>
		/// <param name="buildBundlesDir"></param>
		public static void CopyMainifestToCommon(string buildBundlesDir)
        {
			string _manifestABPath=PathHelper.Combine(buildBundlesDir, $"{buildBundlesDir.GetPureDirName()}");
			string _manifestABMataPath=PathHelper.Combine(buildBundlesDir, $"{buildBundlesDir.GetPureDirName()}.manifest");

			if (File.Exists(_manifestABPath))
            {
				File.Move(_manifestABPath, PathHelper.Combine(buildBundlesDir, $"{BuildCommonDirName}/{buildBundlesDir.GetPureDirName()}"));
            }
            else
            {
				throw new System.Exception("对不起，未能找到 manifest 文件的AB包");
            }

			if (File.Exists(_manifestABMataPath))
			{
				File.Move(_manifestABMataPath, PathHelper.Combine(buildBundlesDir, $"{BuildCommonDirName}/{buildBundlesDir.GetPureDirName()}.manifest"));
			}
		}

		/// <summary>
		/// 为各模块生成文件版本列表
		/// </summary>
		/// <param name="buildBundlesDir"></param>
		private static void GenerateFileVersionConfigs(string buildBundlesDir)
		{
			System.IO.DirectoryInfo _dirInfo = new DirectoryInfo(buildBundlesDir);

			foreach (FileInfo _fileInfo in _dirInfo.GetFiles())
			{
				throw new System.Exception($"对不起，构建根目录中的文件 {_fileInfo.Name} 无法纳入文件版本管理");
            }

			foreach (DirectoryInfo _subDirInfo in  _dirInfo.GetDirectories())
			{
				FileVersionConfig versionProto = new FileVersionConfig();
				GenerateFileVersionConfig(PathHelper.Combine(buildBundlesDir,_subDirInfo.Name), versionProto, "");

				using (FileStream fileStream = new FileStream($"{buildBundlesDir}/{_subDirInfo.Name}/{FileVersionConfigFileName}", FileMode.Create))
				{
					byte[] bytes = JsonHelper.ToJson(versionProto).ToUtf8();
					fileStream.Write(bytes, 0, bytes.Length);
				}
			}
		}

		/// <summary>
		/// 将文件夹中的所有文件的相对地址和md5 写入文件配置中
		/// </summary>
		/// <param name="dir"></param>
		/// <param name="versionProto"></param>
		/// <param name="relativePath"></param>
		private static void GenerateFileVersionConfig(string dir, FileVersionConfig fileVersionConfig, string relativePath)
		{
			foreach (string file in Directory.GetFiles(dir))
			{
				string md5 = MD5Helper.FileMD5(file);
				FileInfo fi = new FileInfo(file);
				long size = fi.Length;
				string filePath = relativePath == "" ? fi.Name : $"{relativePath}/{fi.Name}";

				fileVersionConfig.FileInfoDict.Add(filePath, new FileVersionInfo
				{
					File = filePath,
					SignMD5 = md5,
					Size = size,
				});
			}

			//每一个目录都会生成自己的version.txt文件，方便分目录更新，用于子游戏延迟更新资源
			foreach (string directory in Directory.GetDirectories(dir))
			{
				DirectoryInfo dinfo = new DirectoryInfo(directory);
				string rel = relativePath == "" ? dinfo.Name : $"{relativePath}/{dinfo.Name}";
				GenerateFileVersionConfig($"{dir}/{dinfo.Name}", fileVersionConfig, rel);
			}
		}

		/// <summary>
		/// 更新各模块的 FileVersion.txt 的MD5 到 AppVersion.txt文件中
		/// </summary>
		/// <param name="buildBundlesDirPath"></param>
		/// <param name="type"></param>
		private static void UpdateToyVersionConfigAll(string buildBundlesDirPath, PlatformType type)
        {
			AppVersionConfig _appVersionConfig = AppVersionEditor.LoadAppVersionConfig(type);
			//先清理到toyVersionConfig数据
			_appVersionConfig.ToyVersionConfigs.Clear();

			System.IO.DirectoryInfo _dirInfo = new DirectoryInfo(buildBundlesDirPath);
			DirectoryInfo _bundlsDirInfo = new DirectoryInfo(BundlesDir);

			foreach (DirectoryInfo _subDirInfo in _dirInfo.GetDirectories())
			{
				string _originToyDirName = null;
				//检查模块的资源包根目录必须能对应到 Assets/Bundles 下的模块目录名
				foreach(DirectoryInfo _originSubDirInfo in _bundlsDirInfo.GetDirectories())
                {
					if(_originSubDirInfo.Name.ToLower()==_subDirInfo.Name)
                    {
						_originToyDirName = _originSubDirInfo.Name;
                    }
                }
				if (_originToyDirName == null) throw new System.Exception($"未能找到{_subDirInfo.Name} 对应的 Assets/Bundles 下的模块目录名");


				string _fileVersionConfigPath = $"{buildBundlesDirPath}/{_subDirInfo.Name}/{FileVersionConfigFileName}";
				if (!File.Exists(_fileVersionConfigPath)) throw new System.Exception($"{_fileVersionConfigPath} 不存在");

				//记录FileVersion.json文件的MD5
				byte[] bytes = File.ReadAllBytes(_fileVersionConfigPath);
				_appVersionConfig.ToyVersionConfigs.Add(_originToyDirName, new ToyVersionConfig()
				{
					Name = _originToyDirName,
					SignMD5 = MD5Helper.BytesMD5(bytes)
				});
			}

			AppVersionEditor.SaveAppVersionConfig(_appVersionConfig,type);
		}

		/// <summary>
		/// 自动设置资源包名称
		/// </summary>
        public static void BuildAssetBundle(string buildBundlesDirPath, BuildTarget buildTarget)
		{
			List<string> _pathList = new List<string>();   //所有要打budnle的文件和文件夹路径
			Dictionary<string, string> _pathDict = new Dictionary<string, string>();  //key=path, value=bundleName
			int _index = 0;

			#region 获取所有Bundles目录下的资源
			//获得所有需要打bundle的文件和文件夹路径
			EditorUtility.DisplayProgressBar("获取Assets/Bundles目录下的所有文件", "...", 1);
			string[] _guidArray = AssetDatabase.FindAssets("t:object", new string[] { BundlesDir });
			foreach (var _guid in _guidArray)
			{
				string _path = AssetDatabase.GUIDToAssetPath(_guid);
				if (AssetDatabase.IsValidFolder(_path)
					 || _pathList.Contains(_path)    //某些可以点开前面三角形的文件会获得多次，处理掉
					|| _path.EndsWith(".cs")
					|| _path.EndsWith(".mp4"))   //mp4文件直接复制，无需打bundle    
				{
					continue;
				}

				_pathList.Add(_path);
			}
            #endregion

            #region 处理依赖

			//查找依赖，依赖数>=2的，也作为ab打包，防止依赖过于复杂
			Dictionary<string, int> _abDependDict = new Dictionary<string, int>();
			for (int i = 0; i < _pathList.Count; i++)
			{
				string _path = _pathList[i];
				EditorUtility.DisplayProgressBar("获取依赖", _path, (float)i / _pathList.Count);
				string[] _dependPathArray = AssetDatabase.GetDependencies(_path);
				foreach (var _dependPath in _dependPathArray)
				{
					//过滤特殊情况
					if (_dependPath.Contains(BundlesDir)
						|| _dependPath == _path
						|| _dependPath.Contains("AtlasSprites")
						|| _dependPath.EndsWith(".cs")
						|| _dependPath.EndsWith(".mp4"))
					{
						continue;
					}
					//依赖文件与主体文件重名检测
					string _originName = Path.GetFileNameWithoutExtension(_path);
					string _dependName = Path.GetFileNameWithoutExtension(_dependPath);
					if (_originName == _dependName)
					{
						Debug.LogError("依赖文件与主体文件重名！请检查：" + _dependName);
					}
					if (_abDependDict.ContainsKey(_dependPath))
					{
						_abDependDict[_dependPath] += 1;
					}
					else
					{
						_abDependDict.Add(_dependPath, 1);
					}
				}
			}

			foreach (var kv in _abDependDict)
			{
				if (kv.Value >= 2)
				{
					if (_pathList.Contains(kv.Key)) continue;
					_pathList.Add(kv.Key);
				}
			}
            #endregion

			#region 设置abName

            EditorUtility.DisplayProgressBar("设置abName", "...", 1);

			foreach (var _path in _pathList)
			{
				string _abName=null;
				if (_path.Contains(BundlesDir))
                {
					string _tempPath = _path.Replace($"{BundlesDir}/", "");
					_abName = _tempPath.Substring(0, _tempPath.LastIndexOf("."));
				}
				else if (_path.Contains(ResDir))
                {
					string _tempPath = _path.Replace($"{ResDir}/", "");
					_abName = _tempPath.Substring(0, _tempPath.LastIndexOf("."));
				}
                else
                {
					string _tempPath = _path.Replace($"Assets/", $"{BuildOtherDirName}/");
					_abName = _tempPath.Substring(0, _tempPath.LastIndexOf("."));
				}
				_pathDict.Add(_path, _abName);
			}

			
			foreach(var _kv in _pathDict)
            {
				EditorUtility.DisplayProgressBar("设置abName", _kv.Value, (float)_index / _pathDict.Count);
				SetABName(_kv.Key, _kv.Value);
			}
            #endregion

            #region 生成
            EditorUtility.DisplayProgressBar("清理资源包生成目录", "...", 0);
			if (Directory.Exists(buildBundlesDirPath)) Directory.Delete(buildBundlesDirPath, true);
			Directory.CreateDirectory(buildBundlesDirPath);

			EditorUtility.DisplayProgressBar("生成资源包", "...", 0);
			BuildPipeline.BuildAssetBundles(buildBundlesDirPath, BuildAssetBundleOptions.ChunkBasedCompression, buildTarget);

			EditorUtility.DisplayProgressBar("拷贝manifest文件到Common目录", "...", 0);
			CopyMainifestToCommon(buildBundlesDirPath);

            #endregion

			#region 还原abName
            EditorUtility.DisplayProgressBar("还原abName", "...", 0);
			_index = 0;
			foreach (var _kv in _pathDict)
			{
				EditorUtility.DisplayProgressBar("还原abName", _kv.Value, (float)_index / _pathDict.Count);
				SetABName(_kv.Key, string.Empty);
			}
            #endregion

            EditorUtility.ClearProgressBar();
		}

		/// <summary>
		/// 设置AB包名称
		/// </summary>
		/// <param name="path"></param>
		/// <param name="abName"></param>
		static void SetABName(string path, string abName)
		{
			AssetImporter assetImporter = AssetImporter.GetAtPath(path);
			assetImporter.assetBundleName = abName.ToLower();
			if (abName != string.Empty)
			{
				assetImporter.assetBundleVariant = string.Empty;
			}
		}

	}
}
