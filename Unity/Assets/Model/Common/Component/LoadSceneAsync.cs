using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ETModel
{
	[ObjectSystem]
	public class LoadSceneAsyncUpdateSystem : UpdateSystem<LoadSceneAsync>
	{
		public override void Update(LoadSceneAsync self)
		{
			if (self.loadAsyncOperation == null) return;

			if (self.loadAsyncOperation.isDone)
			{
				self.tcs.SetResult();
            }
            else
            {
				self.g_progress = 50+(int)self.loadAsyncOperation.progress*50;
				self.progressCallback?.Invoke(self);
			}
		}
	}

	public class LoadSceneAsync: Component
	{
		public AsyncOperation loadAsyncOperation;
		public ETTaskCompletionSource tcs;

		public Action<LoadSceneAsync> progressCallback;
		public int g_progress;
		public bool g_isDone;
		public Exception g_error;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetBundleName"></param>
		/// <param name="mode"></param>
		/// <param name="progressCallback"></param>
		/// <returns></returns>
		public async ETTask Load(string toyDirName,string sceneName, LoadSceneMode mode,Action<LoadSceneAsync> progressCallback=null)
		{
			g_isDone = false;
			g_progress = 0;
			g_error = null;

			try
			{
				await ResourcesComponent.Instance.LoadBundleAsync($"{toyDirName}/Scenes/{sceneName}", (prograss, isDone, abName, error) =>
				{
					g_progress = prograss / 50;
					progressCallback?.Invoke(this);
				});
			}
			catch(Exception error)
            {
				g_isDone = true;
				g_error = error;
				progressCallback?.Invoke(this);
				throw error;
			}

			// 加载map
			this.progressCallback = progressCallback;
			this.tcs = new ETTaskCompletionSource();
			this.loadAsyncOperation = SceneManager.LoadSceneAsync(sceneName, mode);

			try
			{
				await this.tcs.Task;
			}
			catch(Exception error)
            {
				g_isDone = true;
				g_error = error;
				progressCallback?.Invoke(this);
				throw error;
			}

			g_progress = 100;
			g_isDone = true;
			progressCallback?.Invoke(this);
		}

		public async ETTask UnLoad(string sceneName, Action<LoadSceneAsync> progressCallback = null)
		{
			g_isDone = false;
			g_progress = 0;
			g_error = null;


			// 加载map
			this.progressCallback = progressCallback;
			this.tcs = new ETTaskCompletionSource();
			this.loadAsyncOperation = SceneManager.UnloadSceneAsync(sceneName);

			try
			{
				await this.tcs.Task;
			}
			catch (Exception error)
			{
				g_isDone = true;
				g_error = error;
				progressCallback?.Invoke(this);
				throw error;
			}

			g_progress = 100;
			g_isDone = true;
			progressCallback?.Invoke(this);

		}
		/// <summary>
		/// 
		/// </summary>
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			loadAsyncOperation = null;
			tcs = null;
			progressCallback = null;
			g_progress = 0;
			g_isDone = true;
			g_error = null;
		}
	}
}