using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{

    public static class LanguageHelper
    {
        public static string GetStatic(this string id)
        {
            return LanguageComponent.Instance.GetStatic(id);
        }

        public static string Get(this string id)
        {
            return LanguageComponent.Instance.Get(id);
        }

        public static string GetCurrentLan()
        {
            return LanguageComponent.Instance.GetCurrentLan();
        }
    }
}
