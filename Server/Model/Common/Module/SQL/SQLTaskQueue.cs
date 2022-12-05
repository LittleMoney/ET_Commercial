using System;
using System.Collections.Generic;

namespace ETModel
{
    [ObjectSystem]
    public class SQLTaskQueueAwakeSystem : AwakeSystem<SQLTaskQueue>
    {
        public override void Awake(SQLTaskQueue self)
        {
            self.queue.Clear();
        }
    }

    [ObjectSystem]
    public class SQLTaskQueueStartSystem : StartSystem<SQLTaskQueue>
    {
        public override void Start(SQLTaskQueue self)
        {
            StartAsync(self).Coroutine();
        }

        public async ETVoid StartAsync(SQLTaskQueue self)
        {
            long instanceId = self.InstanceId;

            while (self.InstanceId == instanceId)
            {

                SQLTask task = await self.Get();

                try
                {
                    await task.Run();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

                //task.Dispose(); 执行完成后由外界决定是否销毁
            }
        }
    }

    public sealed class SQLTaskQueue : Component
    {
        public Queue<SQLTask> queue = new Queue<SQLTask>();

        public ETTaskCompletionSource<SQLTask> tcs;

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            base.Dispose();


            if (this.queue.Count > 0)
            {
                Exception _error = new Exception("执行队列关闭");

                foreach (SQLTask dbTask in this.queue)
                {
                    dbTask.Abort(_error);
                }

                this.queue.Clear();
            }

        }

        public void Add(SQLTask task)
        {
            if (this.tcs != null)
            {
                var t = this.tcs;
                this.tcs = null;
                t.SetResult(task);
                return;
            }

            this.queue.Enqueue(task);
        }

        public ETTask<SQLTask> Get()
        {
            if (this.queue.Count > 0)
            {
                SQLTask task = this.queue.Dequeue();
                return ETTask.FromResult(task);
            }

            ETTaskCompletionSource<SQLTask> t = new ETTaskCompletionSource<SQLTask>();
            this.tcs = t;
            return t.Task;
        }
    }
}