using System;
using System.Collections.Generic;
using DaligeServer;
using DaligeServer.Util.Concurrent;

namespace CardGameServer.Cache.Room
{
    /// <summary>
    /// 匹配房间数据缓存层
    /// </summary>
    public class MatchCache
    {
        /// <summary>
        /// 用户id 和房间id 的映射
        /// </summary>
        private Dictionary<int, int> uIdRidDic;

        /// <summary>
        /// 房间id 和 房间的映射
        /// </summary>
        private Dictionary<int, MatchRoom> rIdRoomDic;

        /// <summary>
        /// room缓存池
        /// </summary>
        private Queue<MatchRoom> roomPool;

        /// <summary>
        /// room缓存池数量
        /// </summary>
        private int roomCapacity = 10;

        /// <summary>
        /// 安全类型房间id
        /// </summary>
        private ConcurrentInt id;

        

        public MatchCache()
        {
            id = new ConcurrentInt(-1);
            uIdRidDic = new Dictionary<int, int>();
            rIdRoomDic = new Dictionary<int, MatchRoom>();
            //默认保存10个房间
            roomPool = new Queue<MatchRoom>(roomCapacity);
            for (int i = 0; i < roomCapacity; i++)
            {
                roomPool.Enqueue(new MatchRoom(id.Add_Get()));
            }
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public MatchRoom Enter(int userId,ClientPeer client)
        {
            
            //先遍历已经存在的房间
            foreach (var room in rIdRoomDic.Values)
            {
                if (room.IsFull())
                    continue;
                room.Enter(userId,client);
                uIdRidDic.Add(userId,room.id);
                rIdRoomDic.Add(room.id,room);
                return room;
            }
            MatchRoom tempRoom = null;
            //都满了需要新建房间
            //先在缓存池中寻找
            if (roomPool.Count > 0)
            {
                tempRoom = roomPool.Dequeue();
                tempRoom.Enter(userId, client);
                uIdRidDic.Add(userId, tempRoom.id);
                rIdRoomDic.Add(tempRoom.id, tempRoom);
                return tempRoom;
            }

            //到这说明缓存池中没有需要new 一个出来
            tempRoom = new MatchRoom(this.id.Add_Get());
            tempRoom.Enter(userId,client);
            uIdRidDic.Add(userId,tempRoom.id);
            rIdRoomDic.Add(tempRoom.id,tempRoom);
            return tempRoom;

        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MatchRoom Leave(int userId)
        {
            int roomId = uIdRidDic[userId];
            MatchRoom room = rIdRoomDic[roomId];
            room.Leave(userId);
            uIdRidDic.Remove(userId);
            if (room.IsEmpty())
            {
                //这里按流程一步一步走的，所有不需要清空数据就可以入队列
                rIdRoomDic.Remove(room.id);
                if (roomPool.Count < roomCapacity)
                {
                    roomPool.Enqueue(room);
                }
            }

            return room;
        }

        /// <summary>
        /// 摧毁房间，就是当玩家都准备了，开始游戏了，需要将该房间初始化 并入队列
        /// </summary>
        /// <param name="room"></param>
        public void Destroy(MatchRoom room)
        {
            rIdRoomDic.Remove(room.id);
            foreach (var userId in room.readyUidList)
            {
                uIdRidDic.Remove(userId);
            }

            //清空数据
            
            if (roomPool.Count < roomCapacity)
            {
                room.Destroy();
                roomPool.Enqueue(room);
            }

        }

        public bool IsMatching(int userId)
        {
            return uIdRidDic.ContainsKey(userId);
        }

        public MatchRoom GetRoom(int userId)
        {
            int roomId = uIdRidDic[userId];
            MatchRoom room = rIdRoomDic[roomId];
            return room;
        }

    }
}
