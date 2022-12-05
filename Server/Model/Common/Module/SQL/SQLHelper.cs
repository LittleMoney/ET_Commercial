using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    public static partial class SQLHelper
    {
        public static SQLTask AttachExecuteComponent(SQLTask dbTask)
        {
            switch (dbTask.executeType)
            {
                case SQLTask.ExecuteType.NonQuery:
                    break;
                case SQLTask.ExecuteType.DataSet:
                    dbTask.AddComponent<SQLDataSetComponent>();
                    break;
                case SQLTask.ExecuteType.Scalar:
                    dbTask.AddComponent<SQLScalarComponent>();
                    break;
                case SQLTask.ExecuteType.SingleRow:
                    dbTask.AddComponent<SQLSingleRowComponent>();
                    break;
            }

            return dbTask;
        }
    }
}
