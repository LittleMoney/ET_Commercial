using ETModel;
namespace ETModel
{
	[Message(OuterOpcode.C2R_Ping)]
	public partial class C2R_Ping : IRequest {}

	[Message(OuterOpcode.R2C_Ping)]
	public partial class R2C_Ping : IResponse {}

	[Message(OuterOpcode.C2M_Reload)]
	public partial class C2M_Reload : IRequest {}

	[Message(OuterOpcode.M2C_Reload)]
	public partial class M2C_Reload : IResponse {}

}
namespace ETModel
{
	public static partial class OuterOpcode
	{
		 public const ushort C2R_Ping = 101;
		 public const ushort R2C_Ping = 102;
		 public const ushort C2M_Reload = 103;
		 public const ushort M2C_Reload = 104;
	}
}
