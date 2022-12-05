using System;
using System.Collections.Generic;
using System.Text;
using ETModel;

namespace ETHotfix
{
    public class UserComponentAwakeSystem : AwakeSystem<UserComponent>
    {
        public override void Awake(UserComponent self)
        {
			self.Awake();
		}
    }

	public static class UserComponentSystem
	{
		public static void Awake(this UserComponent self)
		{

		}

		public static void Replace(this UserComponent self, User user)
		{
			User _oldUser = null;
			if(self.userDict.TryGetValue(user.userId,out _oldUser))
            {
				//提醒强制下线
				Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(_oldUser.gateActorId).Send(new Actor_ForceOffline());
				self.userDict.Remove(user.userId);
			}
			self.userDict.Add(user.userId, user);
		}

		public static void Add(this UserComponent self, User user)
		{
			self.userDict.Add(user.userId, user);
		}

		public static User Get(this UserComponent self, int userId)
		{
			self.userDict.TryGetValue(userId, out User user);
			return user;
		}

		public static void Remove(this UserComponent self, int userId)
		{
			if (self.userDict.TryGetValue(userId, out User user))
			{
				self.userDict.Remove(userId);
			}
		}

		public static int Count(this UserComponent self)
		{
			return self.userDict.Count;
		}

		public static User[] GetAllToArray(this UserComponent self)
		{
			User[] _userArray = new User[self.userDict.Count];
			int _index = 0;
			foreach (KeyValuePair<long, User> keyValue in self.userDict)
			{
				_userArray[_index] =keyValue.Value;
			}
			return _userArray;
		}

		public static IEnumerator<User> GetAll(this UserComponent self)
        {
			return self.userDict.Values.GetEnumerator() as IEnumerator<User>;
        }
	}
}
