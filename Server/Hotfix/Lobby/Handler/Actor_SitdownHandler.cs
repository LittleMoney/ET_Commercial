using ETModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Centor)]
    public class Actor_SitdownHandler : AMActorRpcHandler<User, Actor_SitdownRequest, Actor_SitdownResponse>
    {
        protected override async ETTask Run(User user, Actor_SitdownRequest request, Actor_SitdownResponse response, Action reply)
        {
            if(user.club==null)
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "你当前不在俱乐部中";
                reply();
                return;
            }
            else if(user.table!=null)
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "你已经当前已经是坐下状态";
                reply();
                return;
            }
       
            if(!user.club.tableDict.TryGetValue(request.TableId,out Table _table))
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "对不起，没有找到你要坐下的桌子";
                reply();
                return;
            }
            else
            {
                if(!_table.ValidateSitDown(request.ChairId,user,out string _sitMessage))
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
                    _table.Sitdown(request.ChairId,user);
                }
            }
        }
    }
}
