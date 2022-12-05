using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHotfix
{
    public static class ETModelHelper
    {
        public static T GetSceneComponent<T>() where T:ETModel.Component
        {
            return ETModel.Game.Scene.GetComponent<T>();
        }
    }
}
