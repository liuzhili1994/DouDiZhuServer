using System;
namespace Protocol.Dto.Card
{
    [Serializable]
    public class CardDto
    {
        /// <summary>
        /// 唯一id
        /// </summary>
        public int id;
        /// <summary>
        /// 名称
        /// </summary>
        public string name;
        /// <summary>
        /// 卡牌花色
        /// </summary>
        public int color;
        /// <summary>
        /// 权重
        /// </summary>
        public int weight;

        public CardDto()
        {
            
        }

        public CardDto(int id,string name,int color,int weight)
        {
            this.id = id;
            this.name = name;
            this.color = color;
            this.weight = weight;
        }
    }
}
