using System;
namespace Protocol.Code
{
    public class MatchRoomCode
    {
        public const int STARTMATCH_CREQ = 0;
        public const int STARTMATCH_SRES = 1;
        public const int STARTMATCH_BRO = 7;

        public const int CANCELMATCH_CREQ = 2;
        public const int CANCELMATCH_BRO = 3;

        public const int ENTERROOM_CREQ = 8;
        public const int ENTERROOM_SRES = 9;

        public const int READY_CREQ = 4;
        public const int READY_BRO = 5;

        public const int START_BRO = 6;
    }
}
