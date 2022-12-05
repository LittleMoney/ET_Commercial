using System;
using System.Collections.Generic;
using System.Text;
using ETModel;

namespace ETHotfix
{
    [Event(EventIdType.ServerInit)]
    public class ServerInitEventHandler : AEvent
    {
		public override void Run()
		{
			OnProcess().Coroutine();
		}

		public async ETTask OnProcess()
		{ 
			StartConfig startConfig = Game.Scene.GetComponent<StartConfigComponent>().StartConfig;

			// 根据不同的AppType添加不同的组件
			OuterConfig outerConfig = startConfig.GetComponent<OuterConfig>();
			InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
			ClientConfig clientConfig = startConfig.GetComponent<ClientConfig>();

			Game.Scene.AddComponent<TimerComponent>();
			Game.Scene.AddComponent<OpcodeTypeComponent>();
			Game.Scene.AddComponent<MessageDispatcherComponent>();



			switch (startConfig.AppType)
			{
				case AppType.Manager:
					Game.Scene.AddComponent<CoroutineLockComponent>();
					Game.Scene.AddComponent<AppManagerComponent>();
					Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
					Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
					break;
				case AppType.Realm:
					Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
					Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);

					Game.Scene.AddComponent<ConfigComponent>();
					Game.Scene.AddComponent<SQLComponent>();
					Game.Scene.AddComponent<ConsoleComponent>();
					break;
				case AppType.Gate:
					Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
					Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address).MessageDispatcher = new ETHotfix.LobbyOuterMessageDispatcher();

					Game.Scene.AddComponent<MailboxDispatcherComponent>();
					Game.Scene.AddComponent<ActorMessageDispatcherComponent>();
					Game.Scene.AddComponent<ActorMessageSenderComponent>();
					Game.Scene.AddComponent<ActorLocationSenderComponent>();
					Game.Scene.AddComponent<CoroutineLockComponent>();
					Game.Scene.AddComponent<LocationProxyComponent>();
					Game.Scene.AddComponent<ConsoleComponent>();

					Game.Scene.AddComponent<ConfigComponent>();
					Game.Scene.AddComponent<PlayerComponent>();
					break;
				case AppType.Centor:
					Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);

					Game.Scene.AddComponent<MailboxDispatcherComponent>();
					Game.Scene.AddComponent<ActorMessageDispatcherComponent>();
					Game.Scene.AddComponent<ActorMessageSenderComponent>();
					Game.Scene.AddComponent<ActorLocationSenderComponent>();
					Game.Scene.AddComponent<CoroutineLockComponent>();
					Game.Scene.AddComponent<LocationProxyComponent>();
					Game.Scene.AddComponent<ConsoleComponent>();

					Game.Scene.AddComponent<ConfigComponent>();
					Game.Scene.AddComponent<SQLComponent>();
					Game.Scene.AddComponent<UserComponent>();
					Game.Scene.AddComponent<ClubComponent>();
					await	Game.Scene.GetComponent<ClubComponent>().LoadClubDataFormDB();

					break;
				case AppType.Location:
					Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
					Game.Scene.AddComponent<LocationComponent>();
					Game.Scene.AddComponent<CoroutineLockComponent>();
					break;
				case AppType.Map:
					Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);

					Game.Scene.AddComponent<MailboxDispatcherComponent>();
					Game.Scene.AddComponent<ActorMessageDispatcherComponent>();
					Game.Scene.AddComponent<ActorMessageSenderComponent>();
					Game.Scene.AddComponent<ActorLocationSenderComponent>();
					Game.Scene.AddComponent<CoroutineLockComponent>();
					Game.Scene.AddComponent<LocationProxyComponent>();
					Game.Scene.AddComponent<ConsoleComponent>();
					break;
				case AppType.AllServer:

					Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
					Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address).MessageDispatcher = new ETHotfix.LobbyOuterMessageDispatcher();

					Game.Scene.AddComponent<MailboxDispatcherComponent>();
					Game.Scene.AddComponent<ActorMessageDispatcherComponent>();
					Game.Scene.AddComponent<ActorMessageSenderComponent>();
					Game.Scene.AddComponent<ActorLocationSenderComponent>();
					Game.Scene.AddComponent<CoroutineLockComponent>();
					Game.Scene.AddComponent<AppManagerComponent>();
					Game.Scene.AddComponent<LocationProxyComponent>();
					Game.Scene.AddComponent<ConsoleComponent>();

					Game.Scene.AddComponent<ConfigComponent>();
					Game.Scene.AddComponent<PlayerComponent>();
					Game.Scene.AddComponent<SQLComponent>();

					Game.Scene.AddComponent<UserComponent>();
					Game.Scene.AddComponent<ClubComponent>();
					await Game.Scene.GetComponent<ClubComponent>().LoadClubDataFormDB();

					break;
				case AppType.Benchmark:
					Game.Scene.AddComponent<CoroutineLockComponent>();
					Game.Scene.AddComponent<NetOuterComponent>();
					Game.Scene.AddComponent<BenchmarkComponent, string>(clientConfig.Address);
					break;
				case AppType.BenchmarkWebsocketServer:
					Game.Scene.AddComponent<CoroutineLockComponent>();
					Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
					break;
				case AppType.BenchmarkWebsocketClient:
					Game.Scene.AddComponent<CoroutineLockComponent>();
					Game.Scene.AddComponent<NetOuterComponent>();
					Game.Scene.AddComponent<WebSocketBenchmarkComponent, string>(clientConfig.Address);
					break;
				default:
					throw new Exception($"命令行参数没有设置正确的AppType: {startConfig.AppType}");
			}
		}
    }


}

