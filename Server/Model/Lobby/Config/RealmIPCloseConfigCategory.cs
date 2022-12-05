using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ETModel;

namespace ETModel
{
    public partial class RealmIPCloseConfigCategory
    {
        public struct IPCloseItem
        {
            public uint begin;
            public uint end;
            public int flag;
        }

        public IPCloseItem[] ipCloseItems;

        public int Count
        {
            get
            {
                return this.dictLong.Count;
            }
        }

        public override void EndInit()
        {
            base.EndInit();

            ipCloseItems = new IPCloseItem[this.dictLong.Count];
            RealmIPCloseConfig _config = null;
            int _index = 0;
            foreach (KeyValuePair<long, IConfig> keyValue in this.dictLong)
            {
                _config = keyValue.Value as RealmIPCloseConfig;
                string[] _ips = _config.IPBegin.Split('.');
                ipCloseItems[_index].begin = uint.Parse(_ips[0]) * 255 * 255 * 255 + uint.Parse(_ips[1]) * 255 * 255 + uint.Parse(_ips[2]) * 255 + uint.Parse(_ips[3]);
                _ips = _config.IPEnd.Split('.');
                ipCloseItems[_index].end = uint.Parse(_ips[0]) * 255 * 255 * 255 + uint.Parse(_ips[1]) * 255 * 255 + uint.Parse(_ips[2]) * 255 + uint.Parse(_ips[3]);
                ipCloseItems[_index].flag = _config.Flag;

            }
        }

        /// <summary>
        /// 获取含有地址的第一个tag
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public int TryGetFlag(string ipAddress)
        {
            string[] _ips = ipAddress.Split('.');
            uint _ipNum = uint.Parse(_ips[0]) * 255 * 255 * 255 + uint.Parse(_ips[1]) * 255 * 255 + uint.Parse(_ips[2]) * 255 + uint.Parse(_ips[3]);

            for (int i = 0; i < ipCloseItems.Length; i++)
            {
                if (ipCloseItems[i].begin <= _ipNum && ipCloseItems[i].end >= _ipNum)
                {
                    return ipCloseItems[i].flag;
                }
            }

            return 0;
        }
    }
}
