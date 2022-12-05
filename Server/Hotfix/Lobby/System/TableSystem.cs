using System;
using System.Text;
using ETModel;

namespace ETHotfix
{
    public class TableAwakeSystem : AwakeSystem<Table, Club,int, int>
    {
        public override void Awake(Table self,Club club, int tableId,int gameKindId)
        {
            self.tableId = tableId;
            self.gameKindId = gameKindId;
            self.status = TableStatus.Idle;
            self.club = club;
            self.users = new User[self.club.data.GameKindConfigs[gameKindId].ChairCount];
            self.isAutoStartingGame = false;
            return;
        }
    }

    public static class TableSystem
    {
        /// <summary>
        /// 检查替换用户
        /// </summary>
        /// <param name="self"></param>
        /// <param name="chairId"></param>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ValidateReplaceUser(this Table self, int chairId, User user, out string message)
        {
            message = null;
            User _oldUser = self.users[chairId];

            if (_oldUser == null) {
                message = "对不起旧用户不存在";
                return false;
            }

            if(_oldUser.userId!=user.userId)
            {
                message = "旧用户与新用户id不一致";
                return false;
            }

            if(_oldUser.isOffline)
            {
                message = "离线状态不能替换";
                return false;
            }
            return false;
        }

        /// <summary>
        /// 替换用户
        /// </summary>
        /// <param name="self"></param>
        /// <param name="chairId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static User ReplaceUser(this Table self,int chairId,User user)
        {
            User _oldUser = self.users[chairId];

            self.users[chairId] = user;

            user.tableId = self.tableId;
            user.table = self;
            user.chairId = chairId;
            user.status = _oldUser.status;

            if (user.status == UserStatus.Play)
            {
                Actor_UserReplace _protocol = new Actor_UserReplace()
                {
                    ChairId=user.chairId,
                    UserId = user.userId,
                    GateActorId = user.gateActorId
                };
                Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(self.gameTableActorId).Send(_protocol);
            }
            return _oldUser;
        }

        /// <summary>
        /// 检查用户重连
        /// </summary>
        /// <param name="self"></param>
        /// <param name="chairId"></param>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ValidateReconnectUser(this Table self, int chairId, User user, out string message)
        {
            message = null;
            User _oldUser = self.users[chairId];
            if (_oldUser == null)
            {
                message = "对不起用户不存在";
                return false;
            }

            if (user.status != UserStatus.Play)
            {
                message = "用户状态不正确";
                return false;
            }

            if (!user.isOffline)
            {
                message = "用户状态不正确";
                return false;
            }

            if(self.status!=TableStatus.Play)
            {
                message = "当前非游戏状态";
                return false;
            }
            return true;
        }

        /// <summary>
        /// 用户重连
        /// </summary>
        /// <param name="self"></param>
        /// <param name="chairId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static User ReconnectUser(this Table self, int chairId, User user)
        {
            User _oldUser = self.users[chairId];
           
            self.users[chairId] = user;
            user.tableId = self.tableId;
            user.table = self;
            user.chairId = chairId;
            user.isOffline = false;
            user.status = UserStatus.Play;

            self.club.BroadcastUserStatus(user);

            Actor_UserReconnect _protocol = new Actor_UserReconnect()
            {
                UserId = user.userId,
                GateActorId = user.gateActorId
            };
            Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(self.gameTableActorId).Send(_protocol);

            return _oldUser;
        }

        /// <summary>
        /// 检查用户坐下
        /// </summary>
        /// <param name="self"></param>
        /// <param name="chairId"></param>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ValidateSitDown(this Table self, int chairId, User user, out string message)
        {
            message = null;

            if (self.users.Length < chairId)
            {
                message = "椅子没有找到";
                return false;
            }

            if (self.users[chairId] != null)
            {
                message = "对不起椅子上有人";
                return false;
            }

            if (user.isOffline)
            {
                message = "你的状态不正确";
                return false;
            }

            if (user.status != UserStatus.Free)
            {
                message = "你的状态不正确";
                return false;
            }

            if(self.status!=TableStatus.Idle)
            {
                message = "本桌子不能坐下";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 用户坐下
        /// </summary>
        /// <param name="self"></param>
        /// <param name="chairId"></param>
        /// <param name="user"></param>
        public static void Sitdown(this Table self,int chairId,User user)
        {
            self.users[chairId] = user;
            user.status = UserStatus.Sit;
            user.tableId = self.tableId;
            user.table = self;

            self.club.BroadcastUserStatus(user,true);
        }

        /// <summary>
        /// 检查起立
        /// </summary>
        /// <param name="self"></param>
        /// <param name="chairId"></param>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ValidateStandup(this Table self, int chairId, User user, out string message)
        {
            message = null;

            if (user.status < UserStatus.Sit)
            {
                message = "你的状态不正确";
                return false;
            }

            if (self.users[user.chairId] != user)
            {
                message = "对不起你不是本桌用户";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 用户起立
        /// </summary>
        /// <param name="self"></param>
        /// <param name="chairId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async ETTask Standup(this Table self, int chairId, User user)
        {
            if(self.status==TableStatus.Play)
            {
                #region 桌子在游戏状态中

                IActorResponse _response = null;
                long _tableInstanceId = self.InstanceId;
                long _userInstanceId = user.InstanceId;

                try
                {
                    _response = await Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(self.gameTableActorId).Call(new Actor_UserStandupRequest()
                    {
                        ChairId = user.chairId
                    });
                }
                catch (Exception error)
                {
                    if (error.Message.Contains(ErrorCode.ERR_NotFoundActor.ToString()))
                    {
                        //如果是没有找到桌子的对应游戏实体，可能是之前游戏服务的实体已经结束了，此时等同于游戏服务器已经处理了完成了请求
                    }
                    else
                    {
                        return;
                    }
                }

                if (_response != null && _response.Error != ErrorCode.ERR_Success)
                {
                    return;
                }

                //发送起立消息后，游戏服务器会先返回游戏结束，之所以要在这里继续处理，是有可能有多个用户同时发起逃离起立，所以这里如果用户状态满足条件，则继续做后续处理

                //检查桌子状态是否满足立即起立的条件
                if (self.InstanceId != _tableInstanceId || self.gameTableActorId!=0 || self.status != TableStatus.Idle || self.roundGUID != null) return;

                //检查用户状态
                if (user.InstanceId != _userInstanceId || self.users[chairId] != user || user.table != self || (user.status != UserStatus.Sit && user.status != UserStatus.Ready))
                {
                    return;  //请求已经过期，不做任何处理
                }

                user.status = UserStatus.Free;
                user.tableId = 65535;
                user.table = null;
                user.chairId = 65535;
                self.club.BroadcastUserStatus(user, true);

                #endregion
            }
            else if(self.status==TableStatus.Idle)
            {
                #region 桌子不在游戏中
                //用户起立完毕
                self.users[user.chairId] = null;
                user.status = UserStatus.Free;
                user.tableId = 65535;
                user.table = null;
                user.chairId = 65535;
                
                if (!user.isOffline)
                {
                    self.club.BroadcastUserStatus(user,true);
                }
                else
                {
                    self.club.BroadcastUserStatus(user,false);
                }

                //有人离桌
                if (self.gameTableActorId != 0)
                {
                    Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(self.gameTableActorId).Send(new Actor_HaltGame());
                    self.gameTableActorId = 0;
                }
                self.roundGUID = null;
                self.roundStartTime = null;
                self.turnStartTime = null;
                self.turnCount = 0;

                #endregion
            }
            else
            {
                throw new Exception("不能处理的状态");
            }
           
        }

        /// <summary>
        /// 检查准备
        /// </summary>
        /// <param name="self"></param>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ValidateReady(this Table self,User user, out string message)
        {
            message = null;

            if (user.isOffline)
            {
                message = "你的状态不正确";
                return false;
            }

            if (user.status != UserStatus.Sit)
            {
                message = "你的状态不正确";
                return false;
            }

            if (self.users[user.chairId] != user)
            {
                message = "对不起你不是本桌用户";
                return false;
            }

            if (self.status != TableStatus.Idle)
            {
                message = "当前不可以发起准备";
                return false;
            }
            return true;
        }

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="self"></param>
        /// <param name="user"></param>
        public static void Ready(this Table self, User user)
        {
            user.status = UserStatus.Ready;
            self.club.BroadcastUserStatus(user,true);

            if(self.CheckStartGame())
            {
                self.AutoStartGame().Coroutine();
            }
        }

        /// <summary>
        /// 检查取消准备
        /// </summary>
        /// <param name="self"></param>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ValidateUnReady(this Table self, User user, out string message)
        {
            message = null;

            if (user.isOffline)
            {
                message = "你的状态不正确";
                return false;
            }

            if (user.status != UserStatus.Ready)
            {
                message = "你的状态不正确";
                return false;
            }

            if (self.users[user.chairId] != user)
            {
                message = "对不起你不是本桌用户";
                return false;
            }

            if (self.status != TableStatus.Idle)
            {
                message = "当前桌子不可取消准备";
                return false;
            }
            return true;
        }

        /// <summary>
        /// 取消准备
        /// </summary>
        /// <param name="self"></param>
        /// <param name="user"></param>
        public static void UnReady(this Table self, User user)
        {
            user.status = UserStatus.Sit;
            self.club.BroadcastUserStatus(user, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool ValidateOffline(this Table self, User user)
        {
            if (user != self.users[user.chairId]) return false;
            if (user.isOffline == true) return false;
            return true;
        }

        /// <summary>
        /// 离线
        /// </summary>
        /// <param name="self"></param>
        /// <param name="user"></param>
        public static void Offline(this Table self,User user)
        {
            if (self.status==TableStatus.Idle) //不是游戏状态下离线，强行起立
            {
                user.isOffline = true;
                user.table = null;
                user.tableId = 65535;
                user.chairId = 65535;
                user.status = UserStatus.Free;

                //有人离桌
                if (self.gameTableActorId != 0)
                {
                    Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(self.gameTableActorId).Send(new Actor_HaltGame());
                    self.gameTableActorId = 0;
                }
                self.roundGUID = null;
                self.roundStartTime = null;
                self.turnStartTime = null;
                self.turnCount = 0;

                self.club.BroadcastUserStatus(user);
            }
            else
            {
                user.isOffline = true;
                self.club.BroadcastUserStatus(user);

                Actor_GameUserOffline _protocol = new Actor_GameUserOffline()
                {
                    ChairId = user.chairId,
                    UserId=user.userId
                };
                Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(self.gameTableActorId).Send(_protocol);
            }
        }

        /// <summary>
        /// 检查游戏是否可以结束
        /// </summary>
        /// <param name="self"></param>
        public static bool ValidateGameEnd(this Table self, long[] scores, int leftChairId,long gameActorTableId)
        {
            if (self.status != TableStatus.Play) return false;
            if (self.users.Length != scores.Length) return false;
            if (leftChairId >= self.users.Length) return false;
            if (self.gameTableActorId != gameActorTableId) return false;
            return true;
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        /// <param name="self"></param>
        /// <param name="scores"></param>
        /// <param name="leftUserId"></param>
        public static void GameEnd(this Table self,long[] scores,int leftChairId, string gameRecordGUID)
        {
            //发起写分
            self.WriteScore(scores, leftChairId, gameRecordGUID);

            self.status = TableStatus.Idle;
            self.turnCount++;

            bool _hasStandUser = false;
            User _user = null;
            for(int i=0;i<self.users.Length;i++)
            {
                _user = self.users[i];
                _user.score += scores[i];

                if (i== leftChairId)
                {
                    _user.fleeCount++;
                }
                else if(scores[i]<0)
                {
                    _user.lostCount++;
                }
                else if(scores[i]==0)
                {
                    _user.drawCount++;
                }
                else
                {
                    _user.winCount++;
                }

                if (_user.isOffline || i == leftChairId || _user.score<self.club.data.GameKindConfigs[self.gameKindId].MinScore || 
                    self.turnCount> self.club.data.GameKindConfigs[self.gameKindId].MaxTrunCount) //离线用户,逃跑用户,分数过低用户,或则局数达到上线，强行起立
                {
                    _user.status = UserStatus.Free;
                    _user.table = null;
                    _user.tableId = 65535;
                    self.users[_user.chairId] = null;
                    _hasStandUser = true;
                }
                else
                {
                    _user.status = UserStatus.Sit;
                }
                self.club.BroadcastUserStatus(_user);
            }

            if(_hasStandUser)
            {
                //有人离桌
                if (self.gameTableActorId != 0)
                {
                    Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(self.gameTableActorId).Send(new Actor_HaltGame());
                    self.gameTableActorId = 0;
                }

                self.roundGUID = null;
                self.roundStartTime = null;
                self.turnStartTime = null;
                self.turnCount = 0;
            }

            self.club.BroadcastTableStatus(self);
        }

        /// <summary>
        /// 写分操作
        /// </summary>
        /// <param name="self"></param>
        /// <param name="scores"></param>
        /// <param name="leftUserId"></param>
        public static void WriteScore(this Table self, long[] scores, int leftChairId, string gameRecordGUID)
        {
            User _user = null;
            long _score = 0;
            for (int i = 0; i < self.users.Length; i++)
            {
                _user = self.users[i];
                _score = scores[i];
                using (SQLTask _dbTask = Game.Scene.GetComponent<SQLComponent>().CreateTask(SQLName.GameUser, SQLTask.ExecuteType.NonQuery))
                {
                    _dbTask.SetCommandText("GSP_GR_WriteGameScore", true);
                    _dbTask.AddParameter("@UserID", _user.userId);
                    _dbTask.AddParameter("@Score", _score);
                    _dbTask.AddParameter("@WinCount", (leftChairId != i && _score > 0) ? 1 : 0);
                    _dbTask.AddParameter("@LostCount", (leftChairId != i && _score < 0) ? 1 : 0);
                    _dbTask.AddParameter("@DrawCount", (leftChairId != i && _score == 0) ? 1 : 0);
                    _dbTask.AddParameter("@FleeCount", leftChairId == i ? 1 : 0);

                    _dbTask.AddParameter("@AppID", Game.Scene.GetComponent<StartConfigComponent>().StartConfig.AppId);
                    _dbTask.AddParameter("@ClubID", self.club.clubId);
                    _dbTask.AddParameter("@GameKindID", self.gameKindId);
                    _dbTask.AddParameter("@TableID", self.tableId);
                    _dbTask.AddParameter("@ChairID", i);
                    _dbTask.AddParameter("@ClientIP", _user.lastIpAddress);

                    _dbTask.AddParameter("@RoundGUID", self.roundGUID);
                    _dbTask.AddParameter("@RoundStartTimeUTC", self.roundStartTime);
                    _dbTask.AddParameter("@TurnCount",self.turnCount);
                    _dbTask.AddParameter("@TurnStartTimeUTC", self.turnStartTime);
                    _dbTask.AddParameter("@TurnPlayTimeCount",(System.DateTime.UtcNow-self.turnStartTime.Value).TotalSeconds);

                    _dbTask.AddParameter("@GameRecordGUID", gameRecordGUID);
                    _dbTask.AddParameter("@ErrorDescribe", SQLTask.ParameterDirection.Out, SQLTask.ParameterType.String, null);

                    try
                    {
                        _dbTask.Execute().Coroutine();
                    }
                    catch (Exception error)
                    {
                        Log.Error(error);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 自动开始游戏
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async ETVoid AutoStartGame(this Table self)
        {
            if (self.isAutoStartingGame) return;
            self.isAutoStartingGame = true;
            while(true)
            {
                //等待三秒后启动
                await Game.Scene.GetComponent<TimerComponent>().WaitAsync(3000);

                if (!self.CheckStartGame())
                {
                    self.isAutoStartingGame = false;
                    return;
                }

                if (self.gameTableActorId == 0)
                {
                    await self.StartRoundGame();
                    if (self.status == TableStatus.Play)
                    {
                        self.isAutoStartingGame = false;
                        return;
                    }
                }
                else
                {
                    await self.StartTurnGame();
                    if (self.status == TableStatus.Play)
                    {
                        self.isAutoStartingGame = false;
                        return;
                    }
                }

                if (!self.CheckStartGame())
                {
                    self.isAutoStartingGame = false;
                    return;
                }
            }
        }

        /// <summary>
        /// 检查是否可以启动并开始游戏
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool CheckStartGame(this Table self)
        {
            if (self.status != TableStatus.Idle) return false;

            foreach(User user in self.users)
            {
                if(user==null || user.isOffline || user.status!=UserStatus.Ready || user.score<self.club.data.GameKindConfigs[self.gameKindId].MinScore)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async ETTask StartTurnGame(this Table self)
        {
            //状态校验数据
            long _instanceId = self.InstanceId;
            long[] _userInstanceIds = new long[self.users.Length];
            long[] _userScore = new long[self.users.Length];
            int _userIndex = 0;

            Actor_StartGameRequest _protocol = new Actor_StartGameRequest();
            foreach (User _user in self.users)
            {
                _protocol.UserIds.Add(_user.userId);
                _protocol.GateActorIds.Add(_user.gateActorId);
                _protocol.Scores.Add(_user.score);

                _userInstanceIds[_userIndex] = _user.InstanceId;
                _userScore[_userIndex] = _user.score;
                _userIndex++;
            };

            string _roundGUID = self.roundGUID;
            int _trunCount = self.turnCount;
            long _gameTableActorId = self.gameTableActorId;
            IResponse _resopnse = null;

            try
            {
                _resopnse = await Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(_gameTableActorId).Call(_protocol);
            }
            catch (Exception error)
            {
                return;
            }

            if (_resopnse.Error != ErrorCode.ERR_Success)
            {
                return;
            }

            Actor_StartGameResponse _startGameResponse = _resopnse as Actor_StartGameResponse;
            //桌子实体，状态，局数必须能对应
            bool _abort = false;
            if (_instanceId != self.InstanceId || self.gameTableActorId!= _gameTableActorId ||  self.status != TableStatus.Idle || self.roundGUID!= _roundGUID || self.turnCount != _trunCount)
            {
                _abort = true;
            }
            //用户实体，状态，分数必须能对应
            for (int x = 0; x < _userInstanceIds.Length; x++)
            {
                if (self.users[x] == null || self.users[x].InstanceId != _userInstanceIds[x] || self.users[x].status != UserStatus.Ready || self.users[x].score != _userScore[x])
                {
                    _abort = true;
                    break;
                }
            }

            if (_abort) //桌子或用户状态已经改变，需要中止游戏
            {
                Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(_gameTableActorId).Send(new Actor_HaltGame());
            }
            else
            {
                //进入新一局
                self.turnStartTime = System.DateTime.UtcNow;
                self.turnCount++;   //增加轮数
                self.status = TableStatus.Play;
                foreach (User _user in self.users)
                {
                    _user.status = UserStatus.Play;
                    self.club.BroadcastUserStatus(_user, true);
                }
                self.club.BroadcastTableStatus(self);
                return;
            }
        }

        /// <summary>
        /// 启动并开始游戏
        /// </summary>
        public static async ETTask StartRoundGame(this Table self)
        {
            //轮询启动游戏
            Random _random = new Random();
            int _nIndex = _random.Next(0, self.club.data.GameKindConfigs[self.gameKindId].AppIds.Length - 1);
            for (int i = 0; i < self.club.data.GameKindConfigs[self.gameKindId].AppIds.Length; i++)
            {
                if (!self.CheckStartGame())
                {
                    return;
                }

                int _appId = self.club.data.GameKindConfigs[self.gameKindId].AppIds[(_nIndex + i) % self.club.data.GameKindConfigs[self.gameKindId].AppIds.Length];

                //状态校验数据
                long _instanceId = self.InstanceId;
                long[] _userInstanceIds = new long[self.users.Length];
                long[] _userScore = new long[self.users.Length];
                int _userIndex = 0;

                //转发给游戏服务
                SC2M_BoostGame _protocol = new SC2M_BoostGame();
                _protocol.ClubId = self.club.clubId;
                _protocol.TableId = self.tableId;
                foreach (User _user in self.users)
                {
                    _protocol.UserIds.Add(_user.userId);
                    _protocol.GateActorIds.Add(_user.gateActorId);
                    _protocol.Scores.Add(_user.score);

                    _userInstanceIds[_userIndex] = _user.InstanceId;
                    _userScore[_userIndex] = _user.score;
                    _userIndex++;
                };

                int _trunCount = self.turnCount;
                IResponse _resopnse = null;
                try
                {
                    _resopnse = await Game.Scene.GetComponent<NetInnerComponent>().Get(_appId).Call(_protocol);
                }
                catch (Exception error)
                {
                    continue;
                }

                if (_resopnse.Error != ErrorCode.ERR_Success)
                {
                    continue;
                }

                M2SC_BoostGame _boostGameResponse = _resopnse as M2SC_BoostGame;

                //桌子实体，状态，局数必须能对应
                bool _abort = false;
                if (_instanceId != self.InstanceId || self.status != TableStatus.Idle || self.turnCount != _trunCount)
                {
                    _abort = true;
                }
                //用户实体，状态，分数必须能对应
                for (int x = 0; x < _userInstanceIds.Length; x++)
                {
                    if (self.users[x] == null || self.users[x].InstanceId != _userInstanceIds[x] || self.users[x].status != UserStatus.Ready || self.users[x].score != _userScore[x])
                    {
                        _abort = true;
                        break;
                    }
                }

                if (_abort) //桌子状态已经改变，需要中止游戏
                {
                    Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(_boostGameResponse.GameTableActorId).Send(new Actor_HaltGame());
                }
                else
                {
                    //进入新一轮游戏
                    self.roundGUID = System.Guid.NewGuid().ToString("N");
                    self.roundStartTime= System.DateTime.UtcNow;
                    self.turnStartTime = System.DateTime.UtcNow;
                    self.turnCount = 1;
                    self.status = TableStatus.Play;
                    self.gameTableActorId = _boostGameResponse.GameTableActorId;

                    foreach (User _user in self.users)
                    {
                        _user.status = UserStatus.Play;
                        self.club.BroadcastUserStatus(_user, true);
                    }
                    self.club.BroadcastTableStatus(self);
                    return;
                }
            }
        }



        public static bool HasUser(this Table self)
        {
            for(int i=0;i<self.users.Length;i++)
            {
                if(self.users[i]!=null)
                {
                    return true;
                }

            }
            return false;
        }

    }
}
