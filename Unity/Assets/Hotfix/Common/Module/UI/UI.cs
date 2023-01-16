using System;
using System.Collections.Generic;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
	//[HideInHierarchy]
	public sealed class UI : EntityI
	{
		/// <summary>
		/// 
		/// </summary>
		public bool g_isPaused;

		/// <summary>
		/// 
		/// </summary>
		public bool g_isHided;

		/// <summary>
		/// 被独占显示
		/// </summary>
		public bool g_isExclusived;

		/// <summary>
		/// 是否第一次显示过
		/// </summary>
		public bool g_hasStarted;

		/// <summary>
		/// 
		/// </summary>
		public string g_uiType;

		/// <summary>
		/// 
		/// </summary>
		public int g_sortOrder;

		/// <summary>
		/// 
		/// </summary>
		public object g_data;

		/// <summary>
		/// 
		/// </summary>
		public GameObject g_rootGameObject;

		/// <summary>
		/// 
		/// </summary>
		public RectTransform g_rectTransform;

		/// <summary>
		/// 
		/// </summary>
		public UIBehaviour g_uiBehaviour;

		/// <summary>
		/// 
		/// </summary>
		public Canvas g_canvas;

	}

	[ObjectSystem]
	public class UiAwakeSystem : AwakeSystem<UI, string, GameObject, object>
	{
		public override void Awake(UI self, string uiType, GameObject gameObject, object data)
		{
			gameObject.layer = LayerMask.NameToLayer(LayerNames.UI);
			self.g_uiType = uiType;
			self.g_rootGameObject = gameObject;
			self.g_data = data;
			self.g_rectTransform = gameObject.GetComponent<RectTransform>();
			self.g_uiBehaviour = gameObject.GetComponent<UIBehaviour>();
			self.g_canvas = self.g_rootGameObject.GetComponent<Canvas>();
			if (self.g_canvas == null) self.g_canvas = self.g_rootGameObject.AddComponent<Canvas>();
			if (self.g_rootGameObject.GetComponent<GraphicRaycaster>() == null) self.g_rootGameObject.AddComponent<GraphicRaycaster>();
			self.g_canvas.overrideSorting = true;

			if (self.g_uiBehaviour.maskType == UIMaskType.Black)
			{
				self.AddComponent(typeof(UIMaskComponent));
			}

			self.g_isPaused = true;
			self.g_isHided = !self.g_rootGameObject.activeSelf;
			self.g_hasStarted = false;
		}
	}

	[ObjectSystem]
	public class UiDestroySystem : DestroySystem<UI>
	{

		public override void Destroy(UI self)
		{
			if (self.g_rootGameObject != null)
			{
				UnityEngine.Object.Destroy(self.g_rootGameObject);
				self.g_rootGameObject = null;
			}
		}
	}


	public static class UISystem
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static UILayerType GetUILayerType(this UI self)
		{
			return self.g_uiBehaviour.layerType;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static UIShowType GetUIShowType(this UI self)
		{
			return self.g_uiBehaviour.showType;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static UIMaskType GetUIMaskType(this UI self)
		{
			return self.g_uiBehaviour.maskType;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="active"></param>
		public static void SetActive(this UI self, bool active)
		{
			self.g_rootGameObject.SetActive(active);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="index"></param>
		public static void SetSortOrder(this UI self, int sortOrder)
		{
			self.g_canvas.sortingOrder = sortOrder;
			self.g_sortOrder = sortOrder;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="parentTransform"></param>
		public static void SetParent(this UI self, RectTransform parent)
		{
			self.g_rootGameObject.transform.SetParent(parent, false);
		}


		#region 生命周期

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		public static void Start(this UI self)
		{
			if (!self.g_hasStarted) 
			{
				self.g_hasStarted = true;
				foreach (Component component in self.GetComponents<ETModel.IUICycle>())
				{
					(component as ETModel.IUICycle).OnStart();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		public static void Show(this UI self)
		{
			self.g_rootGameObject.SetActive(true);
			self.g_isHided = false;

			foreach (Component component in self.GetComponents<ETModel.IUICycle>())
			{
				(component as ETModel.IUICycle).OnShow();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="isExclusive"></param>
		public static void Hide(this UI self)
		{
			self.g_rootGameObject.SetActive(false);
			self.g_isHided = true;

			foreach (Component subCompoennt in self.GetComponents(typeof(ETModel.IUICycle)))
			{
				(subCompoennt as ETModel.IUICycle).OnHide();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		public static void Focus(this UI self)
		{
			self.g_isPaused = false;

			foreach (Component subCompoennt in self.GetComponents(typeof(ETModel.IUICycle)))
			{
				(subCompoennt as ETModel.IUICycle).OnFocus();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		public static void Pause(this UI self)
		{

			self.g_isPaused = true;
			foreach (Component subCompoennt in self.GetComponents(typeof(ETModel.IUICycle)))
			{
				(subCompoennt as ETModel.IUICycle).OnPause();
			}
		}

		/// <summary>
		/// 关闭自己
		/// </summary>
		/// <param name="self"></param>
		public static void Close(this UI self)
		{
			(self.Parent as UIComponent).Close(self);
		}

		#endregion
	}
}