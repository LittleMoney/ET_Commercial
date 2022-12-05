using System;
using UnityEngine;
using ETModel;

namespace ETHotfix
{
	public static partial class ConfigHelper
	{
		public static T ToObject<T>(string str)
		{
			return JsonHelper.FromJson<T>(str);
		}
	}
}