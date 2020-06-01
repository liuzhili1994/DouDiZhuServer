using System;
using System.Collections.Generic;
using DaligeServer;
using DaligeServer.Util.Concurrent;

namespace CardGameServer.Cache.Room
{
    /// <summary>
    /// 战斗场景缓存
    /// </summary>
    public class FightCache
    {
        public Dictionary<int, int> uIdRidDic = new Dictionary<int, int>();

        public Dictionary<int, FightRoom> rIdRoomDic = new Dictionary<int, FightRoom>();

        private Queue<FightRoom> roomQueue = new Queue<FightRoom>();

        private ConcurrentInt id = new ConcurrentInt(-1);

        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public FightRoom Creat(List<int> userIds)
        {
            FightRoom room = null;
            if (roomQueue.Count > 0)
            {
                room = roomQueue.Dequeue();
                room.Init(userIds);
            }
            else
            {
                room = new FightRoom(id.Add_Get(),userIds);
            }

            //存储
            foreach (var id in userIds)
            {
                uIdRidDic.Add(id,room.id);
            }

            rIdRoomDic.Add(room.id,room);
            return room;
        }

        /// <summary>
        /// 通过id获取战斗房间
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public FightRoom GetRoom(int userId)
        {
            if (uIdRidDic.ContainsKey(userId))
            {
                int roomId = uIdRidDic[userId];
                if (rIdRoomDic.ContainsKey(roomId))
                {
                    return rIdRoomDic[roomId];
                }
            }
            throw new Exception("该用户不再战斗房间内。。。");
        }
        /// <summary>
        /// 是不是正在战斗
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsFighting(int userId)
        {
            return uIdRidDic.ContainsKey(userId);
        }

        /// <summary>
        /// 销毁房间
        /// </summary>
        /// <param name="room"></param>
        public void Destroy(FightRoom room)
        {
            foreach (var item in room.playerList)
            {
                uIdRidDic.Remove(item.UserId);
            }
            rIdRoomDic.Remove(room.id);

            //清空房间
            room.Destroy();
            roomQueue.Enqueue(room);
        }

    }
}
