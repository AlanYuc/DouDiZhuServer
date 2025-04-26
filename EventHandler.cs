using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EventHandler
{
    /// <summary>
    /// 掉线/下线处理
    /// </summary>
    /// <param name="clientState"></param>
    public static void OnDisconnect(ClientState clientState)
    {
        if(clientState.player != null)
        {
            //掉线的时候，也要把玩家从房间里面移除
            int roomId = clientState.player.roomId;
            if(roomId > -1)
            {
                Room room = RoomManager.GetRoom(roomId);
                room.RemovePlayer(clientState.player.id);
            }

            //先更新玩家数据
            DbManager.UpdatePlayerData(clientState.player.id, clientState.player.playerData);
            //然后将玩家移除
            PlayerManager.RemovePlayer(clientState.player.id);
        }
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
        foreach (ClientState cs in NetManager.clients.Values)
        {
            if (NetManager.GetTimeStamp() - cs.lastPingTime > NetManager.pingInterval * 4)
            {
                Console.WriteLine("心跳机制：已经超时，认为断开连接");
                NetManager.Close(cs);

                //直接返回，因为字典的值不能在foreach中修改
                return;
            }
        }
    }
}