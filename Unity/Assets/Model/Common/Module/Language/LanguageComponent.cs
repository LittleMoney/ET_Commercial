using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
    public class LanguageComponent:Component
    {
        public static LanguageComponent Instance;
        public UnityEngine.SystemLanguage g_currentLanguage;

        public Dictionary<string, string> staticDict=new Dictionary<string, string>();
        public Dictionary<string, string> languageDict = new Dictionary<string, string>();
        public Dictionary<string, List<string>> staticIds = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> languageIds = new Dictionary<string, List<string>>();
    }

	[ObjectSystem]
	public class LanguageComponentAwakeSystem : AwakeSystem<LanguageComponent>
	{
        public override void Awake(LanguageComponent self)
        {
            LanguageComponent.Instance = self;
            self.g_currentLanguage = (SystemLanguage)UnityEngine.PlayerPrefs.GetInt("currentLanguage", (int)Application.systemLanguage);
        }
	}

    public static class LanguageComponentSystem
    {
        public static void SetCurrentLanguage(this LanguageComponent self,SystemLanguage language)
        {
            if (self.g_currentLanguage != language)
            {
                self.g_currentLanguage = language;
                UnityEngine.PlayerPrefs.SetInt("currentLanguage", (int)language);
                self.ReLoad();
            }
        }

        /// <summary>
        /// 加载语言资源包 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">资源位置不带后缀，语言资源应放置在 "toyName/Config/Language/XX" 目录下，并且应包含配置文件 "Language.txt" 和 "Static.txt"</param>
        /// <returns></returns>
        public static void Load(this LanguageComponent self, string toyDirName)
        {
            string _languagePath = PathHelper.Combine(toyDirName, $"Config/Language_{self.GetCurrentLan()}/Language");
            string _staticPath = PathHelper.Combine(toyDirName, $"Config/Language_{self.GetCurrentLan()}/Static");

            ACategory _languageCategory = Game.Scene.GetComponent<ConfigComponent>().Load(_languagePath);
            ACategory _staticCategory = Game.Scene.GetComponent<ConfigComponent>().Load(_staticPath);

            Type _type = null;
            FieldInfo _fieldInfo = null;
            string _id = null;
            List<string> _ids = null;

            //读取language配置
            _type = Type.GetType($"Language");
            _fieldInfo = _type.GetField("Value");
            _ids = new List<string>();
            foreach (IConfig _iconfig in _languageCategory.GetAll())
            {
                _id = (_iconfig as IConfigString).Id;
                if (!self.languageDict.ContainsKey(_id))
                {
                    self.languageDict.Add(_id, _fieldInfo.GetValue(_iconfig) as string);
                    _ids.Add(_id);
                }
            }
            self.languageIds.Add(toyDirName, _ids);


            //读取static配置
            _type = Type.GetType($"Static");
            _fieldInfo = _type.GetField("Value");
            _ids = new List<string>();
            foreach (IConfig _iconfig in _staticCategory.GetAll())
            {
                _id = (_iconfig as IConfigString).Id;
                if (!self.staticDict.ContainsKey(_id))
                {
                    self.staticDict.Add(_id, _fieldInfo.GetValue(_iconfig) as string);
                    _ids.Add(_id);
                }
            }
            self.staticIds.Add(toyDirName, _ids);

            Game.Scene.GetComponent<ConfigComponent>().Remove(_languagePath);
            Game.Scene.GetComponent<ConfigComponent>().Remove(_staticPath);
        }

        /// <summary>
        /// 加载语言资源包 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">资源位置不带后缀，语言资源应放置在 "toyName/Config/Language/XX" 目录下，并且应包含配置文件 "Language.txt" 和 "Static.txt"</param>
        /// <returns></returns>
        public static async ETTask LoadAsync(this LanguageComponent self, string toyDirName)
        {
            string _languagePath = PathHelper.Combine(toyDirName, $"Config/Language_{self.GetCurrentLan()}/Language");
            string _staticPath = PathHelper.Combine(toyDirName, $"Config/Language_{self.GetCurrentLan()}/Static");

            ACategory _languageCategory =await ConfigComponent.Instance.LoadAsync(_languagePath);
            ACategory _staticCategory = await ConfigComponent.Instance.LoadAsync(_staticPath);

            Type _type = null;
            FieldInfo _fieldInfo = null;
            string _id = null;
            List<string> _ids = null;

            //读取language配置
            _type = Type.GetType($"ETModel.Language");
            _fieldInfo = _type.GetField("Value");
            _ids = new List<string>();
            foreach (IConfig _iconfig in _languageCategory.GetAll())
            {
                _id = (_iconfig as IConfigString).Id;
                if (!self.languageDict.ContainsKey(_id))
                {
                    self.languageDict.Add(_id, _fieldInfo.GetValue(_iconfig) as string);
                    _ids.Add(_id);
                }
            }
            self.languageIds.Add(toyDirName, _ids);


            //读取static配置
            _type = Type.GetType($"ETModel.Static");
            _fieldInfo = _type.GetField("Value");
            _ids = new List<string>();
            foreach (IConfig _iconfig in _staticCategory.GetAll())
            {
                _id = (_iconfig as IConfigString).Id;
                if (!self.staticDict.ContainsKey(_id))
                {
                    self.staticDict.Add(_id, _fieldInfo.GetValue(_iconfig) as string);
                    _ids.Add(_id);
                }
            }
            self.staticIds.Add(toyDirName, _ids);

            Game.Scene.GetComponent<ConfigComponent>().Remove(_languagePath);
            Game.Scene.GetComponent<ConfigComponent>().Remove(_staticPath);
        }

        public static void Unload(this LanguageComponent self, string toyDirName)
        {
            if (self.languageIds.TryGetValue(toyDirName, out List<string> _ids1))
            {
                foreach (string _id in _ids1)
                {
                    self.languageDict.Remove(_id);
                }

                self.languageIds.Remove(toyDirName);
            }

            if (self.staticIds.TryGetValue(toyDirName, out List<string> _ids2))
            {
                foreach (string _id in _ids1)
                {
                    self.staticIds.Remove(_id);
                }

                self.staticIds.Remove(toyDirName);
            }
        }

        public static void  ReLoad(this LanguageComponent self)
        {
            string[] _list = self.staticIds.Keys.ToArray();
            self.staticDict.Clear();
            self.staticIds.Clear();
            foreach (string _toyName in _list)
            {
                self.Load(_toyName);
            }
        }

        public static string GetCurrentLan(this LanguageComponent self)
        {
            switch (self.g_currentLanguage)
            {
                case SystemLanguage.ChineseSimplified:
                    return "CN";
                default:
                    return "EN";
            }
        }

        public static string GetStatic(this LanguageComponent self,string id)
        {
            if(self.staticDict.TryGetValue(id,out string value))
            {
                return value;
            }
            return "";
        }

        public static string Get(this LanguageComponent self,string id)
        {

            if (self.languageDict.TryGetValue(id, out string value))
            {
                return value;
            }
            return "";
        }
    }
}
