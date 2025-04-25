using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

public class Player
{
    /// <summary>
    /// 玩家ID
    /// </summary>
    public string id = "";
    /// <summary>
    /// 是否为房主
    /// </summary>
    public bool isHost = false;
    /// <summary>
    /// 该玩家所在的房间号，-1表示不再任何房间内
    /// </summary>
    public int roomId = -1;
    /// <summary>
    /// 玩家所在的客户端
    /// </summary>
    public ClientState clientState;
    /// <summary>
    /// 玩家的数据
    /// </summary>
    public PlayerData playerData;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="clientState">对应的客户端</param>
    public Player(ClientState clientState)
    {
        this.clientState = clientState;
    }

    /// <summary>
    /// 封装好的玩家的发送消息
    /// </summary>
    /// <param name="msgBase">要发送的消息</param>
    public void Send(MsgBase msgBase)
    {
        NetManager.Send(clientState, msgBase);
    }
}