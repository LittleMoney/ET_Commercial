using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{ 
	public class  UILayer
	{

		public UILayerType layerType;

		/// <summary>
		/// 组的根
		/// </summary>
		public GameObject rootGameObject;

		/// <summary>
		/// 排序基础
		/// </summary>
		public int sortOrder;

		/// <summary>
		/// 排序间距
		/// </summary>
		public int intevalSortOrder;

		/// <summary>
		/// 当前正在被组维护的ui队列，不包含准备销毁或已经隐藏的
		/// </summary>
		public List<UI> showQueue;

		/// <summary>
		/// 
		/// </summary>
		public List<int> sortOrderList;

	}
}
