using System;
namespace CardGameServer.Model
{
    public class UserModel
    {
        /// <summary>
        /// 角色唯一id
        /// </summary>
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

        public UserModel(int id,string name,int accountId)
        {
            this.id = id;
            this.name = name;
            this.id = accountId;
            this.beens = 10000;
            this.winCount = this.loseCount = this.runCount = this.lv = this.exp = 0;
            
        }
    }
}
