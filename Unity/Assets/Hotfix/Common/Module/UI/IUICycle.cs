using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
    public interface IUICycle
    {
        /// <summary>
        /// 显示时触发
        /// </summary>
        void OnStart();

        /// <summary>
        /// 显示时触发
        /// </summary>
        void OnShow();

        /// <summary>
        /// 隐藏时触发
        /// </returns>
        void OnHide();

        /// <summary>
        /// 不再被其他窗口遮挡
        /// </summary>
        void OnFocus();

        /// <summary>
        /// 被其他窗口压住
        /// </summary>
        void OnPause();

    }
}
