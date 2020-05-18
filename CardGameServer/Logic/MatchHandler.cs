using System;
using CardGameServer.Cache;
using CardGameServer.Cache.Room;
using CardGameServer.Model;
using DaligeServer;
using Protocol.Code;
using Protocol.Dto;

namespace CardGameServer.Logic
{
    public class MatchHandler : IHandler
    {
        MatchCache match = Caches.Match;
        UserCache user = Caches.User;

        public void OnDisconnect(ClientPeer client)
        {
            Leave(client);
        }

        public void OnReceive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case MatchRoomCode.STARTMATCH_CREQ:
                    Enter(client);
                    break;
                case MatchRoomCode.CANCELMATCH_CREQ:
                    Leave(client);
                    break;
                case MatchRoomCode.READY_CREQ:
                    Ready(client);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 用户加入房间，将所有信息互相发送 新玩家加入了 房间，以及把新玩家信息发送给其他人，其他人的信息也发给新玩家
        /// </summary>
        /// <param name="client"></param>
        private void Enter(ClientPeer client)
        {
            SingleExecute.Instance.Execute(()=> {
                int userId = user.GetId(client);
                if (match.IsMatching(userId))
                    return;
                MatchRoom room = match.Enter(userId, client);

                //构造一个自身信息UserDto  ui需要更新什么信息就构造什么信息
                UserModel model = user.GetModelById(userId);
                UserDto userDto = new UserDto();
                userDto.Set("", model.id, model.name, model.beens, model.winCount, model.loseCount, model.runCount, model.lv, model.exp);
                //对房间内其他玩家进行广播  新用户 加入了房间
                room.Brocast(OpCode.MATCHROOM, MatchRoomCode.STARTMATCH_BRO, userDto,client);
                //将房间信息发送给自己
                var roomDto = MakeRoomDto(room);
                client.StartSend(OpCode.MATCHROOM,MatchRoomCode.STARTMATCH_SRES,roomDto);
                Console.WriteLine("有玩家进入匹配房间 ：" + room.id);
            });
            
        }


        /// <summary>
        /// 用户离开房间
        /// </summary>
        /// <param name="client"></param>
        private void Leave(ClientPeer client)
        {
            SingleExecute.Instance.Execute(()=> {
                if (!user.IsOnLine(client))
                {
                    //不在线
                    return;
                }

                int userId = user.GetId(client);
                //用户没有匹配
                if (!match.IsMatching(userId))
                {
                    return;//非法操作 不能离开
                }

                //正常离开
                MatchRoom room = match.Leave(userId);
                if (!room.IsEmpty())
                {
                    room.Brocast(OpCode.MATCHROOM,MatchRoomCode.CANCELMATCH_BRO,user.GetModelByClient(client).name,client);
                }
                Console.WriteLine("有玩家了离开匹配房间 ：" + room.id);

            });
        }

        /// <summary>
        /// 用户准备
        /// </summary>
        /// <param name="client"></param>
        private void Ready(ClientPeer client)
        {
            SingleExecute.Instance.Execute(()=> {
                if (!user.IsOnLine(client))
                {
                    //不在线
                    return;
                }

                int userId = user.GetId(client);
                if (!match.IsMatching(userId))
                {
                    return;//非法操作 不能准备
                }
                MatchRoom room = match.GetRoom(userId);
                room.Ready(userId);

                //广播消息  准备了 给其他两个发就行了
                room.Brocast(OpCode.MATCHROOM,MatchRoomCode.READY_BRO,userId,client);

                //是否所有玩家都准备了
                if (room.IsAllUserReady())
                {
                    //开始战斗
                    //TODO
                    //通知房间内的玩家 要进行战斗了 群发消息
                    room.Brocast(OpCode.MATCHROOM,MatchRoomCode.START_BRO,null);
                    match.Destroy(room);
                }
            });
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        private MatchRoomDto MakeRoomDto(MatchRoom room)
        {
            MatchRoomDto roomDto = new MatchRoomDto();
            roomDto.readyUidList = room.readyUidList;
            //给roomDto中的所有玩家信息字典 赋值
            foreach (var id in room.uIdClientDic.Keys)
            {
                //if (id == userId) //TODO   自己的信息不需要发送给自己
                //    continue;
                UserModel model = user.GetModelById(id);
                UserDto userDto = new UserDto();
                userDto.Set("", model.id, model.name, model.beens, model.winCount, model.loseCount, model.runCount, model.lv, model.exp);

                roomDto.uIdUdtoDic.Add(id, userDto);
            }
            return roomDto;
        }
    }
}
