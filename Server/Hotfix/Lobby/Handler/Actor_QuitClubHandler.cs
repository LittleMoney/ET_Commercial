using ETModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Centor)]
    public class Actor_QuitClubHandler : AMActorRpcHandler<User, Actor_QuitClubRequest, Actor_QuitClubResponse>
    {
        protected override ETTask Run(User unit, Actor_QuitClubRequest request, Actor_QuitClubResponse response, Action reply)
        {
            throw new NotImplementedException();
        }
    }
}
