using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#nullable disable

public static class NetManager
{
    /// <summary>
    /// 服务端Socket
    /// </summary>
    public static Socket socketServer;
    /// <summary>
    /// 客户端Socket字典
    /// </summary>
    public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
    /// <summary>
    /// 用于检测的List
    /// </summary>
    private static List<Socket> sockets = new List<Socket>();
    /// <summary>
    /// 心跳的时间间隔
    /// </summary>
    public static long pingInterval = 30;

    /// <summary>
    /// 服务端连接
    /// </summary>
    /// <param name="ip">IP地址</param>
    /// <param name="port">端口号</param>
    public static void Connect(string ip, int port)
    {
        socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        socketServer.Bind(ipEndPoint);
        socketServer.Listen(0);
        Console.WriteLine("服务器启动成功");

        while (true)
        {
            //填充List
            sockets.Clear();
            sockets.Add(socketServer);
            foreach(ClientState cs in clients.Values)
            {
                sockets.Add(cs.socket);
            }

            /*
             * Socket.Select方法
             * 常用于服务器处理多个客户端连接，避免为每个 Socket 单独开线程。
             * 四个参数：
             * sockets：传入一个 List<Socket>，Select 会检查这些 Socket 是否有 可读数据。
             * null：不检查可写 Socket。
             * null：不检查错误 Socket。
             * 1000：超时时间为 1000 微秒（即 1 毫秒）。
             * 执行流程：
             * Select 会阻塞（最多 1 毫秒），等待 sockets 中的任何一个 Socket 有数据可读。
             * 如果有 Socket 可读，sockets 列表会被修改，只保留 可读的 Socket（其他 Socket 被移除）。
             * 如果超时（1 毫秒内没有 Socket 可读），sockets 会被清空（Count = 0）。
            */
            Socket.Select(sockets, null, null, 1000);
            for (int i = 0; i < sockets.Count; i++)
            {
                Socket s = sockets[i];
                if(s == socketServer)
                {
                    //当前是服务端，需要accept接收客户端连接
                    Accept(s);
                }
                else
                {
                    //当前是客户端，需要receive接收客户端消息
                    Receive(s);
                }
            }

            Timer();
        }
    }

    /// <summary>
    /// 接收客户端Socket
    /// </summary>
    /// <param name="socketServer">服务端Socket</param>
    public static void Accept(Socket socketServer)
    {
        try
        {
            Socket socketClient = socketServer.Accept();
            Console.WriteLine("Accept Success : " + socketClient.RemoteEndPoint);
            ClientState clientState = new ClientState();
            clientState.lastPingTime = GetTimeStamp();
            clientState.socket = socketClient;
            clients.Add(socketClient, clientState);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Accept Fail : " + ex.Message);
        }
    }

    /// <summary>
    /// 接收消息
    /// </summary>
    /// <param name="socketClient">发信息的客户端</param>
    public static void Receive(Socket socketClient)
    {
        ClientState clientState = clients[socketClient];
        ByteArray readBuff = clientState.readBuff;

        int count = 0;
        if (readBuff.Remain <= 0)
        {
            readBuff.MoveBytes();
            if(readBuff.Remain <= 0)
            {
                Console.WriteLine("Receive Fail : 数组长度不足");
                Close(clientState);
                return;
            }
        }

        try
        {
            count = socketClient.Receive(readBuff.bytes, readBuff.writeIndex, readBuff.Remain, SocketFlags.None);
            Console.WriteLine("接收了客户端:{0}的消息", socketClient.RemoteEndPoint);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Receive Fail : ", ex.Message);
            Close(clientState);
            return;
        }

        //接收不到消息后，就可以关闭对应的客户端连接
        if (count <= 0)
        {
            Console.WriteLine("Socket Close : " + socketClient.RemoteEndPoint);
            Close(clientState);
            return;
        }

        //写入消息的位置继续往后移
        readBuff.writeIndex += count;

        //解码
        OnReceiveData(clientState);
        readBuff.MoveBytes();
    }

    /// <summary>
    /// 关闭客户端
    /// </summary>
    /// <param name="clientState"></param>
    public static void Close(ClientState clientState)
    {
        //反射 调用OnDisconnect方法
        MethodInfo methodInfo = typeof(EventHandler).GetMethod("OnDisconnect");
        object[] obj = { clientState };
        methodInfo.Invoke(null, obj);

        clientState.socket.Shutdown(SocketShutdown.Both);
        clientState.socket.Close();
        clients.Remove(clientState.socket);
    }

    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="clientState"></param>
    public static void OnReceiveData(ClientState clientState)
    {
        ByteArray readBuff = clientState.readBuff;
        byte[] bytes = readBuff.bytes;

        if(readBuff.Length<= sizeof(int))
        {
            //接收的消息不完整
            return;
        }

        //先解析第一个数字，代表整个消息的长度
        int bodyLength = BitConverter.ToInt32(bytes, readBuff.readIndex);
        if(readBuff.Length < sizeof(int) + bodyLength)
        {
            //消息没有接收完全
            return;
        }

        //解析协议名
        readBuff.readIndex += sizeof(int);
        int nameCount;
        string protocolName = MsgBase.DecodeProtocolName(readBuff.bytes, readBuff.readIndex, out nameCount);
        if(protocolName == "")
        {
            Console.WriteLine("OnReceiveData Fail : 解析消息名失败");
            Close(clientState);
            return;
        }

        //解析协议体
        readBuff.readIndex += nameCount;
        int bodycount = bodyLength - nameCount;
        MsgBase msgBase = MsgBase.Decode(protocolName, readBuff.bytes, readBuff.readIndex, bodycount);
        readBuff.readIndex += bodycount;
        readBuff.MoveBytes();

        //分发消息
        MethodInfo methodInfo = typeof(MsgHandler).GetMethod(protocolName);
        Console.WriteLine("Receive : " + protocolName);
        if (methodInfo != null)
        {
            object[] obj = { clientState, msgBase };
            methodInfo.Invoke(null, obj);
        }
        else
        {
            Console.WriteLine("OnReceiveData 调用methodInfo函数失败");
        }

        //继续处理
        if (readBuff.Length > sizeof(int))
        {
            OnReceiveData(clientState);
        }
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void Send(ClientState clientState,MsgBase msgBase)
    {
        if(clientState == null || !clientState.socket.Connected)
        {
            return;
        }

        //编码
        byte[] nameBytes = MsgBase.EncodeProtocolName(msgBase);
        byte[] bodyBytes = MsgBase.Encode(msgBase);
        Console.WriteLine("当前发送的协议名为：" + msgBase.protocolName);

        //在消息前加上一个int的大小，用于分包粘包的处理
        int indexNum = sizeof(int) + nameBytes.Length + bodyBytes.Length;

        int index = 0;
        byte[] sendBytes = new byte[indexNum];
        BitConverter.GetBytes(nameBytes.Length + bodyBytes.Length).CopyTo(sendBytes, index);
        index += sizeof(int);
        nameBytes.CopyTo(sendBytes, index);
        index += nameBytes.Length;
        bodyBytes.CopyTo(sendBytes, index);
        index += bodyBytes.Length;

        try
        {
            clientState.socket.Send(sendBytes, 0, sendBytes.Length, SocketFlags.None);
            Console.WriteLine("Send Success!");
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Send Fail : " + ex.Message);
            Close(clientState);
            return;
        }
    }

    public static void Timer()
    {
        //还是反射
        MethodInfo methodInfo = typeof(EventHandler).GetMethod("OnTimer");
        object[] obj = { };
        methodInfo.Invoke(null, obj);
    }

    /// <summary>
    /// 获取时间戳，效果类似于Time.time
    /// </summary>
    public static long GetTimeStamp()
    {
        TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(timeSpan.TotalSeconds);
    }
}
