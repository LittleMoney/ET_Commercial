using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHotfix
{
	/// <summary>
	/// 动态事件
	/// </summary>
	public class Broadcast
	{
		Dictionary<object, HashSet<object>> handlerDict = new Dictionary<object, HashSet<object>>();
		object[] actionList;
		KeyValuePair<object, HashSet<object>>[] handlerList;

		public bool HasListener()
		{
			return handlerDict.Count > 0;
		}

		/// <summary>
		/// 增加监听，如果已经监听则不做处理 （事件调用中也可使用）
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="action"></param>
		public void AddListener(object scope, object action)
		{
			object _tag = scope == null ? action : scope;
			HashSet<object> _actionHashSet = null;

			if (handlerDict.TryGetValue(_tag, out _actionHashSet))
			{
				if (!_actionHashSet.Contains(action))
				{
					_actionHashSet.Add(action);

					if (_actionHashSet.Count > actionList.Length)
					{
						actionList = new object[_actionHashSet.Count];
					}
				}
				if (actionList == null || actionList.Length < _actionHashSet.Count) actionList = new object[_actionHashSet.Count];
			}
			else
			{
				_actionHashSet = new HashSet<object>();
				_actionHashSet.Add(action);
				handlerDict.Add(scope, _actionHashSet);

				if (handlerList == null || handlerList.Length < handlerDict.Count) handlerList = new KeyValuePair<object, HashSet<object>>[handlerDict.Count];
				if (actionList == null) actionList = new object[1];
			}
		}

		/// <summary> 
		/// 移除监听 （事件调用中也可使用）
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="action"></param>
		public void RemoveListener(object scope, object action = null)
		{
			object _tag = scope == null ? action : scope;

			if (handlerDict.TryGetValue(_tag, out HashSet<object> _actionHashSet))
			{
				if (action == null)
				{
					handlerDict.Remove(_tag);
				}
				else
				{
					_actionHashSet.Remove(action);

					if (_actionHashSet.Count == 0)
					{
						handlerDict.Remove(_tag);
					}
				}
			}
		}

		/// <summary>
		/// 移除所有的监听成员
		/// </summary>
		public void RemoveAllListener()
		{
			handlerDict.Clear();
		}

		public void Run()
		{
			int _index = 0;
			object[] _actionList;
			KeyValuePair<object, HashSet<object>>[] _handlerList = handlerList;

			foreach (KeyValuePair<object, HashSet<object>> kv in handlerDict)
			{
				_handlerList[_index++] = kv;
			}

			for (int i = 0; i < _index; i++)
			{
				_actionList = actionList;
				_handlerList[i].Value.CopyTo(_actionList);
				int _aindex = _handlerList[i].Value.Count;

				for (int x = 0; x < _aindex; x++)
				{
					(_actionList[x] as Action<object>).Invoke(_handlerList[i].Key);
					_actionList[x] = null;
				}

				_handlerList[i] = default(KeyValuePair<object, HashSet<object>>);
			}
		}

		public void Run<A>(A a)
		{
			int _index = 0;
			object[] _actionList;
			KeyValuePair<object, HashSet<object>>[] _handlerList = handlerList;

			foreach (KeyValuePair<object, HashSet<object>> kv in handlerDict)
			{
				_handlerList[_index++] = kv;
			}

			for (int i = 0; i < _index; i++)
			{
				_actionList = actionList;
				_handlerList[i].Value.CopyTo(_actionList);
				int _aindex = _handlerList[i].Value.Count;

				for (int x = 0; x < _aindex; x++)
				{
					(_actionList[x] as Action<object, A>).Invoke(_handlerList[i].Key, a);
					_actionList[x] = null;
				}

				_handlerList[i] = default(KeyValuePair<object, HashSet<object>>);
			}
		}

		public void Run<A, B>(A a, B b)
		{
			int _index = 0;
			object[] _actionList;
			KeyValuePair<object, HashSet<object>>[] _handlerList = handlerList;

			foreach (KeyValuePair<object, HashSet<object>> kv in handlerDict)
			{
				_handlerList[_index++] = kv;
			}

			for (int i = 0; i < _index; i++)
			{
				_actionList = actionList;
				_handlerList[i].Value.CopyTo(_actionList);
				int _aindex = _handlerList[i].Value.Count;

				for (int x = 0; x < _aindex; x++)
				{
					(_actionList[x] as Action<object, A, B>).Invoke(_handlerList[i].Key, a, b);
					_actionList[x] = null;
				}

				_handlerList[i] = default(KeyValuePair<object, HashSet<object>>);
			}
		}

		public void Run<A, B, C>(A a, B b, C c)
		{
			int _index = 0;
			object[] _actionList;
			KeyValuePair<object, HashSet<object>>[] _handlerList = handlerList;

			foreach (KeyValuePair<object, HashSet<object>> kv in handlerDict)
			{
				_handlerList[_index++] = kv;
			}

			for (int i = 0; i < _index; i++)
			{
				_actionList = actionList;
				_handlerList[i].Value.CopyTo(_actionList);
				int _aindex = _handlerList[i].Value.Count;

				for (int x = 0; x < _aindex; x++)
				{
					(_actionList[x] as Action<object, A, B, C>).Invoke(_handlerList[i].Key, a, b, c);
					_actionList[x] = null;
				}

				_handlerList[i] = default(KeyValuePair<object, HashSet<object>>);
			}
		}

	}
}
