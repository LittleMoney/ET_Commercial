using System;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Centor)]
    public class Actor_JoinClubHandler : AMActorRpcHandler<User, Actor_JoinClubRequest, Actor_JoinClubResponse>
    {
        protected override ETTask Run(User unit, Actor_JoinClubRequest request, Actor_JoinClubResponse response, Action reply)
        {
            throw new NotImplementedException();
        }
    }
}
