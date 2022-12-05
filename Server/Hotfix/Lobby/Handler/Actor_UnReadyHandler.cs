using ETModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Centor)]
    public class Actor_UnReadyHandler : AMActorRpcHandler<User, Actor_UnReadyRequest, Actor_UnReadyResponse>
    {
        protected override async ETTask Run(User user, Actor_UnReadyRequest request, Actor_UnReadyResponse response, Action reply)
        {
            if (user.table == null)
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "你当前不在桌子上";
                reply();
                return;
            }

            if (!user.table.ValidateUnReady(user, out string _message))
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = _message;
                reply();
                return;
            }
            else
            {
                response.Error = ErrorCode.ERR_Success;
                reply();

                user.table.UnReady(user);
            }

        }
    }
}
