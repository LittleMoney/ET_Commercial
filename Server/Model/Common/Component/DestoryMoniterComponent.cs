using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{

    [ObjectSystem]
    public class DestroyMoniterComponentDestorySystem : DestroySystem<DestroyMoniterComponent>
    {
        public override void Destroy(DestroyMoniterComponent self)
        {
            if (self.s_callback != null) self.s_callback(self.s_scope);
            self.s_scope = null;
            self.s_callback = null;
        }
    }

    public class DestroyMoniterComponent:Component
    {
        public object s_scope;
        public Action<object> s_callback;
    }
}
