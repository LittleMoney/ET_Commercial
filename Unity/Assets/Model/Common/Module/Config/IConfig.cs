namespace ETModel
{
	/// <summary>
	/// 每个Config的基类
	/// </summary>
	public interface IConfig
	{
	}

	public interface IConfigLong: IConfig
	{
		long Id { get; set; }
	}

	public interface IConfigString : IConfig
	{
		string Id { get; set; }
	}
}