using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
    [Event(EventIdType.InitSceneStart)]
    public class InitSceneStartEventHandler : AEvent
    {
        public override void Run()
        {
            ProcedureComponent.Instance.SwitchAsync(ProcedureNames.LobbyUpdate).Coroutine();
        }
    }
}
