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
        /// 模块签名，用于验证模块是否需要资源更新
        /// </summary>
        public string SignMD5;

    }
}
