using ETModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Centor)]
    public class Actor_ReadyHandler : AMActorRpcHandler<User, Actor_ReadyRequest, Actor_ReadyResponse>
    {
        protected override async ETTask Run(User user, Actor_ReadyRequest request, Actor_ReadyResponse response, Action reply)
        {
            if(user.table==null)
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "你当前不在桌子上";
                reply();
                return;
            }

            if(!user.table.ValidateReady(user,out string _message))
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

                user.table.Ready(user);
            }
        }
    }
}
