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
            CancelMatch(client);
        }

        public void OnReceive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case MatchRoomCode.STARTMATCH_CREQ:
                    StartMatch(client);
                    break;
                case MatchRoomCode.CANCELMATCH_CREQ:
                    CancelMatch(client);
                    break;
                case MatchRoomCode.ENTERROOM_CREQ:
                    EnterRoom(client);
                    break;
                case MatchRoomCode.READY_CREQ:
                    Ready(client);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 用户开始匹配
        /// </summary>
        /// <param name="client"></param>
        private void StartMatch(ClientPeer client)
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
                //将匹配到的房间号给玩家
                var roomDto = MakeRoomDto(room);
                client.StartSend(OpCode.MATCHROOM,MatchRoomCode.STARTMATCH_SRES, roomDto);
                Console.WriteLine(string.Format("玩家 : {0}  匹配到房间 ：{1}", userDto.name,room.id));
                //int index = -1;
                //for (int i = 0; i < roomDto.uIdList.Count; i++)
                //{
                //    Console.WriteLine(roomDto.uIdList[i]);
                //    if (roomDto.uIdList[i] == userDto.id)
                //    {
                //        index = i + 1;
                //    }
                //}
                //Console.WriteLine(string.Format("有{1}个人。第{0}个进来的",index,roomDto.uIdList.Count));
            });
            
        }


        /// <summary>
        /// 用户离开匹配
        /// </summary>
        /// <param name="client"></param>
        private void CancelMatch(ClientPeer client)
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
                    room.Brocast(OpCode.MATCHROOM,MatchRoomCode.CANCELMATCH_BRO,userId,client);
                }
                Console.WriteLine(string.Format("玩家 : {0}  取消进入房间 ：{1}", user.GetModelByClient(client).name, room.id));


            });
        }

        /// <summary>
        /// 用户进入了房间  获取房间信息  发送给客户端
        /// </summary>
        /// <param name="client"></param>
        private void EnterRoom(ClientPeer client)
        {
            SingleExecute.Instance.Execute(()=> {
                MatchRoom room = match.GetRoom(user.GetId(client));
                var roomDto = MakeRoomDto(room);
                client.StartSend(OpCode.MATCHROOM,MatchRoomCode.ENTERROOM_SRES,roomDto);
                Console.WriteLine(string.Format("玩家 : {0}  进入房间 ：{1}", user.GetModelByClient(client).name, room.id));

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

                //广播消息  准备了 为什么要给自己发，确保服务器收到准备请求 回复消息后 将准备按钮隐藏
                room.Brocast(OpCode.MATCHROOM,MatchRoomCode.READY_BRO,userId);

                //是否所有玩家都准备了
                if (room.IsAllUserReady())
                {
                    //开始战斗
                    //TODO
                    //通知房间内的玩家 要进行战斗了 群发消息
                    room.Brocast(OpCode.MATCHROOM,MatchRoomCode.START_BRO,null);
                    match.Destroy(room);
                }
                Console.WriteLine(string.Format("玩家 : {0}  在房间 ：{1} 准备了", user.GetModelByClient(client).name, room.id));

            });
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        private MatchRoomDto MakeRoomDto(MatchRoom room)
        {
            MatchRoomDto roomDto = new MatchRoomDto(room.id);
            roomDto.readyUidList = room.readyUidList;
            //给roomDto中的所有玩家信息字典 赋值
            foreach (var id in room.uidList)
            {
                UserModel model = user.GetModelById(id);
                UserDto userDto = new UserDto();
                userDto.Set("", model.id, model.name, model.beens, model.winCount, model.loseCount, model.runCount, model.lv, model.exp);

                roomDto.Add(userDto);
            }
            return roomDto;
        }
    }
}
