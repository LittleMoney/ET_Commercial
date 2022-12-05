using ETModel;

namespace ETHotfix
{
	[Config((int)(AppType.Centor|AppType.ClientH))]
	public partial class GameKindConfigCategory : ACategory<GameKindConfig>
	{
	}

	public class GameKindConfig: IConfigLong
	{
		public long Id{ get; set; }
		public string Name;
		public string Icon;
		public int ChairCount;
		public int MaxTrunCount;
		public long MinScore;
	}
}
