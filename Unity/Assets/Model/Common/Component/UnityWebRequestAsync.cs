using System;
using UnityEngine.Networking;

namespace ETModel
{


	[ObjectSystem]
	public class UnityWebRequestUpdateSystem : UpdateSystem<UnityWebRequestAsync>
	{
		public override void Update(UnityWebRequestAsync self)
		{
			self.Update();
		}
	}
	
	public class UnityWebRequestAsync : Component
	{
		public Action<UnityWebRequestAsync> progressCallback;

		public class AcceptAllCertificate: CertificateHandler
		{
			protected override bool ValidateCertificate(byte[] certificateData)
			{
				return true;
			}
		}
		
		public static AcceptAllCertificate certificateHandler = new AcceptAllCertificate();
		
		public UnityWebRequest Request;

		public bool isCancel;

		public ETTaskCompletionSource tcs;
		
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			this.Request?.Dispose();
			this.Request = null;
			this.isCancel = false;
		}

		public void Update()
		{
			if (this.isCancel)
			{
				this.progressCallback?.Invoke(this);
				this.progressCallback = null;
				this.tcs.SetException(new Exception($"request error: {this.Request.error}"));
				return;
			}
			
			if (!this.Request.isDone)
			{
				this.progressCallback?.Invoke(this);
				return;
			}

			if (!string.IsNullOrEmpty(this.Request.error))
			{
				this.progressCallback?.Invoke(this);
				this.progressCallback = null;
				this.tcs.SetException(new Exception($"request error: {this.Request.error}"));
				return;
			}

			this.progressCallback?.Invoke(this);
			this.progressCallback = null;
			this.tcs.SetResult();
		}

		public ETTask DownloadAsync(string url, Action<UnityWebRequestAsync> progressCallback=null)
		{
			this.progressCallback = progressCallback;

			this.tcs = new ETTaskCompletionSource();
			
			url = url.Replace(" ", "%20");
			this.Request = UnityWebRequest.Get(url);
			this.Request.certificateHandler = certificateHandler;
			this.Request.SendWebRequest();
			
			return this.tcs.Task;
		}

	}
}
