using System;
using System.Collections.Generic;
using Protocol.Dto.Card;

namespace Protocol.Dto.Constant
{
    /// <summary>
    /// 单牌的花色，权值
    /// 判断出的牌的类型和权值  ==> 传过来的牌是经过排序的
    /// </summary>
    public static class CardConstant
    {
        /// <summary>
        /// 出的牌合法
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public static bool IsLegal(this List<CardDto> cards)
        {
            return GetCardsTypeWeight(cards).type != CardsType.None;
        }


        private static bool IsSingle(List<CardDto> cards)
        {
            return cards.Count == 1;
        }

        private static bool IsDouble(List<CardDto> cards)
        {
            
            if (cards.Count != 2) return false;
            if (cards[0].weight == cards[1].weight)
                return true;

            return false;
        }

        private static bool IsStraight(List<CardDto> cards)
        {
            //长度限制5-12张
            if (cards.Count < 5 || cards.Count > 12) return false;
            //第一张不能超过A
            if (cards[0].weight > CardWeight.One) return false;
            //是不是连着的 且不能超过A
            for (int i = 0; i < cards.Count - 1; i++)
            {
                //判断是否是连续的
                if (cards[i].weight != cards[i + 1].weight + 1) return false;
            }
            return true;
        }

        private static bool IsDouble_Straight(List<CardDto> cards)
        {
            //长度限制
            if (cards.Count < 6 || cards.Count % 2 != 0) return false;
            //第一张不能超过A
            if (cards[0].weight > CardWeight.One) return false;
            //是不是连着的 且不能超过A
            for (int i = 0; i < cards.Count - 2; i+=2)
            {
                //判断是否是连续的
                if (cards[i].weight != cards[i + 1].weight) return false;

                if (cards[i].weight != cards[i + 2].weight + 1) return false;
            }
            return true;
        }

        private static bool IsThree_Straight(List<CardDto> cards)
        {
            //长度限制
            if (cards.Count < 6 || cards.Count % 3 != 0) return false;
            //第一张不能超过A
            if (cards[0].weight > CardWeight.One) return false;
            //是不是连着的 且不能超过A
            for (int i = 0; i < cards.Count - 3; i += 3)
            {
                //判断是否是连续的
                if (cards[i].weight != cards[i + 1].weight) return false;

                if (cards[i + 1].weight != cards[i + 2].weight) return false;

                if (cards[i].weight != cards[i + 3].weight + 1) return false;
            }
            return true;
        }


        private static bool IsThree(List<CardDto> cards)
        {
            if (cards.Count != 3) return false;

            if (cards[0].weight == cards[1].weight && cards[1].weight == cards[2].weight) return true;

            return false;
        }


        private static ThreeFront IsThree_One(List<CardDto> cards)
        {
            if (cards.Count != 4) return new ThreeFront();

            if (cards[0].weight != cards[1].weight)//单在前
            {
                if (cards[1].weight == cards[2].weight && cards[2].weight == cards[3].weight) return new ThreeFront(true,false);
                return new ThreeFront();
            }
            else //单在后
            {
                if (cards[0].weight == cards[1].weight && cards[1].weight == cards[2].weight) return new ThreeFront(true,true);
                return new ThreeFront();
            }
        }



        private static ThreeFront IsThree_Two(List<CardDto> cards)
        {
            if (cards.Count != 5) return new ThreeFront();

            if (cards[1].weight != cards[2].weight)//双在前
            {
                //判断后三个是否一样
                if (cards[2].weight == cards[3].weight && cards[3].weight == cards[4].weight)
                {
                    //判断前两个是否一样
                    if (cards[0].weight == cards[1].weight) return new ThreeFront(true,false);
                }
                return new ThreeFront();
            }
            else //双在后
            {
                //判断前三个是否一样
                if (cards[0].weight == cards[1].weight && cards[1].weight == cards[2].weight)
                {
                    //判断后两个是否一样
                    if (cards[3].weight == cards[4].weight) return new ThreeFront(true,true);
                }
                return new ThreeFront();
            }
        }


        private static bool IsBoom(List<CardDto> cards)
        {
            if (cards.Count != 4) return false;
            var weight = cards[0].weight;
            for (int i = 1; i < cards.Count; i++)
            {
                if (cards[i].weight != weight)
                    return false;
            }

            return true;
        }

        private static bool IsJokerBoom(List<CardDto> cards)
        {
            if (cards.Count != 2) return false;

            if (cards[0].weight == CardWeight.BJoker && cards[1].weight == CardWeight.SJoker) return true;

            return false;
        }

        public static CardsTypeWeight GetCardsTypeWeight(this List<CardDto> cards)
        {
            var type = CardsType.None;
            var weight = cards[0].weight;
            switch (cards.Count)
            {
                case 1:
                    {
                        if (IsSingle(cards))
                        {
                            type = CardsType.Single;
                        }
                    }
                    break;
                case 2:
                    {
                        if (IsDouble(cards))
                        {
                            type = CardsType.Double;
                        }
                        else if (IsJokerBoom(cards))
                        {
                            type = CardsType.Joker_Boom;
                        }
                    }
                    break;
                case 3:
                    {
                        if (IsThree(cards))
                        {
                            type = CardsType.Three;
                        }
                    }
                    break;
                case 4:
                    {
                        if (IsBoom(cards))
                        {
                            type = CardsType.Boom;
                        }
                        else
                        {
                            var threeFront = IsThree_One(cards);
                            if (threeFront.flag)//是的
                            {
                                type = CardsType.Three_One;
                                weight = threeFront.isFront ? cards[0].weight : cards[1].weight ;
                            }
                        }
                    }
                    break;
                case 5:
                    {
                        if (IsStraight(cards))
                        {
                            type = CardsType.Straight;
                        }
                        else 
                        {
                            var threeFront = IsThree_Two(cards);
                            if (threeFront.flag)//是的
                            {
                                type = CardsType.Three_Two;
                                weight = threeFront.isFront ? cards[0].weight : cards[2].weight;
                            }
                        }
                    }
                    break;
                case 6:
                    {
                        if (IsStraight(cards))
                        {
                            type = CardsType.Straight;
                        }
                        else if (IsDouble_Straight(cards))
                        {
                            type = CardsType.Double_Straight;
                        }
                        else if (IsThree_Straight(cards))
                        {
                            type = CardsType.Three_Straight;
                        }
                    }
                    break;
                case 7:
                    {
                        if (IsStraight(cards))
                        {
                            type = CardsType.Straight;
                        }
                    }
                    break;
                case 8:
                    {
                        if (IsStraight(cards))
                        {
                            type = CardsType.Straight;
                        }
                        else if (IsDouble_Straight(cards))
                        {
                            type = CardsType.Double_Straight;
                        }
                    }
                    break;
                case 9:
                    {
                        if (IsStraight(cards))
                        {
                            type = CardsType.Straight;
                        }
                        else if (IsThree_Straight(cards))
                        {
                            type = CardsType.Three_Straight;
                        }
                    }
                    break;
                case 10:
                    {
                        if (IsStraight(cards))
                        {
                            type = CardsType.Straight;
                        }
                        else if (IsDouble_Straight(cards))
                        {
                            type = CardsType.Double_Straight;
                        }
                    }
                    break;
                case 11:
                    {
                        if (IsStraight(cards))
                        {
                            type = CardsType.Straight;
                        }
                    }
                    break;
                case 12:
                    {
                        if (IsStraight(cards))
                        {
                            type = CardsType.Straight;
                        }
                        else if (IsDouble_Straight(cards))
                        {
                            type = CardsType.Double_Straight;
                        }
                        else if (IsThree_Straight(cards))
                        {
                            type = CardsType.Three_Straight;
                        }
                    }
                    break;
                case 13:
                    Console.WriteLine("13张是不可能的！！！");
                    break;
                case 14:
                    {
                        if (IsDouble_Straight(cards))
                        {
                            type = CardsType.Double_Straight;
                        }
                    }
                    break;
                case 15:
                    {
                        if (IsThree_Straight(cards))
                        {
                            type = CardsType.Three_Straight;
                        }
                    }
                    break;
                case 16:
                    {
                        if (IsDouble_Straight(cards))
                        {
                            type = CardsType.Double_Straight;
                        }
                    }
                    break;
                case 17:
                    Console.WriteLine("13张是不可能的！！！");
                    break;
                case 18:
                    {
                        if (IsDouble_Straight(cards))
                        {
                            type = CardsType.Double_Straight;
                        }
                        else if (IsThree_Straight(cards))
                        {
                            type = CardsType.Three_Straight;
                        }
                    }
                    break;
                case 19:
                    {
                        
                    }
                    break;
                case 20:
                    {
                        if (IsDouble_Straight(cards))
                        {
                            type = CardsType.Double_Straight;
                        }
                    }
                    break;
                default:
                    break;
            }

            var result = new CardsTypeWeight();
            result.type = type;
            result.weight = weight;
            return result;
        }

    }

    [Serializable]
    public enum CardColor
    {
        Joker,
        Spade,
        Heart,
        Diamond,
        Club,

    }
    [Serializable]
    public enum CardWeight
    {
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,

        Jack,
        Queen,
        King,

        One,
        Two,

        SJoker,
        BJoker,
    }

    [Serializable]
    public enum CardsType
    {
        None,
        Single,//单
        Double,//双
        Straight,//顺子
        Double_Straight,//双顺
        Three_Straight,//三顺  飞机  不带
        Three_Straight_One,//三顺  飞机  带单
        Three_Straight_Two,//三顺  飞机  带双
        Three,//三不带
        Three_One,//三带一
        Three_Two,//三带二
        Boom,//普通炸弹
        Joker_Boom,//王炸

    }

    /// <summary>
    /// 判断三个一样的 是在前还是在后 3334  isFront = true;
    /// </summary>
    public class ThreeFront
    {
        public bool flag;
        public bool isFront;//如果flag为false  那么这个属性没有意义
        public ThreeFront()
        {
            flag = false;
            isFront = false;
        }
        public ThreeFront(bool flag,bool isFront)
        {
            this.flag = flag;
            this.isFront = isFront;
        }
    }

    public class CardsTypeWeight
    {
        public CardsType type;
        public CardWeight weight;

    }
}