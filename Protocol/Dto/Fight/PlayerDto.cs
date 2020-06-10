using System;
using System.Collections.Generic;
using Protocol.Dto.Card;

namespace Protocol.Dto.Fight
{
    /// <summary>
    /// 存储战斗场景中玩家信息
    /// </summary>
    [Serializable]
    public class PlayerDto
    {
        public int UserId { get; private set; }

        public Identity Identity { get; set; }

        private List<CardDto> cardList;

        public int CardsCount { get { return cardList.Count; } }

        public PlayerDto()
        {

        }
        public PlayerDto(int userId)
        {
            this.UserId = userId;
            this.Identity = Identity.Farmer;
            this.cardList = new List<CardDto>();
        }

       

        public bool IsEmptyCards()
        {
            return CardsCount == 0;
        }

        public void AddCard(CardDto[] cards)
        {
            cardList.AddRange(cards);
        }

        public void AddCard(CardDto card)
        {
            cardList.Add(card);
        }

        public void RemoveCard(CardDto[] cards,bool isAll = false)
        {
            if (isAll)
            {
                cardList.Clear();
                return;
            }
            for (int i = 0; i < cards.Length; i++)
            {
                RemoveCard(cards[i]);
            }
        }

        public void RemoveCard(CardDto card)
        {
            for (int i = 0; i < cardList.Count; i++)
            {
                if (cardList[i].name == card.name)
                {
                    cardList.RemoveAt(i);
                }
            }
            
            //else
            //{
            //    throw new Exception("没有这个牌，你是咋出的？");
            //}
        }

        public List<CardDto> GetCards()
        {
            return cardList;
        }

        
    }

    [Serializable]
    public enum Identity
    {
        Farmer,
        Landlord,
    }
}
