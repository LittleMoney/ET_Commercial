using System;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
	public static class Init
	{
		public static void Start()
		{
            #region 注册热更层回调
            //宏可以在 Editor/Project Settings/Player/Scripting Define Symbols 中设置
#if ILRuntime
			Log.Info("Hotfix层是ILRuntime模式 Start()");
#else
            Log.Info("Hotfix层是mono模式 Start()");
#endif
			// 注册热更层回调到Model层
			ETModel.Game.Hotfix.Update = () => { Update(); };
			ETModel.Game.Hotfix.LateUpdate = () => { LateUpdate(); };
			ETModel.Game.Hotfix.OnApplicationQuit = () => { OnApplicationQuit(); };

			#endregion

			Game.Scene.AddComponent<OpcodeTypeComponent>();
			Game.Scene.AddComponent<MessageDispatcherComponent>();
			Game.Scene.AddComponent<ConfigComponent>();
			Game.Scene.AddComponent<ProcedureComponent>();
			Game.Scene.AddComponent<BroadcastComponent>();

			//设置UI层级
			Game.Scene.AddComponent<UIComponent, GameObject>(GameObject.Find("Global/UICamera/UIRoot"));
			UIComponent.Instance.AddLayer(UILayerType.Background, 1000, 20);
			UIComponent.Instance.AddLayer(UILayerType.Normal,  2000, 20);
			UIComponent.Instance.AddLayer(UILayerType.Fix,  3000, 20);
			UIComponent.Instance.AddLayer(UILayerType.Pop,  4000, 20);
			UIComponent.Instance.AddLayer(UILayerType.Front, 5000, 20);

			//关闭Model层UI，改用Hotfix层UI
			UIComponent.Instance.Open(UIType.CommonLoadingPanel);
			BroadcastComponent.Instance.GetDefault().Run<int, string>(BroadcastId.ProgressMessage, 0, "启动应用...");
			ETModel.UIComponent.Instance.Close(ETModel.UIType.CommonLoadingPanel);
			Game.EventSystem.Run(EventIdType.InitSceneStart);
		}

		public static void Update()
		{
			try
			{
				Game.EventSystem.Update();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public static void LateUpdate()
		{
			try
			{
				Game.EventSystem.LateUpdate();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public static void OnApplicationQuit()
		{
			Game.Close();
		}

	}
}