using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
    public class UnitDefaultFactory:IUnitFactory
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
		public Unit Create(string unitType,long id, GameObject root, object data)
		{
			try
			{
				GameObject _perfab = ResourcesComponent.Instance.LoadAsset(unitType, typeof(GameObject)) as GameObject;
				GameObject gameObject = UnityEngine.Object.Instantiate(_perfab, root.transform);
				gameObject.layer = LayerMask.NameToLayer(LayerNames.UNIT);

				Unit _unit = ComponentFactory.CreateWithId<Unit, string, GameObject, object>(id,unitType, gameObject, data, false);
				Type _type = Type.GetType($"ETModel.{unitType.GetPureFileName()}");
				if (_type == null) _type = Type.GetType($"ETModel.{unitType.GetPureFileName()}Component");
				object _objComponent = _unit.AddComponent(_type);

				return _unit;
			}
			catch (Exception e)
			{
				throw new Exception($"打开unit错误:{unitType} {e.Message}");
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
		public async ETTask<object> CreateAsync(string unitType,long id, GameObject root, object data)
		{
			try
			{
				GameObject _perfab = (await ResourcesComponent.Instance.LoadAssetAsync(unitType, typeof(GameObject))) as GameObject;
				GameObject gameObject = UnityEngine.Object.Instantiate(_perfab, root.transform);
				gameObject.layer = LayerMask.NameToLayer(LayerNames.UNIT);

				Unit _unit = ComponentFactory.CreateWithId<Unit, string, GameObject, object>(id, unitType, gameObject, data, false);
				Type _type = Type.GetType($"ETModel.{unitType.GetPureFileName()}");
				if (_type == null) _type = Type.GetType($"ETModel.{unitType.GetPureFileName()}Component");
				object _objComponent = _unit.AddComponent(_type);

				return _unit;
			}
			catch (Exception e)
			{
				throw new Exception($"打开unit错误:{unitType} {e.Message}");
			}
		}


		public void Remove(Unit unit)
		{
			GameObject.Destroy(unit.g_rootGameObject);
			unit.Dispose();

		}
	}
}
