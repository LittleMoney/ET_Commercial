using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHotfix
{
    public class HeartbeatComponent:Component
    {
        public float lastTime;
    }

    [ObjectSystem]
    public class HeartbeatComponentAwakeSystem : AwakeSystem<HeartbeatComponent>
    {
        public override void Awake(HeartbeatComponent self)
        {
            self.lastTime = UnityEngine.Time.unscaledTime;
        }
    }

    [ObjectSystem]
    public class HeartbeatComponentUpdateSystem : UpdateSystem<HeartbeatComponent>
    {
        public override void Update(HeartbeatComponent self)
        {
            if(UnityEngine.Time.unscaledTime-self.lastTime>30)
            {
                self.lastTime = UnityEngine.Time.unscaledTime;
                Lobby.Instance.GetComponent<NetComponent>().session?.Send(new C2G_Heart());
            }
        }
    }
}
