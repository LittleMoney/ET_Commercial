using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHotfix
{
    public class Lobby:EntityI
    {
        public static Lobby Instance;

        public Account gs_account;
        public User gs_me;
        public Club gs_currentClub;

    }

    [ObjectSystem]
    public class LobbyAwakeSystem : AwakeSystem<Lobby>
    {
        public override void Awake(Lobby self)
        {
            self.AddComponent<NetComponent>();
            Lobby.Instance = self;
        }
    }

    [ObjectSystem]
    public class LobbyDestroySystem : DestroySystem<Lobby>
    {
        public override void Destroy(Lobby self)
        {
            self.gs_account?.Dispose();
            self.gs_me?.Dispose();
            self.gs_currentClub?.Dispose();
            Lobby.Instance = null;
        }
    }


}
