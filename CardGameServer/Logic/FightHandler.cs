using System;
using DaligeServer;

namespace CardGameServer.Logic
{
    public class FightHandler : IHandler
    {
        public FightHandler()
        {
        }

        public void OnDisconnect(ClientPeer client)
        {
            
        }

        public void OnReceive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                default:
                    break;
            }
        }
    }
}
