using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
#nullable disable

public class DbManager
{
    /// <summary>
    /// 数据库对象
    /// </summary>
    public static MySqlConnection mySql;

    /// <summary>
    /// 连接数据库，返回一个bool，表示是否连接成功
    /// </summary>
    /// <param name="db">数据库的名字（例如，本机的一个数据库名为game）</param>
    /// <param name="ip">IP地址</param>
    /// <param name="port">端口号</param>
    /// <param name="user">用户名</param>
    /// <param name="pw">密码</param>
    /// <returns></returns>
    public static bool Connect(string db,string ip,int port,string user,string pw)
    {
        mySql = new MySqlConnection();
        string s = string.Format("Database={0};Data Source={1};Port={2};User ID={3};Password={4};", db, ip, port, user, pw);
        mySql.ConnectionString = s;
        try
        {
            mySql.Open();
            Console.WriteLine("Connect：数据库启动成功！");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Connect：数据库启动失败。" + e.Message);
            return false;
        }
    }
}