using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace ETModel
{
    public class ETTaskCompletionSource: IAwaiter
    {

        private static Queue<ETTaskCompletionSource> ecsPools = new Queue<ETTaskCompletionSource>();
        private static int ecsPoolMax = 100;

        public static void SetPoolMax(int maxCount)
        {
            ecsPoolMax = maxCount;
        }

        public static ETTaskCompletionSource GetFormPool()
        {
            if (ecsPools.Count > 0)
            {
                return ecsPools.Dequeue();
            }
            else
            {
                return new ETTaskCompletionSource();
            }
        }

        public static void RecycleToPool(ETTaskCompletionSource ecs)
        {

            if(ecsPools.Count< ecsPoolMax)
            {
                ecs.Reset();
                ecsPools.Enqueue(ecs);
            }
        }

        // State(= AwaiterStatus)
        private const int Pending = 0;
        private const int Succeeded = 1;
        private const int Faulted = 2;
        private const int Canceled = 3;

        private int state;
        private ExceptionDispatchInfo exception;
        private Action continuation; // action or list

        AwaiterStatus IAwaiter.Status => (AwaiterStatus) state;

        bool IAwaiter.IsCompleted => state != Pending;

        public ETTask Task => new ETTask(this);

        void IAwaiter.GetResult()
        {
            switch (this.state)
            {
                case Succeeded:
                    return;
                case Faulted:
                    this.exception?.Throw();
                    this.exception = null;
                    return;
                case Canceled:
                {
                    this.exception?.Throw(); // guranteed operation canceled exception.
                    this.exception = null;
                    throw new OperationCanceledException();
                }
                default:
                    throw new NotSupportedException("ETTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }

        public void Reset()
        {
            state = Pending;
            exception = null;
            continuation = null;
        }

        void ICriticalNotifyCompletion.UnsafeOnCompleted(Action action)
        {
            this.continuation = action;
            if (state != Pending)
            {
                TryInvokeContinuation();
            }
        }

        private void TryInvokeContinuation()
        {
            this.continuation?.Invoke();
            this.continuation = null;
        }

        public void SetResult()
        {
            if (this.TrySetResult())
            {
                return;
            }

            throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
        }

        public void SetException(Exception e)
        {
            if (this.TrySetException(e))
            {
                return;
            }

            throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
        }

        public bool TrySetResult()
        {
            if (this.state != Pending)
            {
                return false;
            }

            this.state = Succeeded;

            this.TryInvokeContinuation();
            return true;

        }

        public bool TrySetException(Exception e)
        {
            if (this.state != Pending)
            {
                return false;
            }

            this.state = Faulted;

            this.exception = ExceptionDispatchInfo.Capture(e);
            this.TryInvokeContinuation();
            return true;

        }

        public bool TrySetCanceled()
        {
            if (this.state != Pending)
            {
                return false;
            }

            this.state = Canceled;

            this.TryInvokeContinuation();
            return true;

        }

        public bool TrySetCanceled(OperationCanceledException e)
        {
            if (this.state != Pending)
            {
                return false;
            }

            this.state = Canceled;

            this.exception = ExceptionDispatchInfo.Capture(e);
            this.TryInvokeContinuation();
            return true;

        }

        void INotifyCompletion.OnCompleted(Action action)
        {
            ((ICriticalNotifyCompletion) this).UnsafeOnCompleted(action);
        }
    }

    public class ETTaskCompletionSource<T>: IAwaiter<T>
    {

        private static Queue<ETTaskCompletionSource<T>> ecsPools = new Queue<ETTaskCompletionSource<T>>();
        private static int ecsPoolMax = 100;

        public static void SetPoolMax(int maxCount)
        {
            ecsPoolMax = maxCount;
        }

        public static ETTaskCompletionSource<T> GetFormPool()
        {
            if (ecsPools.Count > 0)
            {
                return ecsPools.Dequeue();
            }
            else
            {
                return new ETTaskCompletionSource<T>();
            }
        }

        public static void RecycleToPool(ETTaskCompletionSource<T> ecs)
        {

            if (ecsPools.Count < ecsPoolMax)
            {
                ecs.Reset();
                ecsPools.Enqueue(ecs);
            }
        }


        // State(= AwaiterStatus)
        private const int Pending = 0;
        private const int Succeeded = 1;
        private const int Faulted = 2;
        private const int Canceled = 3;

        private int state;
        private T value;
        private ExceptionDispatchInfo exception;
        private Action continuation; // action or list

        bool IAwaiter.IsCompleted => state != Pending;

        public ETTask<T> Task => new ETTask<T>(this);

        AwaiterStatus IAwaiter.Status => (AwaiterStatus) state;

        T IAwaiter<T>.GetResult()
        {
            switch (this.state)
            {
                case Succeeded:
                    return this.value;
                case Faulted:
                    this.exception?.Throw();
                    this.exception = null;
                    return default;
                case Canceled:
                {
                    this.exception?.Throw(); // guranteed operation canceled exception.
                    this.exception = null;
                    throw new OperationCanceledException();
                }
                default:
                    throw new NotSupportedException("ETTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }

        public void Reset()
        {
            state = Pending;
            value = default(T);
            exception = null;
            continuation = null;
        }

        void ICriticalNotifyCompletion.UnsafeOnCompleted(Action action)
        {
            this.continuation = action;
            if (state != Pending)
            {
                TryInvokeContinuation();
            }
        }

        private void TryInvokeContinuation()
        {
            this.continuation?.Invoke();
            this.continuation = null;
        }

        public void SetResult(T result)
        {
            if (this.TrySetResult(result))
            {
                return;
            }

            throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
        }

        public void SetException(Exception e)
        {
            if (this.TrySetException(e))
            {
                return;
            }

            throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
        }

        public bool TrySetResult(T result)
        {
            if (this.state != Pending)
            {
                return false;
            }

            this.state = Succeeded;

            this.value = result;
            this.TryInvokeContinuation();
            return true;

        }

        public bool TrySetException(Exception e)
        {
            if (this.state != Pending)
            {
                return false;
            }

            this.state = Faulted;

            this.exception = ExceptionDispatchInfo.Capture(e);
            this.TryInvokeContinuation();
            return true;

        }

        public bool TrySetCanceled()
        {
            if (this.state != Pending)
            {
                return false;
            }

            this.state = Canceled;

            this.TryInvokeContinuation();
            return true;

        }

        public bool TrySetCanceled(OperationCanceledException e)
        {
            if (this.state != Pending)
            {
                return false;
            }

            this.state = Canceled;

            this.exception = ExceptionDispatchInfo.Capture(e);
            this.TryInvokeContinuation();
            return true;

        }

        void IAwaiter.GetResult()
        {
            ((IAwaiter<T>) this).GetResult();
        }

        void INotifyCompletion.OnCompleted(Action action)
        {
            ((ICriticalNotifyCompletion) this).UnsafeOnCompleted(action);
        }
    }
}