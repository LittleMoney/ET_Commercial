using ETModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Centor)]
    public class Actor_GetGameInfoHandler : AMActorHandler<User, Actor_GetGameInfo>
    {
        protected override async ETTask Run(User user, Actor_GetGameInfo message)
        {
            if(user.table!=null && user.status==UserStatus.Play)
            {
                Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(user.table.gameTableActorId).Send(new Actor_UserRequestGameInfo()
                {
                    ChairId = user.chairId,
                    UserId = user.userId
                });
            }
        }
    }

}
