using System;
namespace Protocol.Dto
{
    [Serializable]
    public class UserDto
    {
        public int id;
        /// <summary>
        /// 角色名称
        /// </summary>
        public string name;
        /// <summary>
        /// 角色豆子的数量
        /// </summary>
        public int beens;
        /// <summary>
        /// 胜利场次
        /// </summary>
        public int winCount;
        /// <summary>
        /// 输的场次
        /// </summary>
        public int loseCount;
        /// <summary>
        /// 逃跑场次
        /// </summary>
        public int runCount;
        /// <summary>
        /// 角色等级
        /// </summary>
        public int lv;
        /// <summary>
        /// 角色当前等级下的经验
        /// </summary>
        public int exp;
        /// <summary>
        /// 服务器根据此info 辨别信息
        /// </summary>
        public string info;
        public UserDto()
        {
        }

        public void Set(string info)
        {
            this.info = info;
        }

        public void Set(string info,int id, string name,int beens,int winCount,int loseCount,int runCount,int lv,int exp)
        {
            this.id = id;
            this.info = info;
            this.name = name;
            this.beens = beens;
            this.winCount = winCount;
            this.loseCount = loseCount;
            this.runCount = runCount;
            this.lv = lv;
            this.exp = exp;
        }
    }
}
