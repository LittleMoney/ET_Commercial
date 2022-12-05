using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    [ObjectSystem]
    public class DestoryTimerComponentAwakeSystem : AwakeSystem<DestoryTimerComponent, long>
    {

        public override void Awake(DestoryTimerComponent self, long ticks)
        {
            self.Awake(ticks).Coroutine();
        }
    }

    /// <summary>
    /// 销毁定时器
    /// </summary>
    public class DestoryTimerComponent : Component
    {
        ETCancellationTokenSource cts;

        /// <summary>
        /// 指定时间到达时销毁entity
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public async ETVoid Awake(long ticks)
        {
            this.cts = new ETCancellationTokenSource();
            try
            {
                await TimerComponent.Instance.WaitAsync(ticks, this.cts.Token);
                this.Parent.Dispose();
            }
            finally
            {
                this.cts.Dispose();
                this.cts = null;
            }

        }

        public override void Dispose()
        {
            if (IsDisposed) return;

            base.Dispose();

            if (cts != null)
            {
                cts.Cancel();
            }
        }
    }
}
