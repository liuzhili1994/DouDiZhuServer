using System;
using System.Collections.Generic;
using CardGameServer.Model;
using DaligeServer;
using DaligeServer.Util.Concurrent;

namespace CardGameServer.Cache
{
    /// <summary>
    /// 角色数据缓存层
    /// </summary>
    public class UserCache
    {
        /// <summary>
        /// 角色id 对应的 角色数据模型
        /// </summary>
        private Dictionary<int, UserModel> idModelDic = new Dictionary<int, UserModel>();
        /// <summary>
        /// 账号id 对应的 角色id
        /// </summary>
        private Dictionary<int, int> accountIdUidDic = new Dictionary<int, int>();

        private ConcurrentInt id = new ConcurrentInt(-1);


        
        /// <summary>
        /// 角色id 对应在线的客户端 ==> 只有在线玩家才不为空
        /// </summary>
        private Dictionary<int, ClientPeer> idClientDic = new Dictionary<int, ClientPeer>();
        /// <summary>
        /// 在线客户端对应的 角色id ==> 只有在线玩家才不为空
        /// </summary>
        private Dictionary<ClientPeer, int> clientIdDic = new Dictionary<ClientPeer, int>();


        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="name"></param>
        /// <param name="accountId"></param>
        public void Creat(string name,int accountId)
        {
            UserModel model = new UserModel(id.Add_Get(),name,accountId);
            //保存到字典里
            idModelDic.Add(model.id,model);
            accountIdUidDic.Add(model.id,model.id);
        }

        /// <summary>
        /// 判断该账号id下是否有角色
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public bool IsExist(int accountId)
        {
            return accountIdUidDic.ContainsKey(accountId);
        }

        /// <summary>
        /// 根据账号id 获取 角色数据模型
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public UserModel GetModelByAccountId(int accountId)
        {
            UserModel model = idModelDic[GetId(accountId)];
            return model;
        }

        /// <summary>
        /// 根据账号id 获取 角色id
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public int GetId(int accountId)
        {
            return accountIdUidDic[accountId];
        }

        /// <summary>
        /// 角色是否在线
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsOnLine(int id)
        {
            return idClientDic.ContainsKey(id);
        }

        /// <summary>
        /// 角色是否在线
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool IsOnLine(ClientPeer client)
        {
            return clientIdDic.ContainsKey(client);
        }

        /// <summary>
        /// 角色上线
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        public void OnLine(ClientPeer client,int id)
        {
            idClientDic.Add(id,client);
            clientIdDic.Add(client,id);
        }

        /// <summary>
        /// 角色下线
        /// </summary>
        /// <param name="client"></param>
        public void OffLine(ClientPeer client)
        {
            int id = clientIdDic[client];
            idClientDic.Remove(id);
            clientIdDic.Remove(client);
        }

        /// <summary>
        /// 角色下线
        /// </summary>
        /// <param name="id"></param>
        public void OffLine(int id)
        {
            ClientPeer client = idClientDic[id];
            idClientDic.Remove(id);
            clientIdDic.Remove(client);
        }

        /// <summary>
        /// 根据客户端获取角色数据模型
        /// </summary>
        /// // <param name="client"></param>
        public UserModel GetModelByClient(ClientPeer client)
        {
            int id = clientIdDic[client];
            return idModelDic[id];
        }

        /// <summary>
        /// 根据角色id 获取客户端
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ClientPeer GetModelById(int id)
        {
            return idClientDic[id];
        }

        /// <summary>
        /// 根据客户端获取角色id
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public int GetId(ClientPeer client)
        {
            return clientIdDic[client];
        }

        public UserCache()
        {
        }
    }
}
