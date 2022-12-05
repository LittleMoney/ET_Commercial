using ETModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETHotfix
{
    [ActorMessageHandler(AppType.Centor)]
    public class Actor_UserOfflineHander : AMActorHandler<User, Actor_UserOffline>
    {
        protected override async ETTask Run(User user, Actor_UserOffline message)
        {
            if (user.table != null)
            {
                //先由桌子处理
                if (!user.table.ValidateOffline(user))
                {
                    return;
                }
                else
                {
                    user.table.Offline(user);
                }

                //非游戏状态下，直接退出关闭
                if (user.status != UserStatus.Play)
                {
                    //非游戏状态下，直接退出
                    user.club.OutUser(user);
                    Game.Scene.GetComponent<UserComponent>().Remove(user.userId);
                    user.Dispose();
                }
            }
            else
            {
                if (user.club != null)
                {
                    //非游戏状态下，直接退出
                    user.club.OutUser(user);
                }
                Game.Scene.GetComponent<UserComponent>().Remove(user.userId);
                user.Dispose();
            }
        }
    }
}
