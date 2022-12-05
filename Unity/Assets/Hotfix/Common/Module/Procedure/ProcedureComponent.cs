using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
    public class ProcedureComponent : Component
    {
        public static ProcedureComponent Instance;

        public Procedure g_currentProcedure;

        public Procedure g_swithingProcedure;

        public bool g_isSwithing;

        public Dictionary<string, object> g_datas;
    }


    [ObjectSystem]
    public class ProcedureComponentAwakeSystem : AwakeSystem<ProcedureComponent>
    {
        public override void Awake(ProcedureComponent self)
        {
            ProcedureComponent.Instance = self;
            self.g_isSwithing = false;
            self.g_datas = new Dictionary<string, object>();
        }
    }

    [ObjectSystem]
    public class ProcedureComponentDestroySystem : DestroySystem<ProcedureComponent>
    {
        public override void Destroy(ProcedureComponent self)
        {
            if (self.g_currentProcedure != null)
            {
                self.g_currentProcedure.Dispose();
                self.g_currentProcedure = null;
            }

            if (self.g_swithingProcedure!=null)
            {
                self.g_swithingProcedure.Dispose();
                self.g_swithingProcedure = null;
            }

            self.g_isSwithing = true;
        }
    }

    /// <summary>
	/// 
	/// </summary>
	public static class ProcedureComponentSystem
    { 
        /// <summary>
        /// 切换当前流程
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async ETTask SwitchAsync(this ProcedureComponent self, string procedureName)
        {
            if (self.g_isSwithing) throw new Exception("切换中不能重复切换");

            if(self.g_currentProcedure!=null && self.g_currentProcedure.name==procedureName)
            {
                return;
            }

            self.g_isSwithing = true;

            long _instanceId = 0;
            self.g_swithingProcedure = ComponentFactory.CreateWithParent<Procedure, string>(self, procedureName);

            if (self.g_currentProcedure != null)
            {
                _instanceId = self.InstanceId;
                await self.g_currentProcedure.Exit();
                if (self.InstanceId != _instanceId) return;
                self.g_currentProcedure.Dispose();
                self.g_currentProcedure = null;
                
            }

            self.g_currentProcedure = self.g_swithingProcedure;
            self.g_swithingProcedure = null;

            _instanceId = self.InstanceId;
            await self.g_currentProcedure.Enter();
            if (self.InstanceId != _instanceId) return;
            self.g_isSwithing = false;

        }

    }
}
