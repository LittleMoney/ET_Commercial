using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
    public interface IUnitCycle
    {
        /// <summary>
        /// 开始
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
        /// 回收
        /// </summary>
        void OnReset();
    }
}
