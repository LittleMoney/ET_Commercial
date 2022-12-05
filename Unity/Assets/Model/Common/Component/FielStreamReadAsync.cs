using System;
using System.IO;
using UnityEngine.Networking;

namespace ETModel
{
	public class FileStreamReadAsyncAsync : Component
	{

		public byte[] datas;

		public float progress;

		public bool isDone;

		public Exception error;

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			datas = null;
			isDone = true;
			error = null;
		}

		public async ETTask ReadAllAsync(string path, Action<FileStreamReadAsyncAsync> progressCallback = null)
		{
			isDone = false;
			if (!File.Exists(path))
			{
				error = new Exception($"没有找到文件 {path} ");
				progressCallback?.Invoke(this);
				throw  error;
			}

			try
			{
				using (FileStream fs = File.Open(path, FileMode.Open))
				{
					datas = new byte[fs.Length];
					int _readCount = await fs.ReadAsync(datas, 0, datas.Length);
					int _offsetIdnex = _readCount;

					while (_offsetIdnex < datas.Length)
					{
						_readCount = await fs.ReadAsync(datas, _offsetIdnex, datas.Length - _offsetIdnex); //默认utf8

						progress = (float)_readCount / (float)datas.Length;
						progressCallback?.Invoke(this);

						_offsetIdnex += _readCount;
					}
					progress = 1;
					isDone = true;
					progressCallback?.Invoke(this);
				}
			}
			catch (Exception e)
			{
				error = e;
				progressCallback?.Invoke(this);
				throw error;
			}
		}
	}
}
