using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{

	[BsonIgnoreExtraElements]
	public class SQLConfig : AConfigComponent
	{
		public SQLSessionConfig[] Sessions;

	}

	public class SQLSessionConfig
	{
		public string Name;
		public string ConnectionString;
		public int MaxConnectionCount;
	}

}