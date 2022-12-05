using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
    public interface IProcedureCycle
    {

        /// <summary>
        /// 进入流程，不能在该方法中进行流程切换,如果需要抛出异常请先清理之前加载的资源
        /// </summary>
        /// <returns></returns>
        ETTask OnEnter();

        /// <summary>
        /// 离开流程，不能在该方法中进行流程切换,如果需要抛出异常请先清理之前加载的资源
        /// </summary>
        /// <returns></returns>
        ETTask OnExit();

    }
}
