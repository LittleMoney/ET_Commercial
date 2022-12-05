using System;
using System.Collections.Generic;
using System.Text;
using ETModel;

namespace ETHotfix
{
    [Event(EventIdType.GateSessionDestory)]
    public class GateSessionDestoryEventHandler : AEvent<Player>
    {
        public override void Run(Player player)
        {
            OnRun(player).Coroutine();
        }

        public async ETVoid OnRun(Player player) 
        { 
            if(player!=null && !player.IsDisposed )
            {
                
                if(player.userActorId!=0)
                {
                    Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(player.userActorId).Send(new Actor_UserOffline());
                }
                //销毁player
                PlayerComponent.Instance.Remove(player.userId);
                player.Dispose();

            }
        }
    }


}
