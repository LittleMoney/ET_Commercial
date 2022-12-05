using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
    public class AppVersionConfig
    {
        /// <summary>
        /// 服务器是否正常运行
        /// </summary>
        public bool IsServerNormal;

        /// <summary>
        /// 维护公告
        /// </summary>
        public string MaintainAnnouncement;

        /// <summary>
        /// 拉去服务器版本信息地址
        /// </summary>
        public string AppVersionConfigRequestUrl;

        /// <summary>
        /// 热更下载地址
        /// </summary>
        public string HotfixDownloadUrl;

        /// <summary>
        /// app下载地址
        /// </summary>
        public string AppDownloadUrl;

        /// <summary>
        /// 登录服务器地址
        /// </summary>
        public string ServerAddress;

        /// <summary>
        /// 当前app版本，如有资源需要更新则推荐版本
        /// </summary>
        public long Version;

        /// <summary>
        /// 当前App的渠道号
        /// </summary>
        public long Channel;

        /// <summary>
        /// 版本此版本号必须冷更新
        /// </summary>
        public long ColdUpdateVersion;

        /// <summary>
        /// 子模块版本信息
        /// </summary>
        public Dictionary<string, ToyVersionConfig> ToyVersionConfigs = new Dictionary<string, ToyVersionConfig>();
    }
}
