using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
    public class ServerMaintainException : Exception
    {
        public ServerMaintainException(string message):base(message)
        {

        }
    }

    public static class HotUpdateHelper
    {
        /// <summary>
        /// 校验App是否需要采取更新枚举中的一种
        /// </summary>
        /// <param name="toyName"></param>
        /// <param name="isForceUpdate"></param>
        /// <returns></returns>
        public static async ETTask<UpdateType> CheckAppNeedUpdate()
        {
            bool _isForceUpdate = UnityEngine.PlayerPrefs.GetInt("_isForceUpdate", 0) == 1;


            AppVersionComponent _appVersionComponent = ETModel.Game.Scene.GetComponent<AppVersionComponent>();
            if (_appVersionComponent == null)
            {
                _appVersionComponent = ETModel.Game.Scene.AddComponent<AppVersionComponent>();
            }

            //强制更新下，清理掉所有资源和版本数据
            if (_isForceUpdate) 
            {
                _appVersionComponent.RemoveAppVersionConfigInHotfix();

                using (BundleDownloaderComponent _bundleDownloaderComponent = ComponentFactory.Create<BundleDownloaderComponent>())
                {
                    _bundleDownloaderComponent.ClearHotfixDir();
                }

                UnityEngine.PlayerPrefs.GetInt("_isForceUpdate", 0);
            }

            try
            {
                BroadcastComponent.Instance.g_default.Run<int, string>(BroadcastId.ProgressMessage, 0, "核对APP版本信息");

                await _appVersionComponent.LoadServerAppVersionConfigAsync();
                BroadcastComponent.Instance.g_default.Run<int, string>(BroadcastId.ProgressMessage, 40, "获取远程APP版本信息完毕");

                await _appVersionComponent.LoadLocalAppVersionConfigAsync();
                BroadcastComponent.Instance.g_default.Run<int, string>(BroadcastId.ProgressMessage, 20, "载入本地APP版本信息完毕");

            }
            catch (Exception error)
            {
                BroadcastComponent.Instance.g_default.Run<int,string>(BroadcastId.ProgressMessage, 0, $"加载服务端版本信息失败:{error.Message}");
                throw error;
            }

            if (!_appVersionComponent.IsServerNormal())
            {
                BroadcastComponent.Instance.g_default.Run<int,string>(BroadcastId.ProgressMessage, 0, $"{_appVersionComponent.GetServerAnnouncement()}");
                throw new ServerMaintainException($"{_appVersionComponent.GetServerAnnouncement()}");
            }

            //核对版本信息
            UpdateType _updateType = _appVersionComponent.CheckAppNeedUpdate();
            if(_updateType!=UpdateType.Cold)
            {
                _appVersionComponent.ClearDeprecatedLocalToyVersionConfigs();

                using (BundleDownloaderComponent _bundleDownloaderComponent = ComponentFactory.Create<BundleDownloaderComponent>())
                {
                    _bundleDownloaderComponent.ClearHotfixToyDirWithOut(_appVersionComponent.GetAllLocalToyDirNames());
                }

                //同步app版本信息
                _appVersionComponent.OverrideLocalAppVersionConfigWithOutToy();

                await _appVersionComponent.SavelocalAppVersionConfigToHotfixAsync();
            }

            BroadcastComponent.Instance.g_default.Run<int,string>(BroadcastId.ProgressMessage, 100, "检查APP版本信息完毕");

            return _updateType;
        }

        /// <summary>
        /// 启动冷更新流程
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async ETTask AppColdUpdate()
        {
            AppVersionComponent _appVersionComponent = null;

            _appVersionComponent = ETModel.Game.Scene.AddComponent<AppVersionComponent>();

            using (UnityWebRequestAsync webRequestAsync = ComponentFactory.Create<UnityWebRequestAsync>())
            {
                await webRequestAsync.DownloadAsync(_appVersionComponent.GetAppDownloadUrl(), (uwr) => {
                    BroadcastComponent.Instance.GetDefault().Run<int,string>(BroadcastId.ProgressMessage, (int)(uwr.Request.downloadProgress * 100), $"已下载APP安装包...{uwr.Request.downloadedBytes / (1024 * 1024)}M");

                });

                //删除文件
                if (File.Exists(PathHelper.AppInstallFilePath))
                {
                    File.Delete(PathHelper.AppInstallFilePath);
                }

                using (FileStream fs = File.Open(PathHelper.AppInstallFilePath, FileMode.OpenOrCreate))
                {
                    await fs.WriteAsync(webRequestAsync.Request.downloadHandler.data, 0, webRequestAsync.Request.downloadHandler.data.Length);
                }
            }
        }


        /// <summary>
        /// 校验Toy是否需要更新
        /// </summary>
        /// <param name="toyDirName"></param>
        /// <returns></returns>
        public static  bool CheckToyNeedUpdate(string toyDirName)
        {
            //无需更新直接返回
            return ETModel.Game.Scene.GetComponent<AppVersionComponent>().CheckToyNeedUpdate(toyDirName);
        }

        /// <summary>
        /// 更新模块
        /// </summary>
        /// <param name="toyDirName"></param>
        /// <returns></returns>
        public static async ETTask ToyUpdate(string toyDirName,bool isForceUpdate=false)
        {
            AppVersionComponent  _appVersionComponent = ETModel.Game.Scene.GetComponent<AppVersionComponent>();

            //无需更新直接返回
            if (!isForceUpdate && !_appVersionComponent.CheckToyNeedUpdate(toyDirName))
            {
                BroadcastComponent.Instance.GetDefault().Run<int,string>(BroadcastId.ProgressMessage, 100, $"模块 {toyDirName} 无需更新");
                return;
            }

            using (BundleDownloaderComponent _bundleDownloaderComponent = ComponentFactory.Create<BundleDownloaderComponent>())
            {

                BroadcastComponent.Instance.GetDefault().Run<int,string>(BroadcastId.ProgressMessage, 0, $"更新 {toyDirName} 模块资源");

                await _bundleDownloaderComponent.StartAsync(_appVersionComponent.GetAppHotfixUrl(), toyDirName, isForceUpdate, (bdc) =>
                {
                    switch (bdc.g_startAsyncStatus)
                    {
                        case 1:
                            BroadcastComponent.Instance.GetDefault().Run<int,string>(BroadcastId.ProgressMessage, bdc.g_startAsyncStatus / 4 * 100, $"读取 {toyDirName} 本地版本文件");
                            break;
                        case 2:
                            BroadcastComponent.Instance.GetDefault().Run<int,string>(BroadcastId.ProgressMessage, bdc.g_startAsyncStatus / 4 * 100, $"下载 {toyDirName} 远端版本文件");
                            break;
                        case 3:
                            BroadcastComponent.Instance.GetDefault().Run<int,string>(BroadcastId.ProgressMessage, bdc.g_startAsyncStatus / 4 * 100, $"读取App {toyDirName} 版本文件");
                            break;
                        case 4:
                            BroadcastComponent.Instance.GetDefault().Run<int,string>(BroadcastId.ProgressMessage, bdc.g_startAsyncStatus / 4 * 100, $"分析 {toyDirName} 需要下载资源完毕");
                            break;
                        case 5:
                            BroadcastComponent.Instance.GetDefault().Run<int,string>(BroadcastId.ProgressMessage, 100, $"分析 {toyDirName} 需要下载资源完毕，无需更新");
                            break;
                    }

                });

               
                if (_bundleDownloaderComponent.g_totalSize > 0)  //需要更新，等待更新完毕
                {
                    await _bundleDownloaderComponent.DownloadAsync((bdc) =>
                    {
                        BroadcastComponent.Instance.GetDefault().Run<int, string>(BroadcastId.ProgressMessage, bdc.g_progress, $"下载文件{bdc.downloadingBundle}中...");
                    });
                }

                //同步模块的签名
                _appVersionComponent.OverrideLocalToyVersionConfig(toyDirName);
                await _appVersionComponent.SavelocalAppVersionConfigToHotfixAsync();
            }
            return;
        }

        /// <summary>
        /// 启动下次启动强制更新
        /// </summary>
        public static void SetLaterForceUpdate()
        {
            UnityEngine.PlayerPrefs.SetInt("__isForceUpdate", 1);
        }

    }
}
