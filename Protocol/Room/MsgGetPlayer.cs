using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ��ȡ��ҵ�Э��
/// </summary>
public class MsgGetPlayer : MsgBase
{
    public int bean;

    public MsgGetPlayer()
    {
        protocolName = "MsgGetPlayer";
    }
}
