namespace ETModel
{
	[Config((int)(AppType.Realm))]
	public partial class RealmConfigCategory : ACategory<RealmConfig>
	{
	}

	public class RealmConfig: IConfigLong
	{
		public long Id{ get; set; }
		public int CanLogon;
		public int CanRegister;
		public int ServerVersion;
		public int RefuseLogonMessage;
		public int RefuseRegisterMessage;
	}
}
