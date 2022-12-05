using System;
using System.Collections.Generic;
using System.Text;
using ETModel;

namespace ETHotfix
{
    public class SessionPlayerComponentDesotroySystem : DestroySystem<SessionPlayerComponent>
    {
        public override void Destroy(SessionPlayerComponent self)
        {
            if(self.player!=null)
            {
                Game.EventSystem.Run<Player>(EventIdType.GateSessionDestory, self.player);
            }
        }
    }



    public static class SessionPlayerComponentSystem
    {
        public static void Clear(SessionPlayerComponent self)
        {
            self.player = null;
        }
    }
}
