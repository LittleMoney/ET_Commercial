using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using TMPro;

namespace ETModel
{
	public static partial class UIType
	{
		public const string CommonDialogPanel = "Common/Panels/CommonDialogPanel";
	}

	public enum CommonDialogType
    {
        Tip,
        OkOrCanel,
        YesOrNo
    }

	public class CommonDialogMsg
    {
		public string title;
		public string text;
		public CommonDialogType dialogType;
		public Action actionSure;
		public Action<bool> actionYesOrNo;
		public ETTaskCompletionSource tcsSure;
		public ETTaskCompletionSource<bool> tcsYesOrNo;
	}

	public class CommonDialogPanel : ComponentI,IUICycle
    {
		#region RC 
		public UI ui;
		public TMP_Text txtTitle;
		public TMP_Text txtText;
		public Button btnYes;
		public Button btnNo;
		#endregion

		public Queue<CommonDialogMsg> msgQueue;
		public CommonDialogMsg msg;

		public void OnStart() { }
		public void OnShow() { }
		public void OnFocus() { }
		public void OnHide() {  }
		public void OnPause() { }

	}

    [ObjectSystem]
    public class CommonDialogPanelAwakeSystem : AwakeSystem<CommonDialogPanel>
    {
        public override void Awake(CommonDialogPanel self)
        {
			#region RC 
			self.ui = self.Entity as UI;
			self.txtTitle = (self.ui.g_uiBehaviour["txtTitle"] as GameObject).GetComponent<TMP_Text>();
			self.txtText = (self.ui.g_uiBehaviour["txtText"] as GameObject).GetComponent<TMP_Text>();
			self.btnYes = (self.ui.g_uiBehaviour["btnYes"] as GameObject).GetComponent<Button>();
			self.btnNo = (self.ui.g_uiBehaviour["btnNo"] as GameObject).GetComponent<Button>();
			#endregion

			self.Init();
		}
    }

	[ObjectSystem]
	public class CommonDialogPanelDestroySystem : DestroySystem<CommonDialogPanel>
	{
		public override void Destroy(CommonDialogPanel self)
		{
			self.btnYes.onClick.RemoveAllListeners();
			self.btnNo.onClick.RemoveAllListeners();

			CommonDialogMsg _msg= self.msg;
			if (_msg != null)
            {
				if (_msg.dialogType == CommonDialogType.Tip)
				{
					_msg.actionSure?.Invoke();
					_msg.tcsSure?.SetResult();
				}
				else if (_msg.dialogType == CommonDialogType.OkOrCanel || _msg.dialogType == CommonDialogType.YesOrNo)
				{
					_msg.actionYesOrNo?.Invoke(false);
					_msg.tcsYesOrNo?.SetResult(false);
				}
			}

			if(self.msgQueue!=null&& self.msgQueue.Count>0)
            {
				while(self.msgQueue.Count>0)
                {
					_msg = self.msgQueue.Dequeue();

					if (_msg.dialogType == CommonDialogType.Tip)
					{
						_msg.actionSure?.Invoke();
						_msg.tcsSure?.SetResult();
					}
					else if (_msg.dialogType == CommonDialogType.OkOrCanel || _msg.dialogType == CommonDialogType.YesOrNo)
					{
						_msg.actionYesOrNo?.Invoke(false);
						_msg.tcsYesOrNo?.SetResult(false);
					}
				}
            }

			_msg = null;

		}
    }

	public static class CommonDialogPanelSystem
	{
		public static void Init(this CommonDialogPanel self)
		{
			self.btnYes.onClick.AddListener(() =>
			{
				CommonDialogMsg _msg0 = self.msg;
				UIComponent.Instance.Hide(self.ui);
				self.TryClose();

				if ( _msg0.dialogType == CommonDialogType.Tip)
                {
					_msg0.actionSure?.Invoke();
					_msg0.tcsSure?.SetResult();
				}
				else if (_msg0.dialogType == CommonDialogType.OkOrCanel || _msg0.dialogType == CommonDialogType.YesOrNo)
				{
					_msg0.actionYesOrNo?.Invoke(true);
					_msg0.tcsYesOrNo?.SetResult(true);
				}
				else
				{
					throw new Exception("错误的对话框类型");
				}
				

			});

			self.btnNo.onClick.AddListener(() =>
			{
				CommonDialogMsg _msg1 = self.msg;
				UIComponent.Instance.Hide(self.ui);
				self.TryClose();

				if (_msg1.dialogType == CommonDialogType.OkOrCanel || _msg1.dialogType == CommonDialogType.YesOrNo)
				{
					_msg1.actionYesOrNo?.Invoke(false);
					_msg1.tcsYesOrNo?.SetResult(false);
				}
				else
				{
					throw new Exception("错误的对话框类型");
				}

				
			});

			CommonDialogMsg _msg= self.ui.g_data as CommonDialogMsg;
			if(_msg!=null) self.SetMsg(_msg);
		}

		public static void SetMsg(this CommonDialogPanel self, CommonDialogMsg msg)
		{
			self.msg = msg;
			self.txtTitle.text = self.msg.title;
			self.txtText.text = self.msg.text;
			UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(self.txtText.rectTransform);

			if (self.txtText.rectTransform.sizeDelta.x + 200 > UnityEngine.Screen.currentResolution.width)
			{
				self.txtText.text = $"<size=28>{self.msg.text}</size>";
				self.txtText.gameObject.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
				self.txtText.rectTransform.sizeDelta = new Vector2(UnityEngine.Screen.currentResolution.width - 200, self.txtText.rectTransform.sizeDelta.y);
				UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(self.txtText.rectTransform);
			}


			switch (self.msg.dialogType)
			{
				case CommonDialogType.Tip:
					self.btnYes.gameObject.SetActive(true);
					self.btnYes.transform.GetChild(0).GetComponent<TMP_Text>().text = LanguageHelper.GetStatic("ok");
					self.btnNo.gameObject.SetActive(false);
					break;
				case CommonDialogType.OkOrCanel:
					self.btnYes.gameObject.SetActive(false);
					self.btnYes.transform.GetChild(0).GetComponent<TMP_Text>().text = LanguageHelper.GetStatic("ok");
					self.btnNo.gameObject.SetActive(false);
					self.btnNo.transform.GetChild(0).GetComponent<TMP_Text>().text = LanguageHelper.GetStatic("cancel");
					break;
				case CommonDialogType.YesOrNo:
					self.btnYes.gameObject.SetActive(true);
					self.btnYes.transform.GetChild(0).GetComponent<TMP_Text>().text = LanguageHelper.GetStatic("yes");
					self.btnNo.gameObject.SetActive(true);
					self.btnNo.transform.GetChild(0).GetComponent<TMP_Text>().text = LanguageHelper.GetStatic("no");
					break;
			}
		}

		public static void AddMsg(this CommonDialogPanel self, CommonDialogMsg msg)
        {
			if (self.msgQueue == null) self.msgQueue = new Queue<CommonDialogMsg>();
			self.msgQueue.Enqueue(msg);
        }

		public static void TryClose(this CommonDialogPanel self)
        {
			if(self.msgQueue!=null && self.msgQueue.Count>0)
            {
				self.msg = self.msgQueue.Dequeue();
				self.SetMsg(self.msg);
				UIComponent.Instance.Show(self.ui);
			}
            else
            {
				self.msg = null;
				UIComponent.Instance.Close(self.ui);
            }
        }


	}

}
