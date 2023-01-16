using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHotfix
{
    public class NetComponent : Component
    {
        public Session         session;
        public HashSet<string> loginSteps=new HashSet<string>();
    }

    [ObjectSystem]
    public class NetComponentAwakeSystem : AwakeSystem<NetComponent>
    {
        public override void Awake(NetComponent self)
        {
            if (!(self.Parent is Lobby)) throw new Exception(" NetComponent need bunded on Account entity ");
        }
    }

    [ObjectSystem]
    public class NetComponentDestroySystem : DestroySystem<NetComponent>
    {
        public override void Destroy(NetComponent self)
        {
            self.session?.Dispose();
            self.loginSteps.Clear();
        }
    }


    public static class NetComponentSystem
    {
        /// <summary>
        /// 添加登录步骤,登录完成前会等待所有步骤被移除
        /// </summary>
        /// <param name="self"></param>
        /// <param name="flag"></param>
        public static void AddLoginStep(this NetComponent self,string step)
        {
            self.loginSteps.Add(step);
        }

        /// <summary>
        /// 取消登录步骤
        /// </summary>
        /// <param name="self"></param>
        /// <param name="step"></param>
        public static void RemoveLoginStep(this NetComponent self, string step)
        {
            self.loginSteps.Remove(step);
        }

        /// <summary>
        /// 登录到游戏服务器
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async ETTask Login(this NetComponent self)
        {
            long _instanceId = self.InstanceId;
            IResponse _response = null;
            Account _account = (self.Entity as Lobby).gs_account;

            if (self.session!=null)
            {
                self.session.RemoveComponent<DestroyMoniterComponent>();
                self.session.Dispose();
                self.session = null;
            }

            #region 登录到登录服务器

            BroadcastComponent.Instance.g_default.Run(BroadcastId.PreLogin);

            Session _loginSession=ComponentFactory.Create<Session,ETModel.Session>(
                ETModelHelper.GetSceneComponent<NetOuterComponent>().Create(ETModelHelper.GetSceneComponent<AppVersionComponent>().localAppVersionConfig.ServerAddress));

            _loginSession.AddComponent<DestroyMoniterComponent, Action<object>, object>((obj) =>
            {
                BroadcastComponent.Instance.g_default.Run(BroadcastId.UnLogined);
            }, null);

            try
            {
                _response = await self.session.Call(new C2R_Login()
                {
                    ServerVersion = ETModelHelper.GetSceneComponent<AppVersionComponent>().localAppVersionConfig.Version,
                    Accounts = _account.passward,
                    Password = MD5Helper.StringMD5(_account.passward),
                    MachineCode = PlatformHelper.GetMachineCode(),
                    ChannelId = ETModelHelper.GetSceneComponent<AppVersionComponent>().localAppVersionConfig.Channel,
                    PlatformId = PlatformHelper.GetPlatformCode(),
                    PhoneCode = PlatformHelper.GetPhoneCode()
                });

                if (_instanceId != self.InstanceId) return;

                if(_response.Error!=ErrorCode.ERR_Success)
                {
                    throw new Exception($"登录验证服务器失败:{_response.Message}");
                }
            }
            catch(Exception error)
            {
                if (_instanceId != self.InstanceId) return;

                _loginSession.Dispose();
                _loginSession = null;

                throw error;
            }

            R2C_Login _LoginResponse = _response as R2C_Login;
            _loginSession.RemoveComponent<DestroyMoniterComponent>();
            _loginSession.Dispose();

            #endregion

            #region 登录到网关服务器

            self.session= ComponentFactory.Create<Session, ETModel.Session>(
                ETModelHelper.GetSceneComponent<NetOuterComponent>().Create(_LoginResponse.GateIPAddress));
            self.session.AddComponent<DestroyMoniterComponent, Action<object>, object>((obj) =>
            {
                self.session = null;
                BroadcastComponent.Instance.g_default.Run(BroadcastId.UnLogined);
            }, null);

            try
            {
                _response = await self.session.Call(new C2G_LoginGate()
                {
                    LoginKey = _LoginResponse.LoginKey,
                    MachineCode = PlatformHelper.GetMachineCode()
                });
                if (_instanceId != self.InstanceId) return;

                if (_response.Error != ErrorCode.ERR_Success)
                {
                    throw new Exception($"登录网关失败:{_response.Message}");
                }
            }
            catch(Exception error)
            {
                if (_instanceId != self.InstanceId) return;

                self.session.Dispose();
                self.session = null;

                throw error;
            }

            G2C_LoginGate _gateResponse = _response as G2C_LoginGate;
            _account.longKey = _gateResponse.LongKey;

            #endregion

            self.session.AddComponent<HeartbeatComponent>();

            while (self.loginSteps.Count>0 && self.session!=null)
            {
                await GameObjectHelper.WaitOneFrame();
            }

            if (self.session == null) return;
            BroadcastComponent.Instance.g_default.Run(BroadcastId.Logined);
        }
    }
}
