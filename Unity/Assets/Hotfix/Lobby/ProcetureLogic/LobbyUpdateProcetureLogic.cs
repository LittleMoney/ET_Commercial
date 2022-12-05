using ETModel;
using System;
using UnityEngine;

namespace ETHotfix
{
    public static partial class ProcedureNames
    {
        public const string LobbyUpdate = "LobbyUpdateProcetureLogic";
    }

    public class LobbyUpdateProcetureLogic : ComponentI, IProcedureCycle
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


    public static class LobbyUpdateProcetureLogicSystem
    {
        public static async ETTask Enter(this LobbyUpdateProcetureLogic self)
        {
            Debug.Log("进入 ETHotfix.LobbyUpdateProceture");

            if (ETModel.Init.UseAssetBundle)
            {
                try
                {
                    if (ETModel.HotUpdateHelper.ToyNeedUpdate("Lobby"))
                    {
                        if (Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork)
                        {
                            bool _result = await ETModel.UIHelper.WaitYesOrNoDialog("温馨提示", "您当前使用不是wifi网络，是否继续更新大厅资源!");

                            if (_result)
                            {
                                await HotUpdateHelper.ToyUpdate("Lobby", false);
                            }
                            else
                            {
                                Application.Quit();
                                return;
                            }
                        }
                        else
                        {
                            await HotUpdateHelper.ToyUpdate("Lobby", false);
                        }
                    }
                }
                catch (Exception error)
                {
                    await UIHelper.WaitTipDialog("温馨提示", $"更新大厅资源失败!,{error.Message}");
                    Application.Quit();
                    return;
                }
            }
            self.Start().Coroutine();
            return;
        }

        public static async ETTask Exit(this LobbyUpdateProcetureLogic self)
        {
            Debug.Log("退出 ETHotfix.LobbyUpdateProceture");

            return;
        }

        public static async ETVoid Start(this LobbyUpdateProcetureLogic self)
        {
            await GameObjectHelper.WaitOneFrame();

            try
            {
                await ProcedureComponent.Instance.SwitchAsync(ProcedureNames.Lobby);
            }
            catch (Exception error)
            {
                UIHelper.OpenTipDialog("错误", error.Message, () => { Application.Quit(); });
            }
        }

    }
}
