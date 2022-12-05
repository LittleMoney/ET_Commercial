using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    public  class LogonInfo:Entity
    {
        public int userId;
        public string ip;
        public string machineSerial;
        public string loginKey;
        public DateTime RegisteTime;
    }
}
