using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace ETModel
{
	public static partial class UIType
	{
		public const string CommonLoadingPanel = "Common/Panels/CommonLoadingPanel";
	}

	public class CommonLoadingPanel : ComponentI,IUICycle
    {
		#region RC 
		public UI ui;
		public Image imgBg;
		public Image imgIcon;
		public TMP_Text txtText;
		public Slider sldProgress;
		#endregion

		public void OnStart() { }
		public void OnShow() { }
		public void OnFocus() { }
		public void OnHide() { }
        public void OnPause() { }
		
	}

    [ObjectSystem]
	public class CommonLoadingPanelAwakeSystem : AwakeSystem<CommonLoadingPanel>
	{
		public override void Awake(CommonLoadingPanel self)
		{
			#region RC 
			self.ui = self.Entity as UI;
			self.imgBg = (self.ui.g_uiBehaviour["imgBg"] as GameObject).GetComponent<Image>();
			self.imgIcon = (self.ui.g_uiBehaviour["imgIcon"] as GameObject).GetComponent<Image>();
			self.txtText = (self.ui.g_uiBehaviour["txtText"] as GameObject).GetComponent<TMP_Text>();
			self.sldProgress = (self.ui.g_uiBehaviour["sldProgress"] as GameObject).GetComponent<Slider>();
			#endregion

			self.Init();
		}
	}

	[ObjectSystem]
	public class CommonLoadingPanelDestroySystem : DestroySystem<CommonLoadingPanel>
	{
		public override void Destroy(CommonLoadingPanel self)
		{
			BroadcastComponent.Instance.g_default.RemoveListener(BroadcastId.ProgressMessage);
		}
	}

	public static class CommonLoadingSystem
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		public static void Init(this CommonLoadingPanel self)
		{
			BroadcastComponent.Instance.g_default.AddListener<int, string>(BroadcastId.ProgressMessage, self, (scope, a, b) =>
			{
				(scope as CommonLoadingPanel).SetProgress(a, b);
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static void SetProgress(this CommonLoadingPanel self, int progress,string text)
		{
			self.txtText.text = text;
			self.sldProgress.normalizedValue = (float)progress/100.0f;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static void SetIcon(this CommonLoadingPanel self, Sprite sprite)
		{
			self.imgIcon.sprite = sprite;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="sprite"></param>
		/// <returns></returns>
		public static void SetBG(this CommonLoadingPanel self, Sprite sprite)
		{
			self.imgBg.sprite = sprite;
		}
	}
}
