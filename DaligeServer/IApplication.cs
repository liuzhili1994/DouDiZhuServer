namespace DaligeServer
{
    public interface IApplication
    {
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="client"></param>
        void OnDisconnect(ClientPeer client);

        /// <summary>
        /// 接受数据
        /// </summary>
        void OnReceive(ClientPeer client, MessageData message);

        /// <summary>
        /// 连接
        /// </summary>
        void OnConnect(ClientPeer client);
    }
}