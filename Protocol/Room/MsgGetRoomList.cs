using System.Collections;
using System.Collections.Generic;
#nullable disable

/// <summary>
/// 获取房间列表协议
/// </summary>
public class MsgGetRoomList : MsgBase
{
    public RoomInfo[] rooms;

    public MsgGetRoomList()
    {
        protocolName = "MsgGetRoomList";
    }
}
