using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace ETModel
{
    [ObjectSystem]
    public class SQLScalarComponentAwakeSystem : AwakeSystem<SQLScalarComponent>
    {
        public override void Awake(SQLScalarComponent self)
        {
            self.Awake();
        }
    }



    public class SQLScalarComponent : Component, ISQLExecuter
    {
        public object  scalar=null;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Awake()
        {
            (Parent as SQLTask).SetRunner(this);
            scalar = null;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            scalar = null;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ETTask Run()
        {
            return this.OnRun();
        }
    }


    public static class DBScalarComponentSystem
    {
        public static async ETTask OnRun(this SQLScalarComponent self)
        {
            SQLTask _dbTask = (self.Parent as SQLTask);
            SQLSession _dbSession = _dbTask.GetDBSession();

            if (_dbSession==null)
            {
                _dbTask.tcs.SetException(new Exception("DBSeesion已经被销毁"));
                return;
            }

            SqlConnection _connection = _dbSession.connections[_dbTask.runQueueIndex];


            if (_connection.State != ConnectionState.Open)
            {
                _dbTask.tcs.SetException(new Exception("数据库连接不能使用"));
                return;
            }

            _dbTask.command.Connection = _connection;

            long _instanceId = self.InstanceId;

            try
            {
                

                self.scalar = await _dbTask.command.ExecuteScalarAsync();

                if (_instanceId != self.InstanceId) return;

                _dbTask.command.Connection = null;
            }
            catch (Exception error)
            {
                if (_instanceId != self.InstanceId) return;

                _dbTask.tcs.SetException(error);
                return;
            }

            _dbTask.tcs.SetResult();
        }
    }

}
