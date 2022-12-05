using System.Collections.Generic;

namespace ETModel
{
	/// <summary>
	/// 登录token管理器
	/// </summary>
	public class SessionTokenComponent : Component
	{
		public readonly Dictionary<string, LogonInfo> sessionKey = new Dictionary<string, LogonInfo>();

		/// <summary>
		/// 添加一个登录信息，30秒后自动销毁
		/// </summary>
		/// <param name="logonInfo"></param>
		public void Add(LogonInfo logonInfo)
		{
			this.sessionKey.Add(logonInfo.loginKey, logonInfo);
			this.TimeoutRemoveKey(logonInfo.loginKey);
		}

		/// <summary>
		/// 获取并移除一个登录信息实例
		/// </summary>
		/// <param name="loginKey"></param>
		/// <returns></returns>
		public LogonInfo Get(string logonKey)
		{
			LogonInfo _info = null;
			this.sessionKey.TryGetValue(logonKey, out _info);
			if(_info!=null)
            {
				sessionKey.Remove(logonKey);
			}
			return _info;
		}


		/// <summary>
		/// 等待超时30秒后移除
		/// </summary>
		/// <param name="logonKey"></param>
		private async void TimeoutRemoveKey(string logonKey)
		{
			await Game.Scene.GetComponent<TimerComponent>().WaitAsync(30000);
			LogonInfo _info = null;
			this.sessionKey.TryGetValue(logonKey, out _info);
			if (_info != null)
			{
				sessionKey.Remove(logonKey);
				_info.Dispose();
			}
		}
	}
}