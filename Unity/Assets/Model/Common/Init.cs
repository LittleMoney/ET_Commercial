using System;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D;

namespace ETModel
{
	public class Init : MonoBehaviour
	{
		public static Init Instance;

#if UNITY_EDITOR
		[SerializeField]
		protected bool useAssetBundle;

		public static bool UseAssetBundle { get { return Init.Instance.useAssetBundle; } }
#else
		public static bool UseAssetBundle { get { return true; } }
#endif

		private void Start()
		{
			Instance = this;

			this.StartAsync().Coroutine();
		}
		
		private async ETVoid StartAsync()
		{
			try
			{
                #region 系统初始化
                DontDestroyOnLoad(gameObject);

				SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);
				//绑定图集加载
				SpriteAtlasManager.atlasRequested += (string atlasName, Action<SpriteAtlas> callback) =>
				{
					SpriteAtlas _sa = ResourcesComponent.Instance.LoadAsset($"{atlasName.Substring(0, atlasName.IndexOf("_"))}/Atlas/{atlasName}", typeof(SpriteAtlas)) as SpriteAtlas;
					callback(_sa);
				};

                #endregion

                #region 全局组件初始化

                Game.EventSystem.Add(DLLType.Model, typeof(Init).Assembly);

				Game.Scene.AddComponent<ConfigComponent>();
				Game.Scene.AddComponent<TimerComponent>();
				Game.Scene.AddComponent<NetOuterComponent>();
				Game.Scene.AddComponent<OpcodeTypeComponent>();
				Game.Scene.AddComponent<MessageDispatcherComponent>();
				Game.Scene.AddComponent<BroadcastComponent>();
				Game.Scene.AddComponent<ResourcesComponent>();
				Game.Scene.AddComponent<AppVersionComponent>();

				//检查核心文件
				await HotUpdateHelper.CensorCommandCoreBundles();

				//加载语言
				Game.Scene.AddComponent<LanguageComponent>();
				await Game.Scene.GetComponent<LanguageComponent>().LoadAsync("Common");

				GameObject _global = GameObject.Find("Global");
				DontDestroyOnLoad(_global);

				Game.Scene.AddComponent<UIComponent, GameObject>(_global.transform.Find("UICamera/UIRoot").gameObject);
				UIComponent.Instance.AddLayer(UILayerType.Pop, 6000, 20);
				UIComponent.Instance.Open(UIType.CommonLoadingPanel);

				BroadcastComponent.Instance.GetDefault().Run<int, string>(BroadcastId.ProgressMessage, 0, "启动游戏...");

#if !UNITY_EDITOR && DEBUG
				_global.AddComponent<Reporter>();
#endif

                #endregion

                #region 更新处理

                if (Init.UseAssetBundle)
				{
					try
					{
						
						bool _result = false;
						UpdateType _updateType = await HotUpdateHelper.AppNeedUpdate();

						if (_updateType == UpdateType.Cold)
						{

							if (Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork)
							{
								_result = await ETModel.UIHelper.WaitYesOrNoDialog("温馨提示", "您当前使用不是wifi网络，是否继续更新App!");
							}

							if (_result)
							{
								await HotUpdateHelper.AppColdUpdate();
								Application.Quit(0);
								return;
							}
							else
							{
								Application.Quit(0);
								return;
							}
						}
						else
						{
							if (HotUpdateHelper.ToyNeedUpdate("Common"))
							{
								if (Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork)
								{
									_result = await ETModel.UIHelper.WaitYesOrNoDialog("温馨提示", "您当前使用不是wifi网络，是否继续更新App资源!");

									if (_result)
									{
										await HotUpdateHelper.ToyUpdate("Common", false);
									}
									else
									{
										Application.Quit(0);
									}
								}
                                else
                                {
									await HotUpdateHelper.ToyUpdate("Common", false);
								}

								//卸载旧的资源包
								UIComponent.Instance.Close(UIType.CommonLoadingPanel);
								Game.Scene.GetComponent<LanguageComponent>().Unload("Common");
								ResourcesComponent.Instance.UnloadBundleForRegex(".*");

								//重新加载页面
								await Game.Scene.GetComponent<LanguageComponent>().LoadAsync("Common"); 
								UIComponent.Instance.Open(UIType.CommonLoadingPanel);
								BroadcastComponent.Instance.GetDefault().Run<int, string>(BroadcastId.ProgressMessage, 0, "重载资源完毕...");
							}
						}
					}
					catch (Exception error)
					{
						await UIHelper.WaitTipDialog("错误", error.Message);
						Application.Quit(0);
						return;
					}
				}

                #endregion

                Game.Hotfix.LoadHotfixAssembly();
				Game.Hotfix.GotoHotfix();
			}
			catch (Exception error)
			{
				await UIHelper.WaitTipDialog("错误", error.Message);
				Application.Quit(0);
				return;
			}
		}


        private void Update()
		{
			OneThreadSynchronizationContext.Instance.Update();
			Game.Hotfix.Update?.Invoke();
			Game.EventSystem.Update();
		}

		private void LateUpdate()
		{
			Game.Hotfix.LateUpdate?.Invoke();
			Game.EventSystem.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			Game.Hotfix.OnApplicationQuit?.Invoke();
			Game.Close();
		}
	}
}