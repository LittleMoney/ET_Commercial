using System;
using System.Net;
using System.Text;
using ETModel;
using System.Collections.Generic;
using System.IO;


namespace ETHotfix
{
    [MessageHandler(AppType.Realm)]
    public class C2R_LoginHandler : AMRpcHandler<C2R_Login, R2C_Login>
    {
        protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response, Action reply)
        {
            RealmConfig _realmConfig = Game.Scene.GetComponent<ConfigComponent>().Get(typeof(RealmConfig), Game.Scene.GetComponent<StartConfigComponent>().StartConfig.AppId) as RealmConfig;

            long _sessionInstanceId = session.InstanceId;

            //服务器不允许登录
            if (_realmConfig.CanLogon != 1)
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "请选择其他服务器，会有更好的体验！...";
                reply();
                session.Send((MemoryStream)null);
                session.AddComponent<DestoryTimerComponent,long>(30 * 1000);

                return;
            }

            //IP是否被限制登录
            int _tag = (Game.Scene.GetComponent<ConfigComponent>().GetCategory(typeof(RealmIPCloseConfig)) as RealmIPCloseConfigCategory).TryGetFlag(session.RemoteAddress.ToString());
            if (_tag == 1)
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "登录失败";
                reply();

                //定时关闭
                session.Send((MemoryStream)null);
                session.AddComponent<DestoryTimerComponent, long>(30 * 1000);
                return;
            }

            //检验服务器版本
            if (request.ServerVersion != _realmConfig.ServerVersion)
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = "登录客户端请求的服务器版本不匹配！";
                reply();

                //定时关闭
                session.Send((MemoryStream)null);
                session.AddComponent<DestoryTimerComponent, long>( 30 * 1000);

                return;
            }

            using (SQLTask _dbTask = Game.Scene.GetComponent<SQLComponent>().CreateTask(SQLName.GameUser, SQLTask.ExecuteType.SingleRow))
            {
                _dbTask.SetCommandText("GSP_GP_EfficacyAccounts", true);
                _dbTask.AddParameter("@Accounts", request.Accounts);
                _dbTask.AddParameter("@Password", request.Password);
                _dbTask.AddParameter("@ChannelId", request.ChannelId);
                _dbTask.AddParameter("@ClientIP", session.RemoteAddress.Address.ToString());
                _dbTask.AddParameter("@MachineSerial", request.MachineCode.ToString());
                _dbTask.AddParameter("@ErrorDescribe",SQLTask.ParameterDirection.Out,SQLTask.ParameterType.String,null);

                try
                {
                    await _dbTask.Execute();

                    if (_sessionInstanceId != session.InstanceId) return;
                }
                catch (Exception error)
                {
                    Log.Error(error);

                    if (_sessionInstanceId != session.InstanceId) return;

                    response.Error = ErrorCode.ERR_Exception;
                    response.Message = "由于数据库操作异常，请您稍后重试或选择另一服务器登录！";
                    reply();

                    //定时关闭
                    session.Send((MemoryStream)null);
                    session.AddComponent<DestoryTimerComponent, long>( 30 * 1000);
                    return;
                }


                int _returnValue = (int)_dbTask.GetParameterValue("@ReturnValue");
                Dictionary<string, object> _rowData = _dbTask.GetComponent<SQLSingleRowComponent>().rowData;

                if (_returnValue == 0)
                {
                    #region 登录成功

                    //选择网关注册key
                    List<StartConfig> _gateConfigs = Game.Scene.GetComponent<StartConfigComponent>().GateConfigs;
                    StartConfig _gateStartConfig = _gateConfigs[new System.Random().Next(0, _gateConfigs.Count - 1)];
                    Session _gateSession = Game.Scene.GetComponent<NetInnerComponent>().Get(_gateStartConfig.GetComponent<InnerConfig>().IPEndPoint);

                    IResponse _responseGetLoginKey = null;
                    try
                    {
                        //-------------
                        _responseGetLoginKey = await _gateSession.Call(new R2G_GetLoginKey()
                        {
                            UserId = (int)_rowData["UserId"],
                            IP = session.RemoteAddress.ToString(),
                            MachineSerial = request.MachineCode
                        }); 

                        if (_sessionInstanceId != session.InstanceId) return;


                        if (_responseGetLoginKey.Error != ErrorCode.ERR_Success)
                        {
                            throw new Exception(_responseGetLoginKey.Message);
                        }
                    }
                    catch (Exception error)
                    {
                        response.Error = ErrorCode.ERR_Exception;
                        response.Message = "获取对应的网关信息失败,请重新登录！";
                        reply();

                        //定时关闭
                        session.Send((MemoryStream)null);
                        session.AddComponent<DestoryTimerComponent, long>( 30 * 1000);
                        return;
                    }

                    response.GateIPAddress = _gateStartConfig.AddComponent<OuterConfig>().Address;
                    response.LoginKey = (_responseGetLoginKey as G2R_GetLoginKey).LoginKey;
                    reply();

                    session.Send((MemoryStream)null);
                    session.AddComponent<DestoryTimerComponent, long>(60 * 1000);

                    #endregion
                }
                else
                {
                    #region 登录返回失败
                    response.Error = ErrorCode.ERR_Exception;
                    response.Message = _dbTask.GetParameterValue("@ErrorDescribe").ToString();
                    reply();

                    session.Send((MemoryStream)null);
                    session.AddComponent<DestoryTimerComponent, long>(30 * 1000);
                    #endregion
                }
            }
        }
    }
}