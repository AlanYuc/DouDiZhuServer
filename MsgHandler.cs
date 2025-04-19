using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MsgHandler
{
    public static void MsgPing(ClientState clientState, MsgBase msgBase)
    {
        Console.WriteLine("MsgPing");
        clientState.lastPingTime = NetManager.GetTimeStamp();

        //给客户端发送Pong
        MsgPong msgPong = new MsgPong();
        NetManager.Send(clientState, msgPong);
    }

    /// <summary>
    /// 测试客户端发消息
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgTest(ClientState clientState, MsgBase msgBase)
    {
        Console.WriteLine("MsgPing");
        clientState.lastPingTime = NetManager.GetTimeStamp();

        //给客户端发送Pong
        MsgPong msgPong = new MsgPong();
        NetManager.Send(clientState, msgPong);
    }
}
