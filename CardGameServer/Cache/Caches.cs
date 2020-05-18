using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardGameServer.Cache.Room;

namespace CardGameServer.Cache
{
    public static class Caches
    {
        public static AccountCache Account { get; set; }
        public static UserCache User { get; set; }
        public static MatchCache Match { get; set; }


        static Caches() {
            Account = new AccountCache();
            User = new UserCache();
            Match = new MatchCache();
        }
    }
}