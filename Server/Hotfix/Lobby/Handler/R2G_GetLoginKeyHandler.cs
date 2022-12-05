using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ETModel;
using ETHotfix;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class R2G_GetLoginKeyHandler : AMRpcHandler<R2G_GetLoginKey, G2R_GetLoginKey>
    {
        protected override async ETTask Run(Session session, R2G_GetLoginKey request, G2R_GetLoginKey response, Action reply)
        {
            LogonInfo _logonInfo = ComponentFactory.Create<LogonInfo>(true);
            _logonInfo.userId = request.UserId;
            _logonInfo.machineSerial = request.MachineSerial;
            _logonInfo.ip = request.IP;
            _logonInfo.RegisteTime = System.DateTime.Now;
            _logonInfo.loginKey = Guid.NewGuid().ToString("N");
            Game.Scene.GetComponent<SessionTokenComponent>().Add(_logonInfo);

            response.Error = ErrorCode.ERR_Success;
            response.LoginKey = _logonInfo.loginKey;
            reply();
            return;
        }
    }
}
