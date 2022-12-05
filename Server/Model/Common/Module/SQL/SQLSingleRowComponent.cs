using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace ETModel
{
    [ObjectSystem]
    public class SQLSingleRowComponentAwakeSystem : AwakeSystem<SQLSingleRowComponent>
    {
        public override void Awake(SQLSingleRowComponent self)
        {
            self.Awake();
        }
    }



    public class SQLSingleRowComponent : Component, ISQLExecuter
    {
        public Dictionary<string,object> rowData = new Dictionary<string, object>();

        /// <summary>
        /// 初始化
        /// </summary>
        public void Awake()
        {
            (Parent as SQLTask).SetRunner(this);
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

            rowData.Clear();
            base.Dispose();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public  ETTask Run()
        {
            return this.OnRun();
        }
    }


    public static class DBSingleRowComponentSystem
    {
        public static async ETTask OnRun(this SQLSingleRowComponent self)
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
                

                using (SqlDataReader _reader = await _dbTask.command.ExecuteReaderAsync())
                {
                    if (_instanceId != self.InstanceId) return; //如果自己已被销毁

                    if (_reader.HasRows &&　await _reader.ReadAsync())
                    {
                        if (_instanceId != self.InstanceId) return; //如果自己已被销毁

                        for (int i = 0; i < _reader.FieldCount; i++)
                        {
                            self.rowData.Add(_reader.GetName(i), _reader.GetValue(i));
                        }
                    }
                }

                _dbTask.command.Connection = null;
            }
            catch (Exception error)
            {
                if (_instanceId != self.InstanceId) return; //如果自己已被销毁

                _dbTask.tcs.SetException(error);
                return;
            }

            _dbTask.tcs.SetResult();
        }
    }

}
