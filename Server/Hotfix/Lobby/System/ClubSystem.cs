using System;
using System.Collections.Generic;
using System.Text;
using ETModel;

namespace ETHotfix
{
	public class ClubAwakeSystem : AwakeSystem<Club,ClubData>
	{
		public override void Awake(Club self, ClubData data)
		{ 
			self.data = data;

			//每个游戏类型默认创建6个桌子
			int _gameKindId = 0;
			int _defaultTableCount = 6;
			Table _table = null;
			foreach(GameKindConfig gkc in data.GameKindConfigs)
            {
				_gameKindId = (int)gkc.Id;
				for (int i=0;i< _defaultTableCount;i++)
                {
					_table = ComponentFactory.Create<Table>();
					_table.tableId = self.tableDict.Count;
					_table.gameKindId = _gameKindId;
					_table.club = self;
					self.tableDict.Add(_table.tableId, _table);
				}
			}
		}
	}

	public static class ClubSystem
    {
		/// <summary>
		///  检查进入俱乐部是否合法
		/// </summary>
		/// <param name="self"></param>
		/// <param name="user"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static bool ValidateInUser(this Club self, User user,out string message)
		{
			message = null;
			if (user.clubId != 0 || user.status != UserStatus.None)
            {
				message = "用户状态不正确";
				return false;
			}

			if (self.userDict.ContainsKey(user.userId))
            {
				message = "用户已存在";
				return false;
			}
			return true;
		}

		/// <summary>
		/// 用户进入
		/// </summary>
		/// <param name="self"></param>
		/// <param name="user"></param>
		public static void InUser(this Club self, User user)
        {
			user.clubId = self.clubId;
			user.club = self;
			user.status = UserStatus.Free;
			self.userDict.Add(user.userId, user);

			//补充数据
			self.SendTableInfosTo(user);
			self.SendAllUserTo(user);

			self.BroadcastInUser(user, true);
		}

		/// <summary>
		/// 检查退出俱乐部是否合法
		/// </summary>
		/// <param name="self"></param>
		/// <param name="user"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static bool ValidateOutUser(this Club self, User user, out string message)
		{
			message = null;
			if (user.clubId !=self.clubId)
			{
				message = "用户不在当前俱乐部";
				return false;
			}
			
			if(self.userDict.ContainsKey(user.userId) || self.userDict[user.userId]!=user)
            {
				message = "用户不在当前俱乐部";
				return false;
			}

			if (user.status != UserStatus.Free)
			{
				message="用户状态不正确";
			}
			return true;
		}
		
		/// <summary>
		/// 用户退出
		/// </summary>
		/// <param name="self"></param>
		/// <param name="user"></param>
		public static void OutUser(this Club self, User user)
		{
			user.clubId = 0;
			user.club = null;
			user.status = UserStatus.None;
			self.userDict.Remove(user.userId);

			self.BroadcastUserStatus(user,true);
		}

		/// <summary>
		/// 检查退出俱乐部是否合法
		/// </summary>
		/// <param name="self"></param>
		/// <param name="user"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static bool ValidateReplaceUser(this Club self, User user, out string message)
		{
			message = null;
			if (user.clubId != self.clubId)
			{
				message = "用户不在当前俱乐部";
				return false;
			}

			User _oldUser = null;
			if (!self.userDict.TryGetValue(user.userId,out _oldUser))
			{
				message = "用户不在当前俱乐部";
				return false;
			}

			if (user.status != UserStatus.None)
			{
				message = "用户状态不正确";
			}

			if(_oldUser.table!=null)
            {
				if(_oldUser.isOffline)
                {
					return _oldUser.table.ValidateReconnectUser(_oldUser.chairId, user,out message);
                }
                else
                {
					return _oldUser.table.ValidateReplaceUser(_oldUser.chairId, user, out message);
				}
            }
			return true;
		}

		/// <summary>
		/// 替换用户
		/// </summary>
		/// <param name="self"></param>
		/// <param name="user"></param>
		public static User ReplaceUser(this Club self,User user)
        {
			User _oldUser = self.userDict[user.userId];
			if (_oldUser.table == null)
			{
				user.clubId = self.data.clubId;
				user.club = self;
				user.status = _oldUser.status;

				self.userDict.Remove(user.userId);
				self.userDict.Add(user.userId, user);
				//补充数据
				self.SendTableInfosTo(user);
				self.SendAllUserTo(user);
			}
			else
            {
				user.clubId = self.data.clubId;
				user.club = self;
				user.status = UserStatus.Free;

				self.userDict.Remove(user.userId);
				self.userDict.Add(user.userId, user);
				//补充数据
				self.SendTableInfosTo(user);
				self.SendAllUserTo(user);

				if(_oldUser.isOffline)
                {
					_oldUser.table.ReconnectUser(_oldUser.chairId, user);
                }
                else
                {
					_oldUser.table.ReplaceUser(_oldUser.chairId, user);
				}

			}


			return _oldUser;
		}





		/// <summary>
		/// 创建新桌子
		/// </summary>
		/// <param name="self"></param>
		/// <param name="gameKindId"></param>
		/// <returns></returns>
		public static void CreateTable(this Club self,int gameKindId)
        {
			if (self.tableDict.Count == 999)
			{
				Log.Error($"{self.clubId} 桌子已满 ");
				return;
			}

			Table _table = ComponentFactory.Create<Table,Club,int,int>(self, self.CreateTableId(), gameKindId);
			self.tableDict.Add(_table.tableId, _table);
			if(self.tableGameKindDict.TryGetValue(gameKindId,out List<Table> _gameKindTableList))
            {
				_gameKindTableList.Add(_table);
			}
            else
            {
				self.tableGameKindDict.Add(gameKindId,new List<Table>());
				self.tableGameKindDict[gameKindId].Add(_table);
			}
        }

		/// <summary>
		/// 移除旧桌子
		/// </summary>
		/// <param name="self"></param>
		/// <param name="gameKindId"></param>
		/// <returns></returns>
		public static void DestroyTable(this Club self, Table table)
		{
			if (table.club != self) throw new Exception("不是本俱乐部的桌子");

			if (table.status == TableStatus.Idle && !table.HasUser())
			{
				self.BroadcastDestroyTable(table);
				self.tableDict.Remove(table.tableId);
				self.tableGameKindDict[table.gameKindId].Remove(table);
				table.Dispose();
			}
		}

		/// <summary>
		/// 创建TableId 必须保持7位数内 前4位为俱乐部id 后3位为俱乐部桌子，方便快速上桌
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static int CreateTableId(this Club self)
		{
			int _tableId = 0;
			for(int i=0;i<1000;i++)
            {
				_tableId= (self.clubId * 1000) + ((self.nextOriginTableId + i) % 1000);
				if (self.tableDict.ContainsKey(_tableId)) continue;

				self.nextOriginTableId= self.nextOriginTableId + i + 1;
				return _tableId;
            }
			
			throw new Exception("无法创建TableId TableId以满");
		}






		/// <summary>
		/// 广播消息给俱乐部中的所有用户
		/// </summary>
		/// <param name="self"></param>
		/// <param name="actorMessage"></param>
		public static void BroadcastAllUser(this Club self,IActorMessage actorMessage,int ignoreUserId=0)
        {
			ActorMessageSenderComponent _ActorMessageSenderComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
			foreach (KeyValuePair<int, User> _kv in self.userDict)
			{
				if (!_kv.Value.isOffline && _kv.Key!= ignoreUserId)
				{
					_ActorMessageSenderComponent.Get(_kv.Value.gateActorId).Send(actorMessage);
				}
			}
		}

		/// <summary>
		/// 广播新桌子
		/// </summary>
		/// <param name="self"></param>
		/// <param name="table"></param>
		public static void BroadcastCreateTable(this Club self,Table table)
        {
			Actor_TableCreate _protocol = new Actor_TableCreate();
			_protocol.TableInfo.TableId = table.tableId;
			_protocol.TableInfo.GameKindId = table.gameKindId;
			_protocol.TableInfo.CurrentTrun = table.turnCount;
			_protocol.TableInfo.Status =(int) table.status;

			self.BroadcastAllUser(_protocol);

		}

		/// <summary>
		/// 广播新桌子
		/// </summary>
		/// <param name="self"></param>
		/// <param name="table"></param>
		public static void BroadcastDestroyTable(this Club self, Table table)
		{
			Actor_TableDestroy _protocol = new Actor_TableDestroy();
			_protocol.TableId = table.tableId;
			self.BroadcastAllUser(_protocol);
		}

		/// <summary>
		/// 广播桌子状态
		/// </summary>
		/// <param name="self"></param>
		/// <param name="table"></param>
		public static void BroadcastTableStatus(this Club self, Table table)
		{
			Actor_TableStatus _protocol = new Actor_TableStatus();
			_protocol.TableId = table.tableId;
			_protocol.CurrentTrun = table.turnCount;
			_protocol.Status = (int)table.status;

			self.BroadcastAllUser(_protocol);
		}

		/// <summary>
		/// 广播用户进入俱乐部
		/// </summary>
		/// <param name="self"></param>
		/// <param name="user"></param>
		/// <param name="sendSelf"></param>
		public static void BroadcastInUser(this Club self, User user,bool sendSelf=false)
        {
			Actor_UserComes _protocol = new Actor_UserComes();
			_protocol.UserInfo.Add( new UserInfo(){
				GameId = user.gameId,
				Icon = user.icon,
				Name = user.nickName,
				Gender = user.gender,
				ClubId = user.clubId,
				TableId = user.tableId,
				ChiarId = user.chairId,
				Status = (int)user.status,
				Score = user.score
			});

			self.BroadcastAllUser(_protocol, sendSelf ? 0 : user.userId);
		}

		/// <summary>
		/// 广播用户状态变化
		/// </summary>
		/// <param name="self"></param>
		/// <param name="user"></param>
		/// <param name="sendSelf"></param>
		public static void BroadcastUserStatus(this Club self, User user, bool sendSelf = false)
		{
			Actor_UserStatus _protocol = new Actor_UserStatus();
			_protocol.GameId = user.gameId;
			_protocol.TableId = user.tableId;
			_protocol.ChiarId = user.chairId;
			_protocol.Status = (int)user.status;

			self.BroadcastAllUser(_protocol, sendSelf ? 0 : user.userId);
		}

		/// <summary>
		/// 广播用户分数变化
		/// </summary>
		/// <param name="self"></param>
		/// <param name="user"></param>
		/// <param name="sendSelf"></param>
		public static void BroadcastUserScore(this Club self, User user, bool sendSelf = false)
		{
			Actor_UserScore _protocol = new Actor_UserScore();
			_protocol.GameId = user.gameId;
			_protocol.Score = user.score;

			self.BroadcastAllUser(_protocol, sendSelf ? 0 : user.userId);
		}

		/// <summary>
		/// 发送俱乐部全部桌子信息给用户
		/// </summary>
		/// <param name="self"></param>
		/// <param name="user"></param>
		public static void SendTableInfosTo(this Club self,User user)
        {
			Actor_TablelInfoList _tableInfoListPro = new Actor_TablelInfoList();
			TableInfo _tableInfo = null;
			foreach(KeyValuePair<int,Table> _kv in self.tableDict)
            {
				_tableInfo = new TableInfo();
				_tableInfo.TableId = _kv.Value.tableId;
				_tableInfo.GameKindId = _kv.Value.gameKindId;
				_tableInfo.CurrentTrun = _kv.Value.turnCount;
				_tableInfo.Status = (int)_kv.Value.status;
				_tableInfoListPro.TableInfo.Add(_tableInfo);
			}
			ActorMessageSenderComponent _ActorMessageSenderComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
			_ActorMessageSenderComponent.Get(user.gateActorId).Send(_tableInfoListPro);
		}

		/// <summary>
		/// 发送用户状态给单一用户
		/// </summary>
		/// <param name="self"></param>
		/// <param name="user"></param>
		/// <param name="sendUser">为空发送给自己</param>
		public static void SendUserStatusTo(this Club self, User user,User sendUser=null)
        {
			Actor_UserStatus _userStatusPro = new Actor_UserStatus();
			_userStatusPro.GameId = user.gameId;
			_userStatusPro.TableId = user.tableId;
			_userStatusPro.ChiarId = user.chairId;
			_userStatusPro.Status = (int)user.status;

			ActorMessageSenderComponent _ActorMessageSenderComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
			_ActorMessageSenderComponent.Get(sendUser==null?user.gateActorId: sendUser.gateActorId).Send(_userStatusPro);
		}

		/// <summary>
		/// 发送所有用户信息给单一用户
		/// </summary>
		/// <param name="self"></param>
		/// <param name="user"></param>
		/// <param name="sendSelf"></param>
		public static void SendAllUserTo(this Club self, User user, bool sendSelf = false)
		{
			Actor_UserComes _protocol = new Actor_UserComes();

			ActorMessageSenderComponent _ActorMessageSenderComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
			int _index = 0;
			UserInfo _userInfo = null;
			foreach(KeyValuePair<int,User> _kv in self.userDict)
			{
				if(_index==20) //每20个用户发一次
                {
					_ActorMessageSenderComponent.Get(user.gateActorId).Send(_protocol);
					_index = 0;
				}

				if(_index>= _protocol.UserInfo.count)
                {
					_protocol.UserInfo.Add(new UserInfo());
				}

				if (sendSelf || _kv.Value.userId != user.userId)
				{
					_userInfo = _protocol.UserInfo[_index];
					_userInfo.GameId = _kv.Value.gameId;
					_userInfo.Icon = _kv.Value.icon;
					_userInfo.Name = _kv.Value.nickName;
					_userInfo.Gender = _kv.Value.gender;
					_userInfo.ClubId = _kv.Value.clubId;
					_userInfo.TableId = _kv.Value.tableId;
					_userInfo.ChiarId = _kv.Value.chairId;
					_userInfo.Status = (int)_kv.Value.status;
					_userInfo.Score = _kv.Value.score;
					_index++;
				}
			}

			if(_index>0)
            {
				while (_protocol.UserInfo.count > _index) //移除多余重复的用户数据
				{
					_protocol.UserInfo.RemoveAt(_protocol.UserInfo.count - 1);
				}
				_ActorMessageSenderComponent.Get(user.gateActorId).Send(_protocol);
			}
		}

	}
}
