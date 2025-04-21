using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Text.RegularExpressions;
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
            Console.WriteLine("DbManager.Connect：数据库启动成功！");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("DbManager.Connect：数据库启动失败。" + e.Message);
            return false;
        }
    }

    /// <summary>
    /// 判断MySQL的语句字符串是否安全
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsSafeString(string str)
    {
        return !Regex.IsMatch(str, @"[-|;|,|\/|\[|\]|\{|\}|%|@|\*|!|\']");
    }

    /// <summary>
    /// 判断账号是否存在
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool IsAccountExist(string id)
    {
        if (!IsSafeString(id))
        {
            //id不安全就不让继续处理
            return true;
        }

        //MySQL语句
        //这里的写法要求id必须是数字，如果是字母比如tyc作为id，在下面解析时会把tyc当作列名
        string s = string.Format("SELECT * FROM account WHERE id={0}",id);

        try
        {
            //将 SQL 语句与数据库连接关联起来，准备执行
            //s：要执行的 SQL 查询字符串（如 "SELECT * FROM users WHERE id=1"）
            //mySql：已建立的 MySqlConnection 连接对象
            MySqlCommand sqlCmd = new MySqlCommand(s, mySql);
            //执行 SQL 命令并返回一个只进、只读的数据流
            //返回 MySqlDataReader 对象，用于逐行读取查询结果
            MySqlDataReader dataReader = sqlCmd.ExecuteReader();
            //检查查询结果是否包含数据行
            //true：查询结果至少包含一行数据
            //false：查询结果为空（没有匹配记录）
            bool result = dataReader.HasRows;
            //释放数据库连接资源，避免连接泄漏
            dataReader.Close();
            //没读取数据行，返回false，说明账号不存在
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine("DbManager.IsAccountExist Fail : " + e.Message);
            return true;
        }
    }

    /// <summary>
    /// 注册id
    /// </summary>
    /// <param name="id">登录id</param>
    /// <param name="pw">登录密码</param>
    /// <returns></returns>
    public static bool Register(string id, string pw)
    {
        //防止id，pw不合法，直接影响MySQL
        if (!IsSafeString(id))
        {
            Console.WriteLine("DbManager.Register : id不安全.注册失败.");
            return false;
        }
        if (!IsSafeString(pw))
        {
            Console.WriteLine("DbManager.Register : pw不安全.注册失败.");
            return false;
        }
        if (IsAccountExist(id))
        {
            Console.WriteLine("DbManager.Register : id已存在.注册失败.");
            return false;
        }

        //注册的MySQL语句
        string s = string.Format("INSERT INTO account SET id={0},pw={1};", id, pw);

        try
        {
            MySqlCommand sqlCmd = new MySqlCommand(s, mySql);
            sqlCmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("DbManager.Register 注册失败 : " + e.Message);
            return false;
        }
    }
}