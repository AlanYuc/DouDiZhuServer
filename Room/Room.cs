using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

public class Room
{
    /// <summary>
    /// 房间号
    /// </summary>
    public int id;
    /// <summary>
    /// 房间内的最大玩家数量
    /// </summary>
    public int maxPlayer = 3;
    /// <summary>
    /// 房间内的所有玩家,用id储存
    /// </summary>
    public List<string> playerList = new List<string>();
    /// <summary>
    /// 玩家准备状态（不包括房主）
    /// </summary>
    public Dictionary<string, bool> playerDict = new Dictionary<string, bool>();
    /// <summary>
    /// 房主的id
    /// </summary>
    public string hostId = "";
    /// <summary>
    /// 两种状态，表示整个房间是在准备中还是游戏中
    /// </summary>
    public enum Status
    {
        Prepare,
        Start,
    }
    /// <summary>
    /// 状态，表示是否在游戏中
    /// </summary>
    public Status status = Status.Prepare;
    /// <summary>
    /// 当前房间使用的一副扑克牌
    /// </summary>
    public List<Card> cards;
    /// <summary>
    /// 玩家卡牌字典，储存所有玩家对应的卡牌。
    /// 最后三张底牌用id为空的玩家单独保存
    /// </summary>
    public Dictionary<string , List<Card>> playerCards = new Dictionary<string , List<Card>>();
    /// <summary>
    /// 当前正在操作（叫地主等）的玩家id
    /// </summary>
    public string currentPlayer;
    /// <summary>
    /// 当前玩家在playerList中的索引
    /// </summary>
    public int currentPlayerIndex;

    public Room()
    {
        if(cards == null)
        {
            CardManager.Shuffle();
            cards = CardManager.cards;
        }
    }

    /// <summary>
    /// 根据id添加玩家
    /// </summary>
    /// <param name="playerId">玩家id</param>
    /// <returns>是否添加成功</returns>
    public bool AddPlayer(string playerId)
    {
        Player player = PlayerManager.GetPlayer(playerId);

        if(player == null)
        {
            Console.WriteLine("Room.AddPlayer : 添加玩家失败，玩家为空");
            return false;
        }

        if(playerList.Count >= maxPlayer)
        {
            Console.WriteLine("Room.AddPlayer : 添加玩家失败，房间内已达到最大玩家数");
            return false;
        }

        if (status == Status.Start)
        {
            Console.WriteLine("Room.AddPlayer : 添加玩家失败，房间内的游戏已开始");
            return false;
        }

        if (playerList.Contains(playerId))
        {
            Console.WriteLine("Room.AddPlayer : 添加玩家失败，该玩家已在房间内");
            return false;
        }

        //将玩家添加的列表里
        playerList.Add(playerId);
        //修改玩家所处的房间号
        player.roomId = this.id;

        //设置房主,如果当前的hostId为空，就把该玩家设置为房主
        if (hostId == "")
        {
            hostId = playerId;
            player.isHost = true;
        }

        /*
         * 玩家进入房间后，需要通知其他玩家，该玩家已进入房间
         * 否则，该玩家的客户端虽然已经进入房间，但其他玩家的客户端却看不到该玩家在房间内
         */
        Broadcast(ToMsg());

        return true;
    }

    /// <summary>
    /// 根据id删除玩家
    /// </summary>
    /// <param name="playerId">玩家id</param>
    /// <returns>是否删除成功</returns>
    public bool RemovePlayer(string playerId)
    {
        Player player = PlayerManager.GetPlayer(playerId);

        if(player == null)
        {
            Console.WriteLine("Room.RemovePlayer : 删除玩家失败，玩家为空");
            return false;
        }

        if (!playerList.Contains(playerId))
        {
            Console.WriteLine("Room.AddPlayer : 删除玩家失败，该玩家不在房间内");
            return false;
        }

        //移除玩家并更新玩家数据
        playerList.Remove(playerId);
        player.roomId = -1;

        //判断是否已准备
        if (playerDict.ContainsKey(playerId))
        {
            playerDict.Remove(playerId);
            player.isPrepare = false;
        }

        //判断是否需要更换房主
        if(player.isHost)
        {
            //要移除的玩家是房主，那么先取消房主
            player.isHost = false;

            //遍历当前房间内剩余的玩家,找到一个，设置为房主即可
            foreach(string id in playerList)
            {
                //修改房间的数据，设置房主id信息
                hostId = id;
                //修改玩家的数据，将玩家设置为房主
                PlayerManager.GetPlayer(id).isHost = true;
                break;
            }
        }

        //房间内没有人，hostId就置空
        if (playerList.Count == 0)
        {
            hostId = "";

            //房间内没有人了，直接移除该房间
            RoomManager.RemoveRoom(this.id);
        }

        /*
         * 移除玩家后，需要通知其他玩家，该玩家已被移除出房间
         * 否则，该玩家的客户端虽然已经离开房间，但其他玩家的客户端仍能看到该玩家在房间内
         */
        Broadcast(ToMsg());

        return true;
    }

    /// <summary>
    /// 根据玩家id返回其准备信息
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public bool Prepare(string playerId)
    {
        Player player = PlayerManager.GetPlayer(playerId);
        if (player == null)
        {
            Console.WriteLine("Room.Prepare : 准备失败，玩家为空");
            return false;
        }

        if (!playerList.Contains(playerId))
        {
            Console.WriteLine("Room.Prepare : 准备失败，该玩家不在房间内");
            return false;
        }

        if (!playerDict.ContainsKey(playerId))
        {
            playerDict.Add(playerId, true);
        }
        else
        {
            playerDict[playerId] = true;
        }

        player.isPrepare = true;
        Broadcast(ToMsg());
        return true;
    }

    /// <summary>
    /// 成功开始游戏后调用，处理游戏正式开始前的相关信息
    /// </summary>
    public void Start()
    {
        //随机从一个玩家开始
        Random random = new Random();
        currentPlayerIndex = random.Next(maxPlayer);
        currentPlayer = playerList[currentPlayerIndex];

        //分配玩家手牌
        for(int i = 0; i < maxPlayer; i++)
        {
            List<Card> c = new List<Card>();
            //根据最大手牌数，每人分配17张，剩余3张底牌
            for (int j = i * CardManager.maxHandSize; j < (i + 1) * CardManager.maxHandSize; j++)
            {
                c.Add(cards[j]);
            }
            playerCards.Add(playerList[i], c);
        }

        //分配最后三张底牌
        List<Card> lastThree = new List<Card>();
        for(int i= maxPlayer * CardManager.maxHandSize - 3;i< maxPlayer * CardManager.maxHandSize; i++)
        {
            lastThree.Add(cards[i]);
        }
        //把最后三张底牌放进玩家卡牌字典，id用空字符串表示
        playerCards.Add("", lastThree);
    }

    /// <summary>
    /// 广播
    /// </summary>
    /// <param name="msgBase"></param>
    public void Broadcast(MsgBase msgBase)
    {
        foreach(string id in playerList)
        {
            Player player = PlayerManager.GetPlayer(id);
            player.Send(msgBase);
        }
    }

    /// <summary>
    /// 将房间内的所有玩家转成消息发送
    /// </summary>
    /// <returns></returns>
    public MsgBase ToMsg()
    {
        MsgGetRoomInfo msgGetRoomInfo = new MsgGetRoomInfo();
        //获取房间内玩家人数
        int count = playerList.Count;
        //创建PlayerInfo的玩家数组
        msgGetRoomInfo.players = new PlayerInfo[count];

        int i = 0;
        foreach(string id in playerList)
        {
            PlayerInfo playerInfo = new PlayerInfo();
            Player player = PlayerManager.GetPlayer(id);
            playerInfo.playerID = id;
            playerInfo.bean = player.playerData.bean;
            playerInfo.isHost = player.isHost;
            playerInfo.isPrepare = player.isPrepare;

            msgGetRoomInfo.players[i++] = playerInfo;
        }

        return msgGetRoomInfo;
    }
}
