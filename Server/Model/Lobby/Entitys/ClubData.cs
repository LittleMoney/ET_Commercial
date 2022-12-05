using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    public class ClubData : Entity
    {
        public int clubId;            //名字
        public string name;            //名字
        public string icon;            //图标
        public int userCount;          //总人数
        public int tableCount;          //总桌子数
        public long ownGameId;         //创建人GameId
        public int ownUserId;
        public int[] gameKindIds;      //支持的游戏类型编号

        [BsonIgnore]
        public List<GameKindConfig> GameKindConfigs;
        [BsonIgnore]
        public User ownUser;
    }
}
