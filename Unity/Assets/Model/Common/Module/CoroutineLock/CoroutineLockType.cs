namespace ETModel
{
    public enum CoroutineLockType
    {
        None = 0,
        Location, // location进程上使用
        ActorLocationSender, // ActorLocationSender中队列消息 
        Mailbox, // Mailbox中队列
        
        // Client
        Resources,
        ResourcesLoader,
        
        Max, // 这个必须在最后
    }
}