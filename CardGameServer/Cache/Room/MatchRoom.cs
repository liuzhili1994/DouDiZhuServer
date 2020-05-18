using System;
using System.Collections.Generic;
using DaligeServer;

namespace CardGameServer.Cache.Room
{
    /// <summary>
    /// Room 数据模型
    /// </summary>
    public class MatchRoom
    {
        /// <summary>
        /// 唯一id
        /// </summary>
        public int id;

        /// <summary>
        /// 一个房间可以存储多少人数
        /// </summary>
        private int userCapacity = 3;

        /// <summary>
        /// 已存在用户的id 和 对应的客户端的映射
        /// </summary>
        public Dictionary<int, ClientPeer> uIdClientDic;

        /// <summary>
        /// 已准备的用户id 集合   全部准备了就直接开始游戏了
        /// </summary>
        public List<int> readyUidList;


        public bool IsFull()
        {
            return uIdClientDic.Keys.Count == userCapacity;
        }

        public bool IsEmpty()
        {
            return uIdClientDic.Keys.Count == 0;
        }

        public bool IsAllUserReady()
        {
            return readyUidList.Count == userCapacity;
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="client"></param>
        public void Enter(int userId,ClientPeer client)
        {
            uIdClientDic.Add(userId,client);
        }
        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="userId"></param>
        public void Leave(int userId)
        {
            uIdClientDic.Remove(userId);

        }

        public void Ready(int userId)
        {
            readyUidList.Add(userId);
        }

        public void CancelReady(int userId)
        {
            readyUidList.Remove(userId);
        }

        private byte[] GetMsg(int opCode, int subCode, object value)
        {
            MessageData msg = new MessageData(opCode, subCode, value);
            byte[] msgBytes = EncodeTool.EncodeMsg(msg);
            byte[] msgPacket = EncodeTool.EncodeMessage(msgBytes);
            return msgPacket;
        }

        /// <summary>
        /// 广播消息  全部广播
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="subCode"></param>
        /// <param name="value"></param>
        public void Brocast(int opCode,int subCode,object value)
        {
            //优化广播消息，进行一次沾包
            byte[] msg = GetMsg(opCode, subCode, value);
            foreach (var client in uIdClientDic.Values)
            {
                client.StartSend(msg);
            }
        }

        /// <summary>
        /// 广播消息  给别的客户端广播
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="subCode"></param>
        /// <param name="value"></param>
        /// <param name="client"></param>
        public void Brocast(int opCode, int subCode, object value,ClientPeer client)
        {
            if (client == null)
            {
                Brocast(opCode,subCode,value);
                return;
            }
            
            //优化广播消息，进行一次沾包
            byte[] msg = GetMsg(opCode, subCode, value);
            foreach (var tempClient in uIdClientDic.Values)
            {
                //给其他客户端发消息
                if (client == tempClient)
                    continue;
                tempClient.StartSend(msg);
            }
        }


        public void Destroy()
        {
            readyUidList.Clear();
            uIdClientDic.Clear();
        }

        public MatchRoom(int id)
        {
            this.id = id;
            uIdClientDic = new Dictionary<int, ClientPeer>();
            readyUidList = new List<int>();
        }


    }
}
