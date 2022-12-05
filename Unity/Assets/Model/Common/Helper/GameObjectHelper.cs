using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
	public static partial  class GameObjectHelper
	{
		private static GameObject globalGameObject=null;

		private static MonoBehaviour globalMonoBehaviour=null;

		
		private static Queue<ETTaskCompletionSource> runFrameTaskSource = new System.Collections.Generic.Queue<ETTaskCompletionSource>();
		private static Queue<ETTaskCompletionSource> runFrameTaskSource2 = new System.Collections.Generic.Queue<ETTaskCompletionSource>();
		private static IEnumerator runFrameEnumerator = null;

		public static GameObject GlobalGameObject
		{
            get
            {
				if(globalGameObject==null)
                {
					globalGameObject = GameObject.Find("Global");
					globalMonoBehaviour = globalGameObject.AddComponent<ReferenceCollector>();
					GameObject.DontDestroyOnLoad(globalGameObject);
				}
				return globalGameObject;
            }
		}

		public static MonoBehaviour GlobalMonoBehaviour
		{
			get
			{
				if (globalGameObject == null)
				{
					globalGameObject = new GameObject();
					globalMonoBehaviour = globalGameObject.AddComponent<ReferenceCollector>();
					GameObject.DontDestroyOnLoad(globalGameObject);
				}
				return globalMonoBehaviour;
			}
		}

		public static T Get<T>(this GameObject gameObject, string key) where T : class
		{
			try
			{
				return gameObject.GetComponent<ReferenceCollector>().Get<T>(key);
			}
			catch (Exception e)
			{
				throw new Exception($"获取{gameObject.name}的ReferenceCollector key失败, key: {key}", e);
			}
		}

		/// <summary>
		/// 开启一个携程
		/// </summary>
		/// <param name="enumerator"></param>
		public static void StartCoroutine(IEnumerator enumerator)
        {
			GlobalMonoBehaviour.StartCoroutine(enumerator);
		}

		/// <summary>
		/// 停止一个携程
		/// </summary>
		/// <param name="enumerator"></param>
		public static void StopCoroutine(IEnumerator enumerator)
        {
			GlobalMonoBehaviour.StopCoroutine(enumerator);
        }

		/// <summary>
		/// 停止所有携程
		/// </summary>
		public static void StopAllCoroutines()
        {
			GlobalMonoBehaviour.StopAllCoroutines();
        }

		/// <summary>
		/// 等待到下一帧触发，放心使用修改了ETTaskCompleteSource使其可以重复利用，所以这里的等待会重复利用ETTask不必担心回收问题
		/// 但是不要在 await 之外持有ETTask实例，因为ETTask会在设置完成后立即回收
		/// </summary>
		/// <returns></returns>
		public static ETTask WaitOneFrame()
        {
			ETTaskCompletionSource _ecs = ETTaskCompletionSource.GetFormPool();
			runFrameTaskSource.Enqueue(_ecs);

			if (runFrameEnumerator == null)
			{
				runFrameEnumerator = OnRunFrame();
				StartCoroutine(runFrameEnumerator);
			}
			return _ecs.Task;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="asyncOperation"></param>
		/// <returns></returns>
		public static ETTask WaitAsyncOperation(AsyncOperation asyncOperation)
        {
			if (asyncOperation.isDone)
			{
				return ETTask.CompletedTask;
			}
			else
			{
				ETTaskCompletionSource _ecs = ETTaskCompletionSource.GetFormPool();

				StartCoroutine(OnRunAsyncOperation(asyncOperation, _ecs));

				return _ecs.Task;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static IEnumerator OnRunAsyncOperation(AsyncOperation asyncOperation, ETTaskCompletionSource ecs)
		{
			yield return asyncOperation;
			ecs.SetResult();
			ETTaskCompletionSource.RecycleToPool(ecs);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static IEnumerator OnRunFrame()
        {
			int _idelCount = 0;
			while (_idelCount < 60) //空闲60针退出运行
			{
				yield return null;
				if (runFrameTaskSource.Count == 0)
				{
					_idelCount++;
					continue;
				}
				else
				{
					_idelCount = 0;

					Queue<ETTaskCompletionSource> _runFrameTaskSource = runFrameTaskSource;
					runFrameTaskSource = runFrameTaskSource2;
					runFrameTaskSource2 = _runFrameTaskSource;

					while (_runFrameTaskSource.Count>0)
					{
						ETTaskCompletionSource _ecs = _runFrameTaskSource.Dequeue();
						_ecs.SetResult();
						ETTaskCompletionSource.RecycleToPool(_ecs);
					}
				}
			}
			runFrameEnumerator = null;
		}
	
	}
}