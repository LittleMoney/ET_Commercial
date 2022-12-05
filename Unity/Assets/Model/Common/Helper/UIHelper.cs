using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace ETModel
{
	public static partial class UIHelper
	{
		public static Button BindAnim(this Button button)
        {
			Animator _animator = button.gameObject.AddComponent<Animator>();
			_animator.runtimeAnimatorController=ResourcesComponent.Instance.LoadAsset("Common/Anim/Button",typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
			_animator.cullingMode = UnityEngine.AnimatorCullingMode.CullUpdateTransforms;
			button.transition = Selectable.Transition.Animation;
			return button;
		}

		public static void BindRC(Component component)
		{
			ReferenceCollector _rc=(component.Entity as UI).g_rootGameObject.GetComponent<ReferenceCollector>();
			FieldInfo[] _fieldInfos= component.GetType().GetFields();
			foreach(FieldInfo _fileInfo in _fieldInfos)
            {
				object _obj = _rc[_fileInfo.Name];
				if (_obj != null) _fileInfo.SetValue(component, _obj);
            }
		}

		public static void OpenLoading()
        {
			UIComponent.Instance.Open(UIType.CommonLoadingPanel);
		}

		public static void OpenTipDialog(string title,string message, Action callback)
        {
			CommonDialogMsg _msg = new CommonDialogMsg() { title = title, text = message,dialogType=CommonDialogType.Tip,actionSure= callback };
			UI _ui=UIComponent.Instance.Get(UIType.CommonDialogPanel);
			if(_ui==null)
            {
				UIComponent.Instance.Open(UIType.CommonDialogPanel, _msg);
            }
            else
            {
				_ui.GetComponent<CommonDialogPanel>().AddMsg(_msg);
            }
		}

		public static ETTask WaitTipDialog(string title, string message)
		{
			CommonDialogMsg _msg = new CommonDialogMsg() { title = title, text = message, tcsSure = new ETTaskCompletionSource() };
			UI _ui = UIComponent.Instance.Get(UIType.CommonDialogPanel);
			if (_ui == null)
			{
				UIComponent.Instance.Open(UIType.CommonDialogPanel, _msg);
				return _msg.tcsSure.Task;
			}
			else
			{
				_ui.GetComponent<CommonDialogPanel>().AddMsg(_msg);
				return _msg.tcsSure.Task;
			}
		}

		public static void OpenYesOrNoDialog(string title, string message,Action<bool> callback)
		{
			CommonDialogMsg _msg = new CommonDialogMsg() { title = title, text = message,dialogType=CommonDialogType.YesOrNo,actionYesOrNo= callback };
			UI _ui = UIComponent.Instance.Get(UIType.CommonDialogPanel);
			if (_ui == null)
			{
				UIComponent.Instance.Open(UIType.CommonDialogPanel, _msg);
			}
			else
			{
				_ui.GetComponent<CommonDialogPanel>().AddMsg(_msg);
			}
		}

		public static ETTask<bool> WaitYesOrNoDialog(string title, string message)
		{
			CommonDialogMsg _msg = new CommonDialogMsg() { title = title, text = message, dialogType = CommonDialogType.YesOrNo, tcsYesOrNo = new ETTaskCompletionSource<bool>() };
			UI _ui = UIComponent.Instance.Get(UIType.CommonDialogPanel);
			if (_ui == null)
			{
				UIComponent.Instance.Open(UIType.CommonDialogPanel, _msg);
			}
			else
			{
				_ui.GetComponent<CommonDialogPanel>().AddMsg(_msg);
			}

			return _msg.tcsYesOrNo.Task;
		}

		public static void OpenOkOrCancelDialog(string title, string message, Action<bool> callback)
		{
			CommonDialogMsg _msg = new CommonDialogMsg() { title = title, text = message, dialogType = CommonDialogType.OkOrCanel, actionYesOrNo = callback };
			UI _ui = UIComponent.Instance.Get(UIType.CommonDialogPanel);
			if (_ui == null)
			{
				UIComponent.Instance.Open(UIType.CommonDialogPanel, _msg);
			}
			else
			{
				_ui.GetComponent<CommonDialogPanel>().AddMsg(_msg);
			}
		}

		public static ETTask<bool> WaitOkOrCancelDialog(string title, string message)
		{
			CommonDialogMsg _msg = new CommonDialogMsg() { title = title, text = message, dialogType = CommonDialogType.OkOrCanel, tcsYesOrNo = new ETTaskCompletionSource<bool>() };
			UI _ui = UIComponent.Instance.Get(UIType.CommonDialogPanel);
			if (_ui == null)
			{
				UIComponent.Instance.Open(UIType.CommonDialogPanel, _msg);
			}
			else
			{
				_ui.GetComponent<CommonDialogPanel>().AddMsg(_msg);
			}

			return _msg.tcsYesOrNo.Task;
		}

	}
}
