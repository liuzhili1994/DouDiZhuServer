using System;
using System.Collections;
using System.Collections.Generic;
using Protocol.Dto.Card;

namespace Protocol.Dto.Constant
{
    /// <summary>
    /// 牌库
    /// </summary>
    public class CardLibrary
    {
        private int cardTotalCount = 54;
        /// <summary>
        /// 存储牌的Queue 创建一次不会变化
        /// </summary>
        private readonly Queue<CardDto> cardsQueue;
        /// <summary>
        /// 存储乱序后的54张牌
        /// </summary>
        private readonly List<CardDto> cardsList;
        /// <summary>
        /// 创建54张牌
        /// </summary>
        private void Creat()
        {
            string[] cardColors = Enum.GetNames(typeof(CardColor));
            string[] cardWeights = Enum.GetNames(typeof(CardWeight));
            string cardColor = string.Empty;
            string cardWeight = string.Empty;
            
            for (int i = 1; i < cardColors.Length; i++) //排除大小王
            {
                cardColor = cardColors[i];
                for (int j = 0; j < cardWeights.Length - 2; j++)//大小王不是每个花色都有的
                {
                    cardWeight = cardWeights[j];

                    cardsQueue.Enqueue(new CardDto(cardColor + cardWeight,(CardColor)i,(CardWeight)j));
                }
            }

            //创建小王
            cardWeight = CardWeight.SJoker.ToString();
            cardsQueue.Enqueue(new CardDto(cardWeight, CardColor.Joker,CardWeight.SJoker));
            //创建大王
            cardWeight = CardWeight.BJoker.ToString();
            cardsQueue.Enqueue(new CardDto(cardWeight, CardColor.Joker, CardWeight.BJoker));

            Console.WriteLine("创建牌库成功 ： {0}张牌！",cardsQueue.Count);
            
        }

        /// <summary>
        /// 洗牌   打乱顺序
        /// </summary>
        public void Reflesh()
        {
            cardsList.Clear();
            Random r = new Random();
            foreach (var card in cardsQueue)
            {
                int index = r.Next(0, cardsList.Count + 1);
                cardsList.Insert(index,card);
            }
            Console.WriteLine("牌库乱顺序成功。。。");
        }
        /// <summary>
        /// 发牌
        /// </summary>
        /// <param name="count">需要牌的数量</param>
        /// <returns></returns>
        public CardDto[] SendPlayerCards(int count)
        {
            List<CardDto> result = new List<CardDto>();
            for (int i = 0; i < count; i++)
            {
                var temp = cardsList[i];
                result.Add(temp);
            }

            cardsList.RemoveRange(0,count);

            return result.ToArray();
        }

        public CardDto[] SendTableCards()
        {
            CardDto[] result;
            if (cardsList.Count == 3)
            {
                result = cardsList.ToArray();
                cardsList.Clear();
            }
            else
            {
                throw new Exception("牌发错了。。。");
            }

            return result;
        }

        public CardLibrary()
        {
            
            this.cardsQueue = new Queue<CardDto>(cardTotalCount);
            this.cardsList = new List<CardDto>(cardTotalCount);
            Creat();
        }
    }
}
