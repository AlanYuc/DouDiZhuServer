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
