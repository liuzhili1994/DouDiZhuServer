using System.Collections.Generic;
using DaligeServer;
using DaligeServer.Util.Concurrent;
using CardGameServer.Model;

namespace CardGameServer.Cache
{
    /// <summary>
    /// 存储账号的
    /// </summary>
    public class AccountCache
    {
        /// <summary>
        /// 账号对应的数据模型
        /// </summary>
        private Dictionary<string, AccountModel> accModelDic = new Dictionary<string, AccountModel>();


        /// <summary>
        /// 账号对应的连接对象（判断该账号是否在线）
        /// </summary>
        private Dictionary<string, ClientPeer> accClientDic = new Dictionary<string, ClientPeer>();
        private Dictionary<ClientPeer, string> clientAccDic = new Dictionary<ClientPeer,string>();

        /// <summary>
        /// 存储账号的id
        /// </summary>
        private ConcurrentInt id = new ConcurrentInt(-1);

        /// <summary>
        /// 是否存在该账号
        /// </summary>
        /// <returns></returns>
        public bool IsExist(string acc) {
            return accModelDic.ContainsKey(acc);
        }


        public void Creat(string acc, string pwd) {
            AccountModel model = new AccountModel(id.Add_Get(), acc, pwd);
            accModelDic.Add(model.account, model);
        }

        /// <summary>
        /// 获取账号对应的数据模型
        /// </summary>
        /// <returns></returns>
        public AccountModel GetModel(string acc) {
            return accModelDic[acc];
        }

        /// <summary>
        /// 账号密码是否匹配
        /// </summary>
        /// <param name="acc"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public bool IsMatch(string acc, string pwd) {
            AccountModel model = accModelDic[acc];
            return model.password == pwd;
        }

        /// <summary>
        /// 是否在线
        /// </summary>
        /// <param name="acc"></param>
        /// <returns></returns>
        public bool IsOnline(string acc) {
            return accClientDic.ContainsKey(acc);
        }

        /// <summary>
        /// 是否在线
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool IsOnline(ClientPeer client) {
            return clientAccDic.ContainsKey(client);
        }

        /// <summary>
        /// 用户上线
        /// </summary>
        public void Online(string acc,ClientPeer client) {
            accClientDic.Add(acc, client);
            clientAccDic.Add(client,acc);
        }

        /// <summary>
        /// 用户下线
        /// </summary>
        public void Offline(ClientPeer client) {
            string acc = clientAccDic[client];
            clientAccDic.Remove(client);
            accClientDic.Remove(acc);
        }

        /// <summary>
        /// 下线
        /// </summary>
        public void Offline(string acc) {
            ClientPeer client = accClientDic[acc];
            clientAccDic.Remove(client);
            accClientDic.Remove(acc);
        }

        /// <summary>
        /// 获取在线玩家的ID
        /// </summary>
        /// <returns></returns>
        public int GetID(ClientPeer client) {
            string acc = clientAccDic[client];
            AccountModel model = accModelDic[acc];
            return model.id;
        }
    }
}
