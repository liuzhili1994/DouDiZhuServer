using System;
using CardGameServer.Cache;
using CardGameServer.Model;
using DaligeServer;
using Protocol.Code;
using Protocol.Dto;

namespace CardGameServer.Logic
{
    /// <summary>
    /// 用户角色逻辑处理
    /// </summary>
    public class UserHandler : IHandler
    {
        UserCache userCache = Caches.User;
        AccountCache accountCache = Caches.Account;

        UserDto user = new UserDto();
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
                    GetInfo(client);
                    break;
                case UserCode.CREAT_CREQ:
                    Creat(client, value.ToString()); ;
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
                    user.Set("非法登陆");
                    client.StartSend(OpCode.USER, UserCode.CREAT_SRES, user);
                    Console.WriteLine("非法登陆");
                    return;
                }

                //获取账号id
                int accountId = accountCache.GetID(client);
                //判断有没有角色
                if (userCache.IsExist(accountId))
                {
                    //重复创建
                    user.Set("重复创建");
                    client.StartSend(OpCode.USER, UserCode.CREAT_SRES, user);
                    Console.WriteLine("重复创建");
                    return;
                }
                //创建角色
                userCache.Creat(name, accountId);
                user.Set("创建角色成功");
                client.StartSend(OpCode.USER,UserCode.CREAT_SRES,user);
                Console.WriteLine("创建角色成功");
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
                    user.Set("非法登陆"); 
                    client.StartSend(OpCode.USER,UserCode.GET_INFO_SRES, user);
                    Console.WriteLine("非法登陆");
                    return;
                }

                int accountId = accountCache.GetID(client);
                if (!userCache.IsExist(accountId))
                {
                    //不存在角色
                    user.Set("没有角色");
                    client.StartSend(OpCode.USER,UserCode.GET_INFO_SRES, user);
                    Console.WriteLine("没有角色");
                    return;
                }

                
                UserModel model = userCache.GetModelByAccountId(accountId);
                user.Set("获取角色成功",model.id, model.name,model.beens,model.winCount,model.loseCount,model.runCount,model.lv,model.exp);
                client.StartSend(OpCode.USER,UserCode.GET_INFO_SRES,user);//获取成功
                Console.WriteLine("获取角色成功"); 
                //有角色就自动上线
                OnLine(client);
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
                    user.Set("非法登陆");
                    client.StartSend(OpCode.USER, UserCode.ONLINE_SRES, user);
                    Console.WriteLine("非法登陆");
                    return;
                }

                int accountId = accountCache.GetID(client);
                if (!userCache.IsExist(accountId))
                {
                    //不存在角色
                    user.Set("没有角色");
                    client.StartSend(OpCode.USER, UserCode.ONLINE_SRES, user);
                    Console.WriteLine("没有角色");
                    return;
                }

                if (userCache.IsOnLine(client))
                {
                    Console.WriteLine("角色已经在线，只是更新一下角色信息");
                    return;
                }

                //上线成功
                int userId = userCache.GetId(accountId);
                userCache.OnLine(client,userId);
                user.Set("上线成功");
                client.StartSend(OpCode.USER, UserCode.ONLINE_SRES, user);//上线成功
                Console.WriteLine("角色上线成功");
            });
        }
    }
}
