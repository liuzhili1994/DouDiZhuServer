using System.Collections.Generic;
using System.Net.Sockets;

namespace DaligeServer
{
    public class ClientPeer
    {
        public ClientPeer() {
            this.ReceiveArgs = new SocketAsyncEventArgs();
            this.SendArgs = new SocketAsyncEventArgs();
            this.ReceiveArgs.UserToken = this;
            this.ReceiveArgs.SetBuffer(new byte[1024],0,1024);
            this.SendArgs.Completed += SendArgs_Completed;
        }

        

        public Socket ClientSocket { get; set; }

        

        #region 接受数据

        /// <summary>
        /// 一旦接受到数据就存到缓存区
        /// </summary>
        private List<byte> data = new List<byte>();


        /// <summary>
        /// 接收的异步套接字请求
        /// </summary>
        public SocketAsyncEventArgs ReceiveArgs { get; set; }

        /// <summary>
        /// 是否正在处理接收的数据
        /// </summary>
        private bool isReceiveProcess = false;


        public delegate void ReceiveCompleted(ClientPeer client,MessageData message);

        /// <summary>
        /// 一个消息解析完成的回调
        /// </summary>
        public ReceiveCompleted receiveCompleted;

        /// <summary>
        /// 自身处理接收到的数据包
        /// </summary>
        public void StartReceive(byte[] byteArray)
        {
            data.AddRange(byteArray);
            if (!isReceiveProcess)
            {
                ProcessReceive();
            }
        }

        /// <summary>
        /// 处理接收到的数据包
        /// </summary>
        private void ProcessReceive()
        {
            isReceiveProcess = true;

            //处理数据
            byte[] msgBytes = EncodeTool.DecodeMessage(ref data);

            if (msgBytes == null)  //数据包没有解析成功
            {
                isReceiveProcess = false;
                return;
            }

            
            MessageData msg = EncodeTool.DecodeMsg(msgBytes);
            //回调给上层
            receiveCompleted?.Invoke(this,msg);
            //递归
            ProcessReceive();
        }

        //粘包拆包问题 ： 解决策略： 消息头和消息尾
        //比如发送的数据为：123456

        private void Test() {
            //byte[] msg = Encoding.Default.GetBytes("123456");

            ////怎么构造 (消息头+消息尾组成一个新的消息)
            //int length = msg.Length;
            //byte[] msg_head = BitConverter.GetBytes(length);//消息头  为此消息的长度
            //byte[] msg_new = msg_head + msg;  //两条消息合体
            
            ////怎么读取
            //int length = 前四个字节转成int类型  //得到本条消息的长度  再往后读取该长度个字节
            ////读取到的内容就是本条内容的全部内容，
        }
        #endregion

        #region 发送数据

        /// <summary>
        /// 发送消息队列
        /// </summary>
        private Queue<byte[]> sendQueue = new Queue<byte[]>();

        //是否正在发送
        private bool isSendProcess = false;

        /// <summary>
        /// 发送的异步套接字操作
        /// </summary>
        private SocketAsyncEventArgs SendArgs { set; get; }


        public delegate void SendDisconnect(ClientPeer client,string reason);

        public SendDisconnect sendDisconnect;


        /// <summary>
        /// 发送网络消息
        /// </summary>
        /// <param name="opCode">操作码</param>
        /// <param name="subCode">子操作</param>
        /// <param name="value">参数</param>
        public void StartSend(int opCode,int subCode,object value) {
            MessageData msg = new MessageData(opCode,subCode,value);
            byte[] msgBytes = EncodeTool.EncodeMsg(msg);
            byte[] msgPacket = EncodeTool.EncodeMessage(msgBytes);

            ////存到消息队列中
            //sendQueue.Enqueue(msgPacket);
            //if (!isSendProcess)
            //{
            //    Send();
            //}
            StartSend(msgPacket);
        }

        public void StartSend(byte[] msg)
        {
            //存到消息队列中
            sendQueue.Enqueue(msg);
            if (!isSendProcess)
            {
                Send();
            }
        }

        /// <summary>
        /// 处理发送消息的方法
        /// </summary>
        private void Send() {
            isSendProcess = true;

            if (sendQueue.Count == 0)
            {
                isSendProcess = false;
                return;
            }
            //取出一条消息
            byte[] msgPacket = sendQueue.Dequeue();
            //设置消息发送异步对象的发送数据缓冲区
            SendArgs.SetBuffer(msgPacket,0,msgPacket.Length);
            bool result = ClientSocket.SendAsync(SendArgs);
            if (result == false)
            {
                ProcessSend();
            }
        }

        private void SendArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend();
        }

        /// <summary>
        /// 当异步发送请求完成时调用
        /// </summary>
        private void ProcessSend() {
            //发送的有没有错误
            if (SendArgs.SocketError != SocketError.Success)
            {
                //发送有错  客户端断开连接了
                if (sendDisconnect != null)
                {
                    sendDisconnect(this,SendArgs.SocketError.ToString());
                }
                
            }
            else
            {
                Send();
            }
        }

        #endregion

        #region 断开连接

        public void Disconnect() {
            //清空数据
            data.Clear();
            isReceiveProcess = false;
            //清空发送数据缓存
            sendQueue.Clear();
            isSendProcess = false;

            
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            ClientSocket = null;
        }

        #endregion
    }
}
