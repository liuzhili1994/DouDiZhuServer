using System.Collections.Generic;
namespace DaligeServer
{
    /// <summary>
    /// 客户端连接池
    /// 作用：重用客户端的连接对象，每一次连接客户端都需要new 一个socket  
    /// 为了减少new 的次数 使用对象池的原理 创建一个客户端连接池
    /// </summary>
    public class ClientPeerPool
    {
        private Queue<ClientPeer> clientPeerQueue;

        public ClientPeerPool(int capacity) {
            clientPeerQueue = new Queue<ClientPeer>(capacity);
        }

        public void Enqueue(ClientPeer client) {
            clientPeerQueue.Enqueue(client);
        }

        public ClientPeer Dequeue() {
            return clientPeerQueue.Dequeue();
        }
    }
}