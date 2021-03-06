﻿using System;
using System.Collections.Generic;
using DaligeServer;
using Protocol.Dto.Card;
using Protocol.Dto.Constant;
using Protocol.Dto.Fight;

namespace CardGameServer.Cache.Room
{
    public class FightRoom
    {
        public bool isFighting;
        /// <summary>
        /// 不抢多少次重开？ 
        /// </summary>
        private int buQiangCount;
        
        /// <summary>
        /// 唯一id
        /// </summary>
        public int id;
        /// <summary>
        /// 当前场景的所有玩家信息
        /// </summary>
        public List<PlayerDto> playerList;
       
        /// <summary>
        /// 中途离开的玩家信息
        /// </summary>
        public List<int> leavePlayerIdList;
        /// <summary>
        /// 底牌
        /// </summary>
        public List<CardDto> tableCards;
        /// <summary>
        /// 牌库
        /// </summary>
        public CardLibrary cardLibrary;
        /// <summary>
        /// 当前倍数
        /// </summary>
        public int multiple;
        /// <summary>
        /// 回合信息
        /// </summary>
        public Round round;


        public int autoBuQiang = -1;
        public int autoBuChu = -1;


        public FightRoom(int id, List<int> userIds)
        {
            this.id = id;
            Init(userIds);
        }
        /// <summary>
        /// 初始化房间
        /// </summary>
        /// <param name="userIds"></param>
        public void Init(List<int> userIds)
        {
            
            this.multiple = 1;
            this.isFighting = false;
            
            leavePlayerIdList = new List<int>();
            tableCards = new List<CardDto>();
            cardLibrary = new CardLibrary();
            round = new Round();

            playerList = new List<PlayerDto>(3);
            foreach (var userId in userIds)
            {
                playerList.Add(new PlayerDto(userId));
            }

            InitCards();

            //初始化回合
            round.Turn(userIds[0]);
        }

        /// <summary>
        /// 重新洗牌
        /// </summary>
        public void InitCards()
        {
            foreach (var player in playerList)
            {
                player.RemoveCard(null,true);
            }
            tableCards.Clear();
            //洗牌
            cardLibrary.Reflesh();
            //发牌
            InitPlayerCards();
        }

        /// <summary>
        /// 转换出牌者
        /// </summary>
        public int TurnNext()
        {
            int nextUserId = GetNextUserId(round.currentUserId);
            round.Turn(nextUserId);
            return nextUserId;
        }


        /// <summary>
        /// 获取下一个出牌者
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <returns></returns>                             
        private int GetNextUserId(int currentUserId)        
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].UserId == currentUserId)
                {
                    return playerList[(i + 1) % 3].UserId;
                    //return (i + 1) % 3;
                }
            }

            throw new Exception("没有该玩家。。。");
        }

        /// <summary>
        /// 判断出的牌能不能管上  
        /// </summary>
        /// <returns></returns>
        public bool ChuPai(ChuPaiDto dto)
        {
            bool result = false;
            //自己出牌
            //判断最大是不是自己
            if (dto.userId == round.currentBiggsetId)
            {
                //自己的牌，别人都不要
                //随便出
                //但是要符合规则
                if (dto.type != CardsType.None) {
                    result = true;

                    dto.Set(true);
                }
                    
            }
            //管别人的牌
            //同类型比较
            else if (dto.type == round.lastCardsType)
            {
                
                //特殊的类型  还需要比长度
                if (dto.type == CardsType.Straight || dto.type == CardsType.Double_Straight)
                {
                    if (dto.length > round.lastCardsLength)
                    {
                        //判断权值
                        if (dto.weight > round.lastCardsWeight)
                        {
                            //可以出牌
                            result = true;
                        }
                    }
                }
                else  //普通的类型  只需要比权值
                {
                    if (dto.weight > round.lastCardsWeight)
                    {
                        result = true;
                    }
                }


            }
            else  //跨类型比较
            {
                //王炸
                if (dto.type == CardsType.Joker_Boom)
                {
                    result = true;
                }
                else if(dto.type == CardsType.Boom)//普通炸弹
                {
                    if (round.lastCardsType != CardsType.Joker_Boom)
                    {
                        result = true;
                    }
                }
            }

            //出牌
            if (result)
            {
                //移除手牌
                RemovePlayerCards(dto.userId,dto.cardsList);
                //可能翻倍
                if (dto.type == CardsType.Boom)
                    this.multiple *= 2;
                else if (dto.type == CardsType.Joker_Boom)
                    this.multiple *= 8;

                //改变回合信息
                round.ChangeBiggestId(dto.userId, dto.type, dto.length, dto.weight);
            }
           

            return result;
        }

        /// <summary>
        /// 根据id获取该玩家现有的手牌
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<CardDto> GetPlayerCards(int userId)
        {
            var player = GetPlayerDto(userId);
            return player.GetCards();
        }

        /// <summary>
        /// 移除玩家的手牌
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cardsList"></param>
        public void RemovePlayerCards(int userId,List<CardDto> cardsList)
        {
            var player = GetPlayerDto(userId);
            player.RemoveCard(cardsList.ToArray());
            
        }

        /// <summary>
        /// 给玩家发牌
        /// </summary>
        public void InitPlayerCards()
        {
            //一人17张牌
            foreach (var player in playerList)
            {
                player.AddCard(cardLibrary.SendPlayerCards(17));
            }
            //留3张底牌
            tableCards.AddRange(cardLibrary.SendTableCards());
        }

        /// <summary>
        /// 设置地主身份
        /// </summary>
        /// <param name="userId"></param>
        public void SetLandlord(int userId)
        {
            var player = GetPlayerDto(userId);
            //设置地主
            player.Identity = Identity.Landlord;
            //给地主发牌
            player.AddCard(tableCards.ToArray());
            //开始回合
            round.Start(userId);

            isFighting = true;

        }

        /// <summary>
        /// 获取玩家信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public PlayerDto GetPlayerDto(int userId)
        {
            foreach (var player in playerList)
            {
                if(userId == player.UserId)
                    return player;
            }

            throw new Exception("没有这个玩家。。。");
        }

       
        /// <summary>
        /// 获取用户身份
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Identity GetPlayerIdentity(int userId)
        {
            return GetPlayerDto(userId).Identity;
        }

        /// <summary>
        /// 获取相同身份的用户id
        /// </summary>
        /// <returns></returns>
        public List<int> GetSameIdentityUserIds(Identity identity)
        {
            List<int> result = new List<int>();
            foreach (var player in playerList)
            {
                if (player.Identity == identity)
                {
                    result.Add(player.UserId);
                }
            }
           
            return result;
        }

        /// <summary>
        /// 获取不同身份的用户id
        /// </summary>
        /// <returns></returns>
        public List<int> GetDifferentIdentityUserIds(Identity identity)
        {
            List<int> result = new List<int>();
            foreach (var player in playerList)
            {
                if (player.Identity != identity)
                {
                    result.Add(player.UserId);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取房间第一人的id 方便服务器判断让谁先抢地主  FIXME 如果这个playerList[0]掉线了呢 ？？？
        /// </summary>
        /// <returns></returns>
        public int GetFirstUserId()
        {
            
            return playerList[0].UserId;
        }

        /// <summary>
        /// 玩家退出房间或者掉线
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>返回掉线的人数，人数为3 就销毁房间</returns>
        public int ExitRoom(int userId)
        {
            leavePlayerIdList.Add(userId);
            return leavePlayerIdList.Count;
        }
        /// <summary>
        /// 不抢就调用一次
        /// </summary>
        /// <returns></returns>
        public int BuQiang()
        {
            this.buQiangCount++;
            var result = this.buQiangCount;
            if (this.buQiangCount == 3)
            {
                //3个都不抢
                
                //重新发牌
                InitCards();
                //初始化回合
                round.Turn(GetFirstUserId());
                this.buQiangCount = 0;
            }
            return result;
        }

        /// <summary>
        /// 销毁房间
        /// </summary>
        public void Destroy()
        {
            this.playerList = null;
            this.leavePlayerIdList.Clear();
            this.tableCards.Clear();
            this.multiple = 1;
            this.buQiangCount = 0;
            this.isFighting = false;
            autoBuChu = -1;
            autoBuQiang = -1;
            round.Reset();
        }
    }
}
