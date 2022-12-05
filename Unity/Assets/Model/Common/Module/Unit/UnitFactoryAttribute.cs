using System;

namespace ETModel
{
	[AttributeUsage(AttributeTargets.Class)]
	public class UnitFactoryAttribute : BaseAttribute
	{
		public string Type { get; }

		public UnitFactoryAttribute(string type)
		{
			this.Type = type;
		}
	}
}