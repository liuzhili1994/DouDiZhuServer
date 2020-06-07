using System;
namespace Protocol.Code
{
    public class FightCode
    {
        public const int QIANG_LANDLORD_CREQ= 0;//抢地主请求  true 为抢地主  false 为不抢
        public const int QIANG_LANDLORD_SRES = 12;//抢地主回应 ： 给地主发底牌，给另外两个发广播谁是地主

        public const int QIANG_LANDLORD_BRO = 1;//抢地主广播
        public const int BUQIANG_LANDLORD_BRO= 16;//不抢地主广播

        public const int CHUPAI_CREQ= 3;//出牌请求
        public const int CHUPAI_SRES= 4;//出牌回应 只回应玩家  你还剩下什么牌
        public const int CHUPAI_BRO= 5;//出牌广播 广播ui所有人。谁出了什么牌
        public const int BUCHU_CREQ= 6;//不出请求
        public const int BUCHU_SRES= 6;//不出回应 
        public const int BUCHU_BRO= 7;//不出牌广播
        public const int LEAVE_BRO= 8;//离开房间
        public const int OVER_BRO= 9;//结束
        public const int GET_CARDS_CREQ= 10;//向服务器要牌
        public const int GET_CARDS_SRES= 11;//服务器给牌

        public const int CHUPAI_TURN_BRO = 13;//轮到谁出牌了
        public const int QIANG_TURN_BRO = 15;//广播该谁了

        public const int CHANGE_MUTIPLIER = 14;//改变房间倍数

        public const int Restart = 17;//都不抢重新开始
    }
}
