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

    /// <summary>
    /// 准备
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgPrepare(ClientState clientState, MsgBase msgBase)
    {
        MsgPrepare msgPrepare = msgBase as MsgPrepare;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            msgPrepare.isPrepare = false;
            player.Send(msgPrepare);
            return;
        }

        msgPrepare.isPrepare = room.Prepare(player.id);
        player.Send(msgPrepare);
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgStartGame(ClientState clientState, MsgBase msgBase)
    {
        MsgStartGame msgStartGame = msgBase as MsgStartGame;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        Room room = RoomManager.GetRoom(player.roomId);
        if(room == null)
        {
            msgStartGame.result = 3;
            player.Send(msgStartGame);
            return;
        }
        
        //人数不足，不能开始游戏
        if(room.playerList.Count < room.maxPlayer)
        {
            msgStartGame.result = 1;
            player.Send(msgStartGame);
            return;
        }

        foreach(string id in room.playerList)
        {
            //房主不需要准备
            if(id == room.hostId)
            {
                continue;
            }

            //有玩家未准备
            if(!room.playerDict.ContainsKey(id) || !room.playerDict[id])
            {
                msgStartGame.result = 2;
                player.Send(msgStartGame);
                return;
            }
        }

        //成功开始游戏后，先将卡牌分配给玩家
        room.Start();

        msgStartGame.result = 0;
        //成功开始游戏的消息需要广播给每一个房间内的玩家
        room.Broadcast(msgStartGame);
        return;
    }
    #endregion

    #region Game
    /// <summary>
    /// 获取卡牌
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgGetCardList(ClientState clientState, MsgBase msgBase)
    {
        MsgGetCardList msgGetCardList = msgBase as MsgGetCardList;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            player.Send(msgGetCardList);
            return;
        }

        //获取玩家手牌
        Card[] cards = room.playerCards[player.id].ToArray();
        msgGetCardList.cardInfos = CardManager.GetCardInfos(cards);

        //获取三张底牌
        Card[] threeCards = room.playerCards[""].ToArray();
        msgGetCardList.threeCards = CardManager.GetCardInfos(threeCards);
        
        player.Send(msgGetCardList);
        return;
    }

    /// <summary>
    /// 从哪个玩家开始叫地主
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgGetStartPlayer(ClientState clientState, MsgBase msgBase)
    {
        MsgGetStartPlayer msgGetStartPlayer = msgBase as MsgGetStartPlayer;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            player.Send(msgGetStartPlayer);
            return;
        }

        msgGetStartPlayer.playerId = room.currentPlayer;
        room.Broadcast(msgGetStartPlayer);
        return;
    }

    /// <summary>
    /// 更换回合
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgSwitchTurn(ClientState clientState, MsgBase msgBase)
    {
        MsgSwitchTurn msgSwitchTurn = msgBase as MsgSwitchTurn;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            return;
        }

        //更新为下一个玩家的索引
        //room.currentPlayerIndex++;
        room.currentPlayerIndex += msgSwitchTurn.round;
        //防止下标越界
        room.currentPlayerIndex %= room.maxPlayer;
        //更新当前房间内进行操作的玩家id
        room.currentPlayer = room.playerList[room.currentPlayerIndex];

        msgSwitchTurn.nextPlayerId = room.currentPlayer;
        room.Broadcast(msgSwitchTurn);
        return;
    }

    /// <summary>
    /// 获得同桌的其他玩家
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgGetOtherPlayers(ClientState clientState, MsgBase msgBase)
    {
        MsgGetOtherPlayers msgGetOtherPlayers = msgBase as MsgGetOtherPlayers;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            return;
        }

        for(int i = 0; i < room.playerList.Count; i++)
        {
            if (room.playerList[i] == player.id)
            {
                //先赋值自己
                msgGetOtherPlayers.playerId = room.playerList[i];

                //找到坐在左边的玩家
                if (i - 1 < 0)
                {
                    msgGetOtherPlayers.leftPlayerId = room.playerList[i - 1 + room.maxPlayer];
                }
                else
                {
                    msgGetOtherPlayers.leftPlayerId = room.playerList[i - 1];
                }

                //找到坐在右边的玩家
                if (i + 1 >= room.maxPlayer)
                {
                    msgGetOtherPlayers.rightPlayerId = room.playerList[i + 1 - room.maxPlayer];
                }
                else
                {
                    msgGetOtherPlayers.rightPlayerId = room.playerList[i + 1];
                }
            }
        }
        player.Send(msgGetOtherPlayers);
        return;
    }

    /// <summary>
    /// 处理叫地主的逻辑
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgCallLandlord(ClientState clientState, MsgBase msgBase)
    {
        MsgCallLandlord msgCallLandlord = msgBase as MsgCallLandlord;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            return;
        }

        msgCallLandlord.id = player.id;

        if (msgCallLandlord.isCall)
        {
            //叫地主

            //更新叫地主的玩家id
            room.callLandlordPlayerId = msgCallLandlord.id;
            //更新叫地主权值
            room.landLordRank[msgCallLandlord.id] = 1;

            if (room.CheckCall())
            {
                //其他玩家不能再抢地主
                msgCallLandlord.result = 3;
            }
            else
            {
                //其他玩家还能抢地主
                msgCallLandlord.result = 1;
            }
            room.Broadcast(msgCallLandlord);
            return;
        }
        else
        {
            //不叫地主
            //不更新叫地主的玩家id
            //room.callLandlordPlayerId = msgCallLandlord.id;
            //更新叫地主权值
            room.landLordRank[msgCallLandlord.id] = 0;

            if (room.CheckNotCall())
            {
                //重新洗牌
                msgCallLandlord.result = 2;
            }
            else
            {
                //其他玩家继续叫地主
                msgCallLandlord.result = 0;
            }
            room.Broadcast(msgCallLandlord);
            return;
        }
    }

    /// <summary>
    /// 都没有叫地主的时候，重新开始并洗牌
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgReStart(ClientState clientState, MsgBase msgBase)
    {
        MsgReStart msgReStart = msgBase as MsgReStart;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            return;
        }

        //洗牌
        CardManager.Shuffle();
        //更新房间内的牌的信息
        room.cards = CardManager.cards;
        //重新分配给其他玩家
        room.Start();
        room.Broadcast(msgReStart);
        return;
    }


    /// <summary>
    /// 开始抢地主
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgStartRobLandlord(ClientState clientState, MsgBase msgBase)
    {
        MsgStartRobLandlord msgStartRobLandlord = msgBase as MsgStartRobLandlord;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            return;
        }

        room.Broadcast(msgStartRobLandlord);
        return;
    }

    /// <summary>
    /// 抢地主
    /// </summary>
    /// <param name="clientState"></param>
    /// <param name="msgBase"></param>
    public static void MsgRobLandlord(ClientState clientState, MsgBase msgBase)
    {
        MsgRobLandlord msgRobLandlord = msgBase as MsgRobLandlord;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            return;
        }

        msgRobLandlord.id = player.id;

        if (msgRobLandlord.isRob)
        {
            //玩家选择抢地主
            room.landLordRank[player.id] += room.robRank;
            room.robRank++;
        }
        else
        {
            //玩家选择不抢地主
            room.landLordRank[player.id] = 0;
            if (room.CheckCall())
            {
                msgRobLandlord.landLordId = room.callLandlordPlayerId;
            }
        }

        if (msgRobLandlord.id == room.callLandlordPlayerId)
        {
            //当抢/不抢地主的玩家就是之前叫地主的玩家，说明一轮抢地主结束，开始判断谁是地主
            msgRobLandlord.landLordId = room.CheckLandLord();
        }

        //获取下一个玩家id
        int nextPlayerId = (room.currentPlayerIndex + 1) % room.maxPlayer;
        //判断下一个玩家是否叫了地主
        if (room.landLordRank[room.playerList[nextPlayerId]] == 0)
        {
            //landLordRank == 0 , 表示该玩家没叫地主，那该玩家就不能执行抢地主操作
            msgRobLandlord.isNeedRob = false;
        }
        else
        {
            msgRobLandlord.isNeedRob = true;
        }
        room.Broadcast(msgRobLandlord);
        return;
    }

    public static void MsgPlayCards(ClientState clientState, MsgBase msgBase)
    {
        MsgPlayCards msgPlayCards = msgBase as MsgPlayCards;
        Player player = clientState.player;
        if (player == null)
        {
            return;
        }

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            return;
        }

        Card[] cards = CardManager.GetCards(msgPlayCards.cardInfos);
        msgPlayCards.cardType = (int)CardManager.GetCardType(cards);
        player.Send(msgPlayCards);
    }
    #endregion
}
