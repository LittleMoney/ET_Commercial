using System;
using System.Collections.Generic;
using System.Text;
using ETModel;

namespace ETHotfix
{
    public  class UserAwakeSystem : AwakeSystem<User>
    {
        public override void Awake(User self)
        {
            self.userId = 0;
            self.status = UserStatus.None;
            self.isOffline = false;
        }
    }

    public static class UserSystem
    {
        /// <summary>
        /// 发送被强制离线信息给自己
        /// </summary>
        /// <param name="self"></param>
        public static void SendForceOffline(this User self)
        {
            if (self.gateActorId != 0) return;
            Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(self.gateActorId).Send(new Actor_ForceOffline());
        }

        /// <summary>
        /// 发送所有用户信息给自己
        /// </summary>
        /// <param name="self"></param>
        /// <param name="clubDatas"></param>
        public static void SendAllInfo(this User self,ClubData[] clubDatas)
        {
            if (self.gateActorId == 0) return;

            Actor_CurrUserInfo _currUserInfoPro = new Actor_CurrUserInfo();
            _currUserInfoPro.UserInfo.GameId = self.gameId;
            _currUserInfoPro.UserInfo.Icon = self.icon;
            _currUserInfoPro.UserInfo.Name = self.nickName;
            _currUserInfoPro.UserInfo.Gender = self.gender;
            _currUserInfoPro.UserInfo.ClubId = self.clubId;
            _currUserInfoPro.UserInfo.TableId = self.tableId;
            _currUserInfoPro.UserInfo.ChiarId = self.chairId;
            _currUserInfoPro.UserInfo.Status = (int)self.status;
            _currUserInfoPro.UserInfo.Score = self.score;

            for (int i = 0; i < clubDatas.Length; i++)
            {
                ClubInfo _clubInfo = new ClubInfo();
                _clubInfo.ClubId = clubDatas[i].clubId;
                _clubInfo.Name = clubDatas[i].name;
                _clubInfo.OwnGameId = clubDatas[i].ownGameId;
                _clubInfo.UserCount = clubDatas[i].userCount;
                _clubInfo.TableCount = clubDatas[i].tableCount;
                _clubInfo.GameKindIds.Add(clubDatas[i].gameKindIds);
                _currUserInfoPro.ClubInfos.Add(_clubInfo);
            }

            Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(self.gateActorId).Send(_currUserInfoPro);
        }
    }
}
