using UnityEngine;

namespace ETModel
{
	public interface IUIFactory
	{
		/// <summary>
		/// 创建UI
		/// </summary>
		/// <param name="groupName"></param>
		/// <param name="mode"></param>
		/// <param name="uiType"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		UI Create(string uiType, GameObject uiRoot, object data);

		/// <summary>
		/// 创建UI
		/// </summary>
		/// <param name="groupName"></param>
		/// <param name="mode"></param>
		/// <param name="uiType"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		ETTask<object> CreateAsync(string uiType, GameObject uiRoot, object data);

		/// <summary>
		/// 移除UI 工厂负责调用Dispose
		/// </summary>
		/// <param name="uiType"></param>
		/// <param name="ui"></param>
		void Remove(UI ui);
	}
}