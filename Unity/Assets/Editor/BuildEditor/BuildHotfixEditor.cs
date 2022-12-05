using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ETModel;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
    [InitializeOnLoad]
    public class Startup
    {
        //Unity代码生成dll位置
        private const string ScriptAssembliesDir = "Library/ScriptAssemblies";
        //热更代码dll文件
        private const string HotfixDll = "Unity.Hotfix.dll";
        //热更代码pdb文件
        private const string HotfixPdb = "Unity.Hotfix.pdb";
        //热更代码存放位置
        private const string CodeDirPath = "Assets/Bundles/Common";

        static Startup()
        {
            //拷贝热更代码
            CopyCode();
        }

        public static void CopyCode()
        {
            //Log.Info($"Copy Hotfix Code");
            if (!Directory.Exists(CodeDirPath))
            {
                Directory.CreateDirectory(CodeDirPath);
            }

            File.Copy(Path.Combine(ScriptAssembliesDir, HotfixDll), Path.Combine(CodeDirPath, "Hotfix.dll.bytes"), true);
            File.Copy(Path.Combine(ScriptAssembliesDir, HotfixPdb), Path.Combine(CodeDirPath, "Hotfix.pdb.bytes"), true);
            //Log.Info($"复制Hotfix.dll, Hotfix.pdb到{CodeDirPath}完成");
            //AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Log.Info($"Copy Hotfix Code to {CodeDirPath} Completed! ");
        }

    }
}
