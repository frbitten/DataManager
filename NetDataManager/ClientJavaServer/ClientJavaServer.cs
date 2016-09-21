using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public class ClientJavaServer
    {
        public enum MSG_TYPE:short
        {
            DISCONNECT = 1,
            USER_MSG = 2,
            PING = 3
        }
        private static  String serverDefault;
        private static  int portDefault;

        public static void setServer(String server)
        {
            serverDefault = server;
        }
        public static void setPort(int port)
        {
            portDefault = port;
        }


        #region [ Fields ]
        private System.Net.Sockets.TcpClient tcpClient;
        #endregion

        #region [ Constructor ]
        public ClientJavaServer()
        {
            tcpClient = new System.Net.Sockets.TcpClient();
        }
        #endregion

        #region [ Public Methods ]
        public virtual void Connect()
        {
            tcpClient.Connect(serverDefault, portDefault);
        }
        public virtual void Connect(string hostname, int port)
        {            
                tcpClient.Connect(hostname, port);            
        }
        public virtual void Disconnect()
        {
            List<byte> list = new List<byte>();
            list.Add((byte)'j');
            list.Add((byte)'o');
            list.Add((byte)'o');
            list.Add((byte)1);
            list.Add((byte)1);
            NetworkStream clientStream = tcpClient.GetStream();
            clientStream.Write(list.ToArray(), 0, list.Count);
            byte[] buff=Receive();
            tcpClient.Close();
        }
        #endregion
        #region[Properties]
        public bool IsConnected
        {
            get { return tcpClient.Connected; }
        }
        #endregion

        public void Send(byte[] buffer){
            List<byte> list = new List<byte>();
            list.Add((byte)'j');
            list.Add((byte)'o');
            list.Add((byte)'o');
            list.Add((byte)1);
            list.Add((byte)2);
            list.AddRange(BitConverter.GetBytes(buffer.Length));
            list.AddRange(buffer);
            NetworkStream clientStream = tcpClient.GetStream();
            clientStream.Write(list.ToArray(), 0, list.Count);
	    }
        public byte[] Receive()
        {
            List<byte> msg = new List<byte>();
            NetworkStream clientStream = tcpClient.GetStream();
            bool isHeader = false;
            int msgSize = 0;
            while (tcpClient.Connected)
            {
                int qtd = tcpClient.Available;
                if (!isHeader)
                {
                    if (qtd >= 5)
                    {
                        bool isTag = false;
                        for (int i = 0; i < qtd - 2; i++)
                        {
                            if (clientStream.ReadByte() == 106)
                            {
                                if (clientStream.ReadByte() == 111)
                                {
                                    if (clientStream.ReadByte() == 111)
                                    {
                                        isTag = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (isTag)
                        {
                            int version = clientStream.ReadByte();
                            int type = clientStream.ReadByte();

                            switch (type)
                            {
                                case (int)MSG_TYPE.USER_MSG:
                                    byte[] sizeArray = new byte[4];
                                    clientStream.Read(sizeArray, 0, sizeArray.Length);
                                    msgSize = BitConverter.ToInt32(sizeArray,0);
                                    isHeader = true;
                                    break;
                                default:
                                    return null;
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(300);
                    }
                }
                else
                {
                    if (qtd > 0)
                    {
                        if (msg.Count + qtd > msgSize)
                        {
                            byte[] buff = new byte[msgSize - msg.Count];
                            int read = clientStream.Read(buff,0,buff.Length);
                            msg.AddRange(buff);
                        }
                        else
                        {
                            byte[] buff = new byte[qtd];
                            int read = clientStream.Read(buff, 0, buff.Length);
                            msg.AddRange(buff);
                        }
                        if (msg.Count == msgSize)
                        {
                            return msg.ToArray();
                        }
                    }
                    else
                    {
                        Thread.Sleep(300);
                    }
                }
            }
            return null;
        }
    }
}
