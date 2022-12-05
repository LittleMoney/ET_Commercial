using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    public class ClubComponent:Component
    {
        public readonly Dictionary<int, ClubData> clubDataDict=new Dictionary<int, ClubData>();
        public readonly Dictionary<long, Club> clubDict=new Dictionary<long, Club>();
    }
}
