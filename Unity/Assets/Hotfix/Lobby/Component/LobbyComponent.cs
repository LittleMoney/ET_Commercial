using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHotfix
{
    public class LobbyComponent : Component
    {
        public Lobby lobby;
    }

    [ObjectSystem]
    public class LobbyComponentAwakeSystem :AwakeSystem<LobbyComponent>
    {
        public override void Awake(LobbyComponent self)
        {
            self.lobby = ComponentFactory.Create<Lobby>();
        }
    }

    [ObjectSystem]
    public class LobbyComponentDestroySystem : DestroySystem<LobbyComponent>
    {
        public override void Destroy(LobbyComponent self)
        {
            self.lobby.Dispose();
        }
    }
}
