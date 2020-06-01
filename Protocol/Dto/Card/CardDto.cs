using System;
using Protocol.Dto.Constant;

namespace Protocol.Dto.Card
{
    
    [Serializable]
    public class CardDto
    {
        

        /// <summary>
        /// 名称
        /// </summary>
        public string name;
        /// <summary>
        /// 卡牌花色
        /// </summary>
        public CardColor color;
        /// <summary>
        /// 权值
        /// </summary>
        public CardWeight weight;

        public CardDto()
        {
            
        }

        public CardDto(string name,CardColor color,CardWeight weight)
        {
            this.name = name;
            this.color = color;
            this.weight = weight;
        }
    }



    
}
