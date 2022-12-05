using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
	public static class LayerNames
	{
		/// <summary>
		/// UI层
		/// </summary>
		public const string UI = "UI";

		/// <summary>
		/// 游戏单位层
		/// </summary>
		public const string UNIT = "Unit";

		/// <summary>
		/// 地形层
		/// </summary>
		public const string MAP = "Map";

		/// <summary>
		/// 默认层
		/// </summary>
		public const string DEFAULT = "Default";

		/// <summary>
		/// 
		/// </summary>
		public const string HIDDEN = "Hidden";

		/// <summary>
		/// 通过Layers名字得到对应层
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static int GetLayerInt(string name)
		{
			return LayerMask.NameToLayer(name);
		}

		public static string GetLayerStr(int name)
		{
			return LayerMask.LayerToName(name);
		}
	}
}
