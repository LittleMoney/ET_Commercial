using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
    public static class PlatformHelper
    {
        public static int GetPlatformCode()
        {
            return (int)UnityEngine.Application.platform;
        }

        public static string GetPhoneCode()
        {
            return SystemInfo.deviceModel;
        }

        public static string GetMachineCode()
        {
            if (SystemInfo.deviceUniqueIdentifier==SystemInfo.unsupportedIdentifier)
            {
                string _machineCode = UnityEngine.PlayerPrefs.GetString("__MachineCode",null);
                if(_machineCode==null)
                {
                    _machineCode = MD5Helper.BytesMD5(new Guid().ToByteArray());
                    UnityEngine.PlayerPrefs.SetString("__MachineCode", _machineCode);
                }
                return _machineCode;
            }
            else
            {
                return SystemInfo.deviceUniqueIdentifier;
            }
        }

        /// <summary>
        /// 重启应用
        /// </summary>
        /// <returns>是否可以重启，false 表示系统不可以重启</returns>
        public static bool ResartApp()
        {
            Debug.Log(" ResartApp");

#if UNITY_EDITOR
            return false;

#elif UNITY_ANDROID
            restartAndroid();
            return true;
#elif UNITY_IPHONE
            return false;
#else
            return false;
#endif
        }

        private static void RestartAndroid()
        {
            if (Application.isEditor)
                return;

            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                const int kIntent_FLAG_ACTIVITY_CLEAR_TASK = 0x00008000;
                const int kIntent_FLAG_ACTIVITY_NEW_TASK = 0x10000000;

                var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                var intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);

                intent.Call<AndroidJavaObject>("setFlags", kIntent_FLAG_ACTIVITY_NEW_TASK | kIntent_FLAG_ACTIVITY_CLEAR_TASK);
                currentActivity.Call("startActivity", intent);
                currentActivity.Call("finish");
                var process = new AndroidJavaClass("android.os.Process");
                int pid = process.CallStatic<int>("myPid");
                process.CallStatic("killProcess", pid);
            }
        }
    }
}
