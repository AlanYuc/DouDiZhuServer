using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EventHandler
{
    /// <summary>
    /// 掉线处理
    /// </summary>
    /// <param name="clientState"></param>
    public static void OnDisconnect(ClientState clientState)
    {

    }

    /// <summary>
    /// 超时处理
    /// </summary>
    public static void OnTimer()
    {
        CheckPing();
    }

    /// <summary>
    /// 检测ping是否超时（心跳机制）
    /// </summary>
    public static void CheckPing()
    {
        foreach(ClientState cs in NetManager.clients.Values)
        {
            if(NetManager.GetTimeStamp() - cs.lastPingTime > NetManager.pingInterval * 4)
            {
                Console.WriteLine("心跳机制：已经超时，认为断开连接");
                NetManager.Close(cs);

                //直接返回，因为字典的值不能在foreach中修改
                return;
            }
        }
    }
}