using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
    public class BroadcastChannel : Component
    {
        public Dictionary<string, Broadcast> allBroadcasts = new Dictionary<string, Broadcast>();

		public readonly Queue<Broadcast> idelBroadcasts = new Queue<Broadcast>();

		public int idelMaxCount = 100;
    }

	[ObjectSystem]
	public class BroadcastChannelDestroySystem : DestroySystem<BroadcastChannel>
	{
        public override void Destroy(BroadcastChannel self)
        {
			self.RemoveListenerAll();
			self.idelBroadcasts.Clear();
		}
    }


	public static class BroadcastChannelSystem
	{
		/// <summary>
		/// 添加动态事件处理器 （可事件中调用）
		/// </summary>
		/// <param name="type"></param>
		/// <param name="scope"></param>
		/// <param name="action"></param>
		public static void AddListener(this BroadcastChannel self, string broadcastId, object scope, Action<object> action)
		{
			Broadcast _broadcast = null;

			if (!self.allBroadcasts.TryGetValue(broadcastId, out _broadcast))
			{
				if (self.idelBroadcasts.Count > 0)
				{
					_broadcast = self.idelBroadcasts.Dequeue() as Broadcast;
				}
				else
				{
					_broadcast = new Broadcast();
				}

				self.allBroadcasts.Add(broadcastId, _broadcast);

			}

			_broadcast.AddListener(scope, action);
		}

		/// <summary>
		/// 添加动态处理器 （可事件中调用）
		/// </summary>
		/// <typeparam name="A"></typeparam>
		/// <param name="type"></param>
		/// <param name="scope"></param>
		/// <param name="action"></param>
		public static void AddListener<A>(this BroadcastChannel self, string broadcastId, object scope, Action<object, A> action)
		{
			Broadcast _broadcast = null;

			if (!self.allBroadcasts.TryGetValue(broadcastId, out _broadcast))
			{
				if (self.idelBroadcasts.Count > 0)
				{
					_broadcast = self.idelBroadcasts.Dequeue() as Broadcast;
				}
				else
				{
					_broadcast = new Broadcast();
				}

				self.allBroadcasts.Add(broadcastId, _broadcast);

			}

			_broadcast.AddListener(scope, action);
		}

		/// <summary>
		/// 添加动态事件处理器 （可事件中调用）
		/// </summary>
		/// <typeparam name="A"></typeparam>
		/// <typeparam name="B"></typeparam>
		/// <param name="type"></param>
		/// <param name="scope"></param>
		/// <param name="action"></param>
		public static void AddListener<A, B>(this BroadcastChannel self, string broadcastId, object scope, Action<object, A, B> action)
		{
			Broadcast _broadcast = null;

			if (!self.allBroadcasts.TryGetValue(broadcastId, out _broadcast))
			{
				if (self.idelBroadcasts.Count > 0)
				{
					_broadcast = self.idelBroadcasts.Dequeue() as Broadcast;
				}
				else
				{
					_broadcast = new Broadcast();
				}

				self.allBroadcasts.Add(broadcastId, _broadcast);

			}

			_broadcast.AddListener(scope, action);
		}

		/// <summary>
		/// 添加动态事件处理器 （可事件中调用）
		/// </summary>
		/// <typeparam name="A"></typeparam>
		/// <typeparam name="B"></typeparam>
		/// <param name="type"></param>
		/// <param name="scope"></param>
		/// <param name="action"></param>
		public static void AddListener<A, B, C>(this BroadcastChannel self, string broadcastId, object scope, Action<object, A, B, C> action)
		{
			Broadcast _broadcast = null;

			if (!self.allBroadcasts.TryGetValue(broadcastId, out _broadcast))
			{
				if (self.idelBroadcasts.Count > 0)
				{
					_broadcast = self.idelBroadcasts.Dequeue() as Broadcast;
				}
				else
				{
					_broadcast = new Broadcast();
				}

				self.allBroadcasts.Add(broadcastId, _broadcast);

			}

			_broadcast.AddListener(scope, action);
		}

		/// <summary>
		/// 移除动态事件处理器 （可事件中调用）
		/// </summary>
		/// <param name="type"></param>
		/// <param name="scope"></param>
		public static void RemoveListener(this BroadcastChannel self, string broadcastId, object scope, object action = null)
		{
			if (self.allBroadcasts.TryGetValue(broadcastId, out Broadcast _broadcast))
			{
				_broadcast.RemoveListener(scope, action);

				if (!_broadcast.HasListener())
				{
					if (self.idelBroadcasts.Count < self.idelMaxCount)
					{
						self.idelBroadcasts.Enqueue(_broadcast);

					}
				}
			}
		}

		/// <summary>
		/// 移除动态事件处理器 （可事件中调用）
		/// </summary>
		/// <param name="type"></param>
		/// <param name="scope"></param>
		public static void RemoveListener(this BroadcastChannel self, string broadcastId)
		{
			if (self.allBroadcasts.TryGetValue(broadcastId, out Broadcast _broadcast))
			{
				_broadcast.RemoveAllListener();

				if (self.idelBroadcasts.Count < self.idelMaxCount)
				{
					self.idelBroadcasts.Enqueue(_broadcast);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		public static void RemoveListenerAll(this BroadcastChannel self)
		{
			foreach(KeyValuePair<string,Broadcast> kv in self.allBroadcasts)
            {
				kv.Value.RemoveAllListener();
			}
			self.allBroadcasts.Clear();
		}

		/// <summary>
		/// 设置空闲的最大数量
		/// </summary>
		/// <param name="maxCount"></param>
		public static void SetIdelMaxCount(this BroadcastChannel self, int maxCount)
		{
			self.idelMaxCount = maxCount;
			while (self.idelBroadcasts.Count > maxCount)
			{
				self.idelBroadcasts.Dequeue();
			}
		}



		public static void Run(this BroadcastChannel self, string broadcastId)
		{
			if (self.allBroadcasts.TryGetValue(broadcastId, out Broadcast _broadcast))
			{
				try
				{
					_broadcast.Run();
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static void Run<A>(this BroadcastChannel self, string broadcastId, A a)
		{
			if (self.allBroadcasts.TryGetValue(broadcastId, out Broadcast _broadcast))
			{
				try
				{
					_broadcast.Run(a);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static void Run<A, B>(this BroadcastChannel self, string broadcastId, A a, B b)
		{
			if (self.allBroadcasts.TryGetValue(broadcastId, out Broadcast _broadcast))
			{
				try
				{
					_broadcast.Run(a, b);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static void Run<A, B, C>(this BroadcastChannel self, string broadcastId, A a, B b, C c)
		{
			if (self.allBroadcasts.TryGetValue(broadcastId, out Broadcast _broadcast))
			{
				try
				{
					_broadcast.Run(a, b, c);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

	}
}
