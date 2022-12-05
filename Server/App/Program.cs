using System;
using System.Threading;
using ETModel;
using NLog;

namespace App
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			// 异步方法全部会回掉到主线程
			SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);
			
			try
			{			
				Game.EventSystem.Add(DLLType.Model, typeof(Game).Assembly);
				Game.EventSystem.Add(DLLType.Hotfix, DllHelper.GetHotfixAssembly());

				Options options = Game.Scene.AddComponent<OptionComponent, string[]>(args).Options;
				StartConfig startConfig = Game.Scene.AddComponent<StartConfigComponent, string, int>(options.Config, options.AppId).StartConfig;

				if (!options.AppType.Is(startConfig.AppType))
				{
					Log.Error("命令行参数apptype与配置不一致");
					return;
				}

				IdGenerater.AppId = options.AppId;

				LogManager.Configuration.Variables["appType"] = $"{startConfig.AppType}";
				LogManager.Configuration.Variables["appId"] = $"{startConfig.AppId}";
				LogManager.Configuration.Variables["appTypeFormat"] = $"{startConfig.AppType, -8}";
				LogManager.Configuration.Variables["appIdFormat"] = $"{startConfig.AppId:0000}";

				Log.Info($"server start........................ {startConfig.AppId} {startConfig.AppType}");

				Game.EventSystem.Run(EventIdType.ServerInit);

				while (true)
				{
					try
					{
						Thread.Sleep(1);
						OneThreadSynchronizationContext.Instance.Update();
						Game.EventSystem.Update();
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
