using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            for(int i = 0; i < sockets.Count; i++)
            {
                Socket s = sockets[i];
                if(s == socketServer)
                {
                    //当前是服务端，需要accept接收客户端连接
                }
                else
                {
                    //当前是客户端，需要receive接收客户端消息
                }
            }
        }
    }
}
