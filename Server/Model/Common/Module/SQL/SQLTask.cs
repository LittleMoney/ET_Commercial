using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;


namespace ETModel
{
    [ObjectSystem]
    public class SQLTaskAwakeSystem : AwakeSystem<SQLTask, SQLTask.ExecuteType>
    {
        public override void Awake(SQLTask self, SQLTask.ExecuteType executeType)
        {
            self.Awake(executeType);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class SQLTask : Entity
    {
        public enum ParameterDirection
        {
            In,
            Out,
            InOut
        }

        public enum ParameterType
        {
            Int,
            Long,
            String,
            WString,
            Boolean,
            DateTime,
            Date
        }

        public enum ExecuteType
        {
            NonQuery,
            DataSet,
            Scalar,
            SingleRow
        }


        public ISQLExecuter runner =null;

        public int runQueueIndex = 0;

        public ExecuteType executeType=ExecuteType.NonQuery;

        public long parentInstanceId = 0;

        public SqlCommand command = new SqlCommand();

        public ETTaskCompletionSource tcs = null;





        /// <summary>
        /// 初始化
        /// </summary>
        public void Awake(ExecuteType executeType)
        {
            this.executeType = executeType;
            parentInstanceId = Parent.InstanceId;
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

            if (tcs != null && !tcs.Task.IsCompleted)
            {
                tcs.SetException(new Exception("DBTask被销毁"));
                tcs = null;
            }

            try
            {
                if (command.Connection != null)
                {
                    if (command.Connection.State == ConnectionState.Executing)
                    {
                        throw new Exception("这个worker正在执行，不能清理");
                    }
                    command.Connection = null;
                }
            }
            catch
            {

            }

            executeType = default(ExecuteType);
            runner = null;
            runQueueIndex = 0;
            command.CommandTimeout = 0;
            command.Parameters.Clear();

            Parent = null;

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class DBTaskSystem
    {
        /// <summary>
        /// 获取自己的DBSession
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static SQLSession GetDBSession(this SQLTask self)
        {
            return self.parentInstanceId == self.Parent.InstanceId ? (self.Parent as SQLSession) : null;
        }

        /// <summary>
        /// (不要在外部调用)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="queueIndex"></param>
        public static void SetRunner(this SQLTask self, ISQLExecuter runner)
        {
            self.runner = runner;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="queueIndex"></param>
        public static void SetRunQueueIndex(this SQLTask self, int queueIndex)
        {
            self.runQueueIndex = queueIndex;
        }

        /// <summary>
        /// 设置等待超时时间
        /// </summary>
        /// <param name="time"></param>
        public static void SetTimeout(this SQLTask self,int time)
        {
            if (self.tcs != null) throw new Exception("任务执行中，不能修改");
            self.command.CommandTimeout = time;
        }

        /// <summary>
        /// 设置命令参数和类型
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isStoredProcedure"></param>
        public static void SetCommandText(this SQLTask self, string text, bool isStoredProcedure)
        {
            if (self.tcs != null) throw new Exception("任务执行中，不能修改");

            self.command.CommandText = text;
            if (isStoredProcedure)
            {
                self.command.CommandType = CommandType.StoredProcedure;
            }
        }

        /// <summary>
        /// 添加命令参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="direction"></param>
        /// <param name="value"></param>
        public static void AddParameter(this SQLTask self, string name, object value)
        {
            if (self.tcs != null) throw new Exception("任务执行中，不能修改");

            SqlParameter _sqlParameter = new SqlParameter();
            _sqlParameter.ParameterName = name;
            _sqlParameter.Value = value == null ? DBNull.Value : value;
            self.command.Parameters.Add(_sqlParameter);
        }

        /// <summary>
        /// 添加命令参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="direction"></param>
        /// <param name="value"></param>
        public static void AddParameter(this SQLTask self, string name, SQLTask.ParameterDirection direction, SQLTask.ParameterType type, object value)
        {
            if (self.tcs != null) throw new Exception("任务执行中，不能修改");

            SqlParameter _sqlParameter = new SqlParameter();
            _sqlParameter.ParameterName = name;
            _sqlParameter.SqlDbType = SqlDbType.VarChar;
            _sqlParameter.Value = value == null ? DBNull.Value : value;
            _sqlParameter.Direction = System.Data.ParameterDirection.Input;

            self.command.Parameters.Add(_sqlParameter);
        }

        /// <summary>
        /// 读取参数的值
        /// </summary>
        /// <param name="name"></param>
        public static object GetParameterValue(this SQLTask self, string name)
        {
            if (self.tcs != null) throw new Exception("任务执行中，不能修改");

            return self.command.Parameters[name].Value;
        }

        /// <summary>
        /// 转换成db类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SqlDbType SwitchSqlDBType(this SQLTask self, SQLTask.ParameterType type)
        {
            switch (type)
            {
                case SQLTask.ParameterType.Boolean:
                    return SqlDbType.Bit;
                case SQLTask.ParameterType.Int:
                    return SqlDbType.Int;
                case SQLTask.ParameterType.Long:
                    return SqlDbType.BigInt;
                case SQLTask.ParameterType.String:
                    return SqlDbType.VarChar;
                case SQLTask.ParameterType.WString:
                    return SqlDbType.NVarChar;
                case SQLTask.ParameterType.DateTime:
                    return SqlDbType.DateTime;
                case SQLTask.ParameterType.Date:
                    return SqlDbType.Date;
                default:
                    return SqlDbType.VarChar;
            }
        }

        /// <summary>
        /// 转换成db类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static System.Data.ParameterDirection SwitchDBDirection(this SQLTask self, SQLTask.ParameterDirection direction)
        {
            switch (direction)
            {
                case SQLTask.ParameterDirection.In:
                    return System.Data.ParameterDirection.Input;
                case SQLTask.ParameterDirection.Out:
                    return System.Data.ParameterDirection.Output;
                case SQLTask.ParameterDirection.InOut:
                    return System.Data.ParameterDirection.InputOutput;
                default:
                    return System.Data.ParameterDirection.Input;
            }
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        public static ETTask Execute(this SQLTask self)
        {
            SQLSession _dbSession = self.GetDBSession();
            if (_dbSession == null) return ETTask.FromException(new Exception("绑定的 DBSession 已经被销毁"));

            self.tcs = new ETTaskCompletionSource();
            try
            {
                _dbSession.ToRun(self);
                return self.tcs.Task;
            }
            catch(Exception error)
            {
                return ETTask.FromException(error);
            }
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="self"></param>
        public static ETTask Run(this SQLTask self)
        {
            if (self.runner != null)
            {
                return self.runner.Run();
            }
            else
            {
                return self.OnRun();
            }
        }

        /// <summary>
        /// 取消执行
        /// </summary>
        /// <param name="self"></param>
        public static void Abort(this SQLTask self,Exception error)
        {
            self.tcs.SetException(error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async ETTask OnRun(this SQLTask self)
        {

            SQLSession _dbSession = self.GetDBSession();

            if (_dbSession == null)
            {
                self.tcs.SetException(new Exception("DBSeesion已经被销毁"));
                return;
            }

            SqlConnection _connection = _dbSession.connections[self.runQueueIndex];


            if(_connection.State!=ConnectionState.Open)
            {
                self.tcs.SetException(new Exception("数据库连接不能使用"));
                return;
            }

            self.command.Connection = _connection;
            long _instanceId = self.InstanceId;

            try
            {
                await self.command.ExecuteNonQueryAsync();
                
                self.tcs.SetResult();
            }
            catch (Exception error)
            {
                if (_instanceId != self.InstanceId) return;

                self.tcs.SetException(error);
            }
            return;
        }

    }
}
