using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
    public  static partial class ProcedureNames
    {
        public const string Lobby = "LobbyProcedureLogic";
    }

    public class LobbyProcedureLogic : ComponentI, IProcedureCycle
    {
        public ETTask OnEnter()
        {
            return this.Enter();
        }

        public ETTask OnExit()
        {
            return this.Exit();
        }
    }

    public static class LobbyProcedureLogicSystem
    {
        public static async ETTask Enter(this LobbyProcedureLogic self)
        {
            Debug.Log("进入 ETHotfix.LobbyProcedure");
            //清理除Common外的所有资源包
            ResourcesComponent.Instance.UnloadBundleForRegex(ETModel.AssetBundleHelper.GetWithOutPathPatten(PathHelper.CommonDirName));

            //加载场景
            using (LoadSceneAsync _lsa = ETModel.ComponentFactory.Create<LoadSceneAsync>())
            {
                await _lsa.Load("Lobby", "Lobby", UnityEngine.SceneManagement.LoadSceneMode.Additive,
                    (lsa) => { BroadcastComponent.Instance.GetDefault().Run<int, string>(BroadcastId.ProgressMessage, lsa.g_progress, $"加载场景 Lobby {lsa.g_progress.ToString()}"); });
            }

            using (LoadSceneAsync _lsa = ETModel.ComponentFactory.Create<LoadSceneAsync>())
            {
                await _lsa.UnLoad("Init");
            }

            BroadcastComponent.Instance.GetDefault().Run<int, string>(BroadcastId.ProgressMessage, 0, "加载场景资源");

            Game.Scene.AddComponent<LobbyComponent>();
            //await UIComponent.Instance.OpenAsync(UIType.CommonBgPanel);
            UIComponent.Instance.Close(UIType.CommonLoadingPanel);
            await UIComponent.Instance.OpenAsync(UIType.LobbyStartPanel);
        }


        public static async ETTask Exit(this LobbyProcedureLogic self)
        {
            Debug.Log("退出 ETHotfix.LobbyProcedure");
            return;
        }
    }
}
