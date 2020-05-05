using DaligeServer;
using CardGameServer.Logic;
using Protocol.Code;

namespace CardGameServer
{
    public class NetMsgCenter : IApplication
    {
        IHandler account = new AccountHandler();
        public void OnConnect(ClientPeer client)
        {
            //throw new NotImplementedException();
        }

        public void OnDisconnect(ClientPeer client)
        {
            account.OnDisconnect(client);
        }

        public void OnReceive(ClientPeer client, MessageData message)
        {
            switch (message.OpCode)
            {
                case OpCode.ACCOUNT:
                    account.OnReceive(client,message.SubCode,message.Value);
                    break;
                default:
                    break;
            }
        }
    }
}