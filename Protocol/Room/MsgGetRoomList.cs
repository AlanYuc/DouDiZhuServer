using System.Collections;
using System.Collections.Generic;
#nullable disable

/// <summary>
/// ��ȡ�����б�Э��
/// </summary>
public class MsgGetRoomList : MsgBase
{
    public RoomInfo[] rooms;

    public MsgGetRoomList()
    {
        protocolName = "MsgGetRoomList";
    }
}
