using ETModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Centor)]
    public class Actor_StandupHandler : AMActorRpcHandler<User, Actor_StandupRequest, Actor_StandupResponse>
    {
        protected override async ETTask Run(User user, Actor_StandupRequest request, Actor_StandupResponse response, Action reply)
        {
            if (user.club == null)
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "你当前不在俱乐部中";
                reply();
                return;
            }
            else if (user.table == null)
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "你已经当前不是坐下状态";
                reply();
                return;
            }

            if (!user.table.ValidateStandup(user.chairId, user, out string _sitMessage))
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = _sitMessage;
                reply();
                return;
            }
            else
            {
                response.Error = ErrorCode.ERR_Success;
                reply();
                user.table.Sitdown(user.chairId, user);
            }
        }
    }
}
