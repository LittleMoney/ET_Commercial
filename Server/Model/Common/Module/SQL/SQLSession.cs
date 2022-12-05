using System.Net;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace ETModel
{

    [ObjectSystem]
    public class SQLSessionAwakeSystem : AwakeSystem<SQLSession, string, int, string>
    {
        public override void Awake(SQLSession self, string name, int maxConnectionCount, string connectionString)
        {
            self.Awake(name, maxConnectionCount, connectionString);
        }
    }


    /// <summary>
    /// 用来与数据库操作代理
    /// </summary>
    public class SQLSession :Component
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string dbName;

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string connectionString;

        /// <summary>
        /// 最大连接数量
        /// </summary>
        public int maxConnectionCount;

        /// <summary>
        /// 连接检查间隙
        /// </summary>
        public long connectionCenstorInterval;

        /// <summary>
        /// 
        /// </summary>
        public List<SqlConnection> connections = null;

        /// <summary>
        /// 
        /// </summary>
        public List<SQLTaskQueue> runQueues = null;


        /// <summary>
        /// 当前分配的队列索引
        /// </summary>
        public int currentAssignIndex = 0;


        /// <summary>
        /// 
        /// </summary>
        public void Awake(string name, int maxConnectionCount, string connectionString)
        {
            this.dbName = name;
            this.connectionString = connectionString;
            this.maxConnectionCount = maxConnectionCount;
            connectionCenstorInterval = 60000;

            connections = new List<SqlConnection>(maxConnectionCount);
            runQueues = new List<SQLTaskQueue>(maxConnectionCount);

            SqlConnection _connection = null;
            for (int i = 0; i < maxConnectionCount; ++i)
            {
                try
                {
                    _connection = new SqlConnection(connectionString);
                    runQueues.Add(ComponentFactory.Create<SQLTaskQueue>());
                    connections.Add(_connection);

               
                    _connection.StateChange += _connection_StateChange;
                    _connection.OpenAsync();
                }
                catch(Exception error)
                {
                    Log.Error($"{this.dbName} 链接数据库错误{this.connectionString} error: {error}");
                }
            }

            this.OnStartCensorAsync().Coroutine();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            if(IsDisposed)
            {
                return;
            }

            base.Dispose();

            foreach (SQLTaskQueue _taskQueue in runQueues)
            {
                _taskQueue.Dispose();
            }
            runQueues.Clear();

            foreach (SqlConnection _connection in connections)
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    _connection.Close();
                }

                _connection.StateChange -= _connection_StateChange;
            }
            connections.Clear();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _connection_StateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState != ConnectionState.Executing)
            {
                Log.Info($"{dbName} 状态变更 { e.CurrentState} ");
            }
        }
    }


    public static class DBSessionSystem
    {
        /// <summary>
        /// 
        /// </summary>
        public static SQLTask CreateTask(this SQLSession self, SQLTask.ExecuteType executeType)
        {

            SQLTask _dbTask = ComponentFactory.CreateWithParent<SQLTask, SQLTask.ExecuteType>(self, executeType);
            SQLHelper.AttachExecuteComponent(_dbTask);
            //self.tasks.Add(_dbTask);

            return _dbTask;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dBTask"></param>
        public static void ToRun(this SQLSession self, SQLTask dbTask)
        {
            if (dbTask.Parent!=self) throw new System.Exception("对不起 dbTask 和 dbSession 不匹配");

            int _runQueueIndex = -1;
            for (int i = 0; i < self.maxConnectionCount; i++)
            {
                int _index = (self.currentAssignIndex + i) % self.maxConnectionCount;
                if (self.connections[_index].State == ConnectionState.Open)
                {
                    _runQueueIndex = _index;
                    break;
                }
            }

            if (_runQueueIndex == -1)
            {
                throw new System.Exception("没有可用的数据库连接");
            }

            dbTask.SetRunQueueIndex(_runQueueIndex);
            self.runQueues[dbTask.runQueueIndex].Add(dbTask);
            self.currentAssignIndex = _runQueueIndex++;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async ETVoid OnStartCensorAsync(this SQLSession self)
        {
            long _instanceId = self.InstanceId;

            while (_instanceId == self.InstanceId)
            {
                await Game.Scene.GetComponent<TimerComponent>().WaitAsync(self.connectionCenstorInterval);

                foreach (SqlConnection _connection in self.connections)
                {
                    if (_instanceId != self.InstanceId) break;

                    if (_connection.State == ConnectionState.Closed || _connection.State == ConnectionState.Broken)
                    {
                        try
                        {
                            await _connection.OpenAsync();
                        }
                        catch(Exception error)
                        {
                            Log.Error($"{self.dbName} 链接数据库错误{self.connectionString} error: {error}");
                        }
                    }
                }
            }
        }
    }
}