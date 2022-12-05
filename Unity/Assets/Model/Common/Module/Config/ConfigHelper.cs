using System;
using UnityEngine;

namespace ETModel
{
	public static partial class ConfigHelper
	{
		public static T ToObject<T>(string str)
		{
			return JsonHelper.FromJson<T>(str);
		}
	}
}