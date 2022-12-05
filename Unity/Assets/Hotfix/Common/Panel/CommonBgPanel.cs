using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using ETModel;
using TMPro;

namespace ETHotfix
{
	public static partial class UIType
	{
		public const string CommonBgPanel = "Common/Panels/CommonBgPanel";
	}

	public class CommonBgPanel : ComponentI, IUICycle
	{
		#region RC 
		public UI ui;
		public Image imgBg;
		#endregion

		public void OnStart() { }
		public void OnFocus() { }
		public void OnHide() { }
		public void OnPause() { }
		public void OnShow() { }
	}

	[ObjectSystem]
	public class CommonBgPanelAwakeSystem : AwakeSystem<CommonBgPanel>
	{
		public override void Awake(CommonBgPanel self)
		{
			#region RC 
			self.ui = self.Entity as UI;
			self.imgBg = (self.ui.g_uiBehaviour["imgBg"] as GameObject).GetComponent<Image>();
			#endregion

			self.Init();
		}
	}

	public static class CommonBgPanelSystem
	{
		public static void Init(this CommonBgPanel self)
		{

		}
	}
}