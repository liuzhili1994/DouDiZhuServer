using System;
using Protocol.Dto.Card;
using Protocol.Dto.Constant;

namespace CardGameServer.Cache.Room
{
    /// <summary>
    /// 回合管理类  管理抢地主和出牌的顺序
    /// </summary>
    public class Round
    {
        public int currentUserId = -1 ;

        public int currentBiggsetId = -1;

        public CardsType lastCardsType;

        public int lastCardsLength;

        public CardWeight lastCardsWeight;

        public Round()
        {
            Init();
        }

        private void Init()
        {
            currentUserId = -1;
            currentBiggsetId = -1;
            lastCardsType = CardsType.None;
            lastCardsLength = 0;
            lastCardsWeight = 0;
        }
        /// <summary>
        /// 开始出牌   设置完了地主之后 发了底牌就该出牌了 
        /// </summary>
        /// <param name="userId"></param>
        public void Start(int userId)
        {
            this.currentUserId = userId;
            this.currentBiggsetId = userId;

            lastCardsType = CardsType.None;
            lastCardsLength = 0;
            lastCardsWeight = 0;
        }

        /// <summary>
        /// 改变出牌者，被管住了
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cardsType"></param>
        /// <param name="cardsLenght"></param>
        /// <param name="cardsWeight"></param>
        public void ChangeBiggestId(int userId, CardsType cardsType,int cardsLenght,CardWeight cardsWeight)
        {
            this.currentBiggsetId = userId;
            this.lastCardsType = cardsType;
            this.lastCardsLength = cardsLenght;
            this.lastCardsWeight = cardsWeight;
        }

        /// <summary>
        /// 切换到下家
        /// </summary>
        /// <param name="nextUserId"></param>
        public void Turn(int nextUserId)
        {
            this.currentUserId = nextUserId;
        }

        public void Reset()
        {
            Init();
        }
    }
}
