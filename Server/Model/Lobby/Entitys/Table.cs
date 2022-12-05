using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    public enum TableStatus
    {
        Idle = 1,       //空闲中
        Play= 2     //游戏中
    }

    public class Table:Entity
    {
        /// <summary>
        /// 桌子id 由于4位clubId和3位tableId构成，全局唯一
        /// </summary>
        public int tableId; 

        /// <summary>
        /// 游戏类型id
        /// </summary>
        public int gameKindId;



        /// <summary>
        /// 桌子的游戏实体id
        /// </summary>
        public long gameTableActorId;

        /// <summary>
        /// 本轮编号
        /// </summary>
        public string roundGUID;

        /// <summary>
        /// 本轮开始时间
        /// </summary>
        public DateTime? roundStartTime;

        /// <summary>
        /// 当前局数
        /// </summary>
        public int  turnCount;

        /// <summary>
        /// 本局开始时间
        /// </summary>
        public DateTime? turnStartTime;



        /// <summary>
        /// 是否启动自动开始游戏机制
        /// </summary>
        public bool isAutoStartingGame;

        /// <summary>
        /// 按照椅子排列的用户
        /// </summary>
        public User[] users;

        /// <summary>
        /// 俱乐部
        /// </summary>
        public Club club;

        /// <summary>
        /// 桌子状态
        /// </summary>
        public TableStatus status;


    }
}
