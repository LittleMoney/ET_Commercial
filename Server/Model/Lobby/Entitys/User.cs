using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    public enum UserStatus
    {
        None=0,
        Free=1,
        Sit=2,
        Ready=3,
        Play=4,
    }

    /// <summary>
    /// 中心服务器上的用户实体
    /// </summary>
    public sealed class User : Entity
    {
        public int userId;
        public string accounts;
        public long gameId;
        public string nickName;
        public string icon;
        public bool gender;
        public string underWrite;
        public int memberOrder; 
        public string mobilePhone;
        public int channelId;

        public string machineSerial;        
        public string lastIpAddress;
        public DateTime lastLogonTime;

		public int winCount;
        public int lostCount;
        public int drawCount;
        public int fleeCount;
        public long score;

        public int[] joinedClubIds;

        public int  clubId;
        public int tableId;
        public int  chairId;

        public bool isOffline;
        public UserStatus status;

        public long gateActorId;

        [BsonIgnore]
        public Club club;

        [BsonIgnore]
        public Table table;
    }
}
