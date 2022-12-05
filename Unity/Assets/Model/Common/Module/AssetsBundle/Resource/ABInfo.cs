using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ETModel
{
	public class ABInfo : Component
	{
		public string Name;
		public AssetBundle AssetBundle;
		public UnityEngine.Object MainAsset;
		public Type MainAssetType;

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			//Log.Debug($"desdroy assetbundle: {this.Name}");

			if (this.AssetBundle != null)
			{
				this.AssetBundle.Unload(true);
			}

			this.Name = "";
		}
	}

}
