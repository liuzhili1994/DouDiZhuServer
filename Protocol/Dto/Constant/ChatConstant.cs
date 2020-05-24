using System;
using System.Collections.Generic;

namespace Protocol.Dto.Constant
{
    public static class ChatConstant
    {
        private static Dictionary<int, string> chatIdContentDic;
        static ChatConstant()
        {
            chatIdContentDic = new Dictionary<int, string>();
            chatIdContentDic.Add(1,"大家好，很高兴见到各位！");
            chatIdContentDic.Add(2,"和你合作真是太愉快了！");
            chatIdContentDic.Add(3,"快点吧，我等到花都谢了！");
            chatIdContentDic.Add(4,"你的牌打的也太好了！");
            chatIdContentDic.Add(5,"不要吵了，有什么好吵的，专心玩游戏吧！");
            chatIdContentDic.Add(6,"不要走，决战到天亮！");
            chatIdContentDic.Add(7,"再见了，我回想念大家的！");
        }

        public static string GetContent(int id)
        {
            return chatIdContentDic[id];
        }
    }
}
