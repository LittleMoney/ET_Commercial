using ETModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Centor)]
    public class Actor_OutClubHandler : AMActorRpcHandler<User, Actor_OutClubRequest, Actor_OutClubResponse>
    {
        protected override async ETTask Run(User user, Actor_OutClubRequest request, Actor_OutClubResponse response, Action reply)
        {
            if(user.club==null)
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "你当前不在俱乐部中";
                reply();
                return;
            }

            if(!user.club.ValidateOutUser(user,out string _message))
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = _message;
                reply();
                return;
            }
            else
            {
                response.Error = ErrorCode.ERR_Success;
                response.Message = _message;
                reply();
                user.club.OutUser(user);
            }
        }
    }
}
