using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace DaligeServer
{
    public class ServerPeer
    {
        /// <summary>
        /// 服务器端的Socket对象
        /// </summary>
        private Socket serverSocket = null;

        /// <summary>
        /// 限制客户端连接数量的信号量
        /// </summary>
        private Semaphore acceptSemaphore;

        /// <summary>
        /// 客户端对象的连接池
        /// </summary>
        private ClientPeerPool clientPeerPool;

        /// <summary>
        /// 应用层
        /// </summary>
        private IApplication application;

        /// <summary>
        /// 设置应用层
        /// </summary>
        /// <param name="application"></param>
        public void SetApplication(IApplication application) {
            this.application = application;
        }

        public ServerPeer() {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// 用来开启服务器
        /// </summary>
        /// <param name="_port">端口号</param>
        /// <param name="_xCount">最大连接数</param>
        public void Start(int _port,int _maxCount) {
            //使用try catch捕捉异常
            try
            {
                acceptSemaphore = new Semaphore(_maxCount,_maxCount);
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
                serverSocket.Listen(_maxCount);
                //创建连接池
                clientPeerPool = new ClientPeerPool(_maxCount);
                ClientPeer temClientPeer = null;
                for (int i = 0; i < _maxCount; i++)
                {
                    temClientPeer = new ClientPeer();
                    
                    temClientPeer.ReceiveArgs.Completed += ReceiveArgs_Completed;
                    temClientPeer.sendDisconnect += Disconnect;
                    temClientPeer.receiveCompleted += ReceiveCompleted;
                    clientPeerPool.Enqueue(temClientPeer);
                }
                Console.WriteLine("服务器开启成功...端口号：" + _port);

                StartAccept();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        

        #region 连接

        private void StartAccept(SocketAsyncEventArgs e = null) {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += E_Completed;
            }

            //Console.WriteLine("等待客户端连接...");
            
            
            //返回值表示异步事件是否完成，如果返回true 代表正在执行，执行完成后会触发 E_Completed
            //                            如果返回false 代表已经完成 直接处理
            bool result = serverSocket.AcceptAsync(e);

            if (!result)
            {
                ProcessAccept(e);
            }
        }

        /// <summary>
        /// 接受连接请求 异步事件完成时触发
        /// </summary>
        private void E_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }




        /// <summary>
        /// 处理连接请求
        /// </summary>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            //计数    限制进程访问数
            acceptSemaphore.WaitOne();

            //得到一个socket对象
            // Socket clientSocket = e.AcceptSocket;
            ClientPeer clientSocket = clientPeerPool.Dequeue();
            clientSocket.ClientSocket = e.AcceptSocket;

            Console.WriteLine("客户端连接成功 ：" + clientSocket.ClientSocket.RemoteEndPoint.ToString());

            //开始接受数据
            StartReceive(clientSocket);

            //再进行其他操作
            e.AcceptSocket = null;
            StartAccept(e);
        }

        #endregion



        #region 接受数据

        /// <summary>
        /// 开始接受数据
        /// </summary>
        /// <param name="client"></param>
        private void StartReceive(ClientPeer client) {
            try
            {
                bool result = client.ClientSocket.ReceiveAsync(client.ReceiveArgs);
                if (!result)
                {
                    ProcessReceive(client.ReceiveArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 处理接收的请求
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs e) {
            ClientPeer client = e.UserToken as ClientPeer;

            //判断网络消息是否接受成功
            int receiveDataLength = client.ReceiveArgs.BytesTransferred;
            if (client.ReceiveArgs.SocketError == SocketError.Success && receiveDataLength > 0)
            {
                //拷贝数据到数组中
                byte[] byteArray = new byte[receiveDataLength];
                Buffer.BlockCopy(client.ReceiveArgs.Buffer, 0, byteArray, 0, receiveDataLength);
                //让客户端自行解析数据包  （粘包）
                client.StartReceive(byteArray);

                StartReceive(client);
            }
            else
            {
                //断开连接了
                //如果没有传输的字节数，表示断开连接了
                if (client.ReceiveArgs.BytesTransferred == 0)
                {
                    if (client.ReceiveArgs.SocketError == SocketError.Success)
                    {
                        //客户端主动断开连接
                        Disconnect(client,"客户端主动断开连接...");

                    }
                    else
                    {
                        //由于网络异常等原因，被动断开连接
                        Disconnect(client, client.ReceiveArgs.SocketError.ToString());
                    }
                }
            }
        }

        private void ReceiveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        /// <summary>
        /// 一条数据解析完成的处理 
        /// </summary>
        /// <param name="client">对应的连接对象</param>
        /// <param name="value">解析出来的一个具体能使用的类型</param>
        private void ReceiveCompleted(ClientPeer client,MessageData message) {
            //给应用层
            application.OnReceive(client,message);
        }

        #endregion


        #region 断开连接

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="client">断开的客户端连接对象</param>
        /// <param name="reason">断开原因</param>
        public void Disconnect(ClientPeer client,string reason) {
            try
            {
                //清空数据
                if (client == null)
                {
                    throw new Exception("请确保当前有客户端连接...");
                }

                
                //通知应用层，这个客户端断开了
                application.OnDisconnect(client);
                Console.WriteLine(client.ClientSocket.RemoteEndPoint.ToString() + "  客户端断开连接，原因：" + reason);
                client.Disconnect();
                clientPeerPool.Enqueue(client);
                acceptSemaphore.Release();

                
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        #endregion

        #region 发送数据

        #endregion


    }
    
}