using DaligeServer;
using CardGameServer.Logic;
using Protocol.Code;

namespace CardGameServer
{
    public class NetMsgCenter : IApplication
    {
        IHandler account = new AccountHandler();
        IHandler user = new UserHandler();
        public void OnConnect(ClientPeer client)
        {
            //throw new NotImplementedException();
        }

        public void OnDisconnect(ClientPeer client)
        {
            user.OnDisconnect(client);
            account.OnDisconnect(client);
        }

        public void OnReceive(ClientPeer client, MessageData message)
        {
            switch (message.OpCode)
            {
                case OpCode.ACCOUNT:
                    account.OnReceive(client,message.SubCode,message.Value);
                    break;
                case OpCode.USER:
                    user.OnReceive(client,message.SubCode,message.Value);
                    break;
                default:
                    break;
            }
        }
    }
}