using System;
using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
	public abstract class ACategory : Object
	{
		public abstract Type ConfigType { get; }
		public abstract IConfig GetOne();
		public abstract IConfig[] GetAll();
		public abstract IConfig TryGet(long id);
		public abstract IConfig TryGet(string id);
		public abstract void BeginInit(string text);
	}

	/// <summary>
	/// 管理该所有的配置
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ACategory<T> : ACategory where T : IConfig
	{
		protected Dictionary<long, IConfig> dictLong;
		protected Dictionary<string, IConfig> dictString;

		public override void BeginInit()
		{

		}

		public override void BeginInit(string text)
		{
			if(typeof(IConfigLong).IsAssignableFrom(typeof(T)))
            {
				dictLong = new Dictionary<long, IConfig>();
			}
            else
            {
				dictString = new Dictionary<string, IConfig>();
			}

			string configStr = text;

			foreach (string str in configStr.Split(new[] { "\n" }, StringSplitOptions.None))
			{
				try
				{
					string str2 = str.Trim();
					if (str2 == "")
					{
						continue;
					}
					T t = ConfigHelper.ToObject<T>(str2);

					if (this.dictLong != null)
					{
						this.dictLong.Add((t as IConfigLong).Id, t);
					}
                    else
                    {
						this.dictString.Add((t as IConfigString).Id, t);
					}

				}
				catch (Exception e)
				{
					throw new Exception($"parser json fail: {str}", e);
				}
			}
		}

		public override Type ConfigType
		{
			get
			{
				return typeof(T);
			}
		}

		public override void EndInit()
		{
		}

		public override IConfig TryGet(long id)
		{
			IConfig t=null;
			if (this.dictLong!=null && !this.dictLong.TryGetValue(id, out t))
			{
				return null;
			}
			return t;
		}

		public override IConfig TryGet(string id)
		{
			IConfig t=null;
			if (this.dictString != null && !this.dictString.TryGetValue(id, out t))
			{
				return null;
			}
			return t;
		}

		public override IConfig[] GetAll()
		{
			if (this.dictLong != null)
			{
				return this.dictLong.Values.ToArray();
			}
             
			return this.dictString.Values.ToArray();			
		}

		public override IConfig GetOne()
		{
			if (this.dictLong != null)
			{
				return this.dictLong.Values.First();
			}

			return this.dictString.Values.First();
		}
	}
}