using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
