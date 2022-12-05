using ETModel;

namespace ETHotfix
{
	public class LobbyOuterMessageDispatcher : IMessageDispatcher
	{
		public void Dispatch(Session session, ushort opcode, object message)
		{
			DispatchAsync(session, opcode, message).Coroutine();
		}

		public async ETVoid DispatchAsync(Session session, ushort opcode, object message)
		{
			// 根据消息接口判断是不是Actor消息，不同的接口做不同的处理
			switch (message)
			{
                #region 游戏服消息转发
                case IGameActorRequest gameActorRequest: 
					{
						long _actorId = session.GetComponent<SessionPlayerComponent>().player.gameActorId;
						if (_actorId != 0)
						{
							ActorMessageSender _actorMessageSender = Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(_actorId);

							int rpcId = gameActorRequest.RpcId; // 这里要保存客户端的rpcId
							long instanceId = session.InstanceId;
							IResponse response = await _actorMessageSender.Call(gameActorRequest);
							response.RpcId = rpcId;

							// session可能已经断开了，所以这里需要判断
							if (session.InstanceId == instanceId)
							{
								session.Reply(response);
							}
						}
                        else
                        {
							int rpcId = gameActorRequest.RpcId; // 这里要保存客户端的rpcId
							IResponse response = new ActorResponse();
							response.RpcId = rpcId;
							response.Error = ErrorCode.ERR_Exception;
							response.Message = "用户不在游戏中";
						}
						break;
					}
				case IGameActorMessage gameActorMessage:
					{
						long _actorId = session.GetComponent<SessionPlayerComponent>().player.gameActorId;
						if (_actorId != 0)
						{
							ActorMessageSender _actorMessageSender = Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(_actorId);
							_actorMessageSender.Send(gameActorMessage);
						}
						break;
					}
                #endregion

                #region 中心服消息转发
                case ICentorActorRequest centorActorRequest: 
					{
						long _actorId = session.GetComponent<SessionPlayerComponent>().player.userActorId;
						if (_actorId != 0)
						{
							ActorMessageSender _actorMessageSender = Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(_actorId);

							int rpcId = centorActorRequest.RpcId; // 这里要保存客户端的rpcId
							long instanceId = session.InstanceId;
							IResponse response = await _actorMessageSender.Call(centorActorRequest);
							response.RpcId = rpcId;

							// session可能已经断开了，所以这里需要判断
							if (session.InstanceId == instanceId)
							{
								session.Reply(response);
							}
						}
						else
						{
							int rpcId = centorActorRequest.RpcId; // 这里要保存客户端的rpcId
							IResponse response = new ActorResponse();
							response.RpcId = rpcId;
							response.Error = ErrorCode.ERR_Exception;
							response.Message = "用户不在游戏中";
						}
						break;
					}
				case ICentorActorMessage centorActorMessage:
					{
						long _actorId = session.GetComponent<SessionPlayerComponent>().player.userActorId;
						if (_actorId != 0)
						{
							ActorMessageSender _actorMessageSender = Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(_actorId);
							_actorMessageSender.Send(centorActorMessage);
						}
						break;
					}
				#endregion

				case IActorLocationRequest actorRequest:  // 分发IActorRequest消息，目前没有用到，需要的自己添加
					{
						break;
					}
				case IActorLocationMessage actorMessage:  // 分发IActorMessage消息，目前没有用到，需要的自己添加
					{
						break;
					}
				case IActorRequest actorRequest:  // 分发IActorRequest消息，目前没有用到，需要的自己添加
					{
						break;
					}
				case IActorMessage actorMessage:  // 分发IActorMessage消息，目前没有用到，需要的自己添加
					{
						break;
					}
				default:
					{
						// 非Actor消息
						Game.Scene.GetComponent<MessageDispatcherComponent>().Handle(session, new MessageInfo(opcode, message));
						break;
					}
			}
		}
	}
}
