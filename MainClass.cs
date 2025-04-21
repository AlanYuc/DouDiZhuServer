using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouDiZhuServer
{
    public class MainClass
    {
        public static void Main()
        {
            //测试数据库连接
            //所有数据和密码都能在navicat中查看
            //123456是本机设定的MySQL密码
            if (!DbManager.Connect("Game", "127.0.0.1", 3306, "root", "123456"))
            {
                return;
            }
            //测试DbManager.IsAccountExist方法
            Console.WriteLine("数据库是否存在：{0}", DbManager.IsAccountExist("123"));

            //测试注册
            DbManager.Register("214", "123456");
            DbManager.Register("tyc", "123456");

            //测试创建玩家
            //DbManager.CreatPlayer("1234");

            //测试连接
            NetManager.Connect("127.0.0.1", 8888);
        }
    }
}
