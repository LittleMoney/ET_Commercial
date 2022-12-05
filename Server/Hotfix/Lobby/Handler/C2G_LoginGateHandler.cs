using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_LoginGateHandler : AMRpcHandler<C2G_LoginGate, G2C_LoginGate>
    {
        protected static Action<object> onSessionDestory;

        static C2G_LoginGateHandler()
        {
            onSessionDestory=new Action<object>((scope) => { Game.EventSystem.Run<Player>(ETModel.EventIdType.GateSessionDestory,scope as Player); });
        }

        protected override async ETTask Run(Session session, C2G_LoginGate request, G2C_LoginGate response, Action reply)
        {
            long _currSessionInstanceId = session.InstanceId;

            LogonInfo _logonInfo = Game.Scene.GetComponent<SessionTokenComponent>().Get(request.LoginKey);
            bool _isError = false;

            if (!_isError && _logonInfo == null)
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "Gate key验证失败!";
                reply();
            }

            //机器码不符
            if (!_isError && _logonInfo.machineSerial != request.MachineCode)
            {
                _isError = true;
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "Gate 机器码不符!";
                reply();
            }
            
            //地址不符
            if(!_isError && _logonInfo.ip!=session.RemoteAddress.Address.ToString()) 
            {
                _isError = true;
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "Gate IP地址不符!";
                reply();
            }

            //登录到中心服务器
            IResponse _centorResponse = null;
            StartConfig _centerStartConfig = null;
            if (!_isError)
            {  
                //目前使用一个中心服务器，后续扩展成多中心多游戏集群模式
                _centerStartConfig = Game.Scene.GetComponent<StartConfigComponent>().CentorConfigs[0];
                try
                {
                    _centorResponse = await Game.Scene.GetComponent<NetInnerComponent>().Get(_centerStartConfig.GetComponent<InnerConfig>().IPEndPoint).Call(new G2SC_UserEntry()
                    {
                        UserId = _logonInfo.userId,
                        GateActorId = session.InstanceId,
                        IpAddress = session.RemoteAddress.Address.ToString(),
                        MachineCode = _logonInfo.machineSerial
                    });

                    if (session.InstanceId != _currSessionInstanceId)
                    {
                        _isError = true;
                    }
                }
                catch (Exception error)
                {
                    _isError = true;
                    response.Error = ErrorCode.ERR_Exception;
                    response.Message = error.Message;
                    reply();
                }
            }

            if (!_isError)
            {
                if (_centorResponse.Error != ErrorCode.ERR_Success)
                {
                    _isError = true;
                    response.Error = ErrorCode.ERR_Exception;
                    response.Message = _centorResponse.Message;
                    reply();
                }
            }

            if (!_isError)
            {
                SC2G_UserEntry _userEntryResponse = _centorResponse as SC2G_UserEntry;
                //获取Centor 多centor放到第二版实现
                Player _player = ComponentFactory.Create<Player>();
                _player.userId = _logonInfo.userId;
                _player.userActorId = _userEntryResponse.UserActorId;
                _player.gameActorId = 0;

                //绑定会话
                _player.AddComponent<PlayerSessionComponent>().session = session;
                session.AddComponent<SessionPlayerComponent>().player = _player;
                session.AddComponent<MailBoxComponent>();

                //绑定销毁事件
                session.AddComponent<DestroyMoniterComponent>().s_scope = _player;
                session.GetComponent<DestroyMoniterComponent>().s_callback = C2G_LoginGateHandler.onSessionDestory;

                response.Error = ErrorCode.ERR_Success;
                reply();
            }

            _logonInfo.Dispose();
        }
    }
}
