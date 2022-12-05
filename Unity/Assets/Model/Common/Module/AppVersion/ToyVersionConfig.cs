using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
    /// <summary>
    /// 子模块的版本信息
    /// </summary>
    public class ToyVersionConfig
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 模块版本号，比较大小确定该模块资源需要不需要热更新
        /// </summary>
        public string SignMD5;

    }
}
