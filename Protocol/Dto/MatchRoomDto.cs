using System;
using System.Collections.Generic;

namespace Protocol.Dto
{
    [Serializable]
    public class MatchRoomDto
    {
        /// <summary>
        /// 角色id 和 用户信息 的映射
        /// </summary>
        public Dictionary<int, UserDto> uIdUdtoDic;

        /// <summary>
        /// 已经准备的用户id
        /// </summary>
        public List<int> readyUidList;

        /// <summary>
        /// 存储玩家进入房间的顺序
        /// </summary>
        private List<int> uIdList;

        public MatchRoomDto()
        {
            uIdUdtoDic = new Dictionary<int, UserDto>();
            readyUidList = new List<int>();
        }

        public void Add(UserDto dto)
        {
            uIdUdtoDic.Add(dto.id,dto);
            uIdList.Add(dto.id);
        }

        public void Delete(int userId)
        {
            uIdUdtoDic.Remove(userId);
            uIdList.Remove(userId);
        }

        public string GetNameById(int userId)
        {
            return uIdUdtoDic[userId].name;
        }

        public void Ready(int userId)
        {
            readyUidList.Add(userId);
        }

        public int leftPlayerId;
        public int rightPlayerId;

        /// <summary>
        /// 每次玩家进入或者离开房间后 都需要重新调整一下位置  ==> 顺时针
        /// </summary>
        /// <param name="myId"></param>
        public void ResetPosition(int myId)
        {
            leftPlayerId = rightPlayerId = -1;
            //当前有几个玩家

            if (uIdList.Count == 1)//只有玩家自己
            {
                return;
            }
            else if (uIdList.Count == 2)//有两个
            {
                //我是第一个进来的
                if (uIdList[0] == myId) leftPlayerId = uIdList[1];
                //我是第二个进来的
                if (uIdList[1] == myId) rightPlayerId = uIdList[0];
            }
            else if (uIdList.Count == 3)//有三个
            {
                //我是第一个进来的
                if (uIdList[0] == myId)
                {
                    leftPlayerId = uIdList[1];
                    rightPlayerId = uIdList[2];
                }
                //我是第二个进来的
                else if (uIdList[1] == myId)
                {
                    leftPlayerId = uIdList[2];
                    rightPlayerId = uIdList[0];
                }
                //我是第三个进来的
                else if (uIdList[2] == myId)
                {
                    leftPlayerId = uIdList[0];
                    rightPlayerId = uIdList[1];
                }
            }
        }

    }
}
