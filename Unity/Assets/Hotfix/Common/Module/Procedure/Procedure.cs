using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHotfix
{
    public class Procedure:EntityI
    {
        public string name;
    }

    [ObjectSystem]
    public class ProcedureAwakeSystem : AwakeSystem<Procedure, string>
    {
        public override void Awake(Procedure self, string a)
        {
            self.name = a;
            Type _type = System.Type.GetType($"ETHotfix.{self.name}");
            if(_type==null)
            {
                _type = System.Type.GetType($"ETHotfix.{self.name}Component");
            }
            self.AddComponent(_type);
        }
    }

    public static class ProcedureSystem
    {

        /// <summary>
        /// 进入流程，不能在该方法中进行流程切换,如果需要抛出异常请先清理之前加载的资源
        /// </summary>
        /// <returns></returns>
        public static async ETTask Enter(this Procedure self)
        {
            foreach (Component _component in self.GetComponents<IProcedureCycle>())
            {
                await (_component as IProcedureCycle).OnEnter();
            }
        }

        /// <summary>
        /// 离开流程，不能在该方法中进行流程切换,如果需要抛出异常请先清理之前加载的资源
        /// </summary>
        /// <returns></returns>
        public static async ETTask Exit(this Procedure self)
        {
            foreach (Component _component in self.GetComponents<IProcedureCycle>())
            {
                await (_component as IProcedureCycle).OnExit();
            }
        }
    }
}
