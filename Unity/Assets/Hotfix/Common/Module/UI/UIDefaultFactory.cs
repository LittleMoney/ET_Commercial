using ETHotfix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ETModel;

namespace ETHotfix
{
    public class UIDefaultFactory : IUIFactory
    {
		/// <summary>
		/// 默认UI工厂
		/// </summary>
		/// <param name="groupName">组名称</param>
		/// <param name="mode">ui的显示模式</param>
		/// <param name="uiType">ui类型
		/// （注意：
		///		1.uitype AB包全名
		///		2.AB包的文件名(不要后缀) 需要和UI预制体资源名(不要后缀)保持一致  例如 UI 对应包中预制体 UI.perfab
		///		3.AB包的文件名(不要后缀) 需要对应资源组件类类名 例如  UI  对应到组件类名 UIComponent</param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public UI Create(string uiType, GameObject uiRoot, object data)
        {
			try
			{
				GameObject _perfab = ResourcesComponent.Instance.LoadAsset(uiType, typeof(GameObject)) as GameObject; 
				GameObject gameObject = UnityEngine.Object.Instantiate(_perfab, uiRoot.transform);
				gameObject.layer = LayerMask.NameToLayer(LayerNames.UI);

				UI ui = ComponentFactory.Create<UI,string, GameObject,object>(uiType, gameObject, data,false);
				Type _type = Type.GetType($"ETHotfix.{uiType.GetPureFileName()}");
				if(_type==null) _type=Type.GetType($"ETHotfix.{uiType.GetPureFileName()}Component");
				object _objComponent = ui.AddComponent(_type);

				return ui;
			}
			catch (Exception e)
			{
				throw new Exception($"open ui fail : {uiType} {e.Message}");
			}
		}

		/// <summary>
		/// 默认UI工厂
		/// </summary>
		/// <param name="groupName">组名称</param>
		/// <param name="mode">ui的显示模式</param>
		/// <param name="uiType">ui类型
		/// （注意：
		///		1.uitype AB包全名
		///		2.AB包的文件名(不要后缀) 需要和UI预制体资源名(不要后缀)保持一致  例如 UI 对应包中预制体 UI.perfab
		///		3.AB包的文件名(不要后缀) 需要对应资源组件类类名 例如  UI  对应到组件类名 UIComponent</param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public async ETTask<object> CreateAsync(string uiType, GameObject uiRoot, object data)
		{
			try
			{
				GameObject _perfab = await ResourcesComponent.Instance.LoadAssetAsync(uiType, typeof(GameObject)) as GameObject;
				GameObject gameObject = UnityEngine.Object.Instantiate(_perfab, uiRoot.transform);
				gameObject.layer = LayerMask.NameToLayer(LayerNames.UI);

				UI ui = ComponentFactory.Create<UI, string, GameObject, object>(uiType, gameObject, data, false);
				Type _type = Type.GetType($"ETHotfix.{uiType.GetPureFileName()}");
				if (_type == null) _type = Type.GetType($"ETHotfix.{uiType.GetPureFileName()}Component");
				object _objComponent = ui.AddComponent(_type);

				return ui;
			}
			catch (Exception e)
			{
				throw new Exception($"open ui fail : {uiType} {e.Message}");
			}
		}

		public void Remove(UI ui)
        {
			//ETModel.Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle(ui.g_uiType);
			ui.Dispose();
		}
    }
}
