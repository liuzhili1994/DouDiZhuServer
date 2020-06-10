using System;
using System.Collections.Generic;
using CardGameServer.Cache;
using CardGameServer.Cache.Room;
using DaligeServer;
using Protocol.Code;
using Protocol.Dto.Card;
using Protocol.Dto.Constant;
using Protocol.Dto.Fight;

namespace CardGameServer.Logic
{
    public class FightHandler : IHandler
    {
        UserCache user = Caches.User;
        FightCache fight = Caches.Fight;


        public void OnDisconnect(ClientPeer client)
        {
            //玩家退出或者掉线了  存到leaveList中去
            ExitRoom(client);
        }

        public void OnReceive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case FightCode.QIANG_LANDLORD_CREQ://抢地主
                    bool flag = (bool)value;
                    Qiang_Landlord(client,flag);
                    break;

                case FightCode.CHUPAI_CREQ:
                    ChuPai(client,value as ChuPaiDto);
                    break;
                case FightCode.BUCHU_CREQ:
                    BuChu(client);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 不出牌
        /// </summary>
        /// <param name="client"></param>
        private void BuChu(ClientPeer client)
        {
            SingleExecute.Instance.Execute(()=> {
                if (!user.IsOnLine(client))
                {
                    return;
                }

                int userId = user.GetId(client);
                var room = fight.GetRoom(userId);
                if (room.round.currentBiggsetId == userId)
                {
                    //不能不出
                    client.StartSend(OpCode.FIGHT, FightCode.BUCHU_SRES, null);
                    return;
                }
                //给客户端发送消息  我收到了你的不出请求
                //client.StartSend(OpCode.FIGHT,FightCode.BUCHU_BRO,null);
                Brocast(room,OpCode.FIGHT,FightCode.BUCHU_BRO,userId);

                //让下家出牌
                Turn(room,FightCode.CHUPAI_TURN_BRO);
            });
        }
        /// <summary>
        /// 游戏结束
        /// </summary>
        /// <param name="room"></param>
        /// <param name="userId"></param>
        private void GameOver(FightRoom room,int userId)
        {
            var winIdentity = room.GetPlayerIdentity(userId);
            var winUserIds = room.GetSameIdentityUserIds(winIdentity);
            var beens = room.multiple * 1000;
            //给胜利的玩家结算
            for (int i = 0; i < winUserIds.Count; i++)
            {
                var userModel = user.GetModelById(winUserIds[i]);
                userModel.winCount++;
                userModel.beens += beens;
                userModel.exp += 100;
                user.Update(userModel);
            }
            var loseUserIds = room.GetDifferentIdentityUserIds(winIdentity);
            //给失败的玩家结算
            for (int i = 0; i < loseUserIds.Count; i++)
            {
                var userModel = user.GetModelById(loseUserIds[i]);
                userModel.loseCount++;
                userModel.beens -= beens;
                userModel.exp += 50;
                user.Update(userModel);
            }
            //给逃跑玩家结算
            for (int i = 0; i < room.leavePlayerIdList.Count; i++)
            {
                var userModel = user.GetModelById(room.leavePlayerIdList[i]);
                userModel.runCount++;
                userModel.loseCount++;
                userModel.beens -= beens * 3;//逃跑的人减3倍
                user.Update(userModel);
            }
            //给客户端发消息  游戏结束了
            var dto = new OverDto(winIdentity,winUserIds,beens);
            Brocast(room,OpCode.FIGHT,FightCode.OVER_BRO,dto);

            //游戏结束
            fight.Destroy(room);
            
        }

        /// <summary>
        /// 转换下家
        /// </summary>
        /// <param name="room">房间</param>
        /// <param name="subCode">是出牌转换还是抢地主转换</param>
        private void Turn(FightRoom room,int subCode)
        {
            int nextUserId = room.TurnNext();
            if (room.leavePlayerIdList.Contains(nextUserId))
            {
                //掉线了就不出牌，
                Turn(room,subCode);
            }
            else
            {
                //没有掉线
                //var client = user.GetClientById(nextUserId);
                //client.StartSend(OpCode.FIGHT, FightCode.CHUPAI_TURN_BRO, null);
                Brocast(room,OpCode.FIGHT,subCode,nextUserId);
            }
        }

        private void ChuPai(ClientPeer client,ChuPaiDto dto)
        {
            SingleExecute.Instance.Execute(()=> {
                if (!user.IsOnLine(client))
                {
                    return;
                }

                int userId = user.GetId(client);
                if (userId != dto.userId)
                {
                    throw new Exception("玩家作弊！客户端id与服务器保存的id 不一致");
                }
                var room = fight.GetRoom(userId);
                //玩家出牌
                //玩家不在
                if (room.leavePlayerIdList.Contains(userId))
                {
                    //直接换下家
                    Turn(room,FightCode.CHUPAI_TURN_BRO);
                }
                else
                {
                    var flag = room.ChuPai(userId,dto.type,dto.length,dto.weight,dto.cardsList);
                    if (flag)
                    {
                        //能管上
                        //client.StartSend(OpCode.FIGHT,FightCode.CHUPAI_SRES,1);
                        //广播，管上了
                        Brocast(room,OpCode.FIGHT,FightCode.CHUPAI_BRO,dto);
                        //检测手牌，小于1  就赢了
                        var cards = room.GetPlayerCards(userId);
                        if (cards.Count == 0)
                        {
                            //游戏结束
                            GameOver(room,userId);
                        }
                        else
                        {
                            //还有牌
                            //转换出牌
                            Turn(room,FightCode.CHUPAI_TURN_BRO);
                            
                        }
                    }
                    else
                    {
                        //管不上
                        client.StartSend(OpCode.FIGHT,FightCode.CHUPAI_SRES,null);
                    }
                }
            });
        }
        /// <summary>
        /// 抢地主响应
        /// </summary>
        /// <param name="client"></param>
        /// <param name="flag"></param>
        private void Qiang_Landlord(ClientPeer client, bool flag)
        {
            SingleExecute.Instance.Execute(()=> {
                if (!user.IsOnLine(client))
                {
                    return;
                }
                int userId = user.GetId(client);
                var room = fight.GetRoom(userId);
                if (flag)//抢地主
                {
                    room.SetLandlord(userId);
                    //给每个客户端发消息谁抢了地主  还要把底牌发给地主,不是农民需要显示底牌是什么所以也不要把底牌发过去
                    Brocast(room, OpCode.FIGHT, FightCode.QIANG_LANDLORD_BRO, new LandlordDto(userId, room.tableCards));

                    //让地主出牌
                    Brocast(room, OpCode.FIGHT, FightCode.CHUPAI_TURN_BRO, userId);

                }
                else
                {
                    //不抢地主
                   
                   
                    //广播谁不抢 显示他的操作结果
                    //自己的抢地主按钮隐藏
                    Brocast(room,OpCode.FIGHT,FightCode.BUQIANG_LANDLORD_BRO,userId);

                    

                    int count = room.BuQiang();
                    if (count == 3)
                    {
                        //重新开始。给客户端发送消息 重新开始 客户端将ui重置
                        Brocast(room, OpCode.FIGHT, FightCode.Restart, null);

                        //发牌
                        foreach (var uId in room.playerList)
                        {
                            ClientPeer tempClient = user.GetClientById(uId.UserId);
                            List<CardDto> cardsDto = uId.GetCards();
                            tempClient.StartSend(OpCode.FIGHT, FightCode.GET_CARDS_SRES, cardsDto);
                        }

                        //抢地主
                        int firstUserId = room.GetFirstUserId();
                        Brocast(room, OpCode.FIGHT, FightCode.QIANG_TURN_BRO, firstUserId);//让第一个人抢地主
                        return;
                    }

                    //将下轮 该谁抢地主的id广播给客户端
                    Turn(room, FightCode.QIANG_TURN_BRO);

                }
            });
        }

        

        /// <summary>
        /// 开始战斗  三个人都准备了  由匹配房间调用
        /// </summary>
        /// <param name="userIdList"></param>
        public void StartFight(List<int> userIds)
        {
            SingleExecute.Instance.Execute(()=> {
                //创建房间
                var room = fight.Creat(userIds);
                //发牌
                foreach (var userId in userIds)
                {
                    ClientPeer client = user.GetClientById(userId);
                    List<CardDto> cardsDto = room.GetPlayerCards(userId);
                    client.StartSend(OpCode.FIGHT,FightCode.GET_CARDS_SRES, cardsDto);
                }

                //抢地主
                int firstUserId = room.GetFirstUserId();
                Brocast(room,OpCode.FIGHT,FightCode.QIANG_TURN_BRO,firstUserId);//让第一个人抢地主
            });
        }

        /// <summary>
        /// 玩家退出房间或者掉线
        /// </summary>
        /// <param name="client"></param>
        private void ExitRoom(ClientPeer client)
        {
            SingleExecute.Instance.Execute(()=> {
                if (!user.IsOnLine(client))
                {
                    //不在线
                    return;
                }
                int userId = user.GetId(client);

                if (!fight.IsFighting(userId))
                {
                    return;
                }
                var room = fight.GetRoom(userId);
                int count = room.ExitRoom(userId);
                //离线的不要发啦。都离线了还发个鬼
                Brocast(room,OpCode.FIGHT,FightCode.LEAVE_BRO,userId,client);
                //给所有人添加逃跑场次
                //给逃跑玩家结算
                var beens = room.multiple * 1000;
                for (int i = 0; i < room.leavePlayerIdList.Count; i++)
                {
                    var userModel = user.GetModelById(room.leavePlayerIdList[i]);
                    userModel.runCount++;
                    userModel.loseCount++;
                    userModel.beens -= beens * 3;//逃跑的人减3倍
                    user.Update(userModel);
                }
                if (count == 3)
                {
                    fight.Destroy(room);
                }
            });
        }


        #region 广播消息
        /// <summary>
        /// 优化网络消息
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="subCode"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private byte[] GetMsg(int opCode, int subCode, object value)
        {
            MessageData msg = new MessageData(opCode, subCode, value);
            byte[] msgBytes = EncodeTool.EncodeMsg(msg);
            byte[] msgPacket = EncodeTool.EncodeMessage(msgBytes);
            return msgPacket;
        }

        /// <summary>
        /// 广播消息 
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="subCode"></param>
        /// <param name="value"></param>
        /// <param name="client"></param>
        public void Brocast(FightRoom room,int opCode, int subCode, object value, ClientPeer client = null)
        {
            byte[] msg = GetMsg(opCode, subCode, value);
            
            //优化广播消息，进行一次沾包
            foreach (var player in room.playerList)
            {
                if (user.IsOnLine(player.UserId))//为什么做这个判断。因为3个人，退出了一个之后只剩两个，此时player里面还是3个角色  第二个角色再次退出的时候就会抛异常
                {
                    var target = user.GetClientById(player.UserId);
                    //给其他客户端发消息
                    if (client == target)
                        continue;
                    target.StartSend(msg);
                }
                
            }
        }

        #endregion
    }
}
