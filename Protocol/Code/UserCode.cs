using System;
namespace Protocol.Code
{
    public class UserCode
    {
        //获取信息
        public const int GET_INFO_CREQ = 0;
        public const int GET_INFO_SRES = 1;

        //创建角色
        public const int CREAT_CREQ = 2;
        public const int CREAT_SRES = 3;

        //角色上线
        //public const int ONLINE_CREQ = 4;//获取到角色自动上线
        public const int ONLINE_SRES = 5;
    }
}
