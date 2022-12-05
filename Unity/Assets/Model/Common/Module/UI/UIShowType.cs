using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
    public enum UIShowType
    {
        /// <summary>
        /// 固定位置，初次显示在最上层，再次显示时显示在原位置
        /// </summary>
        Fix,

        /// <summary>
        /// 弹出模式，每次显示在最上层，且会将下面UI失去焦点
        /// </summary>
        Pop,

        /// <summary>
        /// 独占模式，每次显示在最上层，且会将下面UI失去焦点并隐藏
        /// </summary>
        Exclusive
    }
}
