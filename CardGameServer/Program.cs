using System;
using System.Collections.Generic;
using System.Threading;
using DaligeServer;

namespace CardGameServer
{
    class MainClass
    {
        
        public static void Main(string[] args)
        {

            ServerPeer server = new ServerPeer();
            server.Start(6666, 10);
            server.SetApplication(new NetMsgCenter());

            //string msg = "I am a message";
            //Console.WriteLine(msg);
            //byte[] msgByte = Encoding.Default.GetBytes(msg);
            //Console.WriteLine("msg.Length  : " + msg.Length);
            //byte[] newMsg = EncodeTool.EncodeMessage(msgByte);
            //Console.WriteLine("newMsg.Length  : " + newMsg.Length);
            //Console.WriteLine(Encoding.Default.GetString(newMsg));
            Console.ReadLine();
        }
    }
}
