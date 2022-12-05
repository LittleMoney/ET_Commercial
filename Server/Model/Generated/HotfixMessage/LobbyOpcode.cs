using ETModel;
namespace ETHotfix
{
//client-realm ///////////////////////////////////////////////////////////////////////////////////////////////////
//登录请求
	[Message(HotfixOpcode.C2R_Login)]
	public partial class C2R_Login : IRequest {}

	[Message(HotfixOpcode.R2C_Login)]
	public partial class R2C_Login : IResponse {}

//登录请求
	[Message(HotfixOpcode.C2R_Register)]
	public partial class C2R_Register : IRequest {}

	[Message(HotfixOpcode.R2C_Register)]
	public partial class R2C_Register : IResponse {}

//client-gate /////////////////////////////////////////////////////////////////////////////////////////
//登录网关
	[Message(HotfixOpcode.C2G_LoginGate)]
	public partial class C2G_LoginGate : IRequest {}

	[Message(HotfixOpcode.G2C_LoginGate)]
	public partial class G2C_LoginGate : IResponse {}

	[Message(HotfixOpcode.C2G_Heart)]
	public partial class C2G_Heart : IMessage {}

//data //////////////////////////////////////////////////////////////////////////////////
//亲友圈信息
	[Message(HotfixOpcode.ClubInfo)]
	public partial class ClubInfo {}

//桌子信息
	[Message(HotfixOpcode.TableInfo)]
	public partial class TableInfo {}

//用户信息
	[Message(HotfixOpcode.UserInfo)]
	public partial class UserInfo {}

//centor-client /////////////////////////////////////////////////////////////////////////////////////////
//中心下发用户信息
	[Message(HotfixOpcode.Actor_CurrUserInfo)]
	public partial class Actor_CurrUserInfo : IActorMessage {}

//俱乐部桌子信息列表,如果用户在club中，下发用户信息后接着下发，或在进入俱乐部后下发
	[Message(HotfixOpcode.Actor_TablelInfoList)]
	public partial class Actor_TablelInfoList : IActorMessage {}

//俱乐部进入用户信息列表，在ClubInfo后下发
	[Message(HotfixOpcode.Actor_UserComes)]
	public partial class Actor_UserComes : IActorMessage {}

//增加桌子
	[Message(HotfixOpcode.Actor_TableCreate)]
	public partial class Actor_TableCreate : IActorMessage {}

//删除桌子
	[Message(HotfixOpcode.Actor_TableDestroy)]
	public partial class Actor_TableDestroy : IActorMessage {}

//桌子状态改变
	[Message(HotfixOpcode.Actor_TableStatus)]
	public partial class Actor_TableStatus : IActorMessage {}

//用户状态改变
	[Message(HotfixOpcode.Actor_UserStatus)]
	public partial class Actor_UserStatus : IActorMessage {}

//用户分数改变
	[Message(HotfixOpcode.Actor_UserScore)]
	public partial class Actor_UserScore : IActorMessage {}

//client-centor //////////////////////////////////////////////////////////////////////////////
//申请加入俱乐部
	[Message(HotfixOpcode.Actor_JoinClubRequest)]
	public partial class Actor_JoinClubRequest : ICentorActorRequest {}

	[Message(HotfixOpcode.Actor_JoinClubResponse)]
	public partial class Actor_JoinClubResponse : ICentorActorResponse {}

//申请加入俱乐部
	[Message(HotfixOpcode.Actor_QuitClubRequest)]
	public partial class Actor_QuitClubRequest : ICentorActorRequest {}

	[Message(HotfixOpcode.Actor_QuitClubResponse)]
	public partial class Actor_QuitClubResponse : ICentorActorResponse {}

//客户端进入俱乐部，中心服务器下发俱乐部所有信息，并开始同步状态
	[Message(HotfixOpcode.Actor_InClubRequest)]
	public partial class Actor_InClubRequest : ICentorActorRequest {}

	[Message(HotfixOpcode.Actor_InClubResponse)]
	public partial class Actor_InClubResponse : ICentorActorResponse {}

//客户端推出俱乐部
	[Message(HotfixOpcode.Actor_OutClubRequest)]
	public partial class Actor_OutClubRequest : ICentorActorRequest {}

	[Message(HotfixOpcode.Actor_OutClubResponse)]
	public partial class Actor_OutClubResponse : ICentorActorResponse {}

//客户端请求中心服务器上桌
	[Message(HotfixOpcode.Actor_SitdownRequest)]
	public partial class Actor_SitdownRequest : ICentorActorRequest {}

	[Message(HotfixOpcode.Actor_SitdownResponse)]
	public partial class Actor_SitdownResponse : ICentorActorResponse {}

//用户请求中心服务器离桌
	[Message(HotfixOpcode.Actor_StandupRequest)]
	public partial class Actor_StandupRequest : ICentorActorRequest {}

	[Message(HotfixOpcode.Actor_StandupResponse)]
	public partial class Actor_StandupResponse : ICentorActorResponse {}

//用户请求中心服务服务器准备
	[Message(HotfixOpcode.Actor_ReadyRequest)]
	public partial class Actor_ReadyRequest : ICentorActorRequest {}

	[Message(HotfixOpcode.Actor_ReadyResponse)]
	public partial class Actor_ReadyResponse : ICentorActorResponse {}

//用户请求中心服务器取消准备
	[Message(HotfixOpcode.Actor_UnReadyRequest)]
	public partial class Actor_UnReadyRequest : ICentorActorRequest {}

	[Message(HotfixOpcode.Actor_UnReadyResponse)]
	public partial class Actor_UnReadyResponse : ICentorActorResponse {}

//客户端请求拉取游戏信息
	[Message(HotfixOpcode.Actor_GetGameInfo)]
	public partial class Actor_GetGameInfo : ICentorActorMessage {}

}
namespace ETHotfix
{
	public static partial class HotfixOpcode
	{
		 public const ushort C2R_Login = 10002;
		 public const ushort R2C_Login = 10003;
		 public const ushort C2R_Register = 10004;
		 public const ushort R2C_Register = 10005;
		 public const ushort C2G_LoginGate = 10006;
		 public const ushort G2C_LoginGate = 10007;
		 public const ushort C2G_Heart = 10008;
		 public const ushort ClubInfo = 10009;
		 public const ushort TableInfo = 10010;
		 public const ushort UserInfo = 10011;
		 public const ushort Actor_CurrUserInfo = 10012;
		 public const ushort Actor_TablelInfoList = 10013;
		 public const ushort Actor_UserComes = 10014;
		 public const ushort Actor_TableCreate = 10015;
		 public const ushort Actor_TableDestroy = 10016;
		 public const ushort Actor_TableStatus = 10017;
		 public const ushort Actor_UserStatus = 10018;
		 public const ushort Actor_UserScore = 10019;
		 public const ushort Actor_JoinClubRequest = 10020;
		 public const ushort Actor_JoinClubResponse = 10021;
		 public const ushort Actor_QuitClubRequest = 10022;
		 public const ushort Actor_QuitClubResponse = 10023;
		 public const ushort Actor_InClubRequest = 10024;
		 public const ushort Actor_InClubResponse = 10025;
		 public const ushort Actor_OutClubRequest = 10026;
		 public const ushort Actor_OutClubResponse = 10027;
		 public const ushort Actor_SitdownRequest = 10028;
		 public const ushort Actor_SitdownResponse = 10029;
		 public const ushort Actor_StandupRequest = 10030;
		 public const ushort Actor_StandupResponse = 10031;
		 public const ushort Actor_ReadyRequest = 10032;
		 public const ushort Actor_ReadyResponse = 10033;
		 public const ushort Actor_UnReadyRequest = 10034;
		 public const ushort Actor_UnReadyResponse = 10035;
		 public const ushort Actor_GetGameInfo = 10036;
	}
}
