using DaligeServer;
namespace CardGameServer.Logic
{
    public interface IHandler
    {
        void OnReceive(ClientPeer client, int subCode,object value);

        void OnDisconnect(ClientPeer client);
    }
}