using System;
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
        public int color;
        /// <summary>
        /// 权值
        /// </summary>
        public int weight;

        public CardDto()
        {
            
        }

        public CardDto(string name,int color,int weight)
        {
            this.name = name;
            this.color = color;
            this.weight = weight;
        }
    }
}
