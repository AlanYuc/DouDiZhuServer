using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MsgLogin : MsgBase
{
    public string id = "";
    public string pw = "";
    /// <summary>
    /// 登录是否成功
    /// </summary>
    public bool result = true;

    public MsgLogin()
    {
        protocolName = "MsgLogin";
    }
}
