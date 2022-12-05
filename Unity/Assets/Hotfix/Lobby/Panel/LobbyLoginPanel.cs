
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
        public const string LobbyLoginPanel = "Lobby/Panels/LobbyLoginPanel"; 
    }

    public class LobbyLoginPanel : Component, IUICycle
    {
        #region AUTO fieldDeclare
		public UI ui;
		public ItemCollector itemCollector;
		public InputField ipfPass;
		public Button btnFogeton;
		public Button btnRegiste;
		public Button btnLogin;
		public InputField ipfAccount;
		public Button btnClose;

        #endregion

        public void OnStart() { }
        public void OnFocus() { }
        public void OnHide() { }
        public void OnPause() { }
        public void OnShow() { }
    }

    [ObjectSystem]
    public class LobbyLoginPanelAwakeSystem : AwakeSystem<LobbyLoginPanel>
    {
        public override void Awake(LobbyLoginPanel self)
        {
            #region AUTO fieldAwake
			self.ui=(self.Entity as UI);
			self.itemCollector = self.ui.g_uiBehaviour as ItemCollector;
			self.ipfPass= (self.itemCollector["ipfPass"] as GameObject).GetComponent<InputField>();
			self.btnFogeton= (self.itemCollector["btnFogeton"] as GameObject).GetComponent<Button>().BindAnim();
			self.btnRegiste= (self.itemCollector["btnRegiste"] as GameObject).GetComponent<Button>().BindAnim();
			self.btnLogin= (self.itemCollector["btnLogin"] as GameObject).GetComponent<Button>().BindAnim();
			self.ipfAccount= (self.itemCollector["ipfAccount"] as GameObject).GetComponent<InputField>();
			self.btnClose= (self.itemCollector["btnClose"] as GameObject).GetComponent<Button>().BindAnim();

            #endregion

            self.Init();
        }
    }


    [ObjectSystem]
    public class LobbyLoginPanelDestroySystem : DestroySystem<LobbyLoginPanel>
    {
        public override void Destroy(LobbyLoginPanel self)
        {
            #region AUTO fieldDestroy
			self.ipfPass.onEndEdit.RemoveAllListeners();
			self.ipfPass.onValueChanged.RemoveAllListeners();
			self.btnFogeton.onClick.RemoveAllListeners();
			self.btnRegiste.onClick.RemoveAllListeners();
			self.btnLogin.onClick.RemoveAllListeners();
			self.ipfAccount.onEndEdit.RemoveAllListeners();
			self.ipfAccount.onValueChanged.RemoveAllListeners();
			self.btnClose.onClick.RemoveAllListeners();

            #endregion

            self.Uninit();
        }
    }

    public static class LobbyLoginPanelSystem
    {
        public static void Init(this LobbyLoginPanel self)
        {

        }

        public static void Uninit(this LobbyLoginPanel self)
        {

        }
    }
}

