using System.Collections.Generic;
using System.IO;
using ETModel;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
    public class AppVersionEditor:EditorWindow
    {
        private const string BuildDirPath = "../Release/{0}/";

        public static string BuildAppVersionConfigFilePath = "../Release/{0}/AppVersion.json";

        public static string AppVersionConfigFileName = "AppVersion.json";


        [MenuItem("Tools/打包/App版本配置")]
        public static void ShowWindow()
        {
            GetWindow<AppVersionEditor>();
        }



        private AppVersionConfig appVersionConfig;
        private PlatformType platformType;


        public void Awake()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="platformType"></param>
        /// <returns></returns>
        public static AppVersionConfig LoadAppVersionConfig(PlatformType platformType)
        {
            string _path = string.Format(BuildAppVersionConfigFilePath, platformType.ToString());
            if (File.Exists(_path))
            {
                return JsonHelper.FromJson<AppVersionConfig>(File.ReadAllText(_path, System.Text.Encoding.UTF8));
            }
            else
            {
                return new AppVersionConfig();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appVersionConfig"></param>
        /// <param name="platformType"></param>
        public static void SaveAppVersionConfig(AppVersionConfig appVersionConfig, PlatformType platformType,bool isShowDialog=false)
        {
            string _dirPath = string.Format(BuildDirPath, platformType.ToString());
            string _path = string.Format(BuildAppVersionConfigFilePath, platformType.ToString());

            if (!Directory.Exists(_dirPath)) Directory.CreateDirectory(_dirPath);
            if (File.Exists(_path)) File.Delete(_path);

            byte[] _textData = JsonHelper.ToJson(appVersionConfig).ToUtf8();
            File.WriteAllBytes(_path, _textData);

            if (isShowDialog)
            {
                EditorUtility.DisplayDialog("提示", "保存 AppVersionConfig 完毕", "是");
            }
        }

        /// <summary>
        /// 保存配置到指定位置
        /// </summary>
        /// <param name="appVersionConfig"></param>
        /// <param name="platformType"></param>
        public static void SaveAppVersionConfig(AppVersionConfig appVersionConfig, string path, bool isShowDialog = false)
        {
            byte[] _textData = JsonHelper.ToJson(appVersionConfig).ToUtf8();
            File.WriteAllBytes(path, _textData);

            if (isShowDialog)
            {
                EditorUtility.DisplayDialog("提示", "保存 AppVersionConfig 完毕", "是");
            }
        }


        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            PlatformType _oldPlatformType = platformType;
            platformType = (PlatformType)EditorGUILayout.EnumPopup(_oldPlatformType);

            if (platformType == PlatformType.None) return;

            if (_oldPlatformType != platformType)
            {
                appVersionConfig = LoadAppVersionConfig(platformType);
            }

            this.appVersionConfig.IsServerNormal = EditorGUILayout.Toggle("服务器是否正常运行: ", this.appVersionConfig.IsServerNormal);
            this.appVersionConfig.MaintainAnnouncement = EditorGUILayout.TextField("维护公告: ", this.appVersionConfig.MaintainAnnouncement == null ? "" : this.appVersionConfig.MaintainAnnouncement);
            this.appVersionConfig.ServerAddress = EditorGUILayout.TextField("登录服务器地址: ", this.appVersionConfig.ServerAddress == null ? "" : this.appVersionConfig.ServerAddress);
            this.appVersionConfig.AppVersionConfigRequestUrl = EditorGUILayout.TextField("app版本配置下载地址: ", this.appVersionConfig.AppVersionConfigRequestUrl == null ? "" : this.appVersionConfig.AppVersionConfigRequestUrl);
            this.appVersionConfig.HotfixDownloadUrl = EditorGUILayout.TextField("热更下载地址: ", this.appVersionConfig.HotfixDownloadUrl == null ? "" : this.appVersionConfig.HotfixDownloadUrl);
            this.appVersionConfig.AppDownloadUrl = EditorGUILayout.TextField("APP下载地址: ", this.appVersionConfig.AppDownloadUrl == null ? "" : this.appVersionConfig.AppDownloadUrl);
            this.appVersionConfig.Channel =int.Parse(EditorGUILayout.TextField("渠道号: ", this.appVersionConfig.Channel.ToString()));

            string _oldVersion = ((int)(this.appVersionConfig.Version >> 32)).ToString() + "." + (int)(this.appVersionConfig.Version);
            string _version = EditorGUILayout.TextField("当前app版本: ", _oldVersion);
            string[] _verstionStr = null;
            int _mainVer = 0;
            int _subVer = 0;

            if (_version != _oldVersion)
            {
                if (_version != "")
                {
                    _verstionStr = _version.Split('.');
                    if (_verstionStr.Length == 2 && int.TryParse(_verstionStr[0], out _mainVer) && int.TryParse(_verstionStr[1], out _subVer))
                    {
                        this.appVersionConfig.Version = ((long)_mainVer << 32) + (long)_subVer;
                    }
                }
            }

            _oldVersion = ((int)(this.appVersionConfig.ColdUpdateVersion >> 32)).ToString() + "." + (int)(this.appVersionConfig.ColdUpdateVersion);
            _version = EditorGUILayout.TextField("app冷更新版本（低于冷更新）: ", _oldVersion);
            if (_version != _oldVersion)
            {
                if (_version != "")
                {
                    _verstionStr = _version.Split('.');
                    if (_verstionStr.Length == 2 && int.TryParse(_verstionStr[0], out _mainVer) && int.TryParse(_verstionStr[1], out _subVer))
                    {
                        this.appVersionConfig.ColdUpdateVersion = ((long)_mainVer << 32) + (long)_subVer;
                    }
                }
            }

            foreach (KeyValuePair<string, ToyVersionConfig> item in this.appVersionConfig.ToyVersionConfigs)
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField("模块名: ", item.Value.Name == null ? "" : item.Value.Name);
                EditorGUILayout.LabelField("MD5: ", item.Value.SignMD5== null ? "" : item.Value.SignMD5);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("保存"))
            {
                SaveAppVersionConfig(appVersionConfig, platformType,true);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}


