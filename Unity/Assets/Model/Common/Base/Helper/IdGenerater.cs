using System.Runtime.InteropServices;

namespace ETModel
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct IdStruct
	{
		public uint Time; // 30bit
		public ushort Value; // 16bit
		public int AppId; // 18bit

		public long ToLong()
		{
			ulong result = 0;
			result |= (uint)this.AppId;
			result |= (ulong)this.Value << 18;
			result |= (ulong)this.Time << 34;
			return (long)result;
		}

		public IdStruct(int appId, uint time, ushort value)
		{
			if (appId == 0) throw new System.Exception("对不起 appid 不可以为0");

			this.AppId = appId;
			this.Time = time;
			this.Value = value;
		}

		public IdStruct(long id)
		{
			ulong result = (ulong)id;
			this.AppId = (int)(result & 0xffff);
			result >>= 16;
			this.Value = (ushort)(result & (ushort.MaxValue));
			result >>= 16;
			this.Time = (uint)result;
		}

		public override string ToString()
		{
			return $"appID: {this.AppId}, time: {this.Time}, value: {this.Value}";
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct uniqueIdStruct
	{
		public uint OriginId; // 32bit
		public ushort IdType; // 16bit

		public long ToLong()
		{
			ulong result = 0;
			result |= (ulong)this.IdType << 16;
			result |= (ulong)this.OriginId << 32;
			return (long)result;
		}

		public uniqueIdStruct(uint originId, ushort idType)
		{
			this.OriginId = originId;
			this.IdType = idType;
		}

		public uniqueIdStruct(long id)
		{
			ulong result = (ulong)id;
			result >>= 16;
			this.IdType = (ushort)(result & (ushort.MaxValue));
			result >>= 16;
			this.OriginId = (uint)result;
		}

		public override string ToString()
		{
			return $" originId: {this.OriginId}, idType: {this.IdType}";
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct InstanceIdStruct
	{
		public ulong Value; // 48bit
		public int AppId; // 16bit

		public long ToLong()
		{
			ulong result = 0;
			result |= (uint)this.AppId;
			result |= this.Value << 16;
			return (long)result;
		}

		public InstanceIdStruct(long id)
		{
			ulong result = (ulong)id;
			this.AppId = (int)(result & 0xffff);
			result >>= 16;
			this.Value = result;
		}

		public InstanceIdStruct(int appId, ulong value)
		{
			this.AppId = appId;
			this.Value = value;
		}

		public override string ToString()
		{
			return $"appId: {this.AppId}, value: {this.Value}";
		}
	}
	public static class IdGenerater
	{
		private static int appId;

		public static int AppId
		{
			set
			{
				appId = value;
			}
		}


		private static uint value;
		public static long lastTime;

		public static long GenerateId()
		{
			long time = TimeHelper.ClientNowSeconds();
			if (time != lastTime)
			{
				value = 0;
				lastTime = time;
			}

			if (++value > ushort.MaxValue - 1)
			{
				Log.Error($"id is not enough! value: {value}");
			}

			if (time > uint.MaxValue)
			{
				Log.Error($"time > int.MaxValue value: {time}");
			}

			IdStruct idStruct = new IdStruct(appId, (uint)time, (ushort)value);
			return idStruct.ToLong();
		}

		public static long GenerateUniqueId(uint originId,ushort idType)
		{
			uniqueIdStruct uniqueIdStruct = new uniqueIdStruct(originId, idType);
			return uniqueIdStruct.ToLong();
		}


		public static ulong instanceIdValue = 0;
		public static long GenerateInstanceId()
		{
			InstanceIdStruct instanceIdStruct = new InstanceIdStruct(appId, ++instanceIdValue);
			return instanceIdStruct.ToLong();
		}

		public static int GetAppId(long v)
		{
			return new IdStruct(v).AppId;
		}
	}
}