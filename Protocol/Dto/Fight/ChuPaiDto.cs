using System;
using System.Collections.Generic;
using Protocol.Dto.Card;
using Protocol.Dto.Constant;

namespace Protocol.Dto.Fight
{
    /// <summary>
    /// 出牌信息类 出的什么牌，是否合法，由服务器传到客户端
    /// </summary>
    [Serializable]
    public class ChuPaiDto
    {
        /// <summary>
        /// 谁出的牌
        /// </summary>
        public int userId;
        /// <summary>
        /// 出的什么牌 ==> 客户端发过来的  发过来就排好顺序
        /// </summary>
        public List<CardDto> cardsList;

        public CardsType type;

        public CardWeight weight;

        public int length;
        /// <summary>
        /// 是否合法
        /// </summary>
        public bool isLeagl;

        public ChuPaiDto()
        {
        }

        public ChuPaiDto(int userId,List<CardDto> cards)
        {
            this.userId = userId;
            this.cardsList = cards;
            var typeWeight = cardsList.GetCardsTypeWeight();
            this.type = typeWeight.type;
            this.weight = typeWeight.weight;
            this.length = cardsList.Count;
            this.isLeagl = this.type == CardsType.None;
        }
    }
}
