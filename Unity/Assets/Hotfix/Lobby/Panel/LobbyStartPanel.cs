
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#region AUTO using
using ETModel;
using UnityEngine.UI;

#endregion

namespace ETHotfix
{
    public static partial class UIType
    {
        public const string LobbyStartPanel = "Lobby/Panels/LobbyStartPanel"; 
    }

    public class LobbyStartPanel : Component, IUICycle
    {
        #region AUTO fieldDeclare
		public UI ui;
		public ItemCollector itemCollector;
		public Button btnFix;
		public Button btnStart;

        #endregion

        public void OnStart() { }
        public void OnFocus() { }
        public void OnHide() { }
        public void OnPause() { }
        public void OnShow() { }
    }

    [ObjectSystem]
    public class LobbyStartPanelAwakeSystem : AwakeSystem<LobbyStartPanel>
    {
        public override void Awake(LobbyStartPanel self)
        {
            #region AUTO fieldAwake
			self.ui=(self.Entity as UI);
			self.itemCollector = self.ui.g_uiBehaviour as ItemCollector;
			self.btnFix= (self.itemCollector["btnFix"] as GameObject).GetComponent<Button>().BindAnim();
			self.btnStart= (self.itemCollector["btnStart"] as GameObject).GetComponent<Button>().BindAnim();

			#endregion

            self.Init();
        }
    }


    [ObjectSystem]
    public class LobbyStartPanelDestroySystem : DestroySystem<LobbyStartPanel>
    {
        public override void Destroy(LobbyStartPanel self)
        {
            #region AUTO fieldDestroy
			self.btnFix.onClick.RemoveAllListeners();
			self.btnStart.onClick.RemoveAllListeners();

			#endregion

            self.Uninit();
        }
    }

    public static class LobbyStartPanelSystem
    {
        public static void Init(this LobbyStartPanel self)
        {
            self.btnStart.onClick.AddListener(() =>
            {
                UIComponent.Instance.Open(UIType.LobbyLoginPanel);
            });

            self.btnFix.onClick.AddListener(() =>
            {
                HotUpdateHelper.SetForceUpdate();
                if (!PlatformHelper.ResartApp())
                {
                    //无法重启App;
                    UIHelper.OpenTipDialog("提醒", "修复完毕，请重启游戏开始！", () =>
                    {
                        Application.Quit();
                    });

                }
            });
        }

        public static void Uninit(this LobbyStartPanel self)
        {

        }
    }
}

