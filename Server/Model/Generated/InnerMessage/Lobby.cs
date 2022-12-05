using ETModel;
using System.Collections.Generic;
namespace ETModel
{
//realm-gate //////////////////////////////////////////////////////////////
//验证服向网关服请求秘钥
	[Message(InnerOpcode.R2G_GetLoginKey)]
	public partial class R2G_GetLoginKey: IRequest
	{
		public int RpcId { get; set; }

		public int UserId { get; set; }

		public string IP { get; set; }

		public string MachineSerial { get; set; }

	}

	[Message(InnerOpcode.G2R_GetLoginKey)]
	public partial class G2R_GetLoginKey: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public string LoginKey { get; set; }

	}

//gite-center/////////////////////////////////////////////////////////
//网关通知中心服务器用户上线
	[Message(InnerOpcode.G2SC_UserEntry)]
	public partial class G2SC_UserEntry: IRequest
	{
		public int RpcId { get; set; }

		public int UserId { get; set; }

		public long GateActorId { get; set; }

		public string IpAddress { get; set; }

		public string MachineCode { get; set; }

	}

	[Message(InnerOpcode.SC2G_UserEntry)]
	public partial class SC2G_UserEntry: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long UserActorId { get; set; }

	}

//网关通知中心服务器用户离线
	[Message(InnerOpcode.Actor_UserOffline)]
	public partial class Actor_UserOffline: IActorMessage
	{
		public long ActorId { get; set; }

	}

//center-gate/////////////////////////////////////////////////////////
//中心服务器通知网关被挤压号
	[Message(InnerOpcode.Actor_ForceOffline)]
	public partial class Actor_ForceOffline: IActorMessage
	{
		public long ActorId { get; set; }

	}

//centor-map //////////////////////////////////////////////////////////////////////////
//中心通知游戏服启动游戏
	[Message(InnerOpcode.SC2M_BoostGame)]
	public partial class SC2M_BoostGame: IRequest
	{
		public int RpcId { get; set; }

		public int ClubId { get; set; }

		public int TableId { get; set; }

		public List<int> UserIds = new List<int>();

		public List<long> GateActorIds = new List<long>();

		public List<long> Scores = new List<long>();

	}

	[Message(InnerOpcode.M2SC_BoostGame)]
	public partial class M2SC_BoostGame: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long GameTableActorId { get; set; }

	}

//中心通知游戏服开始游戏
	[Message(InnerOpcode.Actor_StartGameRequest)]
	public partial class Actor_StartGameRequest: IActorRequest
	{
		public int RpcId { get; set; }

		public long ActorId { get; set; }

		public List<int> GameKindId = new List<int>();

		public List<int> UserIds = new List<int>();

		public List<long> GateActorIds = new List<long>();

		public List<long> Scores = new List<long>();

	}

	[Message(InnerOpcode.Actor_StartGameResponse)]
	public partial class Actor_StartGameResponse: IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

//中心通知游戏服终止游戏
	[Message(InnerOpcode.Actor_HaltGame)]
	public partial class Actor_HaltGame: IActorMessage
	{
		public long ActorId { get; set; }

	}

//中心通知游戏服用户离线
	[Message(InnerOpcode.Actor_GameUserOffline)]
	public partial class Actor_GameUserOffline: IActorMessage
	{
		public long ActorId { get; set; }

		public int ChairId { get; set; }

		public int UserId { get; set; }

	}

//中心通知游戏服用户重连
	[Message(InnerOpcode.Actor_UserReconnect)]
	public partial class Actor_UserReconnect: IActorMessage
	{
		public long ActorId { get; set; }

		public int ChairId { get; set; }

		public int UserId { get; set; }

		public long GateActorId { get; set; }

	}

//中心通知游戏服用户被替换
	[Message(InnerOpcode.Actor_UserReplace)]
	public partial class Actor_UserReplace: IActorMessage
	{
		public long ActorId { get; set; }

		public int ChairId { get; set; }

		public int UserId { get; set; }

		public long GateActorId { get; set; }

	}

//中心通知游戏服用户起立
	[Message(InnerOpcode.Actor_UserStandupRequest)]
	public partial class Actor_UserStandupRequest: IActorRequest
	{
		public int RpcId { get; set; }

		public long ActorId { get; set; }

		public int ChairId { get; set; }

		public long UserId { get; set; }

	}

	[Message(InnerOpcode.Actor_UserStandupResponse)]
	public partial class Actor_UserStandupResponse: IActorResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

//中心通知游戏服用户请求拉取游戏信息
	[Message(InnerOpcode.Actor_UserRequestGameInfo)]
	public partial class Actor_UserRequestGameInfo: IActorMessage
	{
		public long ActorId { get; set; }

		public int ChairId { get; set; }

		public int UserId { get; set; }

	}

//map-centor////////////////////////////////////////////////////////////////////////////////////////////////
//游戏通知中心游戏结束
	[Message(InnerOpcode.M2SC_GameEnd)]
	public partial class M2SC_GameEnd: IMessage
	{
		public long GameTableActorId { get; set; }

		public int ClubId { get; set; }

		public int TableId { get; set; }

		public List<long> EndScores = new List<long>();

		public int LeftChairId { get; set; }

		public string GameRecordGUID { get; set; }

	}

//map-gate ///////////////////////////////////////////////////////////////////////////
//游戏通知网关用户进入游戏
	[Message(InnerOpcode.Actor_UserInGame)]
	public partial class Actor_UserInGame: IActorMessage
	{
		public long ActorId { get; set; }

		public long GameActorId { get; set; }

	}

//游戏通知网关用户离开游戏
	[Message(InnerOpcode.Actor_UserOutGame)]
	public partial class Actor_UserOutGame: IActorMessage
	{
		public long ActorId { get; set; }

		public long GameActorId { get; set; }

	}

}
