using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace ETModel
{
    [ObjectSystem]
    public class SQLDataSetTaskAwakeSystem : AwakeSystem<SQLDataSetComponent>
    {
        public override void Awake(SQLDataSetComponent self)
        {
            self.Awake();
        }
    }



    public class SQLDataSetComponent : Component, ISQLExecuter
    {
        public DataSet dataSet;

        public SqlDataAdapter adapter=new SqlDataAdapter();

        /// <summary>
        /// 初始化
        /// </summary>
        public void Awake()
        {
            (Parent as SQLTask).SetRunner(this);
            dataSet = null;
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

            dataSet = null;

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


    public static class DBDataSetComponentSystem 
    {
        public static async ETTask OnRun(this SQLDataSetComponent self)
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
                
                self.adapter.SelectCommand = _dbTask.command;

                await System.Threading.Tasks.Task.Run(() =>
                {
                    self.dataSet = new DataSet();
                    self.adapter.Fill(self.dataSet);
                });

                if (_instanceId != self.InstanceId) return;

                _dbTask.command.Connection = null;
                self.adapter.SelectCommand = null;
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
