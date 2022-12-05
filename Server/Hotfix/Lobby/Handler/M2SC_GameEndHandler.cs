using ETModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETHotfix
{
    [MessageHandler(AppType.Centor)]
    public class M2SC_GameEndHandler : AMHandler<M2SC_GameEnd>
    {
        protected override async ETTask Run(Session session, M2SC_GameEnd message)
        {
            if(Game.Scene.GetComponent<ClubComponent>().clubDict.TryGetValue(message.ClubId,out Club _club))
            {
                if(_club.tableDict.TryGetValue(message.TableId, out Table _table))
                {
                    if(_table.status==TableStatus.Play && 
                        _table.gameTableActorId==message.GameTableActorId &&
                        _table.users.Length==message.EndScores.Count)
                    {
                        _table.GameEnd(message.EndScores.ToArray(), message.LeftChairId, message.GameRecordGUID);
                    }
                }
            }
        }
    }
}
