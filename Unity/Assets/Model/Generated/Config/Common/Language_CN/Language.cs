namespace ETModel
{
	[Config((int)(AppType.ClientM|AppType.ClientH))]
	public partial class LanguageCategory : ACategory<Language>
	{
	}

	public class Language: IConfigString
	{
		public string Id{ get; set; }
		public string Value;
	}
}
