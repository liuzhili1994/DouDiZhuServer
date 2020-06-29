using System;
using CardGameServer.Cache;
using CardGameServer.Cache.Room;
using DaligeServer;
using Protocol.Code;
using Protocol.Dto;

namespace CardGameServer.Logic
{
    public class ChatHandler : IHandler
    {
        MatchCache match = Caches.Match;
        UserCache user = Caches.User;
        FightCache fight = Caches.Fight;
        public ChatHandler()
        {
        }

        public void OnDisconnect(ClientPeer client)
        {
        }

        public void OnReceive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case ChatCode.CHAT_CREQ:
                    Chat(client,(int)value);
                    break;
                default:
                    break;
            }
        }

        public void Chat(ClientPeer client,int type)
        {
            SingleExecute.Instance.Execute(()=> {
                if (!user.IsOnLine(client))
                {
                    return;
                }
                int userId = user.GetId(client);
                //匹配场景
                if (match.IsMatching(userId))
                {
                    MatchRoom mr = match.GetRoom(userId);
                    ChatDto dto = new ChatDto(userId,type);
                    mr.Brocast(OpCode.CHAT, ChatCode.CHAT_SRES, dto);
                }
                else if(fight.IsFighting(userId))
                {
                    //战斗场景
                    FightRoom mr = fight.GetRoom(userId);
                    ChatDto dto = new ChatDto(userId, type);
                    //fight.(OpCode.CHAT, ChatCode.CHAT_SRES, dto);
                    Brocast(mr, OpCode.CHAT, ChatCode.CHAT_SRES, dto);
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
        public void Brocast(FightRoom room, int opCode, int subCode, object value, ClientPeer client = null)
        {
            byte[] msg = GetMsg(opCode, subCode, value);

            //优化广播消息，进行一次沾包
            foreach (var player in room.playerList)
            {
                if (user.IsOnLine(player.UserId))//为什么做这个判断。因为3个人，退出了一个之后只剩两个，此时player里面还是3个角色  第二个角色再次退出的时候就会抛异常
                {
                    var target = user.GetClientById(player.UserId);
                    //给其他客户端发消息
                    if (client == target || !fight.IsFighting(player.UserId)) //提前退出的玩家不广播
                        continue;
                    target.StartSend(msg);
                }

            }
        }

        #endregion
    }
}
