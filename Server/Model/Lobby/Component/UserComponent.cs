using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    public class UserComponent:Component
    {
        public readonly Dictionary<long, User>      userDict=new Dictionary<long, User>();
    }
}
