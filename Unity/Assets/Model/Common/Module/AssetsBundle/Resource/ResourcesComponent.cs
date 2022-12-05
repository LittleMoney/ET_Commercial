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
	public class ResourcesComponent : Component
	{
		public static ResourcesComponent Instance;

		public struct AsyncBundleRequest
		{
			public AssetBundleCreateRequest request;
			public string assetBundleName;
		}

		public struct AsyncAssetRequest
		{
			public AssetBundleRequest request;
			public ABInfo abInfo;
			public long abInfoInstanceId;
		}


		public static AssetBundleManifest AssetBundleManifest { get; set; }

		private readonly Dictionary<string, ABInfo> bundles = new Dictionary<string, ABInfo>();

		private readonly Dictionary<string, AssetBundleCreateRequest> asyncLoadingBundles = new Dictionary<string, AssetBundleCreateRequest>();

		private readonly Dictionary<ABInfo, AssetBundleRequest> asyncLoadingAssets = new Dictionary<ABInfo, AssetBundleRequest>();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			foreach (var abInfo in this.bundles)
			{
				abInfo.Value.Dispose();
			}

			this.bundles.Clear();
		}



		/// <summary>
		/// 同步加载资源
		/// </summary>
		/// <param name="path">资源路径</param>
		/// <param name="type">资源类型</param>
		/// <returns></returns>
		public UnityEngine.Object LoadAsset(string path,Type type)
		{
			string _assetBundleName = path.BundleNameToLower();
			ABInfo abInfo = null;


#if UNITY_EDITOR
			if (!Init.UseAssetBundle)
			{
				if (bundles.TryGetValue(_assetBundleName, out abInfo))
				{
					return AssetBundleHelper.LoadResource(path, type);
				}
				else
				{
					LoadBundle(path);
					return AssetBundleHelper.LoadResource(path, type);
				}

				return null;
			}
#endif


			if (!bundles.TryGetValue(_assetBundleName, out abInfo))
			{
				LoadBundle(path);

				if (!bundles.TryGetValue(_assetBundleName, out abInfo))
				{
					throw new Exception($"LoadAsset error {path} {type}");
				}
			}

			if (abInfo.MainAsset == null)
			{
				UnityEngine.Object _assets= abInfo.AssetBundle.LoadAsset(path.GetPureFileName(), type);

				if(_assets==null)
                {
					throw new Exception($"LoadAsset error {path} {type} return null");
				}

				abInfo.MainAsset = _assets;
				abInfo.MainAssetType = _assets.GetType();

				return _assets;
			}
			else
			{
				if (abInfo.MainAssetType == type)
				{
					return abInfo.MainAsset;
				}
				else
				{
					UnityEngine.Object _assets = abInfo.AssetBundle.LoadAsset(path.GetPureFileName(), type);

					if (_assets == null)
					{
						throw new Exception($"LoadAsset error {path} {type} return null");
					}
					return _assets;
				}
			}

		}

		/// <summary>
		/// 异步加载资源
		/// </summary>
		/// <param name="path">资源路径</param>
		/// <param name="type">资源类型</param>
		/// <returns></returns>
		public async ETTask<UnityEngine.Object> LoadAssetAsync(string path, Type type)
		{
			string _assetBundleName = path.BundleNameToLower();
			ABInfo abInfo = null;

#if UNITY_EDITOR
			if ( !Init.UseAssetBundle)
			{

				if (bundles.TryGetValue(_assetBundleName, out abInfo))
				{
					return AssetBundleHelper.LoadResource(path, type);
				}
				else
				{
					await LoadBundleAsync(path);
					return AssetBundleHelper.LoadResource(path, type);
				}
			}
#endif

			if (!bundles.TryGetValue(_assetBundleName, out abInfo))
			{
				await LoadBundleAsync(path);

				if (!bundles.TryGetValue(_assetBundleName, out abInfo))
				{
					throw new Exception($"LoadAssetAsync error {path} {type}");
				}
			}

			if (type == typeof(Scene)) return null;

			if (abInfo.MainAsset == null)
			{
				AssetBundleRequest _abr = abInfo.AssetBundle.LoadAssetAsync(path.GetPureFileName(), type);
				while (!_abr.isDone)
				{
					await GameObjectHelper.WaitOneFrame();
				}

				if (_abr.asset==null)
				{
					throw new Exception($"LoadAssetAsync error {path} {type}");
				}

				abInfo.MainAsset = _abr.asset;
				abInfo.MainAssetType = _abr.asset.GetType();
				return _abr.asset;

			}
			else
			{
				if (abInfo.MainAssetType == type)
				{
					return abInfo.MainAsset;
				}
				else
				{
					AssetBundleRequest _abr = abInfo.AssetBundle.LoadAssetAsync(path.GetPureFileName(), type);
					while (!_abr.isDone)
					{
						await GameObjectHelper.WaitOneFrame();
					}

					if (_abr.asset == null)
					{
						throw new Exception($"LoadAssetAsync error {path} {type}");
					}

					return _abr.asset;
				}
			}
		}

		/// <summary>
		/// 卸载正则表达式匹配名称的的资源包
		/// </summary>
		/// <param name="_regexPattern"></param>
		public void UnloadBundleForRegex(string pathRegexPattern)
        {
			System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex(pathRegexPattern);
			List<string> _tempList = new List<string>();
			foreach(KeyValuePair<string,ABInfo> item in bundles)
            {
				if (_regex.IsMatch(item.Key))
				{
					_tempList.Add(item.Key);
				}
            }

			foreach(string assetBundleName in _tempList)
            {
				UnloadBundle(assetBundleName);
			}
        }

		/// <summary>
		/// 卸载资源包
		/// </summary>
		/// <param name="path">资源包的路径，相对于"Bundles/"</param>
		public void UnloadBundle(string path,bool withDepend=false)
		{
			string _assetBundleName= path.BundleNameToLower();

#if UNITY_EDITOR
			if (!Init.UseAssetBundle)
			{

				this.UnloadOneBundle(_assetBundleName);

				return;
			}
#endif

			if (withDepend)
			{
                string[] dependencies = AssetBundleHelper.GetSortedDependencies(_assetBundleName, AssetBundleManifest);

                //Log.Debug($"-----------dep unload {assetBundleName} dep: {dependencies.ToList().ListToString()}");
                foreach (string dependency in dependencies)
                {
                    this.UnloadOneBundle(dependency);
                }
            }
            else
            {
				this.UnloadOneBundle(_assetBundleName);
			}
		}

		/// <summary>
		/// 卸载一个资源包
		/// </summary>
		/// <param name="assetBundleName"></param>
		private void UnloadOneBundle(string assetBundleName)
		{
			if (asyncLoadingBundles.TryGetValue(assetBundleName, out AssetBundleCreateRequest request))
			{
				request.assetBundle.Unload(true);
				asyncLoadingBundles.Remove(assetBundleName);
			}
			else
			{
				ABInfo abInfo;
				if (!this.bundles.TryGetValue(assetBundleName, out abInfo))
				{
					throw new Exception($"not found assetBundle: {assetBundleName}");
				}

				this.bundles.Remove(assetBundleName);
				abInfo.Dispose();
			}
		}



		/// <summary>
		/// 同步加载资源包
		/// </summary>
		/// <param name="path">资源包的路径，相对于"Bundles/"</param>
		/// <returns></returns>
		public void LoadBundle(string path)
		{
			string _assetBundleName = path.BundleNameToLower();
			ABInfo _abInfo;


#if UNITY_EDITOR
			if (!Init.UseAssetBundle)
			{
				if (this.bundles.TryGetValue(_assetBundleName, out _abInfo)) //如果资源包已经加载，增加引用计数
				{
					return;
				}

				_abInfo = ComponentFactory.CreateWithParent<ABInfo>(this);
				_abInfo.Name = _assetBundleName;
				_abInfo.AssetBundle = null;
				this.bundles.Add(_assetBundleName,_abInfo);

				return;
			}
#endif

			if (this.bundles.TryGetValue(_assetBundleName, out _abInfo)) //如果资源包已经加载，增加引用计数
			{
				return;
			}

			LoadABManifest();

			string[] dependencies = AssetBundleHelper.GetSortedDependencies(_assetBundleName, AssetBundleManifest);
			//Log.Debug($"-----------dep load {assetBundleName} dep: {dependencies.ToList().ListToString()}");
			foreach (string dependency in dependencies)
			{
				if (string.IsNullOrEmpty(dependency))
				{
					continue;
				}
				this.LoadOneBundle(dependency);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetBundleName"></param>
		public void LoadOneBundle(string assetBundleName)
		{
			//如果有移除异步加载，直接同步加载
			if (asyncLoadingBundles.TryGetValue(assetBundleName, out AssetBundleCreateRequest request))
			{
				asyncLoadingBundles.Remove(assetBundleName);
				request.assetBundle.Unload(true); //取消异步加载
			}

			ABInfo _abInfo;
			if (this.bundles.TryGetValue(assetBundleName, out _abInfo)) //如果资源包已经加载，增加引用计数
			{
				return;
			}
			else
			{
				string _path = PathHelper.Combine(PathHelper.HotfixResPath, assetBundleName);
				AssetBundle _assetBundle = null;
				if (File.Exists(_path))
				{
					_assetBundle = AssetBundle.LoadFromFile(_path);
				}
				else
				{
					_path = Path.Combine(PathHelper.BuildInResPath, assetBundleName);
					_assetBundle = AssetBundle.LoadFromFile(_path);
				}

				if (_assetBundle == null)
				{
					throw new Exception($"assets bundle not found: {assetBundleName}");
				}

				_abInfo = ComponentFactory.CreateWithParent<ABInfo>(this);
				_abInfo.AssetBundle = _assetBundle;
				_abInfo.Name = assetBundleName;
				this.bundles.Add(assetBundleName,_abInfo);
			}
		}



		/// <summary>
		/// 异步加载资源包(并发式)
		/// </summary>
		/// <param name="path">资源包的路径，相对于"Bundles/"</param>
		/// <param name="progressCallback">进度回调 int 进度(最大100) bool 是否完成 string 加载资源包名 Exception 加载异常(不为null表示加载失败)) </param>
		/// <returns></returns>
		public async ETTask LoadBundleAsync(string path, Action<int,bool,string,Exception> progressCallback=null)
		{
			string _assetBundleName = path.BundleNameToLower();
			ABInfo _abInfo=null;

#if UNITY_EDITOR
			if (!Init.UseAssetBundle)
			{

				if (!this.bundles.TryGetValue(_assetBundleName, out _abInfo)) //如果资源包已经加载
				{
					_abInfo = ComponentFactory.CreateWithParent<ABInfo, string, AssetBundle>(this, _assetBundleName, null);
					_abInfo.Name = _assetBundleName;
					this.bundles[_assetBundleName] = _abInfo;

					progressCallback?.Invoke(100, true, _assetBundleName,null);
				}
				return;
			}
#endif
			if (this.bundles.TryGetValue(_assetBundleName, out _abInfo)) //如果资源包已经加载
			{
                progressCallback?.Invoke(100, true, _assetBundleName,null);
				return;
			}
			else
			{
				LoadABManifest();

				string[] dependencies = AssetBundleHelper.GetSortedDependencies(_assetBundleName, AssetBundleManifest);
				await LoadBundleAsyncProcess(_assetBundleName, dependencies, progressCallback);
			}
		}

		protected async ETTask LoadBundleAsyncProcess(string assetBundleName, string[] dependencies, Action<int, bool, string,Exception> progressCallback = null)
		{
			string _path = null;
			string _assetBundleName = null;
			ABInfo _abInfo = null;

			AssetBundleCreateRequest _bundleRequest = null;
			List<AssetBundleCreateRequest> _bundleRequestList = new List<AssetBundleCreateRequest>();

			int _totalProgress = 0;
			int _progress = 0;

			//发起对所有资源包的异步加载
			foreach (string _assetBundleName2 in dependencies)
			{
				if (this.bundles.TryGetValue(_assetBundleName2, out _abInfo)) //已经被成功加载
				{
					continue;
				}


				if (asyncLoadingBundles.TryGetValue(_assetBundleName2, out _bundleRequest)) //已经在加载中
				{
					_bundleRequestList.Add(_bundleRequest);
					continue;
				}
				else //尚未加载发起加载
				{
					//获取路径
					_path = PathHelper.Combine(PathHelper.HotfixResPath, _assetBundleName2);
					if (!File.Exists(_path))
					{
						_path = PathHelper.Combine(PathHelper.BuildInResPath, _assetBundleName2);
					}

					_bundleRequest = AssetBundle.LoadFromFileAsync(_path);

					asyncLoadingBundles.Add(_assetBundleName2, _bundleRequest);
					_bundleRequestList.Add(_bundleRequest);
				}
			}

			//等待所有的资源包加载完毕
			_totalProgress = _bundleRequestList.Count;
			while (true)
			{
				for (int i = 0; i < _bundleRequestList.Count; i++)
				{
					_bundleRequest = _bundleRequestList[i];
					if (!_bundleRequest.isDone) break;

					if (progressCallback != null)
					{
						_progress = (int)((float)(_totalProgress - _bundleRequestList.Count) / (float)_totalProgress) * 50;
						progressCallback?.Invoke(_progress, false, assetBundleName,null);
					}

					_assetBundleName = _bundleRequest.assetBundle.name;
					if (asyncLoadingBundles.ContainsKey(_assetBundleName))  //尚未被处理
					{
						asyncLoadingBundles.Remove(_assetBundleName);
						_bundleRequestList.RemoveAt(i);
						i--;

						if (_bundleRequest.assetBundle != null) 
						{
							_abInfo = ComponentFactory.CreateWithParent<ABInfo>(this);
							_abInfo.AssetBundle = _bundleRequest.assetBundle;
							_abInfo.Name= _bundleRequest.assetBundle.name;
							this.bundles.Add(_bundleRequest.assetBundle.name,_abInfo);
						}
                        else
                        {
							Exception _error = new Exception($"assets bundle not found: {_assetBundleName} ");
							progressCallback?.Invoke(_progress, true, assetBundleName, _error);
							throw _error;
						}
					}
					else if (this.bundles.TryGetValue(_assetBundleName, out _abInfo)) //已经被其他流程加载完毕
					{
						_bundleRequestList.RemoveAt(i);
						i--;
					}
					else //出现了加载失败
					{
						Exception _error = new Exception($"assets bundle not found: {_assetBundleName} ");
						progressCallback?.Invoke(_progress, true, assetBundleName,_error);
						throw _error;
					}
				}

				if (_bundleRequestList.Count == 0) break;
				await GameObjectHelper.WaitOneFrame();
			}

			# region 等待所有的资源包中的主资源都加载完毕
			// _totalProgress = _assetRequestList.Count;
			//while (true)
			//{
			//	if (progressCallback != null)
			//	{
			//		_progress = 50+(int)((float)(_totalProgress - _bundleRequestList.Count) / (float)_totalProgress) * 50;
			//		progressCallback.Invoke(_progress, false, assetBundleName, null);
			//	}

			//	for (int i = 0; i < _assetRequestList.Count; i++)
			//	{
			//		_assetRequest = _assetRequestList[i].request;
			//		if (!_assetRequest.isDone) break;

			//		_abInfo = _assetRequestList[i].abInfo;

			//		if (_abInfo.InstanceId!= _assetRequestList[i].abInfoInstanceId) //资源包已经被卸载
			//		{
			//			Exception _error = new Exception($"assets bundle is null: {_assetBundleName} ");
			//			progressCallback.Invoke(_progress, true, _assetBundleName, _error);
			//			throw _error;
			//		}
			//		else if (asyncLoadingAssets.ContainsKey(_abInfo)) //尚未处理，立即处理
			//		{
			//			asyncLoadingAssets.Remove(_abInfo);
			//			_assetRequestList.RemoveAt(i);
			//			i--;

			//			if (_assetRequest.asset != null)
			//			{
			//				_abInfo.MainAsset = _assetRequest.asset;
			//			}
			//			else //加载主资源未能成功
			//			{
			//				Exception _error = new Exception($"assets bundle not load main asset: {_assetBundleName} ");
			//				progressCallback.Invoke(_progress, true, _assetBundleName, _error);
			//				throw _error;
			//			}
			//		}
			//		else if(_abInfo.MainAsset!=null) //资源已经被加载
			//		{
			//			_assetRequestList.RemoveAt(i);
			//			i--;
			//		}
   //                 else
   //                 {
			//			Exception _error = new Exception($"assets bundle not load main asset: {_assetBundleName} ");
			//			progressCallback.Invoke(_progress, true, _assetBundleName, _error);
			//			throw _error;
			//		}
			//	}

			//	if (_assetRequestList.Count == 0) break;
			//	await GameObjectHelper.WaitOneFrame();
			//}
			#endregion

			progressCallback?.Invoke(100,true, assetBundleName, null);
		}

		static void LoadABManifest()
		{
			if (AssetBundleManifest != null)
			{
				return;
			}
			string tempPath = PathHelper.Combine(PathHelper.HotfixResPath,PathHelper.CommonDirName, PathHelper.ResRootDirName);
			if (!File.Exists(tempPath))
			{
				tempPath = PathHelper.Combine(PathHelper.BuildInResPath, PathHelper.CommonDirName, PathHelper.ResRootDirName);
			}
			AssetBundle manifestAB = AssetBundle.LoadFromFile(tempPath);
			AssetBundleManifest = manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
		}

		public string DebugString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (ABInfo abInfo in this.bundles.Values)
			{
				sb.Append($"{abInfo.Name}\n");
			}
			return sb.ToString();
		}

	}


	[ObjectSystem]
	public class ResourcesComponentAwakeSystem : AwakeSystem<ResourcesComponent>
	{
		public override void Awake(ResourcesComponent self)
		{
			ResourcesComponent.Instance = self;
		}
	}
}