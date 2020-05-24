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
                else
                {
                    //战斗场景
                    //TODO 聊天
                }
                
            });
        }
    }
}
