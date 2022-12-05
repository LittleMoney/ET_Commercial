using ETModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ETHotfix
{
    [MessageHandler(AppType.Realm)]
    public class C2R_RegisterHandler : AMRpcHandler<C2R_Register, R2C_Register>
    {
        protected override async ETTask Run(Session session, C2R_Register request, R2C_Register response, Action reply)
        {
            RealmConfig _realmConfig = Game.Scene.GetComponent<ConfigComponent>().Get(typeof(RealmConfig), Game.Scene.GetComponent<StartConfigComponent>().StartConfig.AppId) as RealmConfig;

            long _sessionInstanceId = session.InstanceId;

            //服务器不允许注册
            if (_realmConfig.CanRegister != 1)
            {
                response.Error = ErrorCode.ERR_Exception;
                response.Message = _realmConfig.RefuseRegisterMessage.ToString();
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
                session.AddComponent<DestoryTimerComponent,long>(30 * 1000);
                return;
            }

            using (SQLTask _dbTask = Game.Scene.GetComponent<SQLComponent>().CreateTask(SQLName.GameUser, SQLTask.ExecuteType.SingleRow))
            {

                _dbTask.SetCommandText("GSP_GP_EfficacyAccounts", true);
                _dbTask.AddParameter("@Accounts", request.Accounts);
                _dbTask.AddParameter("@Password", request.Password);
                _dbTask.AddParameter("@NickName", request.NickName);
                _dbTask.AddParameter("@SpreadCode", request.SpreadCode);
                _dbTask.AddParameter("@MobilePhone", request.Accounts);
                _dbTask.AddParameter("@ClientIP", session.RemoteAddress.Address.ToString());
                _dbTask.AddParameter("@MachineSerial", request.MachineCode);
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
                    response.Message = "由于数据库操作异常，请您稍后重新注册！";
                    reply();

                    //定时关闭
                    session.Send((MemoryStream)null);
                    session.AddComponent<DestoryTimerComponent,long>(30 * 1000);
                    return;
                }


                int _returnValue = (int)_dbTask.GetParameterValue("@ReturnValue");
                Dictionary<string, object> _rowData = _dbTask.GetComponent<SQLSingleRowComponent>().rowData;

                if (_returnValue == 0)
                {
                    #region 注册成功

                    response.Error = ErrorCode.ERR_Success;
                    reply();

                    session.Send((MemoryStream)null);
                    session.AddComponent<DestoryTimerComponent,long>(30 * 1000);

                    #endregion
                }
                else
                {
                    #region 注册失败

                    response.Error = ErrorCode.ERR_Exception;
                    response.Message = _dbTask.GetParameterValue("@ErrorDescribe").ToString();
                    reply();

                    session.Send((MemoryStream)null);
                    session.AddComponent<DestoryTimerComponent,long>( 30 * 1000);

                    #endregion
                }
            }
        }
    }
}