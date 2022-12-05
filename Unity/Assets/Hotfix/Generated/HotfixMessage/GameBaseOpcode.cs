using ETModel;
namespace ETHotfix
{
//请求准备
	[Message(HotfixOpcode.C2G_Ready)]
	public partial class C2G_Ready : IGameActorMessage {}

}
namespace ETHotfix
{
	public static partial class HotfixOpcode
	{
		 public const ushort C2G_Ready = 10001;
	}
}
