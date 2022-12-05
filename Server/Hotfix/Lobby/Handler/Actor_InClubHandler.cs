using ETModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Centor)]
    public class Actor_InClubHandler : AMActorRpcHandler<User, Actor_InClubRequest, Actor_InClubResponse>
    {
        protected override async ETTask Run(User user, Actor_InClubRequest request, Actor_InClubResponse response, Action reply)
        {
            if (user.status != UserStatus.None || user.club!=null)
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "你已经在俱乐部中，请先退出";
                reply();
                return;
            }

            Club _club = null;
            if (!Game.Scene.GetComponent<ClubComponent>().clubDict.TryGetValue(request.ClubId, out _club))
            {
                //club没有被激活，激活俱乐部
                if (!Game.Scene.GetComponent<ClubComponent>().ValidateActive(request.ClubId, out string _vaMessage))
                {
                    response.Error = ErrorCode.ERR_Exception;
                    response.Message = _vaMessage;
                    reply();
                    return;
                }
                else
                {
                    _club=Game.Scene.GetComponent<ClubComponent>().Active(request.ClubId);
                }
            }

            //club 已经被激活，直接加入到该俱乐部
            if (!_club.ValidateInUser(user, out string _viuMessage))
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = _viuMessage;
                reply();
                return;
            }
            else
            {
                response.Error = ErrorCode.ERR_Success;
                reply();
                _club.InUser(user);
            }
        }
    }
}
