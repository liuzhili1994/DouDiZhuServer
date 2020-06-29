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
        /// <summary>
        /// 是不是最大者出的牌  如果是就播放牌的语音，不是就播放压死，大你
        /// </summary>
        public bool isBiggest;

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
            this.isLeagl = this.type != CardsType.None;
            isBiggest = false;
        }

        public void Set(int userId, List<CardDto> cards)
        {
            this.userId = userId;
            this.cardsList = cards;
            var typeWeight = cardsList.GetCardsTypeWeight();
            this.type = typeWeight.type;
            this.weight = typeWeight.weight;
            this.length = cardsList.Count;
            this.isLeagl = this.type != CardsType.None;
            isBiggest = false;
        }

        public void Set(bool isBiggest)
        {
            this.isBiggest = isBiggest;
        }
    }
}
