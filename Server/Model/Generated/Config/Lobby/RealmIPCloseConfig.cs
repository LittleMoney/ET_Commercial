namespace ETModel
{
	[Config((int)(AppType.Realm))]
	public partial class RealmIPCloseConfigCategory : ACategory<RealmIPCloseConfig>
	{
	}

	public class RealmIPCloseConfig: IConfigLong
	{
		public long Id{ get; set; }
		public string IPBegin;
		public string IPEnd;
		public int Flag;
		public int Notice;
	}
}
