using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    public class Club:Entity
    {
        public int clubId;
        public ClubData data;
        public int nextOriginTableId;
        public readonly Dictionary<int, User> userDict = new Dictionary<int, User>();
        public readonly Dictionary<int, Table> tableDict = new Dictionary<int, Table>();
        public readonly Dictionary<int,List<Table>> tableGameKindDict = new Dictionary<int, List<Table>>();


    }
}
