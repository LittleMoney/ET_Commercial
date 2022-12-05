using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
    public interface IUnitCycle
    {
        /// <summary>
        /// 显示
        /// </summary>
        void OnStart();

        /// <summary>
        /// 显示
        /// </summary>
        void OnShow();

        /// <summary>
        /// 隐藏
        /// </summary>
        void OnHide();

        /// <summary>
        /// 重置
        /// </summary>
        void OnReset();
    }
}
