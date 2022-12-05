﻿using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHotfix
{
	public class BroadcastComponent : Component
	{
		public static BroadcastComponent Instance=null;

		public Dictionary<string, BroadcastChannel> channels=new Dictionary<string, BroadcastChannel>();

		public BroadcastChannel defaultChannel=null;
	}


	[ObjectSystem]
	public class BroadcastComponentAwakeSystem : AwakeSystem<BroadcastComponent>
	{
		public override void Awake(BroadcastComponent self)
		{
			self.defaultChannel = ComponentFactory.CreateWithParent<BroadcastChannel>(self);
			BroadcastComponent.Instance = self;
		}
	}


	public static class BroadcastComponentSystem
	{
		public static BroadcastChannel OpenChannel(this BroadcastComponent self, string channelName)
		{
			if (self.channels.TryGetValue(channelName, out BroadcastChannel channel))
			{
				return channel;
			}
			else
			{
				channel = ComponentFactory.CreateWithParent<BroadcastChannel>(self);
				self.channels.Add(channelName, channel);
				return channel;
			}
		}

		public static void CloseChannel(this BroadcastComponent self, string channelName)
		{
			if (self.channels.TryGetValue(channelName, out BroadcastChannel channel))
			{
				self.channels.Remove(channelName);
				channel.Dispose();
			}
		}

		public static BroadcastChannel Get(this BroadcastComponent self, string channelName)
		{
			return self.channels[channelName];
		}

		public static BroadcastChannel GetDefault(this BroadcastComponent self)
		{
			return self.defaultChannel;
		}
	}


}
