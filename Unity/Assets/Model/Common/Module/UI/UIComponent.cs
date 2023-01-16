using System;
using System.Collections.Generic;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETModel
{
	[ObjectSystem]
	public class UIComponentAwakeSystem : AwakeSystem<UIComponent, GameObject>
	{
		public override void Awake(UIComponent self, GameObject uiRoot)
		{
			UIComponent.Instance = self;
			self.g_uiRoot = uiRoot;
			self.gs_defaultUIFactory = new UIDefaultFactory();
			self.Load();
		}
	}


	[ObjectSystem]
	public class UiComponentLoadSystem : LoadSystem<UIComponent>
	{
		public override void Load(UIComponent self)
		{
			self.Load();
		}
	}


	/// <summary>
	/// 管理所有UI
	/// </summary>
	public class UIComponent: Component
	{
		public static UIComponent Instance;

		public GameObject g_uiRoot;

		public IUIFactory gs_defaultUIFactory = null;

		public Dictionary<UILayerType, UILayer> uiLayers = new Dictionary<UILayerType, UILayer>();

		public Dictionary<string, UI> uiDict = new Dictionary<string, UI>();

		public Dictionary<string, IUIFactory> uiFactorys = new Dictionary<string, IUIFactory>();

	}

	/// <summary>
	/// 
	/// </summary>
	public static class UIComponentSystem
	{
		public static  void Load(this UIComponent self)
		{
			self.uiFactorys.Clear();

			List<Type> types = Game.EventSystem.GetTypes(typeof(UIFactoryAttribute));

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(UIFactoryAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				UIFactoryAttribute attribute = attrs[0] as UIFactoryAttribute;
				if (self.uiFactorys.ContainsKey(attribute.Type))
				{
					Log.Debug($"已经存在同类UI Factory: {attribute.Type}");
					throw new Exception($"已经存在同类UI Factory: {attribute.Type}");
				}

				object o = Activator.CreateInstance(type);
				IUIFactory factory = o as IUIFactory;
				if (factory == null)
				{
					Log.Error($"{o.GetType().FullName} 没有继承 IUIFactory");
					continue;
				}
				self.uiFactorys.Add(attribute.Type, factory);
			}
		}

		/// <summary>
		/// 获取ui类型的实例，如果该类型有多个实例，将获取最后打开的一个
		/// </summary>
		/// <param name="self"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static UI Get(this UIComponent self, string uiType)
		{
			
			if(self.uiDict.TryGetValue(uiType, out UI _holder))
            {
				return _holder;

			}
            else
            {
				return null;
			}
		}


		#region 注册工厂

		/// <summary>
		/// 注册工厂
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiType"></param>
		/// <param name="iUIFactory"></param>
		public static void Register(this UIComponent self, string uiType, IUIFactory iUIFactory)
		{
			if (!self.uiFactorys.ContainsKey(uiType))
			{
				self.uiFactorys.Add(uiType, iUIFactory);
			}
		}

		/// <summary>
		/// 移除工厂
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiType"></param>
		public static void UnRegister(this UIComponent self, string uiType)
		{
			if (self.uiDict.ContainsKey(uiType))
			{
				throw new Exception("该类型的ui在使用中，不能卸载其工厂接口");
			}

			if (self.uiFactorys.ContainsKey(uiType))
			{
				self.uiFactorys.Remove(uiType);
			}
		}

		#endregion

		#region ui Layer 处理

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="groupName"></param>
		/// <param name="groupRoot"></param>
		public static void AddLayer(this UIComponent self, UILayerType layerType, int sortOrder, int intevalSortOrder)
		{
			if (!self.uiLayers.ContainsKey(layerType))
			{
				GameObject _layerGo = new GameObject();
				_layerGo.name = $"Model_{layerType.ToString()}";
				_layerGo.transform.SetParent(self.g_uiRoot.transform);

				Canvas _canvas = _layerGo.AddComponent<Canvas>();
				_layerGo.AddComponent<GraphicRaycaster>();
				_canvas.overrideSorting = true;
				_canvas.sortingOrder = sortOrder;

				RectTransform _rectTransform = _layerGo.GetComponent<RectTransform>();
				_rectTransform.localPosition = Vector3.zero;
				_rectTransform.localScale = Vector3.one;
				_rectTransform.anchorMin = Vector2.zero;
				_rectTransform.anchorMax = Vector2.one;
				_rectTransform.offsetMin = Vector2.zero;
				_rectTransform.offsetMax = Vector2.zero;

				self.uiLayers.Add(layerType, new UILayer()
				{
					layerType = layerType,
					rootGameObject = _layerGo,
					sortOrder = sortOrder,
					intevalSortOrder = intevalSortOrder,
					showQueue = new List<UI>(),
					sortOrderList = new List<int>()
				});

				_layerGo.SetActive(false);
			}
			else
			{
				throw new Exception("ui组名称已经存在");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="groupName"></param>
		/// <param name="groupRoot"></param>
		public static void RemoveLayer(this UIComponent self, UILayerType layerType,bool isDestoryRootGameObject=false)
		{
			if (self.uiLayers.TryGetValue(layerType, out UILayer layer))
			{
				UI _ui = null;

				foreach(KeyValuePair<string,UI> _item in self.uiDict)
                {
					if(_item.Value.GetUILayerType()== layerType)
                    {
						self.uiDict.Remove(_ui.g_uiType);
						self.CloseProcess(_item.Value as UI);
					}
                }

				layer.rootGameObject.SetActive(false);
				self.uiLayers.Remove(layerType);

				if (isDestoryRootGameObject)
				{
					GameObject.Destroy(layer.rootGameObject);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="groupName"></param>
		/// <param name="groupRoot"></param>
		public static void ShowLayer(this UIComponent self, UILayerType layerType)
		{
			if (self.uiLayers.TryGetValue(layerType, out UILayer layer))
			{
				layer.rootGameObject.SetActive(true);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="groupName"></param>
		/// <param name="groupRoot"></param>
		public static void HideLayer(this UIComponent self, UILayerType layerType)
		{
			if (self.uiLayers.TryGetValue(layerType, out UILayer layer))
			{
				layer.rootGameObject.SetActive(false);
			}
		}

		#endregion

		#region UI操作

		/// <summary>
		/// 打开一个UI实例
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiGroupName">组名称</param>
		/// <param name="showMode">显示模式</param>
		/// <param name="uiType">类型</param>
		/// <param name="data">传递给UI的初始化数据</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static UI Open(this UIComponent self,string uiType, object data=null, bool suspendShow = false)
        {
			if (self.uiDict.TryGetValue(uiType, out UI _holder)) return _holder;

			IUIFactory _uiFactory = null;
			if (!self.uiFactorys.TryGetValue(uiType, out _uiFactory))
			{
				_uiFactory = self.gs_defaultUIFactory;
			}

			if(_uiFactory == null ) throw new Exception("打开ui失败");
			UI _ui = _uiFactory.Create(uiType, self.g_uiRoot,data);
			UILayer _uiLayer = self.uiLayers[_ui.GetUILayerType()];
			_ui.Parent = self;
			_ui.SetParent(_uiLayer.rootGameObject.GetComponent<RectTransform>());
			self.uiDict.Add(_ui.g_uiType, _ui);

			if (!_ui.g_isHided) //已经是显示状态,直接加入显示队列
			{
				if (!_uiLayer.rootGameObject.activeSelf)
				{
					_uiLayer.rootGameObject.SetActive(true);
				}

				self.AddShowQueueFirst(_ui, _uiLayer);
			}
			else if (!suspendShow) //影藏状态，且不要求暂停显示，则强制立即显示
			{
				self.ShowProcess(_ui);
			}

			return _ui;
		}

		/// <summary>
		/// 打开一个UI实例
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiGroupName">组名称</param>
		/// <param name="showMode">显示模式</param>
		/// <param name="uiType">类型</param>
		/// <param name="data">传递给UI的初始化数据</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static async ETTask<UI> OpenAsync(this UIComponent self, string uiType, object data = null, bool suspendShow = false)
		{
			if (self.uiDict.TryGetValue(uiType, out UI _holder)) return _holder;

			IUIFactory _uiFactory = null;
			if (!self.uiFactorys.TryGetValue(uiType, out _uiFactory))
			{
				_uiFactory = self.gs_defaultUIFactory;
			}

			if (_uiFactory == null) throw new Exception("打开ui失败");
			UI _ui = await _uiFactory.CreateAsync(uiType, self.g_uiRoot, data) as UI;
			UILayer _uiLayer = self.uiLayers[_ui.GetUILayerType()];
			_ui.Parent = self;
			_ui.SetParent(_uiLayer.rootGameObject.GetComponent<RectTransform>());
			self.uiDict.Add(_ui.g_uiType, _ui);

			if (!_ui.g_isHided) //已经是显示状态,直接加入显示队列
			{
				if (!_uiLayer.rootGameObject.activeSelf)
				{
					_uiLayer.rootGameObject.SetActive(true);
				}

				self.AddShowQueueFirst(_ui, _uiLayer);
			}
			else if (!suspendShow) //影藏状态，且不要求暂停显示，则强制立即显示
			{
				self.ShowProcess(_ui);
			}

			return _ui;
		}


		/// <summary>
		/// 显示一个ui实例，如果ui类型有多个ui则按照Open先后顺序依次显示
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiType"></param>
		public static void Show(this UIComponent self, string uiType)
		{
			if (self.uiDict.TryGetValue(uiType, out UI _holder))
			{
				if (_holder.g_isHided)
				{
					self.ShowProcess(_holder);
				}
			}
		}

		/// <summary>
		/// 显示一个ui实例，如果ui类型有多个ui则按照先后顺序依次显示
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiType"></param>
		public static void Show(this UIComponent self, UI ui)
		{
			if (self.uiDict.TryGetValue(ui.g_uiType, out UI _holder))
			{
				if (_holder.g_isHided)
				{
					self.ShowProcess(_holder);
				}
			}
		}

		/// <summary>
		/// 隐藏一个ui实例,如果uiType对应到多个UI则根据open顺序，从后向前隐藏
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiType"></param>
		public static void Hide(this UIComponent self, UI ui)
		{
			self.HideProcess(ui);
		}

		/// <summary>
		/// 隐藏一个ui实例,如果uiType对应到多个UI则根据open顺序，从后向前隐藏
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiType"></param>
		public static void Hide(this UIComponent self, string uiType)
		{
			if (self.uiDict.TryGetValue(uiType, out UI _holder))
			{
				self.HideProcess(_holder);
			}
		}

		/// <summary>
		/// 关闭一个UI，将会关闭所有打开的该ui类型的实例
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiType"></param>
		public static void Close(this UIComponent self, string uiType)
		{
			if (self.uiDict.TryGetValue(uiType, out UI _holder))
			{
				self.uiDict.Remove(_holder.g_uiType);
				self.CloseProcess(_holder);
			}
		}

		/// <summary>
		/// 关闭一个UI，将会关闭所有打开的该ui类型的实例
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiType"></param>
		public static void Close(this UIComponent self, UI ui)
		{
			if (self.uiDict.TryGetValue(ui.g_uiType, out UI _holder))
			{
				if (ui == _holder)
				{
					self.uiDict.Remove(_holder.g_uiType);
					self.CloseProcess(_holder);
				}
			}
		}


		/// <summary>
		/// 显示ui
		/// </summary>
		/// <param name="self"></param>
		/// <param name="ui"></param>
		private static void ShowProcess(this UIComponent self, UI ui)
		{
			//第一次显示无论如何都调用show方法
			if (!ui.g_isHided) return;

			UILayer _uiLayer = self.uiLayers[ui.GetUILayerType()];

			if (!_uiLayer.rootGameObject.activeSelf)
			{
				_uiLayer.rootGameObject.SetActive(true);
			}

			if (!ui.g_hasStarted)
			{
				self.AddShowQueueFirst(ui, _uiLayer);
			}
			else
			{
				self.AddShowQueue(ui, _uiLayer);
			}
		}

		/// <summary>
		/// 隐藏一个ui实例
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiType"></param>
		private static void HideProcess(this UIComponent self, UI ui)
		{
			if (ui.g_isHided) return;
			UILayer _layer = self.uiLayers[ui.GetUILayerType()];
			self.RemoveShowQueue(ui, _layer);
		}

		/// <summary>
		/// 关闭一个UI实例
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiType"></param>
		private static void CloseProcess(this UIComponent self, UI ui)
		{

			UILayer _layer = self.uiLayers[ui.GetUILayerType()];

			self.RemoveShowQueue(ui, _layer);

			//组中已经没有可需要显示的东西，直接隐藏组
			if (_layer.showQueue.Count == 0)
			{
				_layer.rootGameObject.SetActive(false);
			}

			//谁创建谁释放
			if (self.uiFactorys.TryGetValue(ui.g_uiType, out IUIFactory iuiFactory))
			{
				iuiFactory.Remove(ui);
			}
			else
			{
				self.gs_defaultUIFactory.Remove(ui);
			}
		}


		#endregion

		#region 显示队列处理

		/// <summary>
		/// 移除ui显示
		/// </summary>
		/// <param name="self"></param>
		/// <param name="ui"></param>
		/// <param name="group"></param>
		private static void RemoveShowQueue(this UIComponent self, UI ui, UILayer uiLayer)
		{
			if (!ui.g_isPaused)
			{
				ui.Pause();
			}

			if (!ui.g_isHided)
			{
				ui.Hide();
			}

			switch (ui.GetUIShowType())
			{
				case UIShowType.Fix:
					{
						uiLayer.showQueue.Remove(ui);
						uiLayer.sortOrderList.Remove(ui.g_sortOrder);
						break;
					}
				case UIShowType.Pop:
					{
						#region

						uiLayer.showQueue.Remove(ui);
						uiLayer.sortOrderList.Remove(ui.g_sortOrder);

						//刷新显示
						bool _isPop = false;
						UI _tempUI = null;
						//下面的取消悬停
						for (int i = uiLayer.showQueue.Count - 1; i >= 0; i--)
						{
							_tempUI = uiLayer.showQueue[i];

							if (_tempUI.g_isHided)
							{
								_tempUI.Show();
							}

							if (!_isPop && _tempUI.g_isPaused)
							{
								_tempUI.Focus();
							}

							if (_tempUI.GetUIShowType() == UIShowType.Pop)
							{
								_isPop = true;
							}
							else if (_tempUI.GetUIShowType() == UIShowType.Exclusive)
							{
								break;
							}
						}

						#endregion

						break;
					}
				case UIShowType.Exclusive:
					{

						#region

						uiLayer.showQueue.Remove(ui);
						uiLayer.sortOrderList.Remove(ui.g_sortOrder);

						//刷新显示
						bool _isPop = false;
						UI _tempUI = null;
						//下面的取消悬停
						for (int i = uiLayer.showQueue.Count - 1; i >= 0; i--)
						{
							_tempUI = uiLayer.showQueue[i];

							if (_tempUI.g_isHided)
							{
								_tempUI.Show();
							}

							if (!_isPop && _tempUI.g_isPaused)
							{
								_tempUI.Focus();
							}

							if (_tempUI.GetUIShowType() == UIShowType.Pop)
							{
								_isPop = true;
							}
							else if (_tempUI.GetUIShowType() == UIShowType.Exclusive)
							{
								break;
							}
						}

						#endregion

						break;
					}
			}

			
		}

		/// <summary>
		/// 初次添加到显示队列
		/// </summary>
		/// <param name="self"></param>
		/// <param name="ui"></param>
		/// <param name="group"></param>
		private static void AddShowQueueFirst(this UIComponent self, UI ui, UILayer uiLayer)
        {
			//放到最上
			ui.SetSortOrder(self.GetTopSrotOrder(uiLayer));
			uiLayer.showQueue.Add(ui);

			switch (ui.GetUIShowType())
			{
				case UIShowType.Fix:
					{
						ui.Start();

						if (ui.g_isHided)
						{
							ui.Show();
						}

						if (ui.g_isPaused)
						{
							ui.Focus();
						}

						break;
					}
				case UIShowType.Pop:
					{
						ui.Start();

						if (ui.g_isHided)
						{
							ui.Show();
						}

						if (ui.g_isPaused)
						{
							ui.Focus();
						}

						UI _uiDown = null;
						for (int i = uiLayer.showQueue.Count - 2; i >= 0; i--)
						{
							_uiDown = uiLayer.showQueue[i];
							if (!_uiDown.g_isPaused)
							{
								_uiDown.Pause();
							}
						}

						break;
					}
				case UIShowType.Exclusive:
					{
						ui.Start();
						
						if (ui.g_isHided)
						{
							ui.Show();
						}

						if (ui.g_isPaused)
						{
							ui.Focus();
						}

						UI _uiDown = null;
						for (int i = uiLayer.showQueue.Count - 2; i >= 0; i--)
						{
							_uiDown = uiLayer.showQueue[i];
							if (!_uiDown.g_isPaused)
							{
								_uiDown.Pause();
							}

							if (!_uiDown.g_isHided)
							{
								_uiDown.Hide();
							}
						}

						break;
					}
			}
		}

		/// <summary>
		/// 添加到显示队列
		/// </summary>
		/// <param name="self"></param>
		/// <param name="ui"></param>
		/// <param name="group"></param>
		private static void AddShowQueue(this UIComponent self, UI ui, UILayer uiLayer)
		{
			switch (ui.GetUIShowType())
			{
				case UIShowType.Fix: //上下都不影响,恢复的状态由上面决定
					{
						#region fix

						int _i = uiLayer.showQueue.Count - 1;
						for (; _i >= 0; _i++)
                        {
							if(ui.g_sortOrder> uiLayer.showQueue[_i].g_sortOrder)
                            {
								uiLayer.showQueue.Insert(_i + 1, ui);
                            }
                        }

						//扫描上面的UI看看自己需要处于何种状态
						int _upHasPopOrExclusive = 0;
						UI _ui = null;
						for (int i= uiLayer.showQueue.Count-1;i>= _i+2; i--)
                        {
							_ui = uiLayer.showQueue[i];
							
							if(!_ui.g_isHided)
                            {
								if (_ui.GetUIShowType() == UIShowType.Pop)
								{
									_upHasPopOrExclusive = 1;
								}
								else if (_ui.GetUIShowType() == UIShowType.Exclusive)
								{
									_upHasPopOrExclusive = 2;
									break;
								}
							}
                        }

						//原地还原
						if (_upHasPopOrExclusive == 0) //上面既没有pop ui 也没有exclusive ui
						{
							if (ui.g_isHided)
							{
								ui.Show();
							}

							if (ui.g_isPaused)
							{
								ui.Focus();
							}
						}
						else if (_upHasPopOrExclusive == 1) //上面有pop ui
						{
							if (ui.g_isHided)
							{
								ui.Show();
							}
						}
                        else
                        {
							//上面有 exclusive ui 忽略
                        }
						#endregion

						break;
					}

				case UIShowType.Pop: //恢复到顶层，下面的全部失去焦点
					{
						#region

						//移动到顶部显示
						ui.SetSortOrder(self.GetTopSrotOrder(uiLayer));
						uiLayer.showQueue.Add(ui);

						if (ui.g_isHided)
						{
							ui.Show();
						}

						if (ui.g_isPaused)
						{
							ui.Focus();
						}

						//下面的ui 全部暂停
						UI _uiDown = null;
						for (int i = uiLayer.showQueue.Count - 2; i >= 0; i--)
						{
							_uiDown = uiLayer.showQueue[i];
							if (!_uiDown.g_isPaused)
							{
								_uiDown.Pause();
							}
						}
						#endregion

						break;
					}

				case UIShowType.Exclusive: //恢复到顶层，下面的全部隐藏
					{
						#region

						//移动到顶部显示
						ui.SetSortOrder(self.GetTopSrotOrder(uiLayer));
						uiLayer.showQueue.Add(ui);

						if (ui.g_isHided)
						{
							ui.Show();
						}

						if (ui.g_isPaused)
						{
							ui.Focus();
						}

						//下面的ui 全部停止显示
						UI _uiDown = null;
						for (int i = uiLayer.showQueue.Count - 2; i >= 0; i--)
						{
							_uiDown = uiLayer.showQueue[i];

							if (!_uiDown.g_isPaused)
							{
								_uiDown.Pause();
							}

							if (!_uiDown.g_isHided)
							{
								//必须立即隐藏
								_uiDown.Hide();
							}
						}

						#endregion

						break;
					}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="layer"></param>
		/// <returns></returns>
		private static int GetTopSrotOrder(this UIComponent self,UILayer uiLayer)
        {
			if (uiLayer.sortOrderList.Count > 0)
			{
				uiLayer.sortOrderList.Add(uiLayer.sortOrderList[uiLayer.sortOrderList.Count-1] + uiLayer.intevalSortOrder);
				return uiLayer.sortOrderList[uiLayer.sortOrderList.Count - 1] + uiLayer.intevalSortOrder;
			}
			else
            {
				uiLayer.sortOrderList.Add(uiLayer.sortOrder + uiLayer.intevalSortOrder);
				return uiLayer.sortOrder + uiLayer.intevalSortOrder;
			}
        }
		#endregion

	}
}