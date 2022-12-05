using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
    public class DestroyMoniterComponent : Component
    {
        public Action<object> callback;
        public object scope;
    }

    [ObjectSystem]
    public class DestroyMoniterComponentAwakeSystem : AwakeSystem<DestroyMoniterComponent, Action<object>, object>
    {
        public override void Awake(DestroyMoniterComponent self, Action<object> a, object b)
        {
            self.callback = a;
            self.scope = b;
        }
    }

    [ObjectSystem]
    public class DestroyMoniterComponentDestroySystem : DestroySystem<DestroyMoniterComponent>
    {
        public override void Destroy(DestroyMoniterComponent self)
        {
            self.callback?.Invoke(self.scope);
            self.callback = null;
            self.scope = null;
        }
    }
}
