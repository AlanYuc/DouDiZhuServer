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
            //所有数据和密码都能在navicat中查看
            if(!DbManager.Connect("Game", "127.0.0.1", 3306, "root", "123456"))
            {
                return;
            }
            NetManager.Connect("127.0.0.1", 8888);
        }
    }
}
