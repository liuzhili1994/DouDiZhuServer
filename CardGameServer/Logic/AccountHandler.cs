using System;
using DaligeServer;
using CardGameServer.Cache;
using Protocol.Code;
using Protocol.Dto;

namespace CardGameServer.Logic
{
    public class AccountHandler : IHandler
    {
        AccountCache accountCache = Caches.Account;

        public void OnDisconnect(ClientPeer client)
        {
            if(accountCache.IsOnline(client))
            accountCache.Offline(client);
        }

        
        public void OnReceive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case AccountCode.REGISTER_CREQ:
                    {
                        AccountDto dto = value as AccountDto;
                        Register(client,dto.Account,dto.Password);
                    }
                    break;
                case AccountCode.LOGIN:
                    {
                        AccountDto dto = value as AccountDto;
                        Login(client,dto.Account,dto.Password);
                    }
                    break;
                default:
                    break;
            }
        }

        private void Login(ClientPeer client, string account, string password)
        {
            SingleExecute.Instance.Execute(() => {
                if (accountCache.IsExist(account))  //存在该账号
                {
                    //然后判断密码
                    if (accountCache.IsMatch(account, password))
                    {
                        //密码正确
                        //client.StartSend(OpCode.ACCOUNT, AccountCode.LOGIN, "密码正确...");
                        if (accountCache.IsOnline(account))
                        {
                            //账号在线
                            client.StartSend(OpCode.ACCOUNT, AccountCode.LOGIN, "账号已经在线...");
                            Console.WriteLine("账号已经在线...");
                        }
                        else
                        {
                            //登录成功
                            accountCache.Online(account, client);
                            client.StartSend(OpCode.ACCOUNT, AccountCode.LOGIN, "登录成功...");
                            Console.WriteLine("登录成功...");
                        }
                    }
                    else
                    {
                        //密码输入错误
                        client.StartSend(OpCode.ACCOUNT, AccountCode.LOGIN, "密码输入错误...");
                    }
                }
                else
                {
                    //不存在该账号
                    client.StartSend(OpCode.ACCOUNT, AccountCode.LOGIN, "不存在该账号...");
                }
            });
            
        }

        private void Register(ClientPeer client, string account, string password)
        {

            SingleExecute.Instance.Execute(()=> {
                if (accountCache.IsExist(account))
                {
                    //账号已经存在
                    client.StartSend(OpCode.ACCOUNT, AccountCode.REGISTER_SRES, "账号已经存在...");
                    Console.WriteLine("账号已经存在...");
                    return;
                }

                if (string.IsNullOrEmpty(account))
                {
                    //表示账号输入不合法
                    client.StartSend(OpCode.ACCOUNT, AccountCode.REGISTER_SRES, "账号输入不合法...");
                    Console.WriteLine("账号输入不合法...");
                    return;
                }

                if (string.IsNullOrEmpty(password) || password.Length <= 4 || password.Length >= 16)
                {
                    //密码不合法
                    client.StartSend(OpCode.ACCOUNT, AccountCode.REGISTER_SRES, "密码输入不合法...");
                    Console.WriteLine("密码输入不合法...");
                    return;
                }

                //可以注册了
                accountCache.Creat(account, password);
                client.StartSend(OpCode.ACCOUNT, AccountCode.REGISTER_SRES, "注册成功...");
                Console.WriteLine("注册成功...");
            });
            
        }
    
    }
}