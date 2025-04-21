using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

public class MsgHandler
{
    /// <summary>
    /// 心跳机制
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgPing(ClientState clientState, MsgBase msgBase)
    {
        Console.WriteLine("MsgPing");
        clientState.lastPingTime = NetManager.GetTimeStamp();

        //给客户端发送Pong
        MsgPong msgPong = new MsgPong();
        NetManager.Send(clientState, msgPong);
    }

    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgRegister(ClientState clientState, MsgBase msgBase)
    {
        MsgRegister msgRegister = msgBase as MsgRegister;
        if (DbManager.Register(msgRegister.id, msgRegister.pw))
        {
            msgRegister.result = true;
            DbManager.CreatPlayer(msgRegister.id);
        }
        else
        {
            msgRegister.result = false;
        }
        NetManager.Send(clientState, msgRegister);
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgLogin(ClientState clientState, MsgBase msgBase)
    {
        MsgLogin msgLogin = msgBase as MsgLogin;
        //检验密码
        if (!DbManager.CheckPassword(msgLogin.id, msgLogin.pw))
        {
            msgLogin.result = false;
            NetManager.Send(clientState, msgLogin);
            return;
        }

        //检查玩家是否在线
        if (clientState.player != null)
        {
            msgLogin.result = false;
            NetManager.Send(clientState, msgLogin);
            return;
        }

        //将已有的玩家踢下线
        if (PlayerManager.IsOnline(msgLogin.id))
        {
            Player otherPlayer = PlayerManager.GetPlayer(msgLogin.id);
            MsgKick msgKick = new MsgKick();
            msgKick.isKick = true;
            otherPlayer.Send(msgKick);
            NetManager.Close(otherPlayer.clientState);
        }

        //检查是否获取到玩家信息
        PlayerData playerData = DbManager.GetPlayerData(msgLogin.id);
        if(playerData == null)
        {
            //没有获取到这个玩家
            msgLogin.result=false;
            NetManager.Send(clientState, msgLogin);
            return;
        }

        //创建玩家
        Player player = new Player(clientState);
        player.id = msgLogin.id;
        player.playerData = playerData;
        PlayerManager.AddPlayer(player.id, player);
        clientState.player = player;
        msgLogin.result = true;
        player.Send(msgLogin);
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
