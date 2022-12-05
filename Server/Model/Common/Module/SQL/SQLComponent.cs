using System.Net;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ETModel
{


    [ObjectSystem]
    public class SQLComponentAwakeSystem : AwakeSystem<SQLComponent>
    {
        public override void Awake(SQLComponent self)
        {
            self.Awake();
        }
    }


    /// <summary>
    /// 用来与数据库操作代理
    /// </summary>
    public class SQLComponent : Component
    {
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string,SQLSession> dbSessions = new Dictionary<string, SQLSession>();

        /// <summary>
        /// 
        /// </summary>
        public void Awake()
        {

            SQLConfig config = Game.Scene.GetComponent<StartConfigComponent>().StartConfig.GetComponent<SQLConfig>();

            for (int i = 0; i < config.Sessions.Length; i++)
            {
                SQLSessionConfig _sessionConfig = config.Sessions[i];
                SQLSession _dbSession = ComponentFactory.CreateWithParent<SQLSession,string, int,string>(this, _sessionConfig.Name, _sessionConfig.MaxConnectionCount, _sessionConfig.ConnectionString);
                dbSessions.Add(_dbSession.dbName,_dbSession);
            }
        }

        public override void Dispose()
        {

            base.Dispose();

            foreach (KeyValuePair<string, SQLSession> _keyValue in dbSessions)
            {
                _keyValue.Value.Parent = null;
                _keyValue.Value.Dispose();
            }
            dbSessions.Clear();
            dbSessions = null;
        }
    }


    public static class SQLComponentSystem
    { 
        /// <summary>
        /// 
        /// </summary>
        public static SQLTask CreateTask(this SQLComponent self,string dbName,SQLTask.ExecuteType executeType)
        {
            return self.dbSessions[dbName].CreateTask(executeType);
        }

    }
}