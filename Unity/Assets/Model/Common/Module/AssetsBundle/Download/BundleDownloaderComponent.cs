﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ETModel
{
	[ObjectSystem]
	public class UiBundleDownloaderComponentAwakeSystem : AwakeSystem<BundleDownloaderComponent>
	{
		public override void Awake(BundleDownloaderComponent self)
		{

		}
	}

	/// <summary>
	/// 用来对比web端的资源，比较md5，对比下载资源
	/// </summary>
	public class BundleDownloaderComponent : Component
	{
		/// <summary>
		/// 版本文件名称
		/// </summary>
		public const string FileVersionConfigFileName = "FileVersion.json";

		/// <summary>
		/// 版本文件分析状态 1读取热更版本文件，2下载远端版本文件，3读取app版本文件,4分析下载完毕，5分析下载完毕无需更新
		/// </summary>
		public int g_startAsyncStatus;

		/// <summary>
		/// 下载地址
		/// </summary>
		public string downloadUrl;

		/// <summary>
		/// 当前更新模块的目录
		/// </summary>
		public string toyDirName;

		/// <summary>
		/// 远端文件列表
		/// </summary>
		public FileVersionConfig remoteFileVersionConfig;

		/// <summary>
		/// 热更文件列表
		/// </summary>
		public FileVersionConfig hotfixFileVersionConfig;

		/// <summary>
		/// 需要下载的资源包名称列表
		/// </summary>
		public Queue<FileVersionInfo> g_awaitDownloadbundles;

		/// <summary>
		/// 已下载资源包列表
		/// </summary>
		public HashSet<FileVersionInfo> downloadedBundles;

		/// <summary>
		/// 当前正在下载的资源包
		/// </summary>
		public FileVersionInfo downloadingBundle;

		/// <summary>
		/// 需要下载的字节数
		/// </summary>
		public long g_totalSize;

		/// <summary>
		/// 已经下载的字节数
		/// </summary>
		public long g_alreadyDownloadBytes;

		/// <summary>
		/// 当前下载进度
		/// </summary>
		public int g_progress;

		/// <summary>
		/// 是否已经完毕
		/// </summary>
		public bool g_isDone;

		/// <summary>
		/// 执行错误
		/// </summary>
		public Exception g_error;


		public override void Dispose()
		{
			if (this.IsDisposed)
			{
					return;
			}

			base.Dispose();

			this.remoteFileVersionConfig = null;
			this.g_totalSize = 0;
			this.g_awaitDownloadbundles = null;
			this.downloadedBundles = null;
			this.downloadingBundle = null;
			this.g_error = null;
			this.g_isDone = true;
		}

		/// <summary>
		/// 分析本地和服务端文件，记录需下载的文件数据
		/// </summary>
		/// <param name="downloadUrl">下载的根url(注意：ulr指向文件服务器上资源包存放的根目录)</param>
		/// <param name="toyDirName">要下载的子模块目录名</param>
		/// <param name="forceUpdate">是否强制更新</param>
		/// <param name="progressCallback">分析进度回调 Action( int 进度百分数 100成功，-100失败，bool 是否完毕,string 进度说明)</param>
		/// <returns></returns>
		public async ETTask StartAsync(string downloadUrl, string toyDirName,bool forceUpdate=false,Action<BundleDownloaderComponent> progressCallback=null)
		{
			toyDirName = toyDirName.ToLower();

			// 获取远程的Version.txt
			this.downloadUrl = downloadUrl;
			this.toyDirName = toyDirName;

			FileVersionConfig _buildInFileVersionConfig = null;

			#region 获取hotfix FileVersionConfig
			if (!forceUpdate)
			{
				g_progress = 10;
				g_isDone = false;
				g_startAsyncStatus = 1;
				progressCallback?.Invoke(this);

				string _hotfixFileVersionConfigPath = PathHelper.Combine(PathHelper.HotfixResPath, this.toyDirName, FileVersionConfigFileName);
				if (File.Exists(_hotfixFileVersionConfigPath))
				{
					try
					{
						using (FileStreamReadAsyncAsync _fileStreamReadAsync = ComponentFactory.Create<FileStreamReadAsyncAsync>())
						{
						
							await _fileStreamReadAsync.ReadAllAsync(_hotfixFileVersionConfigPath);
							hotfixFileVersionConfig = JsonHelper.FromJson<FileVersionConfig>(System.Text.Encoding.UTF8.GetString(_fileStreamReadAsync.datas));
						}
					}
					catch (Exception error)
					{
						g_isDone = true;
						g_error = error;
						progressCallback?.Invoke(this);
						Log.Error($"hotfix FileVersionConfig: {_hotfixFileVersionConfigPath} {error.Message}");
					}
				}
			}

			if(hotfixFileVersionConfig==null)
            {
				hotfixFileVersionConfig = new FileVersionConfig();
			}
			#endregion

			#region 获取buildIn FileVersionConfig

			g_progress = 40;
			g_isDone = false;
			g_startAsyncStatus = 2;
			progressCallback?.Invoke(this);

			string _buildInFileVersionConfigPath = PathHelper.Combine(PathHelper.BuildInResPath4Web, this.toyDirName, FileVersionConfigFileName);
			using (UnityWebRequestAsync request = ComponentFactory.Create<UnityWebRequestAsync>())
			{
				try
				{
					await request.DownloadAsync(_buildInFileVersionConfigPath);
					_buildInFileVersionConfig = JsonHelper.FromJson<FileVersionConfig>(request.Request.downloadHandler.text);
				}
				catch (Exception error)
				{
					Log.Error($"获取buildIn FileVersionConfig: {_buildInFileVersionConfigPath} {error.Message}");
				}
			}
			#endregion

			#region 获取server FileVersionConfig

			g_progress = 80;
			g_isDone = false;
			g_startAsyncStatus = 3;
			progressCallback?.Invoke(this);
			//获取服务端AB目录下的version 文件
			string serverFileVersionConfigUrl = "";
			try
			{
				using (UnityWebRequestAsync webRequestAsync = ComponentFactory.Create<UnityWebRequestAsync>())
				{
					serverFileVersionConfigUrl = PathHelper.Combine(downloadUrl, this.toyDirName, FileVersionConfigFileName);

					await webRequestAsync.DownloadAsync(serverFileVersionConfigUrl);
					byte[] _data = webRequestAsync.Request.downloadHandler.data;
					remoteFileVersionConfig = JsonHelper.FromJson<FileVersionConfig>(webRequestAsync.Request.downloadHandler.text); //默认utf8 
				}
			}
			catch (Exception e)
			{
				g_isDone = true;
				g_error = e;
				progressCallback?.Invoke(this);
				//版本文件拉去失败
				throw new Exception($"url: {serverFileVersionConfigUrl}", e);
			}

			#endregion

			g_awaitDownloadbundles = new Queue<FileVersionInfo>();
			downloadedBundles = new HashSet<FileVersionInfo>();
			downloadingBundle = null;

			#region 对比MD5,判断是否需要下载

			FileVersionInfo _localFileVersionInfo = null;
			foreach (FileVersionInfo _remoteFileVersionInfo in remoteFileVersionConfig.FileInfoDict.Values)
			{
				if (hotfixFileVersionConfig.FileInfoDict.TryGetValue(_remoteFileVersionInfo.File, out _localFileVersionInfo))
                {
					if (_localFileVersionInfo.SignMD5 == _remoteFileVersionInfo.SignMD5) //热更文件没有差异，跳过
					{
						continue;  
					}
                    else
                    {
						//签名不一致，删除热更文件
						hotfixFileVersionConfig.FileInfoDict.Remove(_remoteFileVersionInfo.File);
					}
				}

				if (_buildInFileVersionConfig!=null && _buildInFileVersionConfig.FileInfoDict.TryGetValue(_remoteFileVersionInfo.File, out _localFileVersionInfo))
				{
					if (_localFileVersionInfo.SignMD5 == _remoteFileVersionInfo.SignMD5) //内嵌文件没有差异，跳过
					{
						continue; //无需更新
					}
				}

				this.g_awaitDownloadbundles.Enqueue(_remoteFileVersionInfo);
				this.g_totalSize += _remoteFileVersionInfo.Size;
			}

			#endregion

			#region 清理无效的热更资源

			if (hotfixFileVersionConfig.FileInfoDict.Count == 0) //没有需要保留的热更文件，直接重建目录
			{
				string _hotfixToyDirPath = PathHelper.Combine(PathHelper.HotfixResPath, this.toyDirName);
				if (Directory.Exists(_hotfixToyDirPath))
				{
					Directory.Delete(_hotfixToyDirPath, true);
					Directory.CreateDirectory(_hotfixToyDirPath);
				}
			}
			else
			{
				// 删掉已经不需要的热更文件
				DirectoryInfo _directoryInfo = new DirectoryInfo(PathHelper.Combine(PathHelper.HotfixResPath, this.toyDirName));
				if (_directoryInfo.Exists)
				{
					FileInfo[] fileInfos = _directoryInfo.GetFiles("*");

					foreach (FileInfo fileInfo in fileInfos)
					{
						string _relativePath = fileInfo.FullName.Substring(fileInfo.FullName.IndexOf($"{toyDirName}/") + 1);
						if (hotfixFileVersionConfig.FileInfoDict.ContainsKey(_relativePath)) continue;
						//if (fileInfo.Name == FileVersionConfigFileName) continue;
						fileInfo.Delete();
					}
				}
			}

			//清理后保存一下文件列表
			if (hotfixFileVersionConfig.FileInfoDict.Count > 0)
			{
				await UpdateHotfixFileVersionConfig();
			}

			#endregion

			g_progress = 100;
			g_isDone = true;
			g_startAsyncStatus = 4;
			progressCallback?.Invoke(this);

		}

		/// <summary>
		/// 根据已生成的下载列表，下载资源包文件,调用前请确保调用 StartAsync 成功
		/// </summary>
		/// <returns></returns>
		public async ETTask DownloadAsync(Action<BundleDownloaderComponent> progressCallback = null)
		{
			if (this.g_awaitDownloadbundles.Count == 0 && this.downloadingBundle == null)
			{
				await UpdateHotfixFileVersionConfig();

				g_progress = 100;
				g_isDone = true;
				progressCallback(this);
				return;
			}

			try
			{
				g_progress = 0;
				g_isDone = false;
				g_alreadyDownloadBytes = 0;
				progressCallback(this);
				while (true)
				{
					if (this.g_awaitDownloadbundles.Count == 0)
					{
						break;
					}

					downloadingBundle = this.g_awaitDownloadbundles.Dequeue();
					int _downloadTryCount = 0; //连续三次下载，如果任然下载失败，则抛出异常

					while (true)
					{
						//拼接下载地址
						string _storePath = PathHelper.Combine(PathHelper.HotfixResPath, toyDirName, downloadingBundle.File);
						string _storeDirPath = _storePath.Substring(0, _storePath.LastIndexOf("/"));
						if (!Directory.Exists(_storeDirPath)) Directory.CreateDirectory(_storeDirPath);

						try
						{
							using (UnityWebRequestAsync _webRequest = ComponentFactory.Create<UnityWebRequestAsync>())
							{
								await _webRequest.DownloadAsync(PathHelper.Combine(downloadUrl, toyDirName, downloadingBundle.File));

								byte[] _downloadData = _webRequest.Request.downloadHandler.data;
								
								if(MD5Helper.BytesMD5(_downloadData)!= downloadingBundle.SignMD5) //文件签名不一致
                                {
									throw new Exception($"download { PathHelper.Combine(toyDirName, downloadingBundle.File)} file data invalid ");
                                }

								using (FileStream sw = File.Open(_storePath,FileMode.OpenOrCreate,FileAccess.Write))
								{
									sw.Seek(0, SeekOrigin.Begin);
									sw.SetLength(0);
									await sw.WriteAsync(_downloadData, 0, _downloadData.Length);
								}
							}
						}
						catch (Exception error)
						{
							if (_downloadTryCount < 3)
							{
								_downloadTryCount++;
								Log.Error($"download bundle error: { PathHelper.Combine(toyDirName, downloadingBundle.File)}\n{error}");
								continue;
							}
							else
							{
								throw error;
							}
						}

						break;
					}

					long size = this.remoteFileVersionConfig.FileInfoDict[downloadingBundle.File].Size;
					g_alreadyDownloadBytes += size;
					g_progress = (int)(g_alreadyDownloadBytes * 100f / this.g_totalSize);
					g_isDone = false;

					downloadedBundles.Add(downloadingBundle);
					downloadingBundle = null;

					progressCallback(this);
				}

				//已经下载过的文件，记录在热更文件列表中
				if (downloadedBundles.Count > 0)
				{
					foreach (FileVersionInfo fvi in downloadedBundles)
					{
						hotfixFileVersionConfig.FileInfoDict.Add(fvi.File, fvi);
					}
				}
				//更新完毕，更新模块文件版本列表
				await UpdateHotfixFileVersionConfig();

				downloadedBundles.Clear();
				downloadingBundle = null;
				remoteFileVersionConfig = null;

				g_progress = 100;
				g_isDone = true;
				progressCallback(this);
			}
			catch (Exception e)
			{
				//已经下载过的文件，记录在热更文件列表中
				if(downloadedBundles.Count>0)
                {
					foreach(FileVersionInfo fvi in downloadedBundles)
                    {
						hotfixFileVersionConfig.FileInfoDict.Add(fvi.File, fvi);
                    }
					await UpdateHotfixFileVersionConfig();
                }

				g_error = e;
				g_isDone = true;
				progressCallback(this);
				throw e;
				Log.Error(e);
			}
		}

		/// <summary>
		/// 更新热更目录下的文件版本列表
		/// </summary>
		protected async ETTask UpdateHotfixFileVersionConfig()
		{
			if (hotfixFileVersionConfig == null) throw new Exception("hotfixFileVersionConfig is null");

			string _path = PathHelper.Combine(PathHelper.HotfixResPath, toyDirName, FileVersionConfigFileName);
			string _dirPath = PathHelper.Combine(PathHelper.HotfixResPath, toyDirName);
			if (!Directory.Exists(_dirPath)) Directory.CreateDirectory(_dirPath);

			using (FileStream sw = File.Open(_path,FileMode.OpenOrCreate,FileAccess.Write))
			{
				sw.Seek(0, SeekOrigin.Begin);
				sw.SetLength(0);
				byte[] _datas = JsonHelper.ToJson(hotfixFileVersionConfig).ToUtf8();
				await sw.WriteAsync(_datas, 0, _datas.Length);
			}
		}

		/// <summary>
		/// 清空指定范围外的模块目录
		/// </summary>
		/// <param name="toyName"></param>
		public void ClearHotfixToyDirWithOut(string[] toyDirNames)
		{
			for(int i=0;i<toyDirNames.Length;i++)
            {
				toyDirNames[i] = toyDirNames[i].ToLower();
            }

			// 删掉已经不需要的热更文件
			DirectoryInfo _directoryInfo = new DirectoryInfo(PathHelper.HotfixResPath);
			if (_directoryInfo.Exists)
			{
				DirectoryInfo[] dirInfos = _directoryInfo.GetDirectories();

				bool _isHave = false;
				foreach (DirectoryInfo dirInfo in dirInfos)
				{
					foreach(string toyDirName in toyDirNames)
                    {
						if(toyDirName==dirInfo.Name) 
                        {
							_isHave = true;
							break;
						}
                    }

					if (_isHave) continue;
					dirInfo.Delete(true);
				}
			}
		}

		/// <summary>
		/// 清空模块资源
		/// </summary>
		/// <param name="toyName"></param>
		public void ClearHotfixToyDir(string toyDirName)
		{
			string _path = Path.Combine(PathHelper.HotfixResPath, toyDirName.ToLower());

			if (Directory.Exists(_path))
			{
				Directory.Delete(_path, true);
				Directory.CreateDirectory(_path);
			}
		}

		/// <summary>
		/// 清空所有热更模块资源
		/// </summary>
		public void ClearHotfixToyDirAll()
        {
			if (Directory.Exists(PathHelper.HotfixResPath))
			{
				Directory.Delete(PathHelper.HotfixResPath, true);
				Directory.CreateDirectory(PathHelper.HotfixResPath);
			}
		}

		/// <summary>
		/// 检查热更资源包签名是否合法
		/// </summary>
		/// <param name="localToyMD5"></param>
		/// <param name="path"></param>
		public async ETTask<bool> CensorHotfixBundlesSignMD5(string[] paths)
        {
			Dictionary<string, List<string>> _toyAndPaths = new Dictionary<string, List<string>>();

			//整理路径
			for(int i=0;i<paths.Length;i++)
            {
				int _index = paths[i].IndexOf("/");
				if (_index == -1 || _index==paths[i].Length-1) throw new Exception($"invalid bundles path {paths[i]}");

				string _toyDirName = paths[i].Substring(0, _index);
				string _bundlePath= paths[i].Substring(_index+1).ToLower();
				if(!_toyAndPaths.TryGetValue(_toyDirName,out List<string> _bundlePathList))
                {
					_bundlePathList = new List<string>();
					_toyAndPaths.Add(_toyDirName,_bundlePathList);
				}
				_bundlePathList.Add(_bundlePath);
			}

			foreach (KeyValuePair<string, List<string>> kv in _toyAndPaths)
			{
				string _bundleToyName = kv.Key.ToLower();

				//模块目录不存在，则无需校验
				if (!Directory.Exists(PathHelper.Combine(PathHelper.HotfixResPath, _bundleToyName))) continue;
				
				//读取热更资源表
				FileVersionConfig _hotfixFileVersionConfig = null;
				string _hotfixFileVersionConfigPath = PathHelper.Combine(PathHelper.HotfixResPath, _bundleToyName, FileVersionConfigFileName);

				if (!File.Exists(_hotfixFileVersionConfigPath)) //没有资源表，这个模块没有热更新资源，确保资源文件不存在
				{
					//校验文件版本
					for (int i = 0; i < kv.Value.Count; i++)
					{
						string _filePath = PathHelper.Combine(PathHelper.HotfixResPath, _bundleToyName, kv.Value[i]);
						//没有资源表，但是实际文件存在，返回校验失败
						if (File.Exists(_filePath)) return false;
					}
				}
				else
				{
					using (FileStreamReadAsyncAsync _fileStreamReadAsync = ComponentFactory.Create<FileStreamReadAsyncAsync>())
					{

						await _fileStreamReadAsync.ReadAllAsync(_hotfixFileVersionConfigPath);
						_hotfixFileVersionConfig = JsonHelper.FromJson<FileVersionConfig>(System.Text.Encoding.UTF8.GetString(_fileStreamReadAsync.datas));
					}

					//校验文件版本
					for (int i = 0; i < kv.Value.Count; i++)
					{
						string _filePath = PathHelper.Combine(PathHelper.HotfixResPath, _bundleToyName, kv.Value[i]);

						//资源表中没有这个文件,无需校验
						if (!_hotfixFileVersionConfig.FileInfoDict.TryGetValue(kv.Value[i], out FileVersionInfo _fileVersionConfig))
						{
							//资源表没有，但是实际文件有，返回校验失败
							if (File.Exists(_filePath)) return false;
						}
						else
						{
							//资源表中有，但是实际文件没有，返回校验失败
							if (!File.Exists(_filePath)) return false;

							using (FileStreamReadAsyncAsync _fileStreamReadAsync = ComponentFactory.Create<FileStreamReadAsyncAsync>())
							{
								await _fileStreamReadAsync.ReadAllAsync(_filePath);

								//资源文件的实际内容和签名不一致，校验失败
								if (_fileVersionConfig.SignMD5 != MD5Helper.BytesMD5(_fileStreamReadAsync.datas)) return false;
							}
						}
					}
				}
			}

			return true;
		}
	}
}
