using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ETModel;

namespace ETHotfix
{
	public interface IUnitFactory
	{
		/// <summary>
		/// 创建UI
		/// </summary>
		/// <param name="groupName"></param>
		/// <param name="mode"></param>
		/// <param name="uiType"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		Unit Create(string unitType,long id, GameObject root, object data);

		/// <summary>
		/// 创建UI
		/// </summary>
		/// <param name="groupName"></param>
		/// <param name="mode"></param>
		/// <param name="uiType"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		ETTask<object> CreateAsync(string uiType, long id, GameObject root, object data);

		/// <summary>
		/// 移除UI 工厂负责调用Dispose
		/// </summary>
		/// <param name="uiType"></param>
		/// <param name="ui"></param>
		void Remove(Unit ui,bool isRemainGameObject);
	}
}
