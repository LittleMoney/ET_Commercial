using ETModel;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using System.IO;

namespace ETEditor
{
    public static class UIEditor
    {
        public const string UITemplate = @"
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#region AUTO using
{{using}}
#endregion

namespace {{namespace}}
{
    public static partial class UIType
    {
        public const string {{fileName}} = ""{{filePath}}""; 
    }

    public class {{fileName}} : Component, IUICycle
    {
        #region AUTO fieldDeclare
{{fieldDeclare}}
        #endregion

        public void OnStart() { }
        public void OnShow() { }
        public void OnFocus() { }
        public void OnPause() { }
        public void OnHide() { }


    }

    [ObjectSystem]
    public class {{fileName}}AwakeSystem : AwakeSystem<{{fileName}}>
    {
        public override void Awake({{fileName}} self)
        {
            #region AUTO fieldAwake
{{fieldAwake}}
            #endregion

            self.Init();
        }
    }


    [ObjectSystem]
    public class {{fileName}}DestroySystem : DestroySystem<{{fileName}}>
    {
        public override void Destroy({{fileName}} self)
        {
            #region AUTO fieldDestroy
{{filedDestroy}}
            #endregion

            self.Uninit();
        }
    }

    public static class {{fileName}}System
    {
        public static void Init(this {{fileName}} self)
        {

        }

        public static void Uninit(this {{fileName}} self)
        {

        }
    }
}

";

        public const string ComponentTemplate = @"
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#region AUTO using
{{using}}
#endregion

namespace {{namespace}}
{
    public class {{fileName}} : Component
    {
        #region AUTO fieldDeclare
{{fieldDeclare}}
        #endregion

    }

    [ObjectSystem]
    public class {{fileName}}AwakeSystem : AwakeSystem<{{fileName}},GameObject>
    {
        public override void Awake({{fileName}} self,GameObject gameObject)
        {
            #region AUTO fieldAwake
{{fieldAwake}}
            #endregion

            self.Init();
        }
    }


    [ObjectSystem]
    public class {{fileName}}DestroySystem : DestroySystem<{{fileName}}>
    {
        public override void Destroy({{fileName}} self)
        {
            #region AUTO fieldDestroy
{{filedDestroy}}
            #endregion

            self.Uninit();
        }
    }

    public static class {{fileName}}System
    {
        public static void Init(this {{fileName}} self)
        {

        }

        public static void Uninit(this {{fileName}} self)
        {

        }
    }
}

";

        public const string UnitTemplate = @"
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#region AUTO using
{{using}}
#endregion

namespace {{namespace}}
{
    public static partial class UnitType
    {
        public const string {{fileName}} = ""{{filePath}}""; 
    }

    public class {{fileName}} : Component,IUnitCycle
    {
        #region AUTO fieldDeclare
{{fieldDeclare}}
        #endregion

        public void OnStart() { }
        public void OnShow() { }
        public void OnHide() { }
        public void OnReset() { }
    }

    [ObjectSystem]
    public class {{fileName}}AwakeSystem : AwakeSystem<{{fileName}}>
    {
        public override void Awake({{fileName}} self)
        {
            #region AUTO fieldAwake
{{fieldAwake}}
            #endregion

            self.Init();
        }
    }


    [ObjectSystem]
    public class {{fileName}}DestroySystem : DestroySystem<{{fileName}}>
    {
        public override void Destroy({{fileName}} self)
        {
            #region AUTO fieldDestroy
{{filedDestroy}}
            #endregion

            self.Uninit();
        }
    }

    public static class {{fileName}}System
    {
        public static void Init(this {{fileName}} self)
        {

        }

        public static void Uninit(this {{fileName}} self)
        {

        }
    }
}

";

        [MenuItem("Assets/ET/生成 UI 脚本(ETHotfix)")]
        public static void MenuCenerateUIPanelScriptETHotfix()
        {
            UnityEngine.Object[] _arr = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);
            foreach (UnityEngine.Object _obj in _arr)
            {
                if (_obj is GameObject)
                {
                    CenerateUIScript(_obj, true);
                }
            }
        }

        [MenuItem("Assets/ET/生成 UI 脚本(ETModel)")]
        public static void MenuCenerateUIPanelScriptETModel()
        {
            UnityEngine.Object[] _arr = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);
            foreach (UnityEngine.Object _obj in _arr)
            {
                if (_obj is GameObject)
                {
                    CenerateUIScript(_obj, false);
                }
            }
        }

        [MenuItem("Assets/ET/创建Component脚本(ETHotfix)")]
        public static void MenuCenerateComponentScriptETHotfix()
        {
            UnityEngine.Object[] _arr = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);
            foreach (UnityEngine.Object _obj in _arr)
            {
                if (_obj is GameObject)
                {
                    CenerateComponentScript(_obj, true);
                }
            }
        }

        [MenuItem("Assets/ET/创建Component脚本(ETModel)")]
        public static void MenuCenerateComponentScriptETModel()
        {
            UnityEngine.Object[] _arr = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);
            foreach (UnityEngine.Object _obj in _arr)
            {
                if (_obj is GameObject)
                {
                    CenerateComponentScript(_obj, true);
                }
            }
        }

        [MenuItem("Assets/ET/生成 Unit 脚本(ETHotfix)")]
        public static void MenuCenerateUnitScriptETHotfix()
        {
            UnityEngine.Object[] _arr = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);
            foreach (UnityEngine.Object _obj in _arr)
            {
                if (_obj is GameObject)
                {
                    CenerateUnitScript(_obj, true);
                }
            }
        }

        [MenuItem("Assets/ET/生成 Unit 脚本(ETModel)")]
        public static void MenuCenerateUnitScriptETModel()
        {
            UnityEngine.Object[] _arr = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);
            foreach (UnityEngine.Object _obj in _arr)
            {
                if (_obj is GameObject)
                {
                    CenerateUnitScript(_obj, false);
                }
            }
        }



        public static void CenerateUIScript(UnityEngine.Object perfab, bool isHotfix)
        {
            GameObject _go = perfab as GameObject;
            UIBehaviour _uiBehavier = _go.GetComponent<UIBehaviour>();
            if (_uiBehavier != null)
            {
                string _goPath = AssetDatabase.GetAssetPath(_go);

                if (_goPath == null)
                {
                    EditorUtility.DisplayDialog("错误", $"未能获取资源的地址 {_go.name}", "是");
                    return;
                }
                string _fileName = PathHelper.GetPureFileNameNoExtension(_goPath);
                string _filePath = _goPath.Substring(_goPath.IndexOf(PathHelper.EditorResPath) + PathHelper.EditorResPath.Length + 1);
                _filePath = _filePath.Substring(0, _filePath.IndexOf(_fileName) + _fileName.Length);
                string _namespace = null;
                HashSet<string> _using = new HashSet<string>();
                StringBuilder _fieldDeclare = new StringBuilder();
                StringBuilder _fieldAwake = new StringBuilder();
                StringBuilder _filedDestroy = new StringBuilder();

                _fieldDeclare.Append("\t\tpublic UI ui;\r\n\t\tpublic ItemCollector itemCollector;\r\n");
                _fieldAwake.Append("\t\t\tself.ui=(self.Entity as UI);\r\n\t\t\tself.itemCollector = self.ui.g_uiBehaviour as ItemCollector;\r\n");

                if (isHotfix)
                {
                    _namespace = "ETHotfix";
                    _using.Add("using ETModel;\r\n");
                }
                else
                {
                    _namespace = "ETModel";
                }
                CenerateFieldScript(_uiBehavier, _using, _fieldDeclare, _fieldAwake, _filedDestroy);

                string _savePath = EditorUtility.SaveFilePanel("选择文件", _goPath.Substring(0, _goPath.LastIndexOf("/")), _fileName, "cs");
          
                if (string.IsNullOrEmpty(_savePath)) return;

                if (!File.Exists(_savePath))
                {
                    #region 新建
                    string _script = UITemplate;
                    string _usingAll = "";

                    foreach (string str in _using) _usingAll += str;

                    _script = _script.Replace("{{using}}", _usingAll);
                    _script = _script.Replace("{{namespace}}", _namespace);
                    _script = _script.Replace("{{fieldDeclare}}", _fieldDeclare.ToString());
                    _script = _script.Replace("{{fieldAwake}}", _fieldAwake.ToString());
                    _script = _script.Replace("{{filedDestroy}}", _filedDestroy.ToString());
                    _script = _script.Replace("{{fileName}}", _fileName);
                    _script = _script.Replace("{{filePath}}", _filePath);

                    if (!string.IsNullOrEmpty(_savePath))
                    {
                        using (StreamWriter fs = new StreamWriter(File.Create(_savePath), Encoding.UTF8))
                        {
                            fs.Write(_script);
                        }

                        AssetDatabase.Refresh();
                    }
                    #endregion
                }
                else
                {
                    if (!EditorUtility.DisplayDialog("提示", $"脚本文件已经存在，强制更新脚本还是放弃", "是", "否")) return;

                    #region 更新
                    string _oldScript = null;
                    using (StreamReader fs = new StreamReader(File.Open(_savePath, FileMode.Open), Encoding.UTF8))
                    {
                        _oldScript = fs.ReadToEnd();
                    }

                    if (_oldScript != null)
                    {
                        Regex _regex = null;
                        MatchCollection _matchCollection = null;
                        Match _math = null;

                        if (isHotfix)
                        {
                            //验证类名
                            _regex = new Regex("namespace\\s+ETHotfix\\s*{", RegexOptions.Multiline);
                            _matchCollection = _regex.Matches(_oldScript);
                            if (_matchCollection.Count != 1)
                            {
                                EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 ETHotfix 命名空间,确保修改的脚本与Perfab匹配", "确定");
                                return;
                            }
                        }
                        else
                        {
                            //验证类名
                            _regex = new Regex("namespace\\s+ETModel\\s*{", RegexOptions.Multiline);
                            _matchCollection = _regex.Matches(_oldScript);
                            if (_matchCollection.Count != 1)
                            {
                                EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 ETModel 命名空间,确保修改的脚本与Perfab匹配", "确定");
                                return;
                            }
                        }

                        //验证类名
                        _regex = new Regex("public\\s+class\\s+" + _fileName + ".*:.*Component", RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 {_fileName} 类声明代码块,确保修改的脚本与Perfab匹配", "确定");
                            return;
                        }

                        //验证Panel路径
                        _regex = new Regex("public\\s+const\\s+string\\s+" + _fileName + "\\s*=\\s*\"" + _filePath + "\"; ", RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 {_filePath} 类声明代码块,确保修改的脚本与Perfab匹配", "确定");
                            return;
                        }

                        //验证类名
                        _regex = new Regex("public\\s+class\\s+" + _fileName + ".*:.*Component", RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 {_fileName} 类声明代码块", "确定");
                            return;
                        }

                        //修改using
                        _regex = new Regex("(#region AUTO using)[^#]*(#endregion)", System.Text.RegularExpressions.RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", "原代码中没有查找到 #region AUTO using ... #endregion 代码块", "确定");
                            return;
                        }

                        _math = _matchCollection[0];
                        _oldScript = _oldScript.Substring(0, _math.Index) +
                            "#region AUTO using\r\n{{using}}\r\n#endregion" +
                            _oldScript.Substring(_math.Index + _math.Length);

                        //修改fieldDeclare
                        _regex = new Regex("(#region AUTO fieldDeclare)[^#]*(#endregion)", System.Text.RegularExpressions.RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", "原代码中没有查找到 #region AUTO fieldDeclare ... #endregion 代码块", "确定");
                            return;
                        }
                        _math = _matchCollection[0];
                        _oldScript = _oldScript.Substring(0, _math.Index) +
                            "#region AUTO fieldDeclare\r\n{{fieldDeclare}}\r\n\t\t#endregion" +
                            _oldScript.Substring(_math.Index + _math.Length);

                        //修改fieldAwake
                        _regex = new Regex("(#region AUTO fieldAwake)[^#]*(#endregion)", System.Text.RegularExpressions.RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", "原代码中没有查找到 #region AUTO fieldAwake ... #endregion 代码块", "确定");
                            return;
                        }
                        _math = _matchCollection[0];
                        _oldScript = _oldScript.Substring(0, _math.Index) +
                            "#region AUTO fieldAwake\r\n{{fieldAwake}}\r\n\t\t\t#endregion" +
                            _oldScript.Substring(_math.Index + _math.Length);

                        //修改fieldDestroy
                        _regex = new Regex("(#region AUTO fieldDestroy)[^#]*(#endregion)", System.Text.RegularExpressions.RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", "原代码中没有查找到 #region AUTO fieldDestroy ... #endregion 代码块", "确定");
                            return;
                        }
                        _math = _matchCollection[0];
                        _oldScript = _oldScript.Substring(0, _math.Index) +
                            "#region AUTO fieldDestroy\r\n{{fieldDestroy}}\r\n\t\t\t#endregion" +
                            _oldScript.Substring(_math.Index + _math.Length);


                        string _script = _oldScript;
                        string _usingAll = "";

                        foreach (string str in _using) _usingAll += str;

                        _script = _script.Replace("{{using}}", _usingAll);
                        _script = _script.Replace("{{fieldDeclare}}", _fieldDeclare.ToString());
                        _script = _script.Replace("{{fieldAwake}}", _fieldAwake.ToString());
                        _script = _script.Replace("{{fieldDestroy}}", _filedDestroy.ToString());

                        using (StreamWriter fs = new StreamWriter(File.Create(_savePath), Encoding.UTF8))
                        {
                            fs.Write(_script);
                        }

                        AssetDatabase.Refresh();
                    }
                    #endregion
                }
            }
        }

        public static void CenerateComponentScript(UnityEngine.Object perfab, bool isHotfix)
        {
            GameObject _go = perfab as GameObject;
            UIBehaviour _uiBehavier = _go.GetComponent<UIBehaviour>();
            if (_uiBehavier != null)
            {
                string _goPath = AssetDatabase.GetAssetPath(_go);

                if (_goPath == null)
                {
                    EditorUtility.DisplayDialog("错误", $"未能获取资源的地址 {_go.name}", "是");
                    return;
                }
                string _fileName = PathHelper.GetPureFileNameNoExtension(_goPath);
                string _namespace = null;
                HashSet<string> _using = new HashSet<string>();
                StringBuilder _fieldDeclare = new StringBuilder();
                StringBuilder _fieldAwake = new StringBuilder();
                StringBuilder _filedDestroy = new StringBuilder();

                _fieldDeclare.Append("\t\tpublic GameObject rootGameObject;\r\n\t\tpublic ItemCollector itemCollector;\r\n");
                _fieldAwake.Append("\t\t\tself.rootGameObject=gameObject;\r\n\t\t\tself.itemCollector = gameObject.GetComponent<ItemCollector>();\r\n");

                if (isHotfix)
                {
                    _namespace = "ETHotfix";
                    _using.Add("using ETModel;\r\n");
                }
                else
                {
                    _namespace = "ETModel";
                }
                CenerateFieldScript(_uiBehavier, _using, _fieldDeclare, _fieldAwake, _filedDestroy);

                string _savePath = EditorUtility.SaveFilePanel("选择文件", _goPath.Substring(0, _goPath.LastIndexOf("/")), _fileName, "cs");

                if (string.IsNullOrEmpty(_savePath)) return;

                if (!File.Exists(_savePath))
                {
                    #region 新建
                    string _script = ComponentTemplate;
                    string _usingAll = "";

                    foreach (string str in _using) _usingAll += str;

                    _script = _script.Replace("{{using}}", _usingAll);
                    _script = _script.Replace("{{namespace}}", _namespace);
                    _script = _script.Replace("{{fieldDeclare}}", _fieldDeclare.ToString());
                    _script = _script.Replace("{{fieldAwake}}", _fieldAwake.ToString());
                    _script = _script.Replace("{{filedDestroy}}", _filedDestroy.ToString());
                    _script = _script.Replace("{{fileName}}", _fileName);

                    using (StreamWriter fs = new StreamWriter(File.Create(_savePath), Encoding.UTF8))
                    {
                        fs.Write(_script);
                    }

                    AssetDatabase.Refresh();
                    #endregion
                }
                else
                {
                    if (!EditorUtility.DisplayDialog("提示", $"脚本文件已经存在，强制更新脚本还是放弃", "是", "否")) return;

                    #region 更新
                    string _oldScript = null;
                    using (StreamReader fs = new StreamReader(File.Open(_savePath, FileMode.Open), Encoding.UTF8))
                    {
                        _oldScript = fs.ReadToEnd();
                    }

                    if (_oldScript != null)
                    {
                        Regex _regex = null;
                        MatchCollection _matchCollection = null;
                        Match _math = null;

                        if (isHotfix)
                        {
                            //验证类名
                            _regex = new Regex("namespace\\s+ETHotfix\\s*{", RegexOptions.Multiline);
                            _matchCollection = _regex.Matches(_oldScript);
                            if (_matchCollection.Count != 1)
                            {
                                EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 ETHotfix 命名空间,确保修改的脚本与Perfab匹配", "确定");
                                return;
                            }
                        }
                        else
                        {
                            //验证类名
                            _regex = new Regex("namespace\\s+ETModel\\s*{", RegexOptions.Multiline);
                            _matchCollection = _regex.Matches(_oldScript);
                            if (_matchCollection.Count != 1)
                            {
                                EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 ETModel 命名空间,确保修改的脚本与Perfab匹配", "确定");
                                return;
                            }
                        }

                        //验证类名
                        _regex = new Regex("public\\s+class\\s+" + _fileName + ".*:.*Component", RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 {_fileName} 类声明代码块,确保修改的脚本与Perfab匹配", "确定");
                            return;
                        }

                        //验证类名
                        _regex = new Regex("public\\s+class\\s+" + _fileName + ".*:.*Component", RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 {_fileName} 类声明代码块", "确定");
                            return;
                        }

                        //修改using
                        _regex = new Regex("(#region AUTO using)[^#]*(#endregion)", System.Text.RegularExpressions.RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", "原代码中没有查找到 #region AUTO using ... #endregion 代码块", "确定");
                            return;
                        }

                        _math = _matchCollection[0];
                        _oldScript = _oldScript.Substring(0, _math.Index) +
                            "#region AUTO using\r\n{{using}}\r\n#endregion" +
                            _oldScript.Substring(_math.Index + _math.Length);

                        //修改fieldDeclare
                        _regex = new Regex("(#region AUTO fieldDeclare)[^#]*(#endregion)", System.Text.RegularExpressions.RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", "原代码中没有查找到 #region AUTO fieldDeclare ... #endregion 代码块", "确定");
                            return;
                        }
                        _math = _matchCollection[0];
                        _oldScript = _oldScript.Substring(0, _math.Index) +
                            "#region AUTO fieldDeclare\r\n{{fieldDeclare}}\r\n\t\t#endregion" +
                            _oldScript.Substring(_math.Index + _math.Length);

                        //修改fieldAwake
                        _regex = new Regex("(#region AUTO fieldAwake)[^#]*(#endregion)", System.Text.RegularExpressions.RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", "原代码中没有查找到 #region AUTO fieldAwake ... #endregion 代码块", "确定");
                            return;
                        }
                        _math = _matchCollection[0];
                        _oldScript = _oldScript.Substring(0, _math.Index) +
                            "#region AUTO fieldAwake\r\n{{fieldAwake}}\r\n\t\t\t#endregion" +
                            _oldScript.Substring(_math.Index + _math.Length);

                        //修改fieldDestroy
                        _regex = new Regex("(#region AUTO fieldDestroy)[^#]*(#endregion)", System.Text.RegularExpressions.RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", "原代码中没有查找到 #region AUTO fieldDestroy ... #endregion 代码块", "确定");
                            return;
                        }
                        _math = _matchCollection[0];
                        _oldScript = _oldScript.Substring(0, _math.Index) +
                            "#region AUTO fieldDestroy\r\n{{fieldDestroy}}\r\n\t\t\t#endregion" +
                            _oldScript.Substring(_math.Index + _math.Length);

                        string _script = _oldScript;
                        string _usingAll = "";

                        foreach (string str in _using) _usingAll += str;

                        _script = _script.Replace("{{using}}", _usingAll);
                        _script = _script.Replace("{{fieldDeclare}}", _fieldDeclare.ToString());
                        _script = _script.Replace("{{fieldAwake}}", _fieldAwake.ToString());
                        _script = _script.Replace("{{fieldDestroy}}", _filedDestroy.ToString());

                        using (StreamWriter fs = new StreamWriter(File.Create(_savePath), Encoding.UTF8))
                        {
                            fs.Write(_script);
                        }

                        AssetDatabase.Refresh();
                    }
                    #endregion
                }
            }
        }

        public static void CenerateUnitScript(UnityEngine.Object perfab, bool isHotfix)
        {
            GameObject _go = perfab as GameObject;
            UIBehaviour _uiBehavier = _go.GetComponent<UIBehaviour>();
            if (_uiBehavier != null)
            {
                string _goPath = AssetDatabase.GetAssetPath(_go);

                if (_goPath == null)
                {
                    EditorUtility.DisplayDialog("错误", $"未能获取资源的地址 {_go.name}", "是");
                    return;
                }
                string _fileName = PathHelper.GetPureFileNameNoExtension(_goPath);
                string _filePath = _goPath.Substring(_goPath.IndexOf(PathHelper.EditorResPath) + PathHelper.EditorResPath.Length + 1);
                _filePath = _filePath.Substring(0, _filePath.IndexOf(_fileName) + _fileName.Length);
                string _namespace = null;
                HashSet<string> _using = new HashSet<string>();
                StringBuilder _fieldDeclare = new StringBuilder();
                StringBuilder _fieldAwake = new StringBuilder();
                StringBuilder _filedDestroy = new StringBuilder();

                _fieldDeclare.Append("\t\tpublic Unit unit;\r\n\t\tpublic ItemCollector itemCollector;\r\n");
                _fieldAwake.Append("\t\t\tself.unit=(self.Entity as Unit);\r\n\t\t\tself.itemCollector = self.unit.ItemCollector;\r\n");

                if (isHotfix)
                {
                    _namespace = "ETHotfix";
                    _using.Add("using ETModel;\r\n");
                }
                else
                {
                    _namespace = "ETModel";
                }
                CenerateFieldScript(_uiBehavier, _using, _fieldDeclare, _fieldAwake, _filedDestroy);

                string _savePath = EditorUtility.SaveFilePanel("选择文件", _goPath.Substring(0, _goPath.LastIndexOf("/")), _fileName, "cs");

                if (string.IsNullOrEmpty(_savePath)) return;

                if (!File.Exists(_savePath))
                {
                    #region 新建
                    string _script = UnitTemplate;
                    string _usingAll = "";

                    foreach (string str in _using) _usingAll += str;

                    _script = _script.Replace("{{using}}", _usingAll);
                    _script = _script.Replace("{{namespace}}", _namespace);
                    _script = _script.Replace("{{fieldDeclare}}", _fieldDeclare.ToString());
                    _script = _script.Replace("{{fieldAwake}}", _fieldAwake.ToString());
                    _script = _script.Replace("{{filedDestroy}}", _filedDestroy.ToString());
                    _script = _script.Replace("{{fileName}}", _fileName);
                    _script = _script.Replace("{{filePath}}", _filePath);

                    if (!string.IsNullOrEmpty(_savePath))
                    {
                        using (StreamWriter fs = new StreamWriter(File.Create(_savePath), Encoding.UTF8))
                        {
                            fs.Write(_script);
                        }

                        AssetDatabase.Refresh();
                    }
                    #endregion
                }
                else
                {
                    if (!EditorUtility.DisplayDialog("提示", $"脚本文件已经存在，强制更新脚本还是放弃", "是", "否")) return;

                    #region 更新
                    string _oldScript = null;
                    using (StreamReader fs = new StreamReader(File.Open(_savePath, FileMode.Open), Encoding.UTF8))
                    {
                        _oldScript = fs.ReadToEnd();
                    }

                    if (_oldScript != null)
                    {
                        Regex _regex = null;
                        MatchCollection _matchCollection = null;
                        Match _math = null;

                        if (isHotfix)
                        {
                            //验证类名
                            _regex = new Regex("namespace\\s+ETHotfix\\s*{", RegexOptions.Multiline);
                            _matchCollection = _regex.Matches(_oldScript);
                            if (_matchCollection.Count != 1)
                            {
                                EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 ETHotfix 命名空间,确保修改的脚本与Perfab匹配", "确定");
                                return;
                            }
                        }
                        else
                        {
                            //验证类名
                            _regex = new Regex("namespace\\s+ETModel\\s*{", RegexOptions.Multiline);
                            _matchCollection = _regex.Matches(_oldScript);
                            if (_matchCollection.Count != 1)
                            {
                                EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 ETModel 命名空间,确保修改的脚本与Perfab匹配", "确定");
                                return;
                            }
                        }

                        //验证类名
                        _regex = new Regex("public\\s+class\\s+" + _fileName + ".*:.*Component", RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 {_fileName} 类声明代码块,确保修改的脚本与Perfab匹配", "确定");
                            return;
                        }

                        //验证Panel路径
                        _regex = new Regex("public\\s+const\\s+string\\s+" + _fileName + "\\s*=\\s*\"" + _filePath + "\"; ", RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 {_filePath} 类声明代码块,确保修改的脚本与Perfab匹配", "确定");
                            return;
                        }

                        //验证类名
                        _regex = new Regex("public\\s+class\\s+" + _fileName + ".*:.*Component", RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", $"原代码中没有查找到 {_fileName} 类声明代码块", "确定");
                            return;
                        }

                        //修改using
                        _regex = new Regex("(#region AUTO using)[^#]*(#endregion)", System.Text.RegularExpressions.RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", "原代码中没有查找到 #region AUTO using ... #endregion 代码块", "确定");
                            return;
                        }

                        _math = _matchCollection[0];
                        _oldScript = _oldScript.Substring(0, _math.Index) +
                            "#region AUTO using\r\n{{using}}\r\n#endregion" +
                            _oldScript.Substring(_math.Index + _math.Length);

                        //修改fieldDeclare
                        _regex = new Regex("(#region AUTO fieldDeclare)[^#]*(#endregion)", System.Text.RegularExpressions.RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", "原代码中没有查找到 #region AUTO fieldDeclare ... #endregion 代码块", "确定");
                            return;
                        }
                        _math = _matchCollection[0];
                        _oldScript = _oldScript.Substring(0, _math.Index) +
                            "#region AUTO fieldDeclare\r\n{{fieldDeclare}}\r\n\t\t#endregion" +
                            _oldScript.Substring(_math.Index + _math.Length);

                        //修改fieldAwake
                        _regex = new Regex("(#region AUTO fieldAwake)[^#]*(#endregion)", System.Text.RegularExpressions.RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", "原代码中没有查找到 #region AUTO fieldAwake ... #endregion 代码块", "确定");
                            return;
                        }
                        _math = _matchCollection[0];
                        _oldScript = _oldScript.Substring(0, _math.Index) +
                            "#region AUTO fieldAwake\r\n{{fieldAwake}}\r\n\t\t\t#endregion" +
                            _oldScript.Substring(_math.Index + _math.Length);

                        //修改fieldDestroy
                        _regex = new Regex("(#region AUTO fieldDestroy)[^#]*(#endregion)", System.Text.RegularExpressions.RegexOptions.Multiline);
                        _matchCollection = _regex.Matches(_oldScript);
                        if (_matchCollection.Count != 1)
                        {
                            EditorUtility.DisplayDialog("错误", "原代码中没有查找到 #region AUTO fieldDestroy ... #endregion 代码块", "确定");
                            return;
                        }
                        _math = _matchCollection[0];
                        _oldScript = _oldScript.Substring(0, _math.Index) +
                            "#region AUTO fieldDestroy\r\n{{fieldDestroy}}\r\n\t\t\t#endregion" +
                            _oldScript.Substring(_math.Index + _math.Length);


                        string _script = _oldScript;
                        string _usingAll = "";

                        foreach (string str in _using) _usingAll += str;

                        _script = _script.Replace("{{using}}", _usingAll);
                        _script = _script.Replace("{{fieldDeclare}}", _fieldDeclare.ToString());
                        _script = _script.Replace("{{fieldAwake}}", _fieldAwake.ToString());
                        _script = _script.Replace("{{fieldDestroy}}", _filedDestroy.ToString());

                        using (StreamWriter fs = new StreamWriter(File.Create(_savePath), Encoding.UTF8))
                        {
                            fs.Write(_script);
                        }

                        AssetDatabase.Refresh();
                    }
                    #endregion
                }
            }
        }


        public static void CenerateFieldScript(ItemCollector itemCollector,HashSet<string> _using,StringBuilder _fieldDeclare, StringBuilder _fieldAwake, StringBuilder _filedDestroy)
        {
            GameObject _gameObject=null;

            foreach (ItemCollectorData data in itemCollector)
            {
                _gameObject = data.obj as GameObject;
                if (_gameObject == null) continue;

                #region 记录字段
                switch (data.type)
                {
                    case ItemType.GameObject:
                        _fieldDeclare.Append($"\t\tpublic GameObject {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\tself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject);\r\n");
                        break;
                    case ItemType.UIButton:
                        if (!_using.Contains("using UnityEngine.UI;\r\n")) _using.Add("using UnityEngine.UI;\r\n");
                        _fieldDeclare.Append($"\t\tpublic Button {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\tself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<Button>().BindAnim();\r\n");
                        _filedDestroy.Append($"\t\t\tself.{data.key}.onClick.RemoveAllListeners();\r\n");
                        break;
                    case ItemType.UIText:
                        if (!_using.Contains("using UnityEngine.UI;\r\n")) _using.Add("using UnityEngine.UI;\r\n");
                        if (!_using.Contains("using TMPro;\r\n")) _using.Add("using TMPro;\r\n");
                        _fieldDeclare.Append($"\t\tpublic TMP_Text {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\tself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<TMP_Text>();\r\n");
                        break;
                    case ItemType.UIImage:
                        if (!_using.Contains("using UnityEngine.UI;\r\n")) _using.Add("using UnityEngine.UI;\r\n");
                        _fieldDeclare.Append($"\t\tpublic Image {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\tself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<Image>();\r\n");
                        break;
                    case ItemType.UIInputField:
                        if (!_using.Contains("using UnityEngine.UI;\r\n")) _using.Add("using UnityEngine.UI;\r\n");
                        _fieldDeclare.Append($"\t\tpublic InputField {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\tself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<InputField>();\r\n");
                        _filedDestroy.Append($"\t\t\tself.{data.key}.onEndEdit.RemoveAllListeners();\r\n");
                        _filedDestroy.Append($"\t\t\tself.{data.key}.onValueChanged.RemoveAllListeners();\r\n");
                        break;
                    case ItemType.UISlider:
                        if (!_using.Contains("using UnityEngine.UI;\r\n")) _using.Add("using UnityEngine.UI;\r\n");
                        _fieldDeclare.Append($"\t\tpublic Slider {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\ttself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<Slider>()\r\n");
                        _filedDestroy.Append($"\t\t\tself.{data.key}.onValueChanged.RemoveAllListeners();\r\n");
                        break;
                    case ItemType.UIDropdown:
                        if (!_using.Contains("using UnityEngine.UI;\r\n")) _using.Add("using UnityEngine.UI;\r\n");
                        _fieldDeclare.Append($"\t\tpublic Dropdown {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\ttself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<Dropdown>().BindAnim();\r\n");
                        _filedDestroy.Append($"\t\t\tself.{data.key}.onValueChanged.RemoveAllListeners();\r\n");
                        break;
                    case ItemType.UIToggle:
                        if (!_using.Contains("using UnityEngine.UI;\r\n")) _using.Add("using UnityEngine.UI;\r\n");
                        _fieldDeclare.Append($"\t\tpublic Toggle {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\tself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<Toggle>();\r\n");
                        _filedDestroy.Append($"\t\t\tself.{data.key}.onValueChanged.RemoveAllListeners();\r\n");
                        break;
                    case ItemType.UIToggleGroup:
                        if (!_using.Contains("using UnityEngine.UI;\r\n")) _using.Add("using UnityEngine.UI;\r\n");
                        _fieldDeclare.Append($"\t\tpublic ToggleGroup {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\tself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<ToggleGroup>();\r\n");
                        break;
                    case ItemType.UIHorizontalLayoutGroup:
                        if (!_using.Contains("using UnityEngine.UI;\r\n")) _using.Add("using UnityEngine.UI;\r\n");
                        _fieldDeclare.Append($"\t\tpublic HorizontalLayoutGroup {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\tself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<HorizontalLayoutGroup>()\r\n");
                        break;
                    case ItemType.UIVerticalLayoutGroup:
                        if (!_using.Contains("using UnityEngine.UI;\r\n")) _using.Add("using UnityEngine.UI;\r\n");
                        _fieldDeclare.Append($"\t\t\tpublic VerticalLayoutGroup {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\tself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<VerticalLayoutGroup>()\r\n");
                        break;
                    case ItemType.UIGridLayoutGroup:
                        if (!_using.Contains("using UnityEngine.UI;\r\n")) _using.Add("using UnityEngine.UI;\r\n");
                        _fieldDeclare.Append($"\t\tpublic GridLayoutGroup {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\tself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<GridLayoutGroup>()\r\n");
                        break;
                    case ItemType.UIScrollRect:
                        _fieldDeclare.Append($"\t\tpublic ScrollRect {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\ttself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<ScrollRect>()\r\n");
                        _filedDestroy.Append($"\t\t\tself.{data.key}.onValueChanged.RemoveAllListeners();\r\n");
                        break;
                    case ItemType.UICanvas:
                        if (!_using.Contains("using UnityEngine.UI;\r\n")) _using.Add("using UnityEngine.UI;\r\n");
                        _fieldDeclare.Append($"\t\tpublic Canvas {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\tself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<Canvas>()\r\n");
                        break;
                    case ItemType.UISpine:
                        if (!_using.Contains("using Spine.Unity;\r\n")) _using.Add("using Spine.Unity;\r\n");
                        _fieldDeclare.Append($"\t\tpublic SkeletonGraphic {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\tself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<SkeletonGraphic>()\r\n");
                        break;
                    case ItemType.Camera:
                        _fieldDeclare.Append($"\t\tpublic Camera {data.key};\r\n");
                        _fieldAwake.Append($"\t\t\tself.{data.key}= (self.itemCollector[\"{data.key}\"] as GameObject).GetComponent<Camera>()\r\n");
                        break;

                }
                #endregion

            }
        }
    }
}
