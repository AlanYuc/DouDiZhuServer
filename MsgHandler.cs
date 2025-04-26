using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

public class MsgHandler
{
    /*
     * 消息处理类
     * 所有的消息方法都是由NetManager.OnReceiveData中的反射进行调用
     * MethodInfo methodInfo = typeof(MsgHandler).GetMethod(protocolName);
     * 每个消息都有两个参数，一个是通信的客户端，另一个是客户端发来的消息
     * 每个消息处理方法中都将对应的消息进行处理，再打包成新的消息发送回去
     */

    #region HeartBeat
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
    #endregion

    #region Login
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
    #endregion

    #region Test
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
    #endregion

    #region Room
    /// <summary>
    /// 获取玩家数据
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgGetPlayer(ClientState clientState, MsgBase msgBase)
    {
        MsgGetPlayer msgGetPlayer = msgBase as MsgGetPlayer;
        Player player = clientState.player;
        if(player == null)
        {
            return;
        }
        msgGetPlayer.bean = player.playerData.bean;
        player.Send(msgGetPlayer);
    }

    /// <summary>
    /// 获取房间列表
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgGetRoomList(ClientState clientState, MsgBase msgBase)
    {
        MsgGetRoomList msgGetRoomList = msgBase as MsgGetRoomList;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }
        player.Send(RoomManager.ToMsg());
    }

    /// <summary>
    /// 创建房间
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgCreateRoom(ClientState clientState, MsgBase msgBase)
    {
        MsgCreateRoom msgCreateRoom = msgBase as MsgCreateRoom;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        //player.roomId默认为-1，如果不是-1，说明已经在房间里了，因此不能创建房间
        if (player.roomId > -1)
        {
            msgCreateRoom.result = false;
            player.Send(msgCreateRoom);
            return;
        }

        //开始创建房间
        Room room = RoomManager.AddRoom();
        //把玩家添加到房间
        room.AddPlayer(player.id);

        msgCreateRoom.result = true;
        player.Send(msgCreateRoom);
    }
    
    /// <summary>
    /// 玩家进入房间
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgEnterRoom(ClientState clientState, MsgBase msgBase)
    {
        MsgEnterRoom msgEnterRoom = msgBase as MsgEnterRoom;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        //已经在房间里，不能再进入房间
        if (player.roomId > -1)
        {
            msgEnterRoom.result = false;
            player.Send(msgEnterRoom);
            return;
        }

        //找到准备进入的房间
        Room room = RoomManager.GetRoom(msgEnterRoom.roomID);

        //如果房间不存在，则不能进入房间
        if(room == null)
        {
            msgEnterRoom.result = false;
            player.Send(msgEnterRoom);
            return;
        }

        //判断是否能添加玩家
        if (room.AddPlayer(player.id))
        {
            msgEnterRoom.result = true;
        }
        else
        {
            msgEnterRoom.result = false;
        }
        player.Send(msgEnterRoom);
        return;
    }

    /// <summary>
    /// 获取房间信息
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgGetRoomInfo(ClientState clientState, MsgBase msgBase)
    {
        MsgGetRoomInfo msgGetRoomInfo = msgBase as MsgGetRoomInfo;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        Room room = RoomManager.GetRoom(player.roomId);

        //房间不存在，无法获取房间信息
        if (room == null)
        {
            player.Send(msgGetRoomInfo);
            return;
        }

        player.Send(room.ToMsg());
    }

    /// <summary>
    /// 离开房间
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgLeaveRoom(ClientState clientState, MsgBase msgBase)
    {
        MsgLeaveRoom msgLeaveRoom = msgBase as MsgLeaveRoom;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        Room room = RoomManager.GetRoom(player.roomId);

        //房间不存在，无法离开房间
        if (room == null)
        {
            msgLeaveRoom.result = false;
            player.Send(msgLeaveRoom);
            return;
        }

        //玩家要离开房间，因此房间内的玩家列表就需要移除该玩家
        room.RemovePlayer(player.id);
        msgLeaveRoom.result = true;
        player.Send(msgLeaveRoom);
    }
    #endregion
}
