using System;
using CardGameServer.Cache;
using CardGameServer.Model;
using DaligeServer;
using Protocol.Code;
namespace CardGameServer.Logic
{
    /// <summary>
    /// 用户角色逻辑处理
    /// </summary>
    public class UserHandler : IHandler
    {
        UserCache userCache = Caches.User;
        AccountCache accountCache = Caches.Account;
        

        public void OnDisconnect(ClientPeer client)
        {
            if (userCache.IsOnLine(client))
            {
                userCache.OffLine(client);
            }
        }

        public void OnReceive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case UserCode.GET_INFO_CREQ:

                    break;
                case UserCode.CREAT_CREQ:
                    Creat(client, value.ToString()); ;
                    break;
                case UserCode.ONLINE_CREQ:

                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        public void Creat(ClientPeer client,string name)
        {
            SingleExecute.Instance.Execute(()=> {
                //这个客户端是不是非法登陆
                if (!accountCache.IsOnline(client))
                {
                    //非法登陆
                    client.StartSend(OpCode.USER, UserCode.CREAT_SRES, "非法登陆");
                    return;
                }

                //获取账号id
                int accountId = accountCache.GetID(client);
                //判断有没有角色
                if (userCache.IsExist(accountId))
                {
                    //重复创建
                    client.StartSend(OpCode.USER, UserCode.CREAT_SRES, "重复创建");
                    return;
                }
                //创建角色
                userCache.Creat(name, accountId);
            });
            
        }

        /// <summary>
        /// 获取角色信息
        /// </summary>
        public void GetInfo(ClientPeer client)
        {
            SingleExecute.Instance.Execute(()=> {
                //判断这个客户端是不是非法登陆
                if (!accountCache.IsOnline(client))
                {
                    client.StartSend(OpCode.USER,UserCode.GET_INFO_SRES,"非法登陆");
                    return;
                }

                int accountId = accountCache.GetID(client);
                if (!userCache.IsExist(accountId))
                {
                    //不存在角色
                    client.StartSend(OpCode.USER,UserCode.GET_INFO_SRES,"不存在角色");
                    return;
                }

                //TODO  有问题
                UserModel model = userCache.GetModelByAccountId(accountId);
                client.StartSend(OpCode.USER,UserCode.GET_INFO_SRES,model);//获取成功
            });
        }

        /// <summary>
        /// 角色上线
        /// </summary>
        public void OnLine(ClientPeer client)
        {
            SingleExecute.Instance.Execute(()=> {
                //判断这个客户端是不是非法登陆
                if (!accountCache.IsOnline(client))
                {
                    client.StartSend(OpCode.USER, UserCode.ONLINE_SRES, "非法登陆");
                    return;
                }

                int accountId = accountCache.GetID(client);
                if (!userCache.IsExist(accountId))
                {
                    //不存在角色
                    client.StartSend(OpCode.USER, UserCode.ONLINE_SRES, "不存在角色");
                    return;
                }

                //上线成功
                int userId = userCache.GetId(accountId);
                userCache.OnLine(client,userId);
                client.StartSend(OpCode.USER, UserCode.ONLINE_SRES, "上线成功");//上线成功
            });
        }
    }
}
