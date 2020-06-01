using System;
using System.Collections.Generic;
using Protocol.Dto.Card;

namespace Protocol.Dto.Fight
{
    /// <summary>
    /// 抢了地主需要发送的数据   
    /// </summary>
    [Serializable]
    public class LandlordDto
    {
        /// <summary>
        /// 地主的id
        /// </summary>
        public int landLordId;
        /// <summary>
        /// 底牌
        /// </summary>
        public List<CardDto> tableCardsList;
        public LandlordDto()
        {
        }

        public LandlordDto(int id, List<CardDto> tableCards)
        {
            this.landLordId = id;
            this.tableCardsList = tableCards;
        }
    }
}
