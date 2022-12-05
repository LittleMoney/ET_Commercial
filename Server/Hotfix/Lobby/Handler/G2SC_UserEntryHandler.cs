using System;
using System.Collections.Generic;
using ETModel;
using PF;
using UnityEngine;
using System.Data;

namespace ETHotfix
{
	[MessageHandler(AppType.Centor)]
	public class G2SC_UserEntryHandler : AMRpcHandler<G2SC_UserEntry, SC2G_UserEntry>
	{
		protected override async ETTask Run(Session session, G2SC_UserEntry request, SC2G_UserEntry response, Action reply)
		{
			//默认单中心服务器
			User _newUser = await LogonDataBase(request, response);

			if(_newUser==null)
            {
				reply();
				return;
            }

			User _oldUser = Game.Scene.GetComponent<UserComponent>().Get(request.UserId);

			if (_oldUser == null)
			{
				FirstEnter(_newUser,session, request, response, reply);
			}
            else
            {
				ReconnectEnter(_oldUser, _newUser,session, request, response, reply);
			}
		}

		/// <summary>
		/// 初次进入大厅
		/// </summary>
		protected void FirstEnter(User newUser, Session session, G2SC_UserEntry request, SC2G_UserEntry response, Action reply)
        {

			newUser.lastIpAddress = request.IpAddress;
			newUser.lastLogonTime = System.DateTime.UtcNow;
			newUser.gateActorId = request.GateActorId;

			response.Error = ErrorCode.ERR_Success;
			reply();

			Game.Scene.GetComponent<UserComponent>().Add(newUser);
			//先发补充自己的信息
			newUser.SendAllInfo(Game.Scene.GetComponent<ClubComponent>().GetInfos(newUser.joinedClubIds));

		}

		/// <summary>
		/// 挤号登录
		/// </summary>
		protected void ReconnectEnter(User oldUser,User newUser, Session session, G2SC_UserEntry request, SC2G_UserEntry response, Action reply)
		{
			if (oldUser.gateActorId == request.GateActorId)
			{
				//登陆用户的 GateSessionId和之前的相同，不可能发生的异常
				Log.Error("登陆用户的 GateSessionId和之前的相同，不可能发生的异常");
			}

			oldUser.SendForceOffline();

			newUser.lastIpAddress = request.IpAddress;
			newUser.lastLogonTime = System.DateTime.UtcNow;
			newUser.gateActorId = request.GateActorId;
			newUser.status = UserStatus.None;

			//发送用户信息给自己
			if (oldUser.club!=null)
			{
				Club _club = Game.Scene.GetComponent<ClubComponent>().Get(oldUser.clubId);
				string _message = null;

				if (!_club.ValidateReplaceUser(newUser, out _message))
				{
					//移除处理
					oldUser.Dispose();
					newUser.Dispose();

					response.Error = ErrorCode.ERR_Success;
					response.Message = _message;
					reply();
					return;
				}
				else
				{
					//在用户列表中替换用户
					Game.Scene.GetComponent<UserComponent>().Remove(oldUser.userId);
					Game.Scene.GetComponent<UserComponent>().Add(newUser);

					response.Error = ErrorCode.ERR_Success;
					response.UserActorId = newUser.InstanceId;
					reply();

					_club.ReplaceUser(newUser);
				}
			}
            else
            {
				response.Error = ErrorCode.ERR_Success;
				response.UserActorId = newUser.InstanceId;
				reply();
			}

			//发送自己的信息
			newUser.SendAllInfo(Game.Scene.GetComponent<ClubComponent>().GetInfos(newUser.joinedClubIds));

			oldUser.Dispose();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		protected async ETTask<User> LogonDataBase(G2SC_UserEntry request, SC2G_UserEntry response)
        {
			using (SQLTask _task = Game.Scene.GetComponent<SQLComponent>().CreateTask(SQLName.Treasure, SQLTask.ExecuteType.DataSet))
			{
				_task.AddParameter("@UserID", request.UserId);
				_task.AddParameter("@AppID", request.IpAddress);
				_task.AddParameter("@MachineSerial", request.MachineCode);
				_task.AddParameter("@ClientIP", request.IpAddress);
				_task.AddParameter("@ErrorDescribe", SQLTask.ParameterDirection.Out, SQLTask.ParameterType.String, System.DBNull.Value);
				_task.SetCommandText("GSP_GR_EfficacyUserID", true);

				try
				{
					await _task.Execute();
				}
				catch (Exception error)
				{
					response.Error = ErrorCode.ERR_Exception;
					response.Message = $"登陆到数据库失败:{error.Message}";
					return null;
				}

				int _result = (int)_task.GetParameterValue("@Result");
				if (_result != 0)
				{
					response.Error = ErrorCode.ERR_Exception;
					response.Message = $"登陆失败:{_task.GetParameterValue("@ErrorDescribe").ToString()}";
					return null;
				}

				DataRow _row = _task.GetComponent<SQLDataSetComponent>().dataSet.Tables[0].Rows[0];
				DataTable _table = _task.GetComponent<SQLDataSetComponent>().dataSet.Tables[1];

				User _user = ComponentFactory.Create<User>();

				_user.userId = (int)request.UserId;
				_user.gameId = (long)_row["GameID"];
				_user.nickName = (string)_row["NickName"];
				_user.icon = (string)_row["Icon"];
				_user.gender = (byte)_row["Gender"] == 1 ? true : false;
				_user.underWrite = (string)_row["UnderWrite"];
				_user.memberOrder = (byte)_row["memberOrder"];
				_user.mobilePhone = (string)_row["MobilePhone"];
				_user.channelId = (int)_row["ChannelId"];

				_user.winCount = (int)_row["WinCount"];
				_user.lostCount = (int)_row["LostCount"];
				_user.drawCount = (int)_row["DrawCount"];
				_user.fleeCount = (int)_row["FleeCount"];
				_user.score = (long)_row["Score"];

				if (_table.Rows.Count > 0)
				{
					_user.joinedClubIds = new int[_table.Rows.Count];
					int _index = 0;
					foreach (DataRow _currRow in _table.Rows)
					{
						_user.joinedClubIds[_index] = (int)_currRow["ClubID"];
						_index++;
					}
				}

				return _user;
			}
		}
	}
}