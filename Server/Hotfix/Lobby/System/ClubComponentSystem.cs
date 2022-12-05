using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using ETModel;

namespace ETHotfix
{
	public class ClubComponentAwakeSystem : AwakeSystem<UserComponent>
	{
		public override void Awake(UserComponent self)
		{

		}
	}

	public static class ClubComponentSystem
	{
		public static void Awake(this ClubComponent self)
		{

		}

		public static void AddInfo(this ClubComponent self, ClubData clubData)
		{
			self.clubDataDict.Add(clubData.clubId, clubData);
		}

		public static ClubData[] GetInfos(this ClubComponent self,int[] clubIds)
        {
			List<ClubData> _list = new List<ClubData>();
			ClubData _clubInfo = null;
			foreach (int _clubId in clubIds)
            {
				if(self.clubDataDict.TryGetValue(_clubId,out _clubInfo))
                {
					_list.Add(_clubInfo);
                }
                else
                {
					throw new Exception($"_clubId={_clubId} 不存在");
                }
            }
			return _list.ToArray();
        }

		/// <summary>
		/// 校验
		/// </summary>
		/// <param name="self"></param>
		/// <param name="clubId"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static bool ValidateActive(this ClubComponent self, int clubId, out string message)
		{
			message = null;

			if (!self.clubDataDict.ContainsKey(clubId))
			{
				message = $"Club {clubId} 没有对应数据";
				return false;
			}

			if (self.clubDict.ContainsKey(clubId)) {
				message = $"Club {clubId} 已经被激活";
				return false;
			};

			return true;
		}

		public static Club Active(this ClubComponent self,int clubId)
        {
			ClubData _data= self.clubDataDict[clubId];
			Club _club = ComponentFactory.Create<Club, ClubData>(_data);
			self.clubDict.Add(_club.data.clubId, _club);
			return _club;
		}

		public static Club Get(this ClubComponent self, int clubId)
		{
			self.clubDict.TryGetValue(clubId, out Club club);
			return club;
		}

		public static IEnumerator<Club> GetAll(this ClubComponent self)
		{
			return self.clubDict.Values.GetEnumerator() as IEnumerator<Club>;
		}

		/// <summary>
		/// 加载club数据
		/// </summary>
		/// <returns></returns>
		public static async ETVoid LoadClubDataFormDB(this ClubComponent self)
        {
			using (SQLTask _task = Game.Scene.GetComponent<SQLComponent>().CreateTask(SQLName.GameUser, SQLTask.ExecuteType.DataSet))
			{
				_task.SetCommandText("GSP_GP_GetClubInfos", true);

				try
				{
					await _task.Execute();
				}
				catch (Exception error)
				{
					return;
				}

				DataTable _clubTable = _task.GetComponent<SQLDataSetComponent>().dataSet.Tables[0];
				DataTable _clubGameKindTable = _task.GetComponent<SQLDataSetComponent>().dataSet.Tables[1];

				int _clubId = 0;
				ClubData _clubData = null;
				foreach (DataRow _row in _clubTable.Rows)
				{
					_clubId = (int)_row["ClubID"];
					if (!self.clubDataDict.TryGetValue(_clubId, out _clubData))
					{
						_clubData = ComponentFactory.Create<ClubData>();
						_clubData.clubId = _clubId;
						self.clubDataDict.Add(_clubId, _clubData);
					}

					_clubData.name = (string)_row["Name"];
					_clubData.icon = (string)_row["Icon"];
					_clubData.userCount = (int)_row["UserCount"];
					_clubData.ownGameId = (long)_row["OwnGameID"];
					_clubData.ownUserId = (int)_row["OwnUserID"];
				}

				_clubId =-1;
				int _startIndex = 0;
				int _currIndex = 0;
				foreach (DataRow _row in _clubGameKindTable.Rows)
				{
					if(_clubId != (int)_row["ClubID"])
                    {
						if(_currIndex>_startIndex) //回写
                        {
							_clubData = self.clubDataDict[_clubId];
							_clubData.gameKindIds = new int[_currIndex - _startIndex];

							int _index = 0;
							for(int i=_startIndex;i<_currIndex;i++)
                            {
								_clubData.gameKindIds[_index] = (int)_clubGameKindTable.Rows[i]["GameKindID"];
							}
						}
						_startIndex = _currIndex;
						_clubId = (int)_row["ClubID"];
					}
					_currIndex++;
				}

				if (_currIndex > _startIndex) //回写
				{
					_clubData = self.clubDataDict[_clubId];
					_clubData.gameKindIds = new int[_currIndex - _startIndex];

					int _index = 0;
					for (int i = _startIndex; i < _currIndex; i++)
					{
						_clubData.gameKindIds[_index] = (int)_clubGameKindTable.Rows[i]["GameKindID"];
					}
				}
			}
		}
	}
}
