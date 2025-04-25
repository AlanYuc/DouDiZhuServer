using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    /// 房主的id
    /// </summary>
    public string hostId = "";
    /// <summary>
    /// 状态，表示是否开始
    /// </summary>
    public enum Status
    {
        Prepare,
        Start,
    }
    /// <summary>
    /// 状态
    /// </summary>
    public Status status = Status.Prepare;

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

        //判断是否需要更换房主
        if(player.isHost)
        {
            player.isHost = false;

            //遍历当前房间内剩余的玩家,找到一个，设置为房主即可
            foreach(string id in playerList)
            {
                hostId = id;
                break;
            }

            //房间内没有人，hostId就置空
            if (playerList.Count == 0)
            {
                hostId = "";

                //房间内没有人了，直接移除该房间
                //TO DO
                //RoomManager
            }
        }
        return true;
    }
}
