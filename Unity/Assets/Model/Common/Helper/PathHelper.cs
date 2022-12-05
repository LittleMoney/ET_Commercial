using UnityEngine;

namespace ETModel
{
    public static partial class PathHelper
    {
        /// <summary>
        /// 通用模块的的文件夹名称
        /// </summary>
        public const string CommonDirName = "Common";

        /// <summary>
        ///应用程序外部资源路径存放路径(热更新资源路径)
        /// </summary>
        public static string HotfixResPath
        {
            get
            {
                return $"{Application.persistentDataPath}/gameres/"; ;
            }

        }

        /// <summary>
        /// 应用程序内部资源路径存放路径
        /// </summary>
        public static string BuildInResPath
        {
            get
            {
                return $"{Application.streamingAssetsPath}/gameres/"; ;
            }
        }

        /// <summary>
        ///应用程序外部资源路径存放路径(热更新资源路径)
        /// </summary>
        public static string ResRootDirName
        {
            get
            {
                return "gameres";
            }
        }

        /// <summary>
        /// 应用程序内部资源路径存放路径(www/webrequest专用)
        /// </summary>
        public static string BuildInResPath4Web
        {
            get
            {
#if UNITY_IOS || UNITY_STANDALONE_OSX
                return $"file://{Application.streamingAssetsPath}/gameres/";
#else
                return $"{Application.streamingAssetsPath}/gameres/";
#endif

            }
        }

#if UNITY_EDITOR

        public static string EditorResPath
        {
            get
            {
                return "Assets/Bundles";
            }
        }
#endif

        /// <summary>
        /// 更新后的app版本文件路径
        /// </summary>
        public static string HotfixAppVersionConfigPath
        {
            get
            {
                return $"{Application.persistentDataPath}/AppVersion.json";
            }
        }

        /// <summary>
        /// 程序自带的app版本文件路径
        /// </summary>
        public static string BuildInAppVersionConfigPath
        {
            get
            {
                return $"{Application.streamingAssetsPath}/AppVersion.json";
            }
        }

        /// <summary>
        /// 应用程序内部资源路径存放路径(www/webrequest专用)
        /// </summary>
        public static string BuildInAppVersionConfigPath4Web
        {
            get
            {
#if UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
                return $"file://{Application.streamingAssetsPath}/AppVersion.json";
#else
                return $"{Application.streamingAssetsPath}/AppVersion.json";
#endif

            }
        }

        /// <summary>
        /// app版本文件名称带后缀
        /// </summary>
        public static string AppVersionConfigFileName
        {
            get
            {
                return $"AppVersion.json";
            }
        }


        /// <summary>
        /// 冷更新APP安装文件保存路径
        /// </summary>
        public static string AppInstallFilePath
        {
            get
            {
#if UNITY_ANDROID
                return Application.persistentDataPath + "/"+Application.productName+".apk";
#else
                return Application.persistentDataPath + "/"+ Application.productName + ".exe";
#endif
            }
        }


        /// <summary>
        /// 组合路径，如果根路径末尾带有反斜杠，或则最后一个分割符是反斜杠，则使用绝对路径合并。
        /// 其他情况下按照相对路径合并
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string Combine(string rootPath, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return rootPath;
            }
            else
            {

                //在editor情况下，有可能会给绝对地址为到更新的url ，正常情况下不可能出现
#if UNITY_EDITOR
                if (rootPath[rootPath.Length - 1]=='\\' ) //有反斜杠确定为绝对路径
                {
                    return rootPath+ path;
                }
                else if (rootPath[rootPath.Length - 1] != '/') //最后一个既不是反斜杠，也不是斜杠
                {
                    int _backslashLastIndex = rootPath.LastIndexOf('\\');
                    int _slashLastIndex = rootPath.LastIndexOf('/');
                    if(_backslashLastIndex>-1 && (_slashLastIndex==-1 || _backslashLastIndex > _slashLastIndex)) //最后一个路径分隔符是反斜杠
                    {
                        return rootPath+"\\"+path;
                    }
                    else //最后一个路劲分割符不是反斜杠或则没有，使用正斜杆
                    {
                        return rootPath + "/" + path;
                    }
                }
#endif

                if (rootPath[rootPath.Length - 1] == '/')
                {
                    return rootPath + path;
                }
                else
                {
                    return rootPath + "/" + path;
                }
            }
        }

        /// <summary>
        /// 组合路径，如果根路径末尾带有反斜杠，或则最后一个分割符是反斜杠，则使用绝对路径合并。
        /// 其他情况下按照相对路径合并
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string Combine(string rootPath, string path1, string path2)
        {
            string _resultPath = Combine(rootPath, path1);
            return Combine(_resultPath, path2);
        }


        /// <summary>
        /// 返回目录名称
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPureDirName(this string path)
        {
            int _indexSlash = path.LastIndexOf('/');

#if UNITY_EDITOR //反斜杠只会在编辑器模式下出现
            int _indexAntSlash = path.LastIndexOf('\\');

            if (_indexAntSlash > _indexSlash)
            {
                _indexSlash = _indexAntSlash;
            }
#endif

            return path.Substring(_indexSlash + 1);
        }

        /// <summary>
        /// 获取文件名不要后缀 例如 DDZ/UI.unity 则对名为 UI
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPureFileNameNoExtension(this string path)
        {
            int _indexSlash = path.LastIndexOf('/');

#if UNITY_EDITOR //反斜杠只会在编辑器模式下出现
            int _indexAntSlash = path.LastIndexOf('\\');

            if (_indexAntSlash > _indexSlash)
            {
                _indexSlash = _indexAntSlash;
            }
#endif


            int _indexDot = path.LastIndexOf('.');

            if (_indexSlash == -1)
            {
                if (_indexDot == -1)
                {
                    return path;
                }
                else
                {
                    return path.Substring(0, _indexDot);
                }
            }
            else
            {
                if (_indexDot == -1 || _indexDot < _indexSlash)
                {
                    return path.Substring(_indexSlash + 1);
                }
                else
                {
                    return path.Substring(_indexSlash + 1, _indexDot - _indexSlash - 1);
                }
            }
        }

        /// <summary>
        /// 获取文件名不要后缀 例如 DDZ/UI.unity 则对名为 UI
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPureFileName(this string path)
        {
            int _indexSlash = path.LastIndexOf('/');

#if UNITY_EDITOR //反斜杠只会在编辑器模式下出现
            int _indexAntSlash = path.LastIndexOf('\\');

            if (_indexAntSlash > _indexSlash)
            {
                _indexSlash = _indexAntSlash;
            }
#endif

            if (_indexSlash == -1)
            {
                return path;
            }
            else
            {
                return path.Substring(_indexSlash + 1);
            }
        }
    }
}
