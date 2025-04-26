using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

public class RoomManager
{
    /// <summary>
    /// 最大的房间号。每创建一个房间，该值都会加一。
    /// </summary>
    private static int maxRoomId = 0;
    /// <summary>
    /// 房间列表
    /// </summary>
    public static Dictionary<int ,Room> rooms = new Dictionary<int ,Room>();

    /// <summary>
    /// 根据id获取房间
    /// </summary>
    /// <param name="id">房间id</param>
    /// <returns>返回一个Room类型的房间，没有就返回空</returns>
    public static Room GetRoom(int id)
    {
        //先判断空，保证代码安全
        if (rooms.ContainsKey(id))
        {
            return rooms[id];
        }
        return null;
    }

    /// <summary>
    /// 根据maxRoomId，创建并添加一个房间
    /// </summary>
    /// <returns>返回新添加的房间</returns>
    public static Room AddRoom()
    {
        maxRoomId++;
        Room room = new Room();
        room.id = maxRoomId;
        rooms.Add(room.id, room);
        return room;
    }

    /// <summary>
    /// 删除房间
    /// </summary>
    /// <param name="id">房间id</param>
    public static void RemoveRoom(int id)
    {
        if (rooms.ContainsKey(id))
        {
            rooms.Remove(id);
        }
        else
        {
            Console.WriteLine("RoomManager.RemoveRoom : 删除房间失败，没有该房间");
        }
        return;
    }

    /// <summary>
    /// 转成 MsgGetRoomList 的获取房间列表的消息（因为Dictionary不方便反序列化）
    /// </summary>
    /// <returns></returns>
    public static MsgBase ToMsg()
    {
        MsgGetRoomList msgGetRoomList = new MsgGetRoomList();
        
        //获取房间的数量
        int count = rooms.Count;
        //创建对应的数组
        msgGetRoomList.rooms = new RoomInfo[count];

        //遍历所有的房间
        int i = 0;
        foreach (Room room in rooms.Values)
        {
            RoomInfo roomInfo = new RoomInfo();
            roomInfo.roomID = room.id;
            roomInfo.count = room.playerList.Count;
            if (room.status == Room.Status.Prepare)
            {
                roomInfo.isPrepare = true;
            }
            else
            {
                roomInfo.isPrepare = false;
            }

            //将房间的信息全部获取到后，放到rooms数组内
            msgGetRoomList.rooms[i++] = roomInfo;
        }

        return msgGetRoomList;
    }
}
