using System;
using System.Collections.Generic;

namespace Protocol.Dto.Fight
{

    /// <summary>
    /// 游戏结束 需要发送的消息
    /// </summary>
    [Serializable]
    public class OverDto
    {
        /// <summary>
        /// 什么身份的人赢了
        /// </summary>
        public Identity winIdentity;
        /// <summary>
        /// 哪些人赢了
        /// </summary>
        public List<int> winUserIdsList;
        /// <summary>
        /// 结算的豆子
        /// </summary>
        public int beens;
        public OverDto()
        {
        }
        public OverDto(Identity winIdentity, List<int> winUserIdsList, int beens)
        {
            this.winIdentity = winIdentity;
            this.winUserIdsList = winUserIdsList;
            this.beens = beens;
        }
    }
}
