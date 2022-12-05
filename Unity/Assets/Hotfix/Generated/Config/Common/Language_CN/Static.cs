using ETModel;

namespace ETHotfix
{
	[Config((int)(AppType.ClientM|AppType.ClientH))]
	public partial class StaticCategory : ACategory<Static>
	{
	}

	public class Static: IConfigString
	{
		public string Id{ get; set; }
		public string Value;
	}
}
